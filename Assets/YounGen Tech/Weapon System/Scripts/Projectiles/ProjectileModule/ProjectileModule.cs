using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class ProjectileModule {

        public ProjectileEntity AttachedProjectile { get; protected set; }

        public ProjectileModule(ProjectileEntity attachedProjectile) {
            AttachedProjectile = attachedProjectile;
        }
    }
}