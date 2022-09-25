using UnityEngine;
using YounGenTech.Entities.Weapon;

namespace Quadrablaze.WeaponSystem {
    [System.Serializable]
    public struct WeaponProperties {
        public ScriptableProjectileEntity originalProjectile;
        public float maxProjectileDuration;
        public int projectilesPerShot;
        public float shotsPerSecond;
        public float spreadAngle;
        public SpreadType spreadType;
        public float spreadAmplitude;
        public float spreadFrequency;
        public float startAngularVelocity;
        public float startAcceleration;
    }
}