using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Effects;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

#pragma warning disable CS0618

namespace Quadrablaze {
    //[AddComponentMenu("Quadrablaze/Actor"), Obsolete("Use ActorEntity instead")]
    public class Actor : NetworkBehaviour, IPoolInstantiate {

        public static ActorEvent OnActorSpawn = new ActorEvent();
        public static ActorEvent OnActorDeath = new ActorEvent();
        public static ActorEvent OnActorPermadeath = new ActorEvent();

        [SerializeField]
        ActorTypes _actorType;

        [SerializeField]
        InitializationType _initialization = InitializationType.Pooled;

        [SerializeField]
        Transform _alternatePivot;

        [SerializeField]
        float _size = 1;

        [SerializeField]
        Timer _movementInterruptTimer;

        [SerializeField]
        float _invincibilityLength = 1;

        [SerializeField]
        float _damageMultiplier = 1;

        [SerializeField]
        UnityEvent _onDeath;

        [SerializeField]
        UnityEvent _onPermadeath;

        [SerializeField]
        UnityEvent _onSpawn;

        [SerializeField]
        UnityEvent _onDespawn;

        [SerializeField]
        UnityEvent _onNetworkSpawnServer;

        [SerializeField]
        UnityEvent _onRespawn;

        [SerializeField]
        SkillLayoutElementEvent _onAssignedSkill;

        public event Action OnShootStart;
        public event Action OnShootStop;

        protected bool _isInitialized;

        float _invincibilityTime;

        #region Properties
        public virtual ActorTypes ActorType {
            get { return _actorType; }
            set { _actorType = value; }
        }

        public Transform AlternatePivot {
            get { return _alternatePivot; }
            set { _alternatePivot = value; }
        }

        public BaseMovementController BaseMovementControllerComponent { get; protected set; }

        public ScriptableUpgradeSet CurrentUpgradeSet { get; set; }

        public float DamageMultiplier {
            get { return _damageMultiplier; }
            set { _damageMultiplier = value; }
        }

        public bool HasRespawned { get; internal set; }

        public HealthGroup HealthGroupComponent { get; protected set; }

        public InitializationType Initialization {
            get { return _initialization; }
            set { _initialization = value; }
        }

        public float InvincibilityLength {
            get { return _invincibilityLength; }
            set { _invincibilityLength = value; }
        }

        public bool IsAlive {
            get { return HealthGroupComponent && HealthGroupComponent.MainHealthLayer.HealthStatusAlive(); }
        }

        public bool IsEnemy {
            get { return ActorType != ActorTypes.Player; }
        }

        public bool IsInitialized {
            get { return _isInitialized; }
            protected set { _isInitialized = value; }
        }

        public bool IsInvincible {
            get { return Time.time < _invincibilityTime; }
        }

        public Timer MovementInterruptTimer {
            get { return _movementInterruptTimer; }
        }

        public NetworkIdentity NetworkIdentityComponent { get; private set; }

        public NetworkSkillController NetworkSkillControllerComponent { get; private set; }

        public SkillLayoutElementEvent OnAssignedSkill {
            get { return _onAssignedSkill; }
            set { _onAssignedSkill = value; }
        }

        public UnityEvent OnDeath {
            get { return _onDeath; }
            private set { _onDeath = value; }
        }

        public UnityEvent OnDespawn {
            get { return _onDespawn; }
            private set { _onDespawn = value; }
        }

        public UnityEvent OnPermadeath {
            get { return _onPermadeath; }
            private set { _onPermadeath = value; }
        }

        public UnityEvent OnRespawn {
            get { return _onRespawn; }
            private set { _onRespawn = value; }
        }

        public UnityEvent OnSpawn {
            get { return _onSpawn; }
            private set { _onSpawn = value; }
        }

        public Rigidbody RigidbodyComponent { get; protected set; }

        public float Size {
            get { return _size; }
            set { _size = value; }
        }

        public PoolUser UserComponent { get; protected set; }
        #endregion

        //void ActorSpawn() {
        //    if(name.Contains("Reformer"))
        //        Debug.Log("ActorSpawn:" + name + ":" + GetHashCode());

