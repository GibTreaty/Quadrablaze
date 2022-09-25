using UnityEngine;

namespace Quadrablaze.WeaponSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Weapons/List")]
    public class WeaponList : ScriptableObject {

        [SerializeField]
        ScriptableWeapon[] _weapons;

        public ScriptableWeapon[] Weapons => _weapons;
    }
}