using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YounGenTech.PoolGen;

// TODO: Projectile - Entities being kept around after player death wtf
namespace YounGenTech.Entities.Weapon {
    public class ProjectileEntity : Entity, IEntityUpdate {

        public float Acceleration { get; set; }
        public float Angle { get; set; }
        public float AngularVelocity { get; set; }
        public bool DeathFlag => deathFlag;
        public float Duration { get; set; }
        public LayerMask HitMask => SourceWeapon.HitMask;
        public List<string> HitTags => SourceWeapon.HitTags;
        public float HomingAngularSpeed { get; set; }
        public bool HomingProjectile { get; set; }
        public float MaxDuration { get; set; }
        public List<ProjectileModule> Modules { get; private set; }
        public int PoolId { get; set; }
        public string PoolName { get; set; }
        public Vector3 Position { get; set; }
        public ShooterPlayerEntity SourcePlayer { get; set; }
        public Transform SourceTransform { get; set; }
        public WeaponEntity SourceWeaponEntity { get; set; }
        public Quadrablaze.WeaponSystem.Weapon SourceWeapon { get; set; }
        public Transform Target { get; set; }

        List<DurationEvent> durationEvents = null;

        bool deathFlag;

        public ProjectileEntity(uint id, GameObject gameObject, params ProjectileModule[] modules) : base(id, gameObject) {
            Modules = new List<ProjectileModule>(modules);
        }

        public void AddModule(ProjectileModule module) {
            if(!Modules.Contains(module))
                Modules.Add(module);
        }

        public override GameObject CreateGameObject(Vector3 position = default, Quaternion rotation = default, Transform parent = null) {
            var pool = PoolGen.PoolManager.GetPool(PoolName);
            var index = pool.IndexFromPrefabID(PoolId);
            var output = pool.Spawn(index, position, rotation);

            SetGameObject(output.gameObject);

            return output.gameObject;
        }

        public override void DestroyEntity() {
            foreach(var module in Modules)
                (module as IProjectileModuleDestroyed)?.ModuleDestroyed();

            if(CurrentGameObject.GetComponent<PoolUser>() is PoolUser user) {
                user.Despawn();
                InvokeDestroyEvents();
            }
            else
                base.DestroyEntity();
        }

        public void EntityUpdate() {
            Duration += Time.deltaTime;

            if(durationEvents != null)
                UpdateDurationEvents();

            if(!deathFlag)
                if(Duration >= MaxDuration)
                    SetDeathFlag("Duration");

            foreach(var module in Modules)
                (module as IProjectileModuleUpdate)?.ProjectileModuleUpdate();

            Angle += AngularVelocity * Time.deltaTime;

            var rotation = Quaternion.Euler(0, Angle, 0);

            Position += rotation * Vector3.forward * Acceleration * Time.deltaTime;

            if(CurrentGameObject != null) {
                CurrentGameObject.transform.position = Position;
                CurrentGameObject.transform.rotation = rotation;
            }

            if(deathFlag)
                DestroyEntity();
        }

        public T GetModule<T>() where T : ProjectileModule {
            return (T)Modules.FirstOrDefault(m => m is T);
        }
        public ProjectileModule GetModule(Func<ProjectileModule, bool> predicate) {
            foreach(var module in Modules)
                if(predicate(module))
                    return module;

            return default;
        }

        public T[] GetModules<T>() where T : ProjectileModule {
            return (T[])Modules.FindAll(m => m is T).ToArray();
        }
        public ProjectileModule[] GetModules(Predicate<ProjectileModule> predicate) {
            return (ProjectileModule[])Modules.FindAll(predicate).ToArray();
        }

        public bool RemoveModule(ProjectileModule module) {
            return Modules.Remove(module);
        }

        public void SetDeathFlag(object deathReason) {
            deathFlag = true;
            DeathReason = deathReason;
        }

        public void SubscribeDurationEvent(float time, Action action) {
            if(durationEvents == null) durationEvents = new List<DurationEvent>();

            durationEvents.Add(new DurationEvent(time, action));
        }

        void UpdateDurationEvents() {
            List<DurationEvent> clearEvents = new List<DurationEvent>();

            foreach(var durationEvent in durationEvents) {
                if(!durationEvent.Fired)
                    if(Duration >= durationEvent.Time) {
                        durationEvent.Fired = true;
                        durationEvent.Action();

                        clearEvents.Add(durationEvent);
                    }
            }

            foreach(var item in clearEvents)
                durationEvents.Remove(item);
        }

        class DurationEvent {
            public Action Action { get; set; }
            public bool Fired { get; set; }
            public float Time { get; set; }

            public DurationEvent(float time, Action action) {
                this.Time = time;
                this.Action = action;
            }
        }
    }
}