        //    ResetUpgradeSet();
        //}

        //void ActorSpawnLate() {
        //    if(ActorType != ActorTypes.Player)
        //        ResetUpgradeSet();
        //}

        protected virtual void Awake() {
            if(Initialization == InitializationType.Awake)
                Initialize();
        }

        public bool CheckCanRespawn() {
            return CurrentUpgradeSet != null && CurrentUpgradeSet.Lives > 0;
        }

        public void Damage(GameObject effector, float amount, string description = "") {
            //if(!IsInvincible)
            //    HealthGroupComponent?.MainHealthLayer?.HealthComponent?.Damage(new HealthEvent(effector, amount, description));

            HealthGroupComponent?.MainHealthLayer?.Damage(new HealthEvent(effector, amount, description));
        }

        public void DestroyUpgradeData() {
            //if(CurrentUpgradeSet && CurrentUpgradeSet.CurrentSkillLayout) Destroy(CurrentUpgradeSet.CurrentSkillLayout);
            //if(CurrentUpgradeSet) Destroy(CurrentUpgradeSet);
        }

        public void SetShootingStatus(bool status) {
            if(status)
                OnShootStart?.Invoke();
            else
                OnShootStop?.Invoke();
        }


        public void EndInvincibility() {
            _invincibilityTime = Time.time;
        }

        void FixedUpdate() {
            //if(CurrentUpgradeSet != null &&
            //    CurrentUpgradeSet.CurrentSkillLayout != null &&
            //    CurrentUpgradeSet.CurrentSkillLayout.SkillLookupTable != null &&
            //    CurrentUpgradeSet.SkillExecutors != null
            //)
            //    foreach(var skillExecutor in CurrentUpgradeSet.SkillExecutors)
            //        if(skillExecutor is ISkillFixedUpdate)
            //            (skillExecutor as ISkillFixedUpdate).SkillFixedUpdate(this);
        }

        public void GameUpdate() {
            //MovementInterruptTimer.Update();

            //if(CurrentUpgradeSet != null &&
            //     CurrentUpgradeSet.CurrentSkillLayout != null &&
            //     CurrentUpgradeSet.CurrentSkillLayout.SkillLookupTable != null &&
            //     CurrentUpgradeSet.SkillExecutors != null
            // )
            //    foreach(var skillExecutor in CurrentUpgradeSet.SkillExecutors)
            //        if(skillExecutor is ISkillUpdate)
            //            (skillExecutor as ISkillUpdate).SkillUpdate(this);
        }

        public void GiveInvincibility() {
            GiveInvincibility(_invincibilityLength);
        }
        public void GiveInvincibility(float time) {
            _invincibilityTime = Time.time + time;
        }

        public virtual void Initialize() {
            IsInitialized = true;

            if(!UserComponent) UserComponent = GetComponent<PoolUser>();

            BaseMovementControllerComponent = GetComponent<BaseMovementController>();
            HealthGroupComponent = GetComponent<HealthGroup>();
            NetworkIdentityComponent = GetComponent<NetworkIdentity>();
            NetworkSkillControllerComponent = GetComponent<NetworkSkillController>();

            if(OnAssignedSkill == null) OnAssignedSkill = new SkillLayoutElementEvent();
            if(OnDeath == null) OnDeath = new UnityEvent();
            if(OnDespawn == null) OnDespawn = new UnityEvent();
            if(OnPermadeath == null) OnPermadeath = new UnityEvent();
            if(OnRespawn == null) OnRespawn = new UnityEvent();
            if(OnSpawn == null) OnSpawn = new UnityEvent();

            OnDeath.AddListener(() => OnActorDeath.Invoke(this));
            OnSpawn.AddListener(() => OnActorSpawn.Invoke(this));
            OnPermadeath.AddListener(() => OnActorPermadeath.Invoke(this));

            if(GetComponentsInChildren<Rigidbody>().Length == 1)
                RigidbodyComponent = GetComponentInChildren<Rigidbody>(true);

            if(HealthGroupComponent && !HealthGroupComponent.SetupOnAwake)
                HealthGroupComponent.BuildLayerArray();

            SetupHealth();

            //TODO: Move all this shiz into the Entity classes

            switch(ActorType) {
                //case ActorTypes.Boss:
                //    SetupBoss();
                //    break;
                case ActorTypes.SmallEnemy:
                case ActorTypes.MediumEnemy:
                    SetupSmallEnemy();
                    break;
            }

            SetupRigidbody();
            SetupDamageMultiplier();
            SetupActorEvents();
            SetupDeathEvent();
        }

