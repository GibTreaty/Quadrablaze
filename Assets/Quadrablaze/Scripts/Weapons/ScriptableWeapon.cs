using UnityEngine;

namespace Quadrablaze.WeaponSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Weapons/Basic Weapon")]
    public class ScriptableWeapon : ScriptableObject {

        [SerializeField]
        protected AnimationCurve _shootCurve = AnimationCurve.Constant(0, 1, 1);

        [SerializeField]
        protected WeaponProperties _properties;

        public WeaponProperties Properties => _properties;

        public static implicit operator Weapon(ScriptableWeapon item) {
            var weapon = new Weapon(0, null);

            weapon.ShootCurve = item._shootCurve;
            weapon.CurrentWeaponProperties = item._properties;

            return weapon;
        }
    }
}