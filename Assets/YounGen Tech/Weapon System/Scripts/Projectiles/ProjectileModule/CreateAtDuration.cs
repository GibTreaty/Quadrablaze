using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class CreateAtDuration : ProjectileModule {

        public float Time { get; set; }

        public WeaponProperties CreateProjectileProperties { get; set; }

        public CreateAtDuration(ProjectileEntity attachedProjectile, float time, WeaponProperties properties) : base(attachedProjectile) {
            Time = time;
            CreateProjectileProperties = properties;

            attachedProjectile.SubscribeDurationEvent(
                Time,
                () => WeaponEntity.CreateProjectiles(
                    AttachedProjectile.SourcePlayer,
                    AttachedProjectile.SourceWeaponEntity,
                    AttachedProjectile.Position,
                    AttachedProjectile.Angle,
                    0,
                    CreateProjectileProperties
                )
            );
        }
    }
}