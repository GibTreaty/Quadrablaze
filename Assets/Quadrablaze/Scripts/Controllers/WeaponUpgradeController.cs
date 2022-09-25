using UnityEngine;

namespace Quadrablaze.Skills {
    public class WeaponUpgradeController : MonoBehaviour {

        [SerializeField]
        WeaponUpgradeList _upgradeList;

        #region Properties
        public int MaxLevel {
            get { return UpgradeList ? UpgradeList.Weapons.Length : 0; }
        }

        public WeaponUpgradeList UpgradeList {
            get { return _upgradeList; }
            set { _upgradeList = value; }
        }
        #endregion

        public GameObject CreateWeapon(int level, bool startInactive = false) {
            return level > 0 && level <= MaxLevel ? UpgradeList.CreateNewWeapon(level - 1, startInactive) : null;
        }
    }
}