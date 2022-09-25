using System;
using System.Collections.Generic;
using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using StatSystem;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze.Entities {
    public class ActorEntity : Entity, IEntityUpdate {

        public static readonly ProxyListener<ProxyAction> Proxy = new ProxyListener<ProxyAction>();

        const float TimeToRespawn = 1;

        public static event ActorEvent OnGlobalDeath;
        public static event ActorEvent OnGlobalPermadeath;
        public static event ActorEvent OnGlobalRespawn;
        public static event ActorEvent OnGlobalSpawn;

        public event Action OnDeath;
        public event Action OnPermadeath;

        public event Action OnRespawn;  //TODO: Implement this
        public event Action OnSpawn;    //TODO: Implement this

        public event Action OnShootStart;
        public event Action OnShootStop;
        public event Action<SkillLayoutElement> OnAssignedSkill; //TODO: Unassign components from event after death

        Vector3 lastPosition;
        Quaternion lastRotation;

        public BaseMovementController BaseMovementControllerComponent { get; set; }
        public UpgradeSet CurrentUpgradeSet { get; set; }
        public CappedStat[] HealthSlots { get; set; }
        public LayerMask HitLayer { get; set; }
        public string[] HitTags { get; set; }
        public float InvincibilityLength { get; set; }
        public float InvincibilityTime { get; set; }
        public float InvincibilityTimeRemaining {
            get { return Math.Max(InvincibilityTime - Time.time, 0); }
        }
        public bool IsInvincible {
            get { return Time.time < InvincibilityTime; }
        }
        public ActorState LivingState { get; set; }
        public Timer MovementInterruptTimer { get; set; }
        public NetworkIdentity NetworkIdentityComponent { get; set; }
        public string Name { get; set; }
        public PoolManager OriginalPool { get; set; }
        public int OriginalPoolPrefabIndex { get; set; }
        public ScriptableActorEntity OriginalScriptableObject { get; set; }
        public bool PreventDeath { get; set; } /// <summary>Prevents this entity from dying once</summary>
        public List<RotateTransform> RotateTransforms { get; set; }
        public Vector3 ShootDirection { get; set; }
        public float Size { get; set; }
        public PoolUser UserComponent { get; set; }

        float respawnTimer = 0;

        public ActorEntity(GameObject gameObject, string name, uint id, UpgradeSet upgradeSet, float size) : base(id, gameObject) {
            CurrentUpgradeSet = upgradeSet;
            Name = name;
            Size = size;
            LivingState = ActorState.Alive;

            Listener.OnListenEvent += Proxy.RaiseEvent;
            Listener.RaiseEvent(EntityActions.Created, this.ToArgs());
        }

        public void ChangedHealth(StatEvent statEvent) {
            var healthMessage = statEvent.GetMessage<StatChangeValueMessage>();
            var statEventArgs = new StatEventArgs(statEvent);

            Listener.RaiseEvent(HealthActions.ChangedStat, statEventArgs);

            // TODO: FilterEvent
            //HitManager.Current.FilterEvent(statEvent);
            Debug.Log("[Health] Damaged");

            if(healthMessage.AmountChanged < 0) {
                Debug.Log("[Health] Damaged");
                HitManager.Current.FilterEvent(statEvent);
                OnDamaged(statEvent);
                Listener.RaiseEvent(HealthActions.StatDamaged, statEventArgs);

                if(statEvent.SourceObject is ActorEntity sourceEntity)
                    sourceEntity.OnCausedDamage(statEvent);
            }
            else {
                Debug.Log("[Health] Healed");
                OnHealed(statEvent);
                Listener.RaiseEvent(HealthActions.StatHealed, statEventArgs);

                if(statEvent.SourceObject is ActorEntity sourceEntity)
                    sourceEntity.OnCausedHeal(statEvent);
            }

            if(statEvent.AffectedStat.Value == 0)
                Listener.RaiseEvent(HealthActions.StatDeath, statEventArgs);
            else if(statEvent.AffectedStat.Value == (statEvent.AffectedStat as CappedStat).MaxValue)
                Listener.RaiseEvent(HealthActions.StatRestored, statEventArgs);
        }

        public override void DestroyEntity() {
            Debug.Log($"DestroyEntity: {Name}");
            //base.DestroyEntity();
            InvokeDestroyEvents();

            if(CurrentUpgradeSet != null)
                foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    if(element.CurrentExecutor is IExecutorActorDisable disable)
                        disable.OnActorDisabled();

            Listener.RaiseEvent(EntityActions.Destroyed, this.ToArgs());
            Listener.OnListenEvent -= Proxy.RaiseEvent;
        }

        public virtual void EntityUpdate() {
            UpdateLimbo();
            MovementInterruptTimer.Update();

            if(CurrentUpgradeSet != null)
                foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.Elements) {
                    if(element.CurrentExecutor is ISkillUpdate update)
                        update.SkillUpdate();

                    if(element.CurrentExecutor is ISkillFixedUpdate fixedUpdate)
                        fixedUpdate.SkillFixedUpdate();
                }
        }

        protected virtual void GameObjectWasCleared(GameObject previousGameObject) { }
        protected virtual void GameObjectWasSet(GameObject gameObject) { }

        public void GiveInvincibility() {
            GiveInvincibility(InvincibilityLength);
        }
        public void GiveInvincibility(float time) {
            InvincibilityTime = Time.time + time;
        }
        public void GiveInvincibilityMinimum(float time) {
            if(InvincibilityTime - Time.time < time)
                InvincibilityTime = Time.time + time;
        }

        public void InitializeSkillLayout() {
            if(CurrentUpgradeSet != null && CurrentUpgradeSet.CurrentSkillLayout != null)
                foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    if(element.OriginalLayoutElement.BaseLevel > 0)
                        CurrentUpgradeSet.CurrentSkillLayout.SetElementLevel(element, element.OriginalLayoutElement.BaseLevel);

            SkillLayoutInitialized();
        }

        public void Kill() {
            switch(LivingState) {
                case ActorState.Alive:
                    if(CurrentUpgradeSet != null && CurrentUpgradeSet.Lives > 0) {
                        CurrentUpgradeSet.Lives--;
                        respawnTimer = TimeToRespawn;
                        LivingState = ActorState.Limbo;

                        if(CurrentTransform != null) {
                            lastPosition = CurrentTransform.position;
                            lastRotation = CurrentTransform.rotation;
                        }

                        UnloadEntity();
                        Internal_OnDeathEvent();
                    }
                    else {
                        LivingState = ActorState.Dead;

                        UnloadEntity();
                        Internal_OnPermadeathEvent();
                        //DestroyEntity();
                    }

                    break;
            }
        }

        protected void NetworkShoot(bool shoot) {
            var shootEnableMessage = new ShootEnableMessage(NetworkShoot_Receive, Id, shoot, ShootDirection);

            QuadrablazeSteamNetworking.SendEntityNetworkMessage(NetworkShoot_Receive, shootEnableMessage);
        }

        [EntityNetworkListener]
        void NetworkShoot_Receive(NetworkMessage message) {
            var input = message.ReadMessage<ShootEnableMessage>();

            if(input.shootFlag) {
                RotateTransforms.ForEach(s => s.PointInDirection(input.shootDirection));
                OnShootStart.Invoke();
            }
            else {
                OnShootStop.Invoke();
            }
        }

        public virtual void OnCausedDamage(StatEvent statEvent) {
            if(CurrentGameObject != null)
                foreach(var component in CurrentGameObject.GetComponents<IStatCausedDamage>())
                    component.OnCausedDamage(statEvent);
        }

        public virtual void OnCausedHeal(StatEvent statEvent) {
            if(CurrentGameObject != null)
                foreach(var component in CurrentGameObject.GetComponents<IStatCausedHeal>())
                    component.OnCausedHeal(statEvent);
        }

        public virtual void OnDamaged(StatEvent statEvent) {
            if(CurrentGameObject != null)
                foreach(var component in CurrentGameObject.GetComponents<IStatDamaged>())
                    component.OnDamaged(statEvent);
        }

        protected virtual void OnDeathEvent() { }

        public virtual void OnHealed(StatEvent statEvent) {
            if(CurrentGameObject != null)
                foreach(var component in CurrentGameObject.GetComponents<IStatHealed>())
                    component.OnHealed(statEvent);
        }

        protected virtual void OnPermadeathEvent() { }

        protected virtual void OnRespawnEvent() { }

        protected virtual void OnSpawnEvent() { }

        public virtual void Internal_OnDeathEvent() {
            if(CurrentUpgradeSet != null)
                foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    if(element.CurrentExecutor is IEntityDeath entityDeath)
                        entityDeath.EntityDeath();

            OnGlobalDeath?.Invoke(this);
            OnDeath?.Invoke();
            OnDeathEvent();

            Debug.Log("Internal_OnDeathEvent");
        }

        public virtual void Internal_OnPermadeathEvent() {
            OnGlobalPermadeath?.Invoke(this);

            if(PreventDeath) {
                respawnTimer = TimeToRespawn;
                LivingState = ActorState.Limbo;

                if(CurrentTransform != null) {
                    lastPosition = CurrentTransform.position;
                    lastRotation = CurrentTransform.rotation;
                }

                if(this is MinionEntity)
                    if(GameManager.Current.CurrentGameMode is IEnemySpawnController spawnController)
                        spawnController.CurrentEnemyController.MinionCount++;

                PreventDeath = false;
                Listener.RaiseEvent(EntityActions.Death, this.ToArgs());
                Internal_OnDeathEvent();
            }
            else {
                OnDeathEvent();
                OnPermadeathEvent();

                Listener.RaiseEvent(EntityActions.Death, this.ToArgs());
                Listener.RaiseEvent(EntityActions.Permadeath, this.ToArgs());

                DestroyEntity();
            }
        }

        public virtual void Internal_OnRespawnEvent() {
            OnGlobalRespawn?.Invoke(this);
            OnRespawnEvent();
            Listener.RaiseEvent(EntityActions.Respawned, this.ToArgs());
        }

        public virtual void Internal_OnSpawnEvent() {
            OnGlobalSpawn?.Invoke(this);
            OnSpawnEvent();
        }

        public void Permakill() {
            if(CurrentUpgradeSet != null)
                CurrentUpgradeSet.Lives = 0;

            LivingState = ActorState.Dead;
        }

        public void PlaySound(AudioClip clip) {
            CameraSoundController.Current.PlaySound(clip);
        }
        public void PlaySound(string name, bool playOverNetwork = false) {
            CameraSoundController.Current.PlaySound(name, playOverNetwork);
        }

        public void RaiseAssignedSkillEvent(SkillLayoutElement element) {
            SkillWasAssigned(element);

            if(CurrentGameObject != null)
                foreach(var component in CurrentGameObject.GetComponentsInChildren<IActorEntityObjectAssignedSkill>())
                    component.OnAssignedSkill(element);

            OnAssignedSkill?.Invoke(element);
        }

        public void Respawn() {
            DoRespawn(this, lastPosition, lastRotation);

            //TODO: Reinitialize skill executors
            //LivingState = ActorState.Alive;

            //if(CurrentUpgradeSet != null)
            //foreach(var upgrades  in CurrentUpgradeSet.CurrentSkillLayout.ReloadSkillExecutors
        }

        [EntityNetworkListener] //TODO: Check if this is the correct usage
        public void ReceiveTryUseAbility(NetworkMessage message) {
            var input = message.ReadMessage<AbilityMessage>();

            if(GameManager.Current.GetActorEntity(input.id) is ActorEntity entity) {
                var ability = entity.CurrentUpgradeSet.CurrentSkillLayout.Elements[input.skillIndex];

                ability.CurrentExecutor?.Invoke();
            }
        }

        public override void SetGameObject(GameObject gameObject) {
            var gameObjectFlag = gameObject != null;
            GameObject previousGameObject = null;

            if(_currentGameObject != null && gameObject != _currentGameObject)
                previousGameObject = _currentGameObject;

            _currentGameObject = gameObject;

            BaseMovementControllerComponent = gameObject?.GetComponent<BaseMovementController>();
            NetworkIdentityComponent = gameObject?.GetComponent<NetworkIdentity>();

            UserComponent = gameObject?.GetComponent<PoolUser>();

            if(UserComponent != null) {
                OriginalPool = UserComponent.InPool;
                OriginalPoolPrefabIndex = UserComponent.PrefabIndex;
            }

            RigidbodyComponent = gameObject?.GetComponent<Rigidbody>();

            if(gameObjectFlag) {
                RotateTransforms = new List<RotateTransform>(gameObject.GetComponentsInChildren<RotateTransform>());

                Array.ForEach(CurrentGameObject.GetComponentsInChildren<IActorEntityObjectInitialize>(true), value => value.ActorEntityObjectInitialize(this));

                Listener.RaiseEvent(EntityActions.Spawned, this.ToArgs());
                OnSpawn?.Invoke();
                Internal_OnSpawnEvent();

                if(CurrentUpgradeSet != null && CurrentUpgradeSet.CurrentSkillLayout != null)
                    foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.Elements)
                        if(element.CurrentExecutor != null)
                            element.CurrentExecutor.ApplyToGameObject(gameObject);
            }

            if(previousGameObject != null) {
                if(RigidbodyComponent != null)
                    RigidbodyComponent.Sleep();

                GameObjectWasCleared(previousGameObject);

                if(CurrentUpgradeSet != null && CurrentUpgradeSet.CurrentSkillLayout != null)
                    foreach(var element in CurrentUpgradeSet.CurrentSkillLayout.Elements)
                        if(element.CurrentExecutor != null)
                            element.CurrentExecutor.ClearGameObject(gameObject);
            }

            if(gameObjectFlag)
                GameObjectWasSet(_currentGameObject);

            RaiseGameObjectSetEvent(_currentGameObject);
        }

        public virtual void DoDeath(HealthEvent healthEvent) {
            UserComponent.SpawnHere("Explosions");
            Kill();
        }

        public virtual void DoDeath(StatEvent statEvent) {
            bool dead = true;

            if(CurrentGameObject != null)
                if(CurrentGameObject.GetComponent<IStatDeath>() is IStatDeath statDeath)
                    statDeath.OnDeath(statEvent);

            foreach(var slot in HealthSlots)
                if(slot.Value > 0) {
                    dead = false;
                    break;
                }

            if(!dead) return;

            UserComponent.SpawnHere("Explosions");
            Kill();
        }

        protected virtual void SkillWasAssigned(SkillLayoutElement element) { }

        protected virtual void SkillLayoutInitialized() { }

        public void StartMovementInterrupt(float time) {
            if(MovementInterruptTimer.CurrentTime < time)
                MovementInterruptTimer.Start(time);
        }

        public void TryUseAbility(SkillLayoutElement element) {
            if(CurrentUpgradeSet.CurrentSkillLayout.HasSkill(element)) {
                var skillIndex = CurrentUpgradeSet.CurrentSkillLayout.Elements.IndexOf(element);
                var message = new AbilityMessage(ReceiveTryUseAbility, Id, skillIndex);

                Debug.Log($"PlayerEntity({Id}) TryUseAbility SendEntityNetworkMessageToServer");

                QuadrablazeSteamNetworking.SendEntityNetworkMessageToServer(ReceiveTryUseAbility, message);
            }
        }

        public virtual void UnloadEntity() {
            if(CurrentUpgradeSet != null)
                foreach(var skill in CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    if(skill.CurrentExecutor != null) {
                        skill.CurrentExecutor.Unload();
                        // TODO: Check this
                        //skill.CurrentExecutor = null;
                    }

            UserComponent?.Despawn();
            Listener.RaiseEvent(EntityActions.Despawned, this.ToArgs());
        }

        void UpdateLimbo() {
            if(respawnTimer > 0) {
                respawnTimer -= Time.deltaTime;

                if(respawnTimer <= 0)
                    Respawn();
            }
        }

        public static void DoRespawn(ActorEntity actorEntity, Vector3 position, Quaternion rotation) {
            if(actorEntity.OriginalPool == null)
                GameDebug.Log($"DoRespawn Error: ActorEntity({actorEntity.Name}, {actorEntity.Id}) does not come from an object pool");

            var user = actorEntity.OriginalPool.Spawn(actorEntity.OriginalPoolPrefabIndex, position, rotation);

            if(actorEntity is PlayerEntity playerEntity)
                user.GetComponent<NetworkIdentity>().AssignClientAuthority(playerEntity.PlayerInfo.connectionToClient);

            HealthManager.UpdateHealth(actorEntity);
            actorEntity.SetGameObject(user.gameObject);

            if(actorEntity is PlayerEntity) { // TODO: Remove hax
                actorEntity.HealthSlots[0].MaxValue = 200;
                actorEntity.HealthSlots[0].Value = 200;
            }

            if(actorEntity.CurrentUpgradeSet != null)
                foreach(var element in actorEntity.CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    if(element.CurrentExecutor != null) {
                        element.CurrentExecutor.LevelChanged(element.CurrentLevel, element.CurrentLevel);
                        element.CurrentExecutor.ApplyToGameObject(user.gameObject);
                        actorEntity.RaiseAssignedSkillEvent(element);
                    }

            actorEntity.LivingState = ActorState.Alive;
        }

        public delegate void ActorEvent(ActorEntity entity);
    }

    class AbilityMessage : EntityMessageBase {
        public int skillIndex;

        public AbilityMessage() { }
        public AbilityMessage(NetworkMessageDelegate method, uint id, int skillIndex) : base(method, id) {
            this.skillIndex = skillIndex;
        }

        public override void Deserialize(NetworkReader reader) {
            base.Deserialize(reader);

            skillIndex = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer) {
            base.Serialize(writer);

            writer.Write(skillIndex);
        }
    }

    public enum ActorState {
        Alive,
        Limbo,
        Dead
    }

    [Obsolete]
    public enum ActorProxyEvent {
        Created,
        ChangedStat,
        ChangedStatMax,
        Despawned,
        Destroyed,
        Respawned,
        Spawned,
        StatDamaged,
        StatHealed,
        StatDeath,
        StatRestored
    }

    public class CreateEntityArgs : EventArgs {
        public CreateEntityType EntityType { get; }
        public int Index { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Action<GameObject> Callback { get; }

        public CreateEntityArgs(CreateEntityType entityType, int index, Vector3 position, Quaternion rotation, Action<GameObject> callback = null) {
            EntityType = entityType;
            Index = index;
            Position = position;
            Rotation = rotation;
            Callback = callback;
        }
    }

    public static class ActorEntityHelper {
        public static ActorEntity GetActorEntity(this uint entityId) {
            return GameManager.Current.GetActorEntity(entityId);
        }
    }
}

public static partial class EntityActions {
    public static readonly ProxyAction Death = new ProxyAction();
    public static readonly ProxyAction Despawned = new ProxyAction();
    public static readonly ProxyAction Spawned = new ProxyAction();
    public static readonly ProxyAction Respawned = new ProxyAction();
    public static readonly ProxyAction Permadeath = new ProxyAction();

    public static ProxyAction ChangedStat => HealthActions.ChangedStat;
    public static ProxyAction ChangedStatMax => HealthActions.ChangedStatMax;

    public static ProxyAction StatDamaged => HealthActions.StatDamaged;
    public static ProxyAction StatDeath => HealthActions.StatDeath;
    public static ProxyAction StatHealed => HealthActions.StatHealed;
    public static ProxyAction StatRestored => HealthActions.StatRestored;
}