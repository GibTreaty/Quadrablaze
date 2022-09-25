using System;
using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze {
    public static class EnemyProxy {

        public static readonly ProxyListener<ProxyAction> Proxy = new ProxyListener<ProxyAction>();

        #region Properties
        public static Boss.BossController SpawnedBoss { get; set; }

        public static HashSet<Entities.EnemyEntity> Enemies { get; set; }

        public static HashSet<Transform> Targets { get; private set; }
        #endregion

        static Entities.EnemyEntity CheckEvent(EventArgs args) {
            var statEvent = ((StatEventArgs)args).Stat;
            var entity = statEvent.AffectedObject as Entities.EnemyEntity;

            if(entity == null)
                if((statEvent.AffectedObject as Shield)?.CurrentActorEntity is Entities.EnemyEntity ent)
                    entity = ent;

            return entity;
        }

        public static GameObject[] GetBossHitSpots() {
            return SpawnedBoss != null ? SpawnedBoss.GetHitSpots() : null;
        }

        static void HealthProxyEvent_ChangedStat(EventArgs args) {
            RepeatEvent(HealthActions.ChangedStat, args);
        }

        static void HealthProxyEvent_ChangedStatMax(EventArgs args) {
            RepeatEvent(HealthActions.ChangedStatMax, args);
        }

        static void HealthProxyEvent_StatDamaged(EventArgs args) {
            Debug.Log("[Enemy] Damaged");
            if(CheckEvent(args) is Entities.EnemyEntity entity) {
                (entity as MinionEntity)?.DropXPTrickle();
                Proxy.RaiseEvent(HealthActions.StatDamaged, args);
            }
        }

        static void HealthProxyEvent_StatHealed(EventArgs args) {
            RepeatEvent(HealthActions.StatHealed, args);
        }

        static void HealthProxyEvent_StatDeath(EventArgs args) {
            RepeatEvent(HealthActions.StatDeath, args);
        }

        static void HealthProxyEvent_StatRestored(EventArgs args) {
            RepeatEvent(HealthActions.StatRestored, args);
        }

        public static void Initialize() {
            Enemies = new HashSet<Entities.EnemyEntity>();
            Targets = new HashSet<Transform>();

            ActorEntity.Proxy.Subscribe(HealthActions.ChangedStat, HealthProxyEvent_ChangedStat);
            ActorEntity.Proxy.Subscribe(HealthActions.ChangedStatMax, HealthProxyEvent_ChangedStatMax);
            ActorEntity.Proxy.Subscribe(HealthActions.StatDamaged, HealthProxyEvent_StatDamaged);
            ActorEntity.Proxy.Subscribe(HealthActions.StatHealed, HealthProxyEvent_StatHealed);
            ActorEntity.Proxy.Subscribe(HealthActions.StatDeath, HealthProxyEvent_StatDeath);
            ActorEntity.Proxy.Subscribe(HealthActions.StatRestored, HealthProxyEvent_StatRestored);
        }

        static void RepeatEvent(ProxyAction eventType, EventArgs args) {
            if(CheckEvent(args) is Entities.EnemyEntity)
                Proxy.RaiseEvent(eventType, args);
        }

        [Serializable]
        public class EnemyEvent : UnityEvent<Entities.EnemyEntity> { }

        [Serializable]
        public class EnemyHealthEvent : UnityEvent<Entities.EnemyEntity, HealthEvent> { }
    }
}