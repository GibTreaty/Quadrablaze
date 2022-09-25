using System;
using System.Collections;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    [RequireComponent(typeof(EnemyInput))]
    public class ShotformerController : NetworkBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill, ITelegraphHandler {

        const float DistanceFromPlayer = 16;
        const float DistanceFromPlayerSqr = DistanceFromPlayer * DistanceFromPlayer;

        [SerializeField]
        Transform _weaponPivot;

        [SerializeField]
        float _shootDelay = .5f;

        [SerializeField]
        float _shootTimer;

        [SerializeField]
        GameObject _telegraphObject;

        bool initialized;
        Action onShoot;

        #region Properties
        BaseMovementController BaseMovementControllerComponent { get; set; }

        EnemyInput EnemyInputComponent { get; set; }

        public float ShootDelay {
            get { return _shootDelay; }
            set { _shootDelay = value; }
        }

        public float ShootTimer {
            get { return _shootTimer; }
            set { _shootTimer = value; }
        }

        public Transform WeaponPivot {
            get { return _weaponPivot; }
            set { _weaponPivot = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            if(!initialized) {
                BaseMovementControllerComponent = GetComponent<BaseMovementController>();
                EnemyInputComponent = GetComponent<EnemyInput>();

                EnemyInputComponent.onTelegraphEnd.AddListener(EndTelegraph);
                EnemyInputComponent.onTelegraphStart.AddListener(StartTelegraph);
            }

            _telegraphObject.SetActive(false);
            Reset();

            initialized = true;
        }

        [ServerCallback]
        void Update() {
            if(!initialized) return;

            UpdateShootTimer();

            if(EnemyInputComponent.Target) {
                if(GameManager.Current.GetActorEntity(EnemyInputComponent.Target.gameObject, out ActorEntity targetEntity))
                    if(targetEntity.LivingState == ActorState.Alive) {
                        if(EnemyInputComponent.TelegraphTimer == 0) {
                            Vector3 direction = EnemyInputComponent.Target.position - transform.position;
                            float sqrMagnitude = direction.sqrMagnitude;

                            if(sqrMagnitude > .01f && sqrMagnitude <= DistanceFromPlayerSqr) {
                                WeaponPivot.rotation = Quaternion.LookRotation(direction);
                                BaseMovementControllerComponent.Move(Quaternion.Euler(0, 90, 0) * direction.normalized);

                                if(ShootTimer == 0)
                                    Shoot();
                            }
                        }
                    }
                    else
                        EnemyInputComponent.Target = null;

                //var targetActor = EnemyInputComponent.Target.GetComponent<Actor>();

                //if(targetActor && targetActor.gameObject.activeSelf && targetActor.IsAlive) {
                //    if(EnemyInputComponent.TelegraphTimer == 0) {
                //        Vector3 direction = EnemyInputComponent.Target.position - transform.position;
                //        float sqrMagnitude = direction.sqrMagnitude;

                //        if(sqrMagnitude > .01f && sqrMagnitude <= DistanceFromPlayerSqr) {
                //            WeaponPivot.rotation = Quaternion.LookRotation(direction);
                //            BaseMovementControllerComponent.Move(Quaternion.Euler(0, 90, 0) * direction.normalized);

                //            if(ShootTimer == 0)
                //                Shoot();
                //        }
                //    }
                //}
                //else {
                //    EnemyInputComponent.Target = null;
                //}
            }
        }

        public void DelayShootTimer() {
            ShootTimer = ShootDelay;
        }

        void EndTelegraph() {
            onShoot?.Invoke();
            DelayShootTimer();
            SetTelegraphState(false);
            TelegraphStateHandler.SendTelegraphState(gameObject, false);
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Weapon executor)
                onShoot = executor.ShootStart;
        }

        public void ResetShootTimer() {
            ShootTimer = 0;
        }

        public void Reset() {
            ResetShootTimer();
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            _telegraphObject.SetActive(enable);
        }

        public void Shoot() {
            EnemyInputComponent.DelayTelegraphTimer();
            //onShoot?.Invoke();
            //DelayShootTimer();
        }

        void StartTelegraph() {
            SetTelegraphState(true);
            TelegraphStateHandler.SendTelegraphState(gameObject, true);
        }

        void UpdateShootTimer() {
            if(ShootTimer > 0)
                ShootTimer = Mathf.Max(ShootTimer - Time.deltaTime, 0);
        }
    }
}