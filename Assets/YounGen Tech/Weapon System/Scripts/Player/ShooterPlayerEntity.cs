using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class ShooterPlayerEntity : GameEntity, IEntityUpdate {

        public float AccelerateSpeed { get; set; }

        public float Angle { get; set; }

        public List<WeaponEntity> EquippedWeapons { get; private set; }

        public Transform ShootingPoint { get; private set; }

        public float TurnSpeed { get; set; }

        public ShooterPlayerEntity(uint id, GameObject gameObject) : base(id, gameObject) {
            EquippedWeapons = new List<WeaponEntity>();
        }

        public override GameObject CreateGameObject(Vector3 position = default, Quaternion rotation = default, Transform parent = null) {
            var gameObject = base.CreateGameObject(position, rotation, parent);

            ShootingPoint = gameObject.transform.Find("Shooting Point");

            return gameObject;
        }

        public void Equip(WeaponEntity weapon) {
            if(!EquippedWeapons.Contains(weapon)) {
                EquippedWeapons.Add(weapon);
                weapon.SourcePlayer = this;
            }
        }

        public void EntityUpdate() {
            if(EquippedWeapons.Count > 0) {
                var startShootingInput = Input.GetKeyDown(KeyCode.Space);
                var endShootingInput = Input.GetKeyUp(KeyCode.Space);

                if(startShootingInput)
                    SetWeaponShootingEnabled(true);

                if(endShootingInput)
                    SetWeaponShootingEnabled(false);
            }

            float turnInput = -Input.GetAxis("Horizontal");

            Angle += turnInput * TurnSpeed * Time.deltaTime;
            Angle %= 360;
            if(Angle < 0) Angle += 360;

            var rotation = Quaternion.Euler(0, 0, Angle);

            float accelerateInput = Input.GetAxis("Vertical");
            Position += rotation * new Vector3(0, accelerateInput * AccelerateSpeed * Time.deltaTime, 0);

            if(CurrentGameObject != null) 
                CurrentGameObject.transform.rotation = rotation;            
        }

        void SetWeaponShootingEnabled(bool enable) {
            foreach(var weapon in EquippedWeapons)
                weapon.SetShootingEnabled(enable);
        }

        public void Unequip(WeaponEntity weapon) {
            EquippedWeapons.Remove(weapon);
        }
    }
}