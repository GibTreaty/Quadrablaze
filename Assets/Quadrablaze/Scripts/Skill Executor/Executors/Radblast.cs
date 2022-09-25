using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors { // TODO: Ability - Radblast doesn't do damage to bosses
    public class Radblast : SkillExecutor, ICooldownTimer, ISkillUpdate {

        public EventTimer CooldownTimer { get; }
        public override bool IsReady => CooldownTimer.HasElapsed;
        public new ScriptableRadblastSkillExecutor OriginalSkillExecutor { get; }

        public Radblast(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableRadblastSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            CooldownTimer = new EventTimer(OriginalSkillExecutor.CooldownDuration) { AutoDisable = true };
        }

        void DoSkillEffect() {
            Vector3 direction = Vector3.zero;

            if(CurrentActorEntity is PlayerEntity playerEntity) {
                switch(CurrentActorEntity.BaseMovementControllerComponent.MovementStyle) {
                    case BaseMovementController.MovementType.Directional:
                        direction = playerEntity.PlayerInputComponent.ShootDirectionFromAngle;
                        break;

                    case BaseMovementController.MovementType.Arcade:
                        direction = CurrentActorEntity.CurrentTransform.forward;
                        break;
                }
            }
            else {
                direction = CurrentActorEntity.CurrentTransform.forward;
            }

            Debug.DrawRay(CurrentActorEntity.CurrentTransform.position, direction * 2, Color.red, 20, false);

            var rotation = Quaternion.LookRotation(direction);
            var user = PoolManager.Spawn(OriginalSkillExecutor.PrefabPoolName, GetPrefabIndex(), CurrentActorEntity.CurrentTransform.position, rotation);
            var rigidbody = user.GetComponent<Rigidbody>();

            if(direction.sqrMagnitude > .01f)
                rigidbody.velocity = direction.normalized * OriginalSkillExecutor.Speed;
        }

        int GetPrefabIndex() {
            var pool = PoolManager.GetPool(OriginalSkillExecutor.PrefabPoolName);

            return pool.IndexFromPrefabName(GetPrefabName());
        }

        string GetPrefabName() {
            return OriginalSkillExecutor.PrefabNamePerLevel[CurrentLayoutElement.CurrentLevel - 1];
        }

        public override void InvokeSkillServer() {
            if(!IsReady) return;

            DoSkillEffect();
            CooldownTimer.Start(true);
            GiveSkillFeedback(false);
        }

        public override void OnSkillFeedback(byte[] parameters) {
            CooldownTimer.Start(true);
        }

        public void SkillUpdate() {
            CooldownTimer.Update();
        }
    }
}