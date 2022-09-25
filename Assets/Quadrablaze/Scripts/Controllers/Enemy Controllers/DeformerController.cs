using System;
using System.Collections;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

// TODO: Deformer - Shield drain needs to be synced over the network
// TODO: Deformer - Use ActorEntity as the target

namespace Quadrablaze {
    public class DeformerController : NetworkBehaviour, IActorEntityObjectInitialize, IStatCausedDamage {

        public const float DistanceFromPlayer = 16;
        public const float DistanceFromPlayerSqr = DistanceFromPlayer * DistanceFromPlayer;

        [SerializeField]
        Transform _weaponPivot;

        [SerializeField]
        EventTimer _fireTimer = new EventTimer(1);

        [SerializeField]
        EventTimer _rechargeTimer = new EventTimer(5);

        DeformerState _currentState;
        Action onShoot;
        bool initialized;
        Transform targetTransform;

        #region Properties
        EnemyInput EnemyInputComponent { get; set; }

        public EventTimer FireTimer => _fireTimer;

        public EventTimer RechargeTimer => _rechargeTimer;

        public ShieldDrainer ShieldDrainerComponent { get; private set; }

        public Transform WeaponPivot {
            get { return _weaponPivot; }
            set { _weaponPivot = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            if(!initialized) {
                ShieldDrainerComponent = GetComponentInChildren<ShieldDrainer>();
                EnemyInputComponent = GetComponent<EnemyInput>();
                GetComponent<TargetController>().onTargetChanged.AddListener(OnTargetChanged);
                FireTimer.OnElapsed.AddListener(Fire);
                RechargeTimer.OnElapsed.AddListener(() => ShieldDrainerComponent.Active = true);

                initialized = true;
            }

            FireTimer.Reset();
            RechargeTimer.Reset();
            ShieldDrainerComponent.Active = true;
            ShieldDrainerComponent.TargetShieldWasFull = false;
        }

        void Fire() {
            if(_currentState == DeformerState.PoweredShot) {
                onShoot?.Invoke();

                if(GameManager.Current.GetActorEntity(gameObject, out var actorEntity))
                    if(actorEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Weapon>() is var weapon)
                        StartCoroutine(FireRoutine(weapon));

                _currentState = DeformerState.Draining;

                ShieldDrainerComponent.TargetShieldWasFull = false;
                FireTimer.Reset(true);
                RechargeTimer.Start(true);
            }
        }

        IEnumerator FireRoutine(Weapon weapon) {
            weapon.ShootStart();
            yield return null;
            weapon.ShootStop();
        }

        public void OnCausedDamage(StatEvent statEvent) {
            if(_currentState == DeformerState.Draining)
                if(statEvent.AffectedObject is Shield shield)
                    if(statEvent.AffectedStat.Value == 0) {
                        ShieldDrainerComponent.Active = false;
                        _currentState = DeformerState.PoweredShot;
                        FireTimer.Start(true);
                    }
        }

        void OnDisable() {
            targetTransform = null;
            FireTimer.Reset();
        }

        void OnTargetChanged(Transform target) {
            ShieldDrainerComponent.TargetShieldWasFull = false;

            if(target != null) {
                foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                    if(!player.isHost)
                        TargetRpc_SetTarget(player.serverToClientConnection, target.GetComponent<NetworkIdentity>().netId);
            }
            else {
                foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                    if(!player.isHost)
                        TargetRpc_SetTarget(player.serverToClientConnection, new NetworkInstanceId(0));
            }
        }

        //TODO: Make this use an ActorEntity as a target
        void RotateShieldDrainAt(Transform target) {
            if(GameManager.Current.GetActorEntity(target.gameObject, out ActorEntity actorEntity))
                if(actorEntity.LivingState == ActorState.Alive) {
                    Vector3 direction = target.position - transform.position;

                    if(direction.sqrMagnitude <= DistanceFromPlayerSqr)
                        WeaponPivot.rotation = Quaternion.LookRotation(direction);
                }
        }

        [TargetRpc]
        void TargetRpc_SetTarget(NetworkConnection connection, NetworkInstanceId netId) {
            targetTransform = netId.Value == 0 ? null : (ClientScene.FindLocalObject(netId)?.transform);
        }

        void Update() {
            if(NetworkServer.active) {
                if(EnemyInputComponent?.Target)
                    RotateShieldDrainAt(EnemyInputComponent.Target);

                if(_currentState == DeformerState.PoweredShot)
                    FireTimer.Update();

                RechargeTimer.Update();
            }
            else {
                if(targetTransform)
                    RotateShieldDrainAt(targetTransform);
            }
        }

        public enum DeformerState {
            Draining,
            PoweredShot
        }
    }
}