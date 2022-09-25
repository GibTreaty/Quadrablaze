using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using UnityEngine;

namespace Quadrablaze.Entities {
    public class TurretEntity : ActorEntity {

        public DeployableTurret Turret { get; set; }
        public uint OwnerEntityId { get; set; }

        public TurretEntity(GameObject gameObject, string name, uint id, UpgradeSet upgradeSet, float size) : base(gameObject, name, id, upgradeSet, size) { }

        //protected override void GameObjectWasCleared(GameObject previousGameObject) {

        //}

        protected override void GameObjectWasSet(GameObject gameObject) {
            Turret = gameObject.GetComponent<DeployableTurret>();

            BaseMovementControllerComponent.EnableMovementUpdate = Turret.EnableMovement;

            var entity = GameManager.Current.GetActorEntity(OwnerEntityId);

            if(entity is PlayerEntity playerEntity)
                Turret.UpdateMaterial(playerEntity.PlayerInfo.netId);
        }

        protected override void SkillLayoutInitialized() {
            var entity = GameManager.Current.GetActorEntity(OwnerEntityId);

            var turretExecutor = entity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Turret>();
            var deployedLevel = turretExecutor.CurrentLayoutElement.CurrentLevel;

            Turret.EnableHealRing = deployedLevel >= 2;
            Turret.EnableMovement = deployedLevel >= 3;

            var highestWeaponLevel = turretExecutor.GetHighestWeaponLevel();
            var turretWeaponElement = CurrentUpgradeSet.CurrentSkillLayout.GetElement<Weapon>();

            turretExecutor.CurrentActorEntity.CurrentUpgradeSet.CurrentSkillLayout.SetElementLevel(turretWeaponElement, highestWeaponLevel);

            if(deployedLevel >= 4) {
                var weaponExecutor = turretWeaponElement.CurrentExecutor as Weapon;
                float multiplierValue = 1;

                switch(deployedLevel) {
                    case 4: multiplierValue = 1.2f; break;
                    case 5: multiplierValue = 1.6f; break;
                    case 6: multiplierValue = 2f; break;
                }

                weaponExecutor.CurrentWeapon.DamageMultiplier = multiplierValue;
            }

            if(deployedLevel >= 5) {
                var barrageElement = CurrentUpgradeSet.CurrentSkillLayout.GetElement<Barrage>();

                //NetworkUpgradeManager.Current.Server_SetUpgradeLevel(poolUser.gameObject, turretBarrageElement.IndexOf(), 1);

                CurrentUpgradeSet.CurrentSkillLayout.SetElementLevel(barrageElement, 1);
            }
        }

        //protected override void SkillWasAssigned(SkillLayoutElement element) {
        //    switch(element.CurrentExecutor) {
        //        case WeaponSkillExecutor executor: {


        //            break;
        //        }
        //        case BarrageSkillExecutor executor: {


        //            break;
        //        }
        //    }
        //}
    }
}