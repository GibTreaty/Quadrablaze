using Quadrablaze.Entities;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    public class Movement : SkillExecutor {

        int _levelBefore = 0;
        float _currentSpeedMultiplier;
        float _baseSpeedMultiplier;

        public new ScriptableMovementSkillExecutor OriginalSkillExecutor { get; }

        public Movement(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableMovementSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            _baseSpeedMultiplier = CurrentActorEntity.BaseMovementControllerComponent.SpeedMultiplier;
        }

        float GetSpeedAtLevel(int level) {
            int index = Mathf.Clamp(level - 1, 0, OriginalSkillExecutor.SpeedUpgrades.Length - 1);

            return OriginalSkillExecutor.SpeedUpgrades[index];
        }

        public override void LevelChanged(int level, int previousLevel) {
            SetMovementSpeed(level);
        }

        void SetMovementSpeed(int level) {
            if(level == _levelBefore) return;

            CurrentActorEntity.BaseMovementControllerComponent.SpeedMultiplier -= _currentSpeedMultiplier;

            float addSpeedMultiplier = GetSpeedAtLevel(level);

            CurrentActorEntity.BaseMovementControllerComponent.SpeedMultiplier += addSpeedMultiplier;

            _currentSpeedMultiplier = addSpeedMultiplier;
        }

        public override void Unload() {
            CurrentActorEntity.BaseMovementControllerComponent.SpeedMultiplier = _baseSpeedMultiplier;
        }
    }
}