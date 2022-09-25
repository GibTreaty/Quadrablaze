using UnityEngine;
using YounGenTech.Entities.Weapon;

namespace Quadrablaze.WeaponSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Weapon System/Projectile Modules/Damage")]
    public class ScriptableDamageModule : ScriptableProjectileModule {

        [SerializeField]
        float _damageAmount;

        [SerializeField]
        HealthEvent.EventType _healthEventType = HealthEvent.EventType.Normal;

        public float DamageAmount => _damageAmount;

        public override ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new DamageModule(attachedProjectile, _damageAmount, _healthEventType);
        }
    }
}