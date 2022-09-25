using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors {
    public class Respawn : SkillExecutor, ICooldownTimer, IEntityDeath, ISkillUpdate {

        public EventTimer CooldownTimer { get; }
        public new ScriptableRespawnSkillExecutor OriginalSkillExecutor { get; }

        public Respawn(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableRespawnSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            CooldownTimer = new EventTimer(1) { AutoDisable = true };
            SetCooldownTimer(CurrentLayoutElement.CurrentLevel);
            CooldownTimer.OnElapsed.AddListener(CooldownFinished);
        }

        void CooldownFinished() {
            CurrentActorEntity.CurrentUpgradeSet.Lives = 1;
        }

        public void EntityDeath() {
            Debug.Log("Entity died yo");
            CooldownTimer.Start(true);

            if(NetworkServer.active)
                GiveSkillFeedback(false);
            //if(!doRespawn)
            //    if(CooldownTimer.HasElapsed) {
            //        respawnPosition = CurrentActorEntity.CurrentTransform.position;
            //        respawnRotation = CurrentActorEntity.CurrentTransform.rotation;

            //        respawnTime = Time.time + 1;
            //        doRespawn = true;
            //    }
        }

        public override void LevelChanged(int level, int previousLevel) {
            if(level > 0) {
                if(level == previousLevel) { // This means the player was recently respawned
                    //Debug.Log("Entity died yo");
                    //CooldownTimer.Start(true);

                    //if(NetworkServer.active)
                    //    GiveSkillFeedback(false);
                }
                else {
                    SetCooldownTimer(level);

                    if(CurrentActorEntity.CurrentUpgradeSet.Lives < 1)
                        CurrentActorEntity.CurrentUpgradeSet.Lives = 1;
                }
            }
        }

        public override void OnSkillFeedback(byte[] parameters) {
            SetCooldownTimer(CurrentLayoutElement.CurrentLevel);
            CooldownTimer.Start(true);
        }

        void SetCooldownTimer(int level) {
            CooldownTimer.Length = OriginalSkillExecutor.RespawnTimerCurve.Evaluate(level);
            CooldownTimer.CurrentTime = 0;
        }

        public void SkillUpdate() {
            CooldownTimer.Update();
        }
    }
}