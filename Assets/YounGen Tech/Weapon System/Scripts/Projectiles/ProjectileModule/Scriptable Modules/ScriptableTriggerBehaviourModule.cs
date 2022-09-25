using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Projectile Modules/Trigger Death")]
    public class ScriptableTriggerBehaviourModule : ScriptableProjectileModule {
        [SerializeField]
        string[] _triggerTags;

        public override ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new TriggerDeathModule(attachedProjectile, _triggerTags);
        }
    }
}