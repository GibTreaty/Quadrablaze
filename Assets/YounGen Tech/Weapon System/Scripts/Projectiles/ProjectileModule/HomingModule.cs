using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class HomingModule : ProjectileModule, IProjectileModuleUpdate, IModuleTargetPosition {

        public float HomingAngularSpeed { get; set; }

        public bool TargetFound { get; set; }

        public Vector3 TargetPosition { get; set; }

        public HomingModule(ProjectileEntity attachedProjectile, float homingAngularSpeed) : base(attachedProjectile) {
            HomingAngularSpeed = homingAngularSpeed;
        }

        public void ProjectileModuleUpdate() {
            var directionToTarget = (TargetPosition - AttachedProjectile.Position);
            var distanceToTarget = directionToTarget.magnitude;

            if(distanceToTarget > 0.01f) {
                var rotation = Quaternion.Euler(0, 0, AttachedProjectile.Angle);
                var projectileDirection = rotation * Vector2.up;
                float forwardDot = Vector2.Dot(projectileDirection, directionToTarget.normalized);

                Vector3 cross = Vector3.Cross(directionToTarget, rotation * Vector2.up);
                float rotate = cross.normalized.z;

                AttachedProjectile.AngularVelocity = TurnAlgorithm();

                float TurnAlgorithm() {
                    return -rotate * (1 / distanceToTarget) * (HomingAngularSpeed * AttachedProjectile.Acceleration);
                }
            }
        }
    }
}