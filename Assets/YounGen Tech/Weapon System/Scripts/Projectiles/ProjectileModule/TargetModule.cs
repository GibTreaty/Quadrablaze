using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class TargetModule : ProjectileModule, IProjectileModuleUpdate {

        public Transform Target { get; set; }

        public GameEntity TargetEntity { get; set; }

        List<ProjectileModule> targetPositionModules;

        public TargetModule(ProjectileEntity attachedProjectile) : base(attachedProjectile) {
            var array = attachedProjectile.GetModules(module => module is IModuleTargetPosition);
            targetPositionModules = new List<ProjectileModule>(array);
        }

        public void ProjectileModuleUpdate() {
            //if(Target == null)
            //    Target = GameObject.FindWithTag("Target")?.transform;
            //else {
            //    var position = Target.position;
            //    var distanceFromTarget = (AttachedProjectile.Position - position).magnitude;

            //    if(distanceFromTarget < .25f) {
            //        AttachedProjectile.SetDeathFlag();
            //    }
            //    else
            //        foreach(var targetPositions in targetPositionModules)
            //            (targetPositions as IModuleTargetPosition).TargetPosition = position;
            //}

            if(TargetEntity == null)
                TargetEntity = GameEntity.FindWithTag("Target");
            else {
                var position = TargetEntity.Position;
                var distanceFromTarget = (AttachedProjectile.Position - position).magnitude;

                if(distanceFromTarget > 0.001f)
                    foreach(var targetPositions in targetPositionModules)
                        (targetPositions as IModuleTargetPosition).TargetPosition = position;
            }
        }
    }
}