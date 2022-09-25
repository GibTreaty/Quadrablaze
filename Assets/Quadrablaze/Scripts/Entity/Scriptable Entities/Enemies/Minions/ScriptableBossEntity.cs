using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    [CreateAssetMenu(menuName = "Quadrablaze/Entities/Boss")]
    public class ScriptableBossEntity : ScriptableActorEntity {

        public override Entity CreateInstance() {
            var entity = new BossEntity(null, name, 0, _originalUpgradeSet?.CreateInstance(), _size) {
                OriginalScriptableObject = this,
                MovementInterruptTimer = new Timer(_movementInterruptLength, Timer.TimerDirection.Down),
                HealthSlots = GetHealthStatsArray()
            };

            if(_originalUpgradeSet != null)
                entity.CurrentUpgradeSet.CurrentSkillLayout.CurrentEntity = entity;

            return entity;
        }
    }
}