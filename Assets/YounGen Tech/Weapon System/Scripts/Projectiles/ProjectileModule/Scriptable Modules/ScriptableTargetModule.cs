using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Projectile Modules/Targeting")]
    public class ScriptableTargetModule : ScriptableProjectileModule {
        public override ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new TargetModule(attachedProjectile);
        }
    }
}