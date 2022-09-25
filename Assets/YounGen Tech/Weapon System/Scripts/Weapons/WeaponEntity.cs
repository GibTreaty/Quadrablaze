using System;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class WeaponEntity : Entity, IEntityUpdate {

        public float CurrentSpreadTime {
            get { return IsShooting ? Mathf.Max(0, Time.time - StartShootTime) : 0; }
        }

        public WeaponProperties CurrentWeaponProperties { get; set; }

        public bool IsShooting { get; private set; }

        public float LastShootTime { get; private set; }

        public AnimationCurve ShootCurve { get; set; }

        public ShooterPlayerEntity SourcePlayer { get; set; }

        public float StartShootTime { get; set; }

        public WeaponEntity(uint id, GameObject gameObject) : base(id, gameObject) { }

        //ProjectileEntity CreateProjectile(ShooterPlayerEntity player, float startAngle) {
        //    startAngle += SourcePlayer.ShootingPoint.rotation.eulerAngles.z;

        //    return CreateProjectile(player, this, SourcePlayer.ShootingPoint.position, startAngle, CurrentSpreadTime, CurrentWeaponProperties);
        //}

        public void EntityUpdate() {
            //float spreadAngle = Mathf.Clamp(Mathf.Abs(SpreadAngle), 0, 360);
            //float angles = 0;
            //float offset = 0;

            //if(spreadAngle != 0 && ProjectilesPerShot > 1) {
            //    angles = spreadAngle / (ProjectilesPerShot - 1);
            //    offset = spreadAngle * .5f;
            //}

            //for(int i = 0; i < ProjectilesPerShot; i++) 
            //    Debug.DrawRay(SourcePlayer.Position, Quaternion.Euler(0, 0, SourcePlayer.Angle + (angles * i) - offset) * Vector2.up, Color.magenta);


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

            var startAngle = SourcePlayer.ShootingPoint.rotation.eulerAngles.z;

            CreateProjectiles(SourcePlayer, this, SourcePlayer.ShootingPoint.position, startAngle, CurrentSpreadTime, CurrentWeaponProperties);

            LastShootTime = Time.time;
        }


        public static ProjectileEntity CreateProjectile(ShooterPlayerEntity player, WeaponEntity weapon, Vector3 startPosition, float startAngle, float currentSpreadTime, WeaponProperties properties) {
            var projectile = properties.originalProjectile.CreateInstance() as ProjectileEntity;

            projectile.SourcePlayer = player;
            projectile.SourceWeaponEntity = weapon;
            projectile.MaxDuration = properties.maxProjectileDuration;
            projectile.Position = startPosition;

            float angle = startAngle;

            if(properties.spreadAngle > 0) {
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

        public static void CreateProjectiles(ShooterPlayerEntity player, WeaponEntity weapon, Vector3 startPosition, float startAngle, float currentSpreadTime, WeaponProperties properties) {
            float spreadAngle = Mathf.Clamp(Mathf.Abs(properties.spreadAngle), 0, 360);
            float angles = 0;
            float offset = 0;

            if(spreadAngle != 0 && properties.projectilesPerShot > 1) {
                angles = spreadAngle / (properties.projectilesPerShot - 1);
                offset = spreadAngle * .5f;
            }

            for(int i = 0; i < properties.projectilesPerShot; i++)
                CreateProjectile(player, weapon, startPosition, startAngle + (angles * i) - offset, currentSpreadTime, properties);
        }
    }
    public enum SpreadType {
        PingPong = 0,
        ReversePingPong = 1,
        Loop = 2,
        ReverseLoop = 3
    }
}