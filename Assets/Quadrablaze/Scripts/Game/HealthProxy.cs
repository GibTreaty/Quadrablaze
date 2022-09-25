using System;
using Quadrablaze.Entities;
using StatSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze {
    public static class HealthProxy {
        public static void Initialize() {
            ActorEntity.Proxy.Subscribe(HealthActions.StatDamaged, CheckStatDamagedEvent);
            ActorEntity.Proxy.Subscribe(HealthActions.StatDeath, CheckStatDeathEvent);
        }

        static void CheckStatDamagedEvent(EventArgs args) {
            var statEvent = ((StatEventArgs)args).Stat;

            if(statEvent.AffectedObject is ActorEntity actorEntity)
                HitManager.Current.FilterEvent(statEvent);
        }

        static void CheckStatDeathEvent(EventArgs args) {
            if(NetworkServer.active) {
                var statEvent = ((StatEventArgs)args).Stat;

                if(statEvent.AffectedObject is ActorEntity actorEntity)
                    actorEntity.DoDeath(((StatEventArgs)args).Stat);
            }
        }
    }
}

public static class HealthActions {
    public static readonly ProxyAction ChangedStat = new ProxyAction();
    public static readonly ProxyAction ChangedStatMax = new ProxyAction();

    public static readonly ProxyAction StatDamaged = new ProxyAction();
    public static readonly ProxyAction StatDeath = new ProxyAction();
    public static readonly ProxyAction StatHealed = new ProxyAction();
    public static readonly ProxyAction StatRestored = new ProxyAction();
}