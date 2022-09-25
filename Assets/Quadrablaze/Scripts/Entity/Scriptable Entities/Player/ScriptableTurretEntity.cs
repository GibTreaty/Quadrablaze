using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    [CreateAssetMenu(menuName = "Quadrablaze/Entities/Turret")]
    public class ScriptableTurretEntity : ScriptableActorEntity {


        public override Entity CreateInstance() {
            var entity = new TurretEntity(null, name, 0, _originalUpgradeSet?.CreateInstance(), _size) {
                OriginalScriptableObject = this,
                MovementInterruptTimer = new Timer(_movementInterruptLength, Timer.TimerDirection.Down)
            };

            if(_originalUpgradeSet != null)
                entity.CurrentUpgradeSet.CurrentSkillLayout.CurrentEntity = entity;

            return entity;
        }
    }
}