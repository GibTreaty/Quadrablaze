using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using YounGenTech.PoolGen;
using Quadrablaze.Boss;
using UnityEngine.Networking;
using System.Collections.Generic;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class BossSpawner : MonoBehaviour {

        public static BossSpawner Current { get; private set; }

        [SerializeField]
        float _timeDelay = 120;

        [SerializeField]
        float _timeBeforeSpawn;

        [SerializeField]
        bool _timerActive;

        [SerializeField]
        bool _bossHasSpawned;

        [SerializeField]
        GameObject _spawnedBoss;

        [SerializeField]
        BossInfoDatabase _bossDatabase;

#if UNITY_EDITOR
        [SerializeField]
        BossInfoDatabase _editorBossDatabase;
#endif

        [SerializeField]
        PoolManager _bossPool;

        public FloatEvent OnTimerChanged;
        public UnityEvent OnBossSpawned;
        public UnityEvent OnBossDefeated;
        // TODO: Set these UnityEvents to new in Start or Awake

        float lastSendTime = 0;

        #region Properties
        public BossInfoDatabase BossDatabase {
            get {
#if UNITY_EDITOR
                return _editorBossDatabase;
#else
                return _bossDatabase;
#endif
            }
        }

        public bool BossHasSpawned {
            get { return _bossHasSpawned; }
            set { _bossHasSpawned = value; }
        }

        public PoolManager BossPool {
            get { return _bossPool; }
            set { _bossPool = value; }
        }

        public int BossesSpawnedThisGame { get; set; }

        public GameObject SpawnedBoss {
            get { return _spawnedBoss; }
            set { _spawnedBoss = value; }
        }

        public bool TimerActive {
            get { return _timerActive; }
            set {
                if(TimerActive == value) return;

                _timerActive = value;

                if(TimerActive)
                    if(TimeBeforeSpawn == 0) TimeBeforeSpawn = TimeDelay;
            }
        }

        public float TimeBeforeSpawn {
            get { return _timeBeforeSpawn; }
            set {
                if(TimeBeforeSpawn == value) return;

                _timeBeforeSpawn = value;

                if(OnTimerChanged != null) OnTimerChanged.Invoke(TimeBeforeSpawn);
            }
        }

        public float TimeDelay {
            get { return _timeDelay; }
            set { _timeDelay = value; }
        }
#endregion

        void OnEnable() {
            Current = this;
        }

        void Start() {
            if(TimerActive)
                TimeBeforeSpawn = TimeDelay;
        }

        void Update() {
            // TODO: Allow for bosses to be taken out of the list as they are defeated.
            // TODO: Make sure there is a boss left in the pool.

            if(!TimerActive) return;

            if(TimeBeforeSpawn > 0) {
                TimeBeforeSpawn = Mathf.Max(TimeBeforeSpawn - Time.deltaTime, 0);

                if(NetworkServer.active)
                    if(Mathf.Abs(lastSendTime - TimeBeforeSpawn) >= .2f)
                        SendTimeOverNetwork();
            }

            if(NetworkServer.active)
                if(!BossHasSpawned) {
                    if(TimeBeforeSpawn == 0) SpawnBoss();
                }
                else {
                    if(SpawnedBoss && !SpawnedBoss.activeInHierarchy)
                        SpawnedBoss = null;

                    if(!SpawnedBoss) {
                        BossHasSpawned = false;
                        ResetTimer();
                    }
                }
        }

        static void NetworkSetTimer(NetworkMessage networkMessage) {
            float serverTimer = networkMessage.reader.ReadSingle();

            if(Mathf.Abs(serverTimer - Current._timeBeforeSpawn) > 1)
                Current.TimeBeforeSpawn = serverTimer;
        }

        public void Reset() {
            ResetTimer(false);
            //BossPool.DespawnAll();
            BossesSpawnedThisGame = 0;
        }

        public void ResetTimer(bool start = true) {
            TimeBeforeSpawn = TimeDelay;
            TimerActive = start;

            SendTimeOverNetwork();
        }

        void SendTimeOverNetwork() {
            if(QuadrablazeSteamNetworking.Current.SteamConnections.Count == 1) return;

            lastSendTime = TimeBeforeSpawn;

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_SetBossTimer);
            writer.Write(lastSendTime);
            writer.FinishMessage();

            for(int i = 1; i < QuadrablazeSteamNetworking.Current.SteamConnections.Count; i++)
                QuadrablazeSteamNetworking.Current.SteamConnections[i]?.SendWriter(writer, Channels.DefaultUnreliable);
        }

        void SpawnBoss() {
            if(BossHasSpawned) return;

            BossHasSpawned = true;

            var randomBossIndex = BossPool.GetWeightedPrefabIndex(false);
            var bossPrefab = BossPool.PoolGenPrefabs[randomBossIndex];

            var bossPrefabController = bossPrefab.Prefab.GetComponent<BossController>();

            SpawnedBoss = BossPool.Spawn(randomBossIndex, bossPrefabController.GetStartPosition(), bossPrefabController.GetStartRotation()).gameObject;

            if(SpawnedBoss) {
                //SpawnedBoss.GetComponent<BossController>().StartBoss();
                CameraSoundController.Current.PlaySound("Boss Spawn", true);

                BossesSpawnedThisGame++;

                if(OnBossSpawned != null) OnBossSpawned.Invoke();
            }
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_SetBossTimer, NetworkSetTimer);
        }
    }
}