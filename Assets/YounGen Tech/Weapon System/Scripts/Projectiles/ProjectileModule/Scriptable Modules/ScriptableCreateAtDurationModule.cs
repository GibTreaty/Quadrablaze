using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Projectile Modules/Create At Duration")]
    public class ScriptableCreateAtDurationModule : ScriptableProjectileModule {

        [SerializeField]
        float _time;

        [SerializeField]
        WeaponProperties _properties;

        public override ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new CreateAtDuration(attachedProjectile, _time, _properties);
        }
    }
}