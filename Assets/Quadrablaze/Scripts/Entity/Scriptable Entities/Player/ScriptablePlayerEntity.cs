using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    [CreateAssetMenu(menuName = "Quadrablaze/Entities/Player")]
    public class ScriptablePlayerEntity : ScriptableActorEntity {

        public override Entity CreateInstance() {
            var entity = new PlayerEntity(null, name, default, _originalUpgradeSet?.CreateInstance(), _size, null) {
                OriginalScriptableObject = this,
                MovementInterruptTimer = new Timer(_movementInterruptLength, Timer.TimerDirection.Down),
                HealthSlots = GetHealthStatsArray()
            };

            entity.CurrentUpgradeSet.CurrentSkillLayout.CurrentEntity = entity;

            return entity;
        }

        //public static implicit operator PlayerEntity(ScriptablePlayerEntity entity) {
        //    return new PlayerEntity(null,
        //}
    }
}