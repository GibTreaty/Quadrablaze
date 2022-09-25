using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Shooter Player")]
    public class ScriptableShooterPlayerEntity : ScriptableGameEntity {

        [SerializeField]
        List<ScriptableWeaponEntity> _equipWeapons;

        [SerializeField]
        float _accelerateSpeed;

        [SerializeField]
        float _turnSpeed;

        public override Entity CreateInstance() {
            var entity = new ShooterPlayerEntity(0, OriginalGameObject);

            entity.Tag = _tag;
            entity.AccelerateSpeed = _accelerateSpeed;
            entity.TurnSpeed = _turnSpeed;

            foreach(var weapon in _equipWeapons)
                if(weapon != null)
                    entity.Equip(weapon.CreateInstance() as WeaponEntity);

            return entity;
        }
    }
}