        //public void InitializeSkillLayout() {
        //    if(CurrentUpgradeSet != null && CurrentUpgradeSet.CurrentSkillLayout != null)
        //        foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.SkillElements)
        //            if(element.BaseLevel > 0)
        //                CurrentUpgradeSet.CurrentSkillLayout.SetElementLevel(element, element.BaseLevel);
        //}

        [ContextMenu("Kill")]
        [Server]
        public virtual void Kill() {
            return;
            if(!NetworkServer.active) return;

            UserComponent.Despawn();

            var canRespawn = CheckCanRespawn();

            OnDeath.Invoke();

            if(canRespawn) {
                CurrentUpgradeSet.Lives--;

                var upgradeSet = CurrentUpgradeSet;

                //if(this is PlayerActor) {
                //    CurrentUpgradeSet = null;

                //    var playerInfo = (this as PlayerActor).PlayerInfo;

                //    Respawner.Create(playerInfo, UserComponent.InPool, UserComponent.PrefabIndex, 1, transform.position);
                //}
                //else {
                //Destroy(CurrentUpgradeSet.CurrentSkillLayout);

                CurrentUpgradeSet = null;

                Respawner.Create(null, UserComponent.InPool, UserComponent.PrefabIndex, 1, transform.position);
                //}
            }
            else {
                OnPermadeath.Invoke();
                HasRespawned = false;
            }
        }

        //IEnumerator NetworkSendEquipRoutineAlternate(WeaponUpgrade weaponUpgrade) {
        //    yield return new WaitForEndOfFrame();
        //    yield return new WaitForEndOfFrame();
        //    NetworkUpgradeManager.Current.Server_SetWeaponLevel(gameObject, weaponUpgrade.WeaponIndex, weaponUpgrade.CurrentLevel);
        //}

        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = new Color(.1f, 1, .1f);
            Gizmos.DrawWireSphere(AlternatePivot ? AlternatePivot.position : transform.position, Size);
        }

        protected virtual void OnDestroy() {
            DestroyUpgradeData();
        }

        //public override void OnNetworkDestroy() {
        //    base.OnNetworkDestroy();

        //    foreach(var skill in GetSkills<SkillBase>())
        //        skill.SetUpgrade(skill.BaseLevel);
        //}

        //public override void OnStartClient() {
        //    if(!NetworkServer.active)
        //        if(ActorType != ActorTypes.Player)
        //            InitializeUpgradeSet();
        //}

        public override void OnStartServer() {
            GiveInvincibility();
        }

        //void OnDisable() {
        //    if(ActorType != ActorTypes.Player)
        //        Destroy(CurrentUpgradeSet);
        //}

        public virtual void PoolInstantiate(PoolUser user) {
            if(Initialization == InitializationType.Pooled) {
                UserComponent = user;

                Initialize();
            }
        }

        IEnumerator ResetHealth(Health[] healthComponents) {
            //yield break;

            yield return new WaitForEndOfFrame();

            foreach(var health in healthComponents)
                health.Reset();
        }

        protected void SetupActorEvents() {
            if(UserComponent) {
                UserComponent.OnDespawn.AddListener(OnDespawn.Invoke);
                UserComponent.OnSpawn.AddListener(OnSpawn.Invoke);
            }
        }

        protected void SetupBoss() {
            //var healthGroup = Get
            if(HealthGroupComponent) {
                //for(int tier = 0; tier < HealthGroupComponent.TierCount; tier++) {
                //    var layers = HealthGroupComponent.GetLayers(tier);

                //    for(int i = 0; i < layers.Length; i++) {
                //        var layer = layers[i];

                //        layer.HealthComponent.OnDeath.AddListener(s => UserComponent.SpawnHere("Explosions"));
                //    }
                //}
                foreach(var layer in GetComponentsInChildren<HealthLayer>(true))
                    layer.HealthComponent.OnDeath.AddListener(s => UserComponent.SpawnHere("Explosions"));
            }

            //var bossController = GetComponent<Boss.BossController>();

            //if(bossController) {
            //    bossController.OnDefeated.AddListener(() => UserComponent.SpawnHere("Explosions"));
            //}
            //else {
            //    Health mainHealth = GetComponent<Health>();

            //    if(mainHealth && UserComponent) 
            //        mainHealth.OnDeath.AddListener(s => UserComponent.SpawnHere("Explosions"));
            //}
            //bossController.OnDefeated.AddListener(() => { bossController });
        }

        protected void SetupDamageMultiplier() {
            Array.ForEach(GetComponentsInChildren<IDamageMultiplier>(true), value => value.DamageMultiplier = DamageMultiplier);
        }

        protected void SetupDeathEvent() {
            //OnDeath.AddListener(DoDeath);
        }

        protected void SetupHealth() {
            //if(HitManager.Current)
            //    HealthManager.Current.AddActorHealth(this);

            Health[] healthComponents = GetComponentsInChildren<Health>(true);

            //foreach(Health health in healthComponents) {
            //    if(HitManager.Current)
            //        health.OnDamaged.AddListener(HitManager.Current.FilterHit);

            //    //if(UserComponent) UserComponent.OnSpawn.AddListener(health.Reset);
            //}

            if(UserComponent)
                UserComponent.OnSpawn.AddListener(() => StartCoroutine(ResetHealth(healthComponents)));

            //foreach(Health health in healthComponents) {
            //    if(health.name == "Shield")
            //        health.OnDamaged.AddListener(healthEvent => {
            //            Shield shield = SkillControllerComponent.GetSkill<Shield>("Shield");

            //            if(shield && shield.Controller.ShieldActive && shield.Controller.ShieldHealth.NormalizedValue > 0) {
            //                healthEvent.description += HealthHelper.HasShield;
            //                HitMessengerComponent.FilterHit(healthEvent);
            //            }
            //        });
            //    else
            //        health.OnDamaged.AddListener(HitMessengerComponent.FilterHit);

            //    if(User) User.onUnpooled.AddListener(health.Reset);
            //}
        }

        protected void SetupRigidbody() {
            if(RigidbodyComponent && UserComponent)
                UserComponent.OnSpawn.AddListener(RigidbodyComponent.Sleep);
        }

        protected void SetupSmallEnemy() {
            Health mainHealth = GetComponent<Health>();

            if(mainHealth && UserComponent) {
                mainHealth.OnDeath.AddListener(s => {
                    if(NetworkServer.active) {
                        UserComponent.SpawnHere("Explosions");
                        Kill();
                    }
                });
            }
        }

        protected virtual void Start() {
            if(Initialization == InitializationType.Start)
                Initialize();
        }

        public void StartMovementInterrupt(float time) {
            if(MovementInterruptTimer.CurrentTime < time)
                MovementInterruptTimer.Start(time);
        }

        //void Update() {
        //    MovementInterruptTimer.Update();

        //    if(CurrentUpgradeSet != null &&
        //         CurrentUpgradeSet.CurrentSkillLayout != null &&
        //         CurrentUpgradeSet.CurrentSkillLayout.SkillLookupTable != null &&
        //         CurrentUpgradeSet.SkillExecutors != null
        //     )
        //        foreach(var skillExecutor in CurrentUpgradeSet.SkillExecutors)
        //            if(skillExecutor is ISkillUpdate)
        //                (skillExecutor as ISkillUpdate).SkillUpdate(this);
        //}
    }

    [Serializable]
    public class ActorEvent : UnityEvent<Actor> { }

    public enum ActorTypes {
        Player = 1,
        SmallEnemy = 2,
        Boss = 3,
        DeployableTurret = 4,
        MediumEnemy = 5
    }

    public enum InitializationType {
        None = 0,
        Awake = 1,
        Start = 2,
        Pooled = 3
    }
}