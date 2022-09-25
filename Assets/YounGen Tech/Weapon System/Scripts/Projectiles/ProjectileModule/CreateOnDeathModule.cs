using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class CreateOnDeathModule : ProjectileModule {

        public Quadrablaze.WeaponSystem.WeaponProperties CreateProjectileProperties { get; set; }

        public CreateOnDeathModule(ProjectileEntity attachedProjectile, Quadrablaze.WeaponSystem.WeaponProperties properties) : base(attachedProjectile) {
            CreateProjectileProperties = properties;
            attachedProjectile.OnDestroyed += AttachedProjectile_OnDestroyed;
        }

        void AttachedProjectile_OnDestroyed() {
            if(AttachedProjectile.DeathReason is string reason)
                if(reason != "Trigger")
                    Quadrablaze.WeaponSystem.Weapon.CreateProjectiles(AttachedProjectile.SourceWeapon, AttachedProjectile.Position, AttachedProjectile.Angle, 0, CreateProjectileProperties);
                    //WeaponEntity.CreateProjectiles(AttachedProjectile.SourcePlayer, AttachedProjectile.SourceWeaponEntity, AttachedProjectile.Position, AttachedProjectile.Angle, 0, CreateProjectileProperties);
        }
    }
}