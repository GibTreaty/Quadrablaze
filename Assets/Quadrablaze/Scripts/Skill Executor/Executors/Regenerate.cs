using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze.SkillExecutors {
    public class Regenerate : SkillExecutor, ISkillUpdate {

        public new ScriptableRegenerateSkillExecutor OriginalSkillExecutor { get; }

        UnityAction<HealthEvent> onDamagedMethod = null;
        float lastDamageTime;
        float accumlatedHealAmount;

        public Regenerate(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableRegenerateSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;

            if(NetworkServer.active) 
                onDamagedMethod = OnDamaged;
        }

        void OnDamaged(HealthEvent healthEvent) { //TODO: Finish redoing the Regenerate ability
            lastDamageTime = Time.time;
            accumlatedHealAmount = 0;
        }

        public void SkillUpdate() {
            if(NetworkServer.active)
                if(CurrentActorEntity.HealthSlots[0].Value < CurrentActorEntity.HealthSlots[0].MaxValue)
                    if(Time.time > lastDamageTime + OriginalSkillExecutor.HealDelay) {
                        float healRate = OriginalSkillExecutor.HealRatePercent * CurrentLayoutElement.CurrentLevel;

                        accumlatedHealAmount += CurrentActorEntity.HealthSlots[0].MaxValue * healRate * Time.deltaTime;

                        if(accumlatedHealAmount >= 1) {
                            int healAmount = (int)accumlatedHealAmount;

                            accumlatedHealAmount -= healAmount;
                            CurrentActorEntity.DoHealthChange(healAmount, 0, this, false);
                        }
                    }
        }
    }
}