//TODO: Player turret targets things on the client (when it shouldn't)
//TODO: healingActors and healingNetIds needs to be reworked to use entity ids

using System;
using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class DeployableTurret : NetworkBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill {

        [SerializeField]
        Transform _pivot;

        [SerializeField]
        LayerMask _raycastMask = -1;

        [SerializeField]
        Material _material;

        [SerializeField, Header("Healing Ring")]
        GameObject _healRing;

        [SerializeField]
        EventTimer _healTimer = new EventTimer(.2f);

        [SerializeField]
        bool _enableHealRing;

        [SyncVar(hook = "Sync_TargetId")]
        TargetId _targetId;

        [SerializeField]
        bool _enableMovement;

        [SerializeField]
        Vector3 _targetMovePosition;

        [SerializeField]
        float _targetMoveRange = 2;

        [Header("Healing Ring")]
        [SerializeField]
        float _healingRadius = 4.4f;

        [SerializeField]
        LayerMask _healLayer = -1;

        HashSet<uint> healingEntityIds = new HashSet<uint>();

        uint entityId;
        bool initialized;

        #region Properties
        Barrage BarrageExecutor { get; set; }

        BaseMovementController BaseMovementControllerComponent { get; set; }

        public bool EnableHealRing {
            get { return _enableHealRing; }
            set { _enableHealRing = value; }
        }

        public bool EnableMovement {
            get { return _enableMovement; }
            set { _enableMovement = value; }
        }

        TargetController TargetControllerComponent { get; set; }

        public Vector3 TargetMovePosition {
            get { return _targetMovePosition; }
            set { _targetMovePosition = value; }
        }

        Weapon WeaponExecutor { get; set; }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            BaseMovementControllerComponent = entity.BaseMovementControllerComponent;
            TargetControllerComponent = GetComponent<TargetController>();

            entityId = entity.Id;

            entity.UserComponent.OnSpawn.AddListener(OnSpawn);

            var renderers = new HashSet<Renderer>();

            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                if(renderer.sharedMaterial.name == "Turret") {
                    renderers.Add(renderer);

                    if(!_material) {
                        _material = new Material(renderer.sharedMaterial) {
                            name = "Turret Material"
                        };
                    }
                }

            foreach(var renderer in renderers)
                if(renderer.sharedMaterial.name == "Turret")
                    renderer.sharedMaterial = _material;

            _healRing.SetActive(false);
            EnableHealRing = false;

            if(!initialized)
                _healTimer.OnElapsed.AddListener(OnHeal);

            initialized = true;
        }

        [ClientRpc]
        public void Rpc_EnableHealRing(bool enable) {
            if(!NetworkServer.active)
                EnableHealRing = enable;
        }

        [ServerCallback]
        void FixedUpdate() {
            if(!initialized) return;

            if(_enableMovement) {
                float distance = Vector3.Distance(transform.position, _targetMovePosition);

                if(distance > _targetMoveRange) {
                    Vector3 direction = transform.position - _targetMovePosition;
                    Vector3 finalPosition = _targetMovePosition + (Vector3.ClampMagnitude(direction, 1) * (_targetMoveRange + .1f));

                    BaseMovementControllerComponent.MoveTo(finalPosition);
                }
            }
        }

        [ServerCallback]
        void LateUpdate() {
            if(!initialized) return;

            if(TargetControllerComponent.Target) {
                if(Physics.Linecast(transform.position, TargetControllerComponent.Target.position, out RaycastHit hit, _raycastMask, QueryTriggerInteraction.Ignore))
                    if(hit.collider.transform == TargetControllerComponent.Target) {
                        if(!WeaponExecutor.CurrentWeapon.IsShooting)
                            WeaponExecutor.ShootStart();
                    }
                    else {
                        if(WeaponExecutor.CurrentWeapon.IsShooting)
                            WeaponExecutor.ShootStop();
                    }
            }
            else {
                if(WeaponExecutor.CurrentWeapon.IsShooting)
                    WeaponExecutor.ShootStop();
            }
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            switch(element.CurrentExecutor) {
                case Weapon executor: {
                    WeaponExecutor = executor;

                    break;
                }
                case Barrage executor: {
                    if(NetworkServer.active)
                        BarrageExecutor = executor;

                    break;
                }
            }

            UpdateHealRingActive();
        }

        void OnDisable() {
            healingEntityIds.Clear();

            WeaponExecutor = null;
            BarrageExecutor = null;
        }

        void OnHeal() {
            bool healed = false;

            var colliders = Physics.OverlapSphere(transform.position, _healingRadius, _healLayer, QueryTriggerInteraction.Ignore);

            foreach(var collider in colliders) {
                collider.gameObject.DoHealthChange(1, this, false);
                //var gameObject = collider.attachedRigidbody ? collider.attachedRigidbody.gameObject : collider.gameObject;
                //var healthGroup = gameObject.GetComponent<HealthGroup>();
                //var healthComponent = healthGroup.MainHealthLayer.HealthComponent;

                //if(healthComponent.Value < healthComponent.MaxValue) {
                //    healed = true;
                //    healthComponent.Heal(new HealthEvent(this.gameObject, 1, "Turret Heal Ring"));
                //}
            }

            //foreach(var netId in healingNetIds) {
            //    var gameObject = NetworkServer.FindLocalObject(new NetworkInstanceId(netId));
            //    var healthGroup = gameObject.GetComponent<HealthGroup>();
            //    var healthComponent = healthGroup.MainHealthLayer.HealthComponent;

            //    if(healthComponent.Value < healthComponent.MaxValue) {
            //        healed = true;
            //        healthComponent.Heal(new HealthEvent(this.gameObject, 1, "Turret Heal Ring"));
            //    }
            //}

            //foreach(var actor in healingActors) {
            //    var healthComponent = actor.HealthGroupComponent.MainHealthLayer.HealthComponent;

            //    if(healthComponent.Value < healthComponent.MaxValue) {
            //        healed = true;
            //        healthComponent.Heal(new HealthEvent(gameObject, 1, "Turret Heal Ring"));
            //    }
            //}

            if(healed)
                EffectManager.Current.Effects.Play("Turret Heal", transform);
            //}
        }

        public void OnHealEnter(Collider collider) {
            if(collider.attachedRigidbody != null)
                if(GameManager.Current.GetActorEntity(collider.attachedRigidbody.gameObject, out ActorEntity entity))
                    healingEntityIds.Add(entity.Id);
        }

        public void OnHealExit(Collider collider) {
            if(collider.attachedRigidbody != null)
                if(GameManager.Current.GetActorEntity(collider.attachedRigidbody.gameObject, out ActorEntity entity))
                    healingEntityIds.Remove(entity.Id);
        }

        void OnSpawn() {
            _targetId = new TargetId();
            _healTimer.Start(true);
            _healTimer.CurrentTime = _healTimer.Length + 1;
        }

        public override void OnStartClient() {
            Sync_TargetId(_targetId);
        }

        void Update() {
            if(!initialized) return;

            UpdateHealRing();

            if(TargetControllerComponent.Target) {
                if(NetworkServer.active) {
                    var target = TargetControllerComponent.Target;
                    var targetRoot = target.root;
                    var isRoot = targetRoot == target;
                    var networkIdentity = targetRoot.GetComponent<NetworkIdentity>();

                    //TODO: Change the target id stuff to use EntityHitSpots id
                    if(_targetId.netId.Value != networkIdentity.netId.Value) { // Target hasn't changed
                        if(!isRoot) {
                            var healthGroup = target.GetComponentInParent<HealthGroup>();
                            var healthLayer = target.GetComponent<HealthLayer>();
                            (var tierIndex, var layerIndex) = healthGroup.GetHealthLayerIndex(healthLayer);

                            _targetId = new TargetId() {
                                isRoot = false,
                                netId = networkIdentity.netId,
                                healthTierIndex = tierIndex,
                                healthLayerIndex = layerIndex
                            };
                        }
                        else {
                            _targetId = new TargetId() {
                                isRoot = true,
                                netId = networkIdentity.netId
                            };
                        }
                    }
                    else { // Target has changed but root target is the same. Boss hit spot target possibly changed and needs to be updated.
                        if(!isRoot) {
                            var healthGroup = target.GetComponentInParent<HealthGroup>();
                            var healthLayer = target.GetComponent<HealthLayer>();

                            if(healthGroup != null && healthLayer != null) {
                                (var tierIndex, var layerIndex) = healthGroup.GetHealthLayerIndex(healthLayer);

                                _targetId = new TargetId() {
                                    isRoot = false,
                                    netId = networkIdentity.netId,
                                    healthTierIndex = tierIndex,
                                    healthLayerIndex = layerIndex
                                };
                            }
                        }
                    }
                }

                _pivot.LookAt(TargetControllerComponent.Target);
            }
            else {
                if(_targetId.netId.Value != 0)
                    _targetId = new TargetId();
            }

            if(BarrageExecutor != null)
                if(BarrageExecutor.CooldownTimer.HasElapsed)
                    BarrageExecutor.Invoke();
        }

        void UpdateHealRingActive() {
            if(_healRing.activeSelf) {
                if(!_enableHealRing) _healRing.SetActive(false);
            }
            else {
                if(_enableHealRing) _healRing.SetActive(true);
            }
        }

        void UpdateHealRing() {
            UpdateHealRingActive();

            if(NetworkServer.active)
                if(_enableHealRing)
                    _healTimer.Update();
            //if(_enableHealRing)
            //    if(healingActors.Count > 0 || healingNetIds.Count > 0) {
            //        _healTimer.Update();
            //    }
            //    else {
            //        if(_healTimer.NormalizedTime != 1)
            //            _healTimer.SetHigh();
            //    }
        }

        public void UpdateMaterial() {
            ShipSelectionUIManager.Current.CurrentPreset.SetMaterialValues(_material);
        }
        public void UpdateMaterial(NetworkInstanceId gamePlayerInfoNetId) {
            Rpc_UpdateMaterial(gamePlayerInfoNetId);

            //var playerInfoObject = ClientScene.FindLocalObject(gamePlayerInfoNetId);

            //if(!playerInfoObject) {
            //    Debug.LogError("Unable to set turret colors. GamePlayerInfo object not found.", gameObject);
            //    return;
            //}

            //var playerInfo = playerInfoObject.GetComponent<GamePlayerInfo>();

            //playerInfo.ColorPreset.SetMaterialValues(_material);
        }

        [ClientRpc]
        void Rpc_UpdateMaterial(NetworkInstanceId gamePlayerInfoNetId) {
            var playerInfoObject = ClientScene.FindLocalObject(gamePlayerInfoNetId);

            if(!playerInfoObject) {
                Debug.LogError("Unable to set turret colors. GamePlayerInfo object not found.", gameObject);
                return;
            }

            var playerInfo = playerInfoObject.GetComponent<GamePlayerInfo>();

            playerInfo.ColorPreset.SetMaterialValues(_material);
        }

        void Sync_TargetId(TargetId targetId) {
            if(!NetworkServer.active) {
                var targetObject = ClientScene.FindLocalObject(targetId.netId)?.transform;

                if(!targetObject) return;

                if(targetId.isRoot)
                    TargetControllerComponent.Target = targetObject;
                else {
                    var healthGroup = targetObject.GetComponent<HealthGroup>();
                    var healthLayer = healthGroup.GetHealthLayer(targetId.healthTierIndex, targetId.healthLayerIndex);

                    TargetControllerComponent.Target = healthLayer.transform;
                }

            }
        }

        struct TargetId {
            public NetworkInstanceId netId;
            public bool isRoot;
            public int healthTierIndex;
            public int healthLayerIndex;
        }
    }
}