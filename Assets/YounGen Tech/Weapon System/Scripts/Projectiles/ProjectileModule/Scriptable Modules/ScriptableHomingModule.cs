using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Projectile Modules/Homing")]
    public class ScriptableHomingModule : ScriptableProjectileModule {

        [SerializeField]
        float _homingAngularSpeed = 90;

        public override ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new HomingModule(attachedProjectile, _homingAngularSpeed);
        }
    }
}