using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Quadrablaze.Boss;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class BossInfoUI : MonoBehaviour {

        public static BossInfoUI Current { get; private set; }

        [SerializeField]
        TextMeshProUGUI _titleText;

        [SerializeField]
        Slider _healthSlider;

        [SerializeField]
        TextMeshProUGUI _timerText;

        [SerializeField]
        BossSpawner _bossSpawner;

        public void Initialize() {
            Current = this;

            OnBossDefeated();

            TimedBossSpawnController.OnTimerChanged += SetTimer;
            EnemyProxy.Proxy.Subscribe(EntityActions.Spawned, Proxy_Spawned);
        }

        void Proxy_Spawned(System.EventArgs args) {
            var entityArgs = (EntityArgs)args;
            var entity = entityArgs.GetEntity<Entities.BossEntity>();

            if(entity != null)
                OnBossProxySpawned(entity);
        }

        void OnBossProxySpawned(Entities.EnemyEntity enemyEntity) {
            if(enemyEntity is Entities.BossEntity bossEntity)
                OnBossSpawned(bossEntity);
        }

        public void SetTimer(float value) {
            float minutes = Mathf.Floor(value / 60);
            float seconds = Mathf.Floor(value % 60);

            if(minutes > 0)
                _timerText.text = minutes.ToString("0") + ":" + seconds.ToString("00");
            else
                _timerText.text = seconds.ToString("0");
        }

        //public void OnBossSpawned() {
        //    OnBossSpawned(BossSpawner.Current.SpawnedBoss);
        //}
        void OnBossSpawned(Entities.BossEntity bossEntity) {
            _timerText.gameObject.SetActive(false);
            _healthSlider.transform.parent.gameObject.SetActive(true);
            _titleText.text = bossEntity.Name;
        }

        public void OnBossDefeated() {
            if(!NetworkServer.active) return;
            Debug.Log("OnBossDefeated");

            NetworkServer.SendToAll(NetMessageType.Client_SetBossDefeated, new EmptyMessage());
        }

        public void SetHealth(float normalizedHealthValue) {
            if(!NetworkServer.active) return;

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_SetBossHealthUI);
            writer.Write(normalizedHealthValue);
            writer.FinishMessage();

            foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
                connection.SendWriter(writer, Channels.DefaultUnreliable);
        }

        public void Reset() {
            _timerText.gameObject.SetActive(true);
            _healthSlider.transform.parent.gameObject.SetActive(false);
            _titleText.text = "Boss Incoming!";
        }

        static void NetworkSetHealth(NetworkMessage networkMessage) {
            var health = networkMessage.reader.ReadSingle();

            Current._healthSlider.normalizedValue = health;
        }

        static void NetworkSetBossDefeated(NetworkMessage networkMessage) {
            Debug.Log("NetworkSetBossDefeated");
            GameDebug.Log("Boss Defeated", "Boss");
            Current.Reset();
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_SetBossHealthUI, NetworkSetHealth);
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_SetBossDefeated, NetworkSetBossDefeated);
        }

        class GameObjectMessage : MessageBase {
            public GameObject gameObject;

            public GameObjectMessage(GameObject gameObject) {
                this.gameObject = gameObject;
            }

            public override void Serialize(NetworkWriter writer) {
                writer.Write(gameObject);
            }

            public override void Deserialize(NetworkReader reader) {
                gameObject = reader.ReadGameObject();
            }
        }
    }
}