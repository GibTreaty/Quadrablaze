using System;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class MutantAtomController : MonoBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill, ITelegraphHandler {

        [SerializeField]
        Collider _mainCollider;

        [SerializeField]
        GameObject _telegraphObject;

        Action onShoot;

        bool initialized;

        #region Properties
        public BaseMovementController BaseMovementComponent { get; private set; }
        public EnemyInput EnemyInputComponent { get; private set; }
        public PoolUser UserComponent { get; private set; }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            _telegraphObject.SetActive(false);

            if(initialized) return;

            BaseMovementComponent = entity.BaseMovementControllerComponent;
            EnemyInputComponent = GetComponent<EnemyInput>();
            UserComponent = entity.UserComponent;

            //entity.HealthGroupComponent.MainHealthLayer.HealthComponent.OnDeath.AddListener(s => {
            //    entity.UserComponent.SpawnHere("Explosions");

            //    if(NetworkServer.active) entity.UserComponent.Despawn();
            //});

            EnemyProxy.Targets.Add(_mainCollider.transform);

            EnemyInputComponent.onTelegraphEnd.AddListener(EndTelegraph);
            EnemyInputComponent.onTelegraphStart.AddListener(StartTelegraph);

            initialized = true;
        }

        void EndTelegraph() {
            onShoot?.Invoke();
            SetTelegraphState(false);
            TelegraphStateHandler.SendTelegraphState(gameObject, false);
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Weapon executor)
                onShoot = executor.ShootStart;
        }

        void OnDisable() {
            EnemyProxy.Targets.Remove(_mainCollider.transform);
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            _telegraphObject.SetActive(enable);
        }

        public void Shoot() {
            EnemyInputComponent.DelayTelegraphTimer();
        }

        void StartTelegraph() {
            SetTelegraphState(true);
            TelegraphStateHandler.SendTelegraphState(gameObject, true);
        }
    }
}