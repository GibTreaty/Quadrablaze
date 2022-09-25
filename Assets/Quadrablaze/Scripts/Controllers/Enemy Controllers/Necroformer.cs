using System;
using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    [RequireComponent(typeof(EnemyInput))]
    public class Necroformer : NetworkBehaviour, IActorEntityObjectInitialize {
        [SerializeField]
        int _defaultRespawnCount = 3;

        [SerializeField]
        int _currentRespawnCount; // TODO Do something with _currentRespawnCount

        [SerializeField]
        EventTimer _respawnTime = new EventTimer(1) { AutoDisable = true, AutoReset = true };

        [SerializeField]
        RespawnData respawnData;

        #region Properties
        public int CurrentRespawnCount {
            get { return _currentRespawnCount; }
            set { _currentRespawnCount = value; }
        }

        public int DefaultRespawnCount {
            get { return _defaultRespawnCount; }
            set { _defaultRespawnCount = value; }
        }

        public EventTimer RespawnTime {
            get { return _respawnTime; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            ActorEntity.OnGlobalPermadeath += OnEnemyDeath;
            RespawnTime.OnElapsed.AddListener(() => Respawn(respawnData));
            CurrentRespawnCount = DefaultRespawnCount;
        }

        void OnDisable() {
            ActorEntity.OnGlobalPermadeath -= OnEnemyDeath;
        }

        void OnEnemyDeath(ActorEntity entity) {
            Debug.Log($"OnEnemyDeath: {entity.Name}");

            if(!NetworkServer.active) return;
            if(!gameObject.activeInHierarchy) return;

            if(entity is MinionEntity minionEntity) {
                //if(entity.CurrentGameObject.CompareTag("Unrespawnable")) return;
                if(RespawnTime.Active) return;
                if(entity.CurrentGameObject.GetComponent<Necroformer>()) return;

                respawnData = new RespawnData(
                    entity,
                    entity.UserComponent.InPool,
                    entity.UserComponent.PrefabIndex,
                    entity.CurrentTransform.position,
                    entity.CurrentTransform.rotation
                );

                RespawnTime.Start(true);
                entity.PreventDeath = true;
                Debug.Log($"Prevent enemy death: {entity.Name}: {entity.Id}: {entity.PreventDeath}");
            }
        }

        void Respawn(RespawnData data) {
            //ActorEntity.DoRespawn(data.actorEntity, data.position, data.rotation);
        }

        void Update() {
            if(!NetworkServer.active) return;

            RespawnTime.Update();
        }

        [Serializable]
        public struct RespawnData {
            public ActorEntity actorEntity; // TODO: [Necroformer] Use Entity ID here instead
            public PoolManager poolManager;
            public int prefabIndex;
            public Vector3 position;
            public Quaternion rotation;

            public RespawnData(ActorEntity actorEntity, PoolManager poolManager, int prefabIndex, Vector3 position, Quaternion rotation) {
                this.actorEntity = actorEntity;
                this.poolManager = poolManager;
                this.prefabIndex = prefabIndex;
                this.position = position;
                this.rotation = rotation;
            }
        }
    }
}