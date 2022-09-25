using UnityEngine;

namespace Quadrablaze.Skills {
    [System.Serializable]
    public class WeaponUpgradeList : ScriptableObject {

        [SerializeField]
        GameObject[] _weapons;

        public GameObject[] Weapons {
            get { return _weapons; }
            set { _weapons = value; }
        }

        public GameObject CreateNewWeapon(int index, bool startInactive = false) {
            if(index < 0 || index >= Weapons.Length) return null;

            GameObject weapon = Instantiate(Weapons[index]);

            weapon.name = weapon.name.Replace("(Clone)", "");

            if(startInactive) weapon.SetActive(false);

            return weapon;
        }
    }
}