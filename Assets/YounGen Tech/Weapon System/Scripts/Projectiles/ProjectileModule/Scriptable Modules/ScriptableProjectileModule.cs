using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class ScriptableProjectileModule : ScriptableObject {
        public virtual ProjectileModule CreateInstance(ProjectileEntity attachedProjectile) {
            return new ProjectileModule(attachedProjectile);
        }
    }
}