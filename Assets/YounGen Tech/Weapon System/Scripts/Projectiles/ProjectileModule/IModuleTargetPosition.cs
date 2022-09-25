using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public interface IModuleTargetPosition : IProjectileModuleInterface {
        Vector3 TargetPosition { get; set; }
    }
}