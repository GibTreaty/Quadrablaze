using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Weapon")]
    public class ScriptableWeaponEntity : ScriptableEntity {

        [SerializeField]
        AnimationCurve _shootCurve = AnimationCurve.Constant(0, 1, 1);

        [SerializeField]
        WeaponProperties _properties;

        public override Entity CreateInstance() {
            var entity = new WeaponEntity(0, OriginalGameObject);

            entity.ShootCurve = _shootCurve;
            entity.CurrentWeaponProperties = _properties;

            return entity;
        }
    }
}