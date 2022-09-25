using System;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Quadrablaze {
    public static class PlayerProxy {

        public static readonly ProxyListener<ProxyAction> Proxy = new ProxyListener<ProxyAction>();

        public static HashSet<PlayerEntity> Players { get; set; }

        static PlayerEntity CheckEvent(EventArgs args) {
            var statEvent = ((StatEventArgs)args).Stat;
            var entity = statEvent.AffectedObject as PlayerEntity;

            if(entity == null)
                if((statEvent.AffectedObject as Shield)?.CurrentActorEntity is PlayerEntity ent)
                    entity = ent;

            return entity;
        }

        public static List<PlayerEntity> GetOrderedPlayers() {
            return Players.Count > 0 ?
                (Players.Count == 1 ? Players.ToList() : Players.OrderBy(s => s.PlayerInfo.netId.Value).ToList()) :
                new List<PlayerEntity>();
        }

        static void HealthProxyEvent_ChangedStat(EventArgs args) {
            RepeatEvent(HealthActions.ChangedStat, args);
        }

        static void HealthProxyEvent_ChangedStatMax(EventArgs args) {
            RepeatEvent(HealthActions.ChangedStatMax, args);
        }

        static void HealthProxyEvent_StatDamaged(EventArgs args) {
            if(CheckEvent(args) is PlayerEntity entity) {
                if(entity.Owner)
                    ShakeyCamera.Current.ShakeCapped(4);

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
            Players = new HashSet<PlayerEntity>();

            ActorEntity.Proxy.Subscribe(HealthActions.ChangedStat, HealthProxyEvent_ChangedStat);
            ActorEntity.Proxy.Subscribe(HealthActions.ChangedStatMax, HealthProxyEvent_ChangedStatMax);
            ActorEntity.Proxy.Subscribe(HealthActions.StatDamaged, HealthProxyEvent_StatDamaged);
            ActorEntity.Proxy.Subscribe(HealthActions.StatHealed, HealthProxyEvent_StatHealed);
            ActorEntity.Proxy.Subscribe(HealthActions.StatDeath, HealthProxyEvent_StatDeath);
            ActorEntity.Proxy.Subscribe(HealthActions.StatRestored, HealthProxyEvent_StatRestored);
        }

        static void RepeatEvent(ProxyAction eventType, EventArgs args) {
            if(CheckEvent(args) is PlayerEntity)
                Proxy.RaiseEvent(eventType, args);
        }
    }
}

public static partial class PlayerActions {
    public static readonly ProxyAction ChangedLevel = new ProxyAction();
    public static readonly ProxyAction ChangedSkillPoints = new ProxyAction();
    public static readonly ProxyAction ChangedXP = new ProxyAction();
    public static readonly ProxyAction ChangedNextXP = new ProxyAction();
    public static readonly ProxyAction Upgraded = new ProxyAction();
}