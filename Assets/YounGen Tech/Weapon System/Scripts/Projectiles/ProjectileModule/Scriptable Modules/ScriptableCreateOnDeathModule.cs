using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Projectile Modules/Create On Death")]
    public class ScriptableCreateOnDeathModule : ScriptableProjectileModule {

        [SerializeField]
        Quadrablaze.WeaponSystem.WeaponProperties _properties;

        public override ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new CreateOnDeathModule(attachedProjectile, _properties);
        }
    }
}