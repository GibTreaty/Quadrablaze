using System;
using Quadrablaze.Entities;
using StatSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;

#pragma warning disable CS0618

namespace Quadrablaze.Boss {
    public abstract class BossController : NetworkBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        GameObject _bossObject;

        [SerializeField]
        int _stage = 1;

        [SerializeField]
        float _stageUpDelay;

        [SerializeField]
        float _stageUpTime;

        protected bool initialized;

        #region Properties
        public uint EntityId { get; set; }

        public GameObject BossObject {
            get { return _bossObject; }
            set { _bossObject = value; }
        }

        public virtual void OnDamaged(StatEvent statEvent) { }

        public virtual GameObject[] ControlsEntities {
            get { return null; }
        }

        public int Stage { get; set; }

        #endregion

        public virtual void ActorEntityObjectInitialize(ActorEntity entity) {
            BaseInitialize(entity);
            initialized = true;
        }

        protected void BaseInitialize(ActorEntity entity) {
            EntityId = entity.Id;
        }

        public Entities.BossEntity GetEntity() {
            return EntityId.GetActorEntity() as Entities.BossEntity;
        }

        void KillEntity() {
            GetEntity()?.Kill();
        }

        //protected virtual void Start() {
        //    OnDefeated.AddListener(BossInfoUI.Current.OnBossDefeated);
        //    OnDefeated.AddListener(BossSpawner.Current.OnBossDefeated.InvokeEvent);
        //}

        void OnDisable() {
            Stage = 0;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate() {
            _stage = Mathf.Clamp(_stage, 1, 4);
        }
#endif

        public virtual void OnStageUpdate(int stage) { }

        //switch(stage) {
        //    case 1: break;
        //    case 2: break;
        //    case 3: break;
        //}
        protected virtual void OnStage(int stage) { }

        public void Defeat() {
            GetEntity().Defeat();
        }

        public virtual GameObject[] GetHitSpots() { return null; }

        public virtual Vector3 GetStartPosition() { return Vector3.zero; }
        public virtual Quaternion GetStartRotation() { return Quaternion.identity; }

        protected virtual void OnNetworkStageState(int stage) { }

        public void SetStage(int stage) {
            if(Stage == stage) return;

            Stage = stage;

            if(gameObject.activeInHierarchy) {
                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_BossStageState);
                writer.Write(gameObject);
                writer.Write(stage);
                writer.FinishMessage();

                foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
                    connection.SendWriter(writer, Channels.DefaultReliable);
            }

            CameraSoundController.Current.PlaySound("Boss Stage Up", true);
            OnStage(stage);
        }

        public virtual void StageUp() {
            var entity = GetEntity();

            entity.StartStageUp(entity.Stage + 1);
        }

        static void NetworkStageState(NetworkMessage networkMessage) {
            var bossGameObject = networkMessage.reader.ReadGameObject();
            var stage = networkMessage.reader.ReadInt32();

            if(bossGameObject == null) return;

            if(bossGameObject.GetComponent<BossController>() is BossController bossController) {
                bossController.Stage = stage;
                bossController.OnNetworkStageState(stage);
            }
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_BossStageState, NetworkStageState);
        }
    }
}