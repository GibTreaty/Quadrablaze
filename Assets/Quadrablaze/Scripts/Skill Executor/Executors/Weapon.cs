using System;
using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using Quadrablaze.WeaponSystem;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    public class Weapon : SkillExecutor {

        public WeaponSystem.Weapon CurrentWeapon { get; private set; }
        public new ScriptableWeaponSkillExecutor OriginalSkillExecutor { get; }

        string weaponPivotTransformPath = "";

        Action shootStartMethod;
        Action shootStopMethod;

        public Weapon(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableWeaponSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;

            if(CurrentActorEntity is PlayerEntity playerEntity) {
                var shipInfo = ShipImporter.Current.GetPlayerActorShipData(playerEntity);
                var shipInfoObject = shipInfo.rootMeshObject.GetComponent<ShipInfoObject>();
                var weaponPivot = shipInfoObject.WeaponPivots[CurrentLayoutElement.ElementTypeIndex];

                if(weaponPivot.IsChildOf(shipInfo.rootMeshObject.transform))
                    weaponPivotTransformPath = shipInfo.rootMeshObject.transform.GetTransformPath(weaponPivot);
            }
            else
                weaponPivotTransformPath = OriginalSkillExecutor.WeaponPivotTransformPath;

            CurrentActorEntity.OnShootStart += ShootStart;
            CurrentActorEntity.OnShootStop += ShootStop;
        }

        void AssignWeapon(int level) {
            if(OriginalSkillExecutor.List != null) {
                var weapon = OriginalSkillExecutor.List.Weapons[level - 1];

                Equip(weapon);
            }
        }

        public void Equip(WeaponSystem.Weapon weapon) {
            CurrentWeapon = weapon;

            if(CurrentWeapon != null) {
                CurrentWeapon.SkillExecutor = this;
                CurrentWeapon.SourceTransform = GetWeaponPivot();

                if(CurrentActorEntity is TurretEntity)
                    Debug.Log("Weapon? " + (CurrentWeapon.SourceTransform != null) + "\n " + weaponPivotTransformPath);

                CurrentWeapon.HitMask = OriginalSkillExecutor.HitMask;
                CurrentWeapon.HitTags = OriginalSkillExecutor.HitTags;
                CurrentWeapon.Debug = CurrentActorEntity.Name;

                shootStartMethod = () => CurrentWeapon.SetShootingEnabled(true);
                shootStopMethod = () => CurrentWeapon.SetShootingEnabled(false);

                if(OriginalSkillExecutor.AutoShootOnEquip)
                    ShootStart();
            }
            else {
                shootStartMethod = null;
                shootStopMethod = null;
            }
        }

        Transform GetWeaponPivot() {
            return CurrentActorEntity.CurrentTransform.Find(weaponPivotTransformPath);
        }

        public override void LevelChanged(int level, int previousLevel) {
            AssignWeapon(level);
        }

        public void ShootStart() {
            shootStartMethod?.Invoke();
        }

        public void ShootStop() {
            shootStopMethod?.Invoke();
        }

        public override void Unload() {
            if(CurrentWeapon != null) {
                CurrentWeapon.DestroyEntity();
                CurrentWeapon = null;
                //Debug.Log("Unload weapon");
            }

            shootStartMethod = null;
            shootStopMethod = null;
        }
    }
}