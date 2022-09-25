using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using System;

namespace Quadrablaze {
    public static class ShieldProxy {

        public static readonly ProxyEvent<ProxyAction> Proxy = new ProxyEvent<ProxyAction>();

        public static void Initialize() {
            ActorEntity.Proxy.Subscribe(HealthActions.ChangedStat, e => OnChangedStat(HealthActions.ChangedStat, e));
            ActorEntity.Proxy.Subscribe(HealthActions.StatDamaged, e => OnChangedStat(HealthActions.StatDamaged, e));
            ActorEntity.Proxy.Subscribe(HealthActions.StatHealed, e => OnChangedStat(HealthActions.StatHealed, e));
        }

        static void OnChangedStat(ProxyAction eventType, EventArgs args) {
            var statEvent = ((StatEventArgs)args).Stat;

            if(statEvent.AffectedObject is Shield)
                Proxy.RaiseEvent(HealthActions.ChangedStat, args);
        }
    }
}