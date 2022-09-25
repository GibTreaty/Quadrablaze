using System.Collections.Generic;
//using Quadrablaze.SkillExecutors;
using UnityEngine;
using YounGenTech.Entities;
using YounGenTech.Entities.Weapon;

namespace Quadrablaze.WeaponSystem {
    public class Weapon : Entity, IEntityUpdate {

        public float CurrentSpreadTime {
            get { return IsShooting ? Mathf.Max(0, Time.time - StartShootTime) : 0; }
        }
        public WeaponProperties CurrentWeaponProperties { get; set; }
        public float DamageMultiplier { get; set; }
        public string Debug { get; set; } // TODO: Weapon - Remove debug property
        public LayerMask HitMask { get; set; }
        public List<string> HitTags { get; set; }
        public bool IsShooting { get; private set; }
        public float LastShootTime { get; private set; }
        public AnimationCurve ShootCurve { get; set; }
        public SkillExecutors.Weapon SkillExecutor { get; set; }
        public Transform SourceTransform { get; set; }
        public float StartShootTime { get; set; }

        public Weapon(uint id, GameObject gameObject) : base(id, gameObject) {
            DamageMultiplier = 1;
        }

        public void EntityUpdate() {
            if(IsShooting)
                Shoot();
        }

        public void SetShootingEnabled(bool enable) {
            IsShooting = enable;

            if(enable)
                StartShootTime = Time.time;
        }

        public void Shoot() {
            if(CurrentWeaponProperties.shotsPerSecond > 0) {
                if(Time.time > LastShootTime + (1f / CurrentWeaponProperties.shotsPerSecond))
                    ShootProjectiles();
            }
            else {
                ShootProjectiles();
            }
        }

        void ShootProjectiles() {
            float x = Time.time - StartShootTime;
            bool shoot = Mathf.RoundToInt(ShootCurve.Evaluate(x)) != 0;

            if(!shoot) return;

            var startAngle = SourceTransform.rotation.eulerAngles.y;

            CreateProjectiles(this, SourceTransform.position, startAngle, CurrentSpreadTime, CurrentWeaponProperties);

            LastShootTime = Time.time;
        }

        public static ProjectileEntity CreateProjectile(Weapon weapon, Vector3 startPosition, float startAngle, float currentSpreadTime, WeaponProperties properties) {
            var projectile = properties.originalProjectile.CreateInstance(weapon);

            projectile.SourceTransform = weapon.SourceTransform;
            projectile.MaxDuration = properties.maxProjectileDuration;
            projectile.Position = startPosition;

            float angle = startAngle;

            if(properties.spreadAngle > 0 || properties.spreadAmplitude != 0) {
                float offset = 0;

                switch(properties.spreadType) {
                    case SpreadType.PingPong: offset = Mathf.Sin(currentSpreadTime * properties.spreadFrequency); break;
                    case SpreadType.ReversePingPong: offset = Mathf.Sin(-currentSpreadTime * properties.spreadFrequency); break;

                    case SpreadType.Loop: offset = currentSpreadTime * properties.spreadFrequency; break;
                    case SpreadType.ReverseLoop: offset = -currentSpreadTime * properties.spreadFrequency; break;
                }

                angle += offset * properties.spreadAmplitude;
            }

            float finalAngle = angle;

            projectile.AngularVelocity = properties.startAngularVelocity;
            projectile.Acceleration = properties.startAcceleration;
            projectile.Angle = finalAngle;
            projectile.CreateGameObject(projectile.Position);

            return projectile;
        }

        public static void CreateProjectiles(Weapon weapon, Vector3 startPosition, float startAngle, float currentSpreadTime, WeaponProperties properties) {
            float spreadAngle = Mathf.Clamp(Mathf.Abs(properties.spreadAngle), 0, 360);
            float angles = 0;
            float offset = 0;

            if(spreadAngle != 0 && properties.projectilesPerShot > 1) {
                angles = spreadAngle / (properties.projectilesPerShot - 1);
                offset = spreadAngle * .5f;
            }

            for(int i = 0; i < properties.projectilesPerShot; i++)
                CreateProjectile(weapon, startPosition, startAngle + (angles * i) - offset, currentSpreadTime, properties);
        }
    }

    public enum SpreadType {
        PingPong = 0,
        ReversePingPong = 1,
        Loop = 2,
        ReverseLoop = 3
    }
}