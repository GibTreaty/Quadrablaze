using Quadrablaze.SkillExecutors;
using Quadrablaze.WeaponSystem;
using UnityEngine;

namespace Quadrablaze.Skills {
    public class WeaponPropertyListUI : MonoBehaviour {

        [SerializeField]
        WeaponPropertyButton[] _buttons;

        public int ButtonCount {
            get { return _buttons.Length; }
        }

        public void SetWeaponProperties(SkillExecutors.Weapon weaponExecutor) {
            Clear();

            if(weaponExecutor == null) return;

            var weaponList = weaponExecutor.OriginalSkillExecutor.List;
            var weaponLevel = weaponExecutor.CurrentLayoutElement.CurrentLevel;
            var scriptableWeapon = weaponList.Weapons[weaponLevel - 1];

            var details = scriptableWeapon.GenerateWeaponDetails();

            for(int i = 0; i < ButtonCount; i++) {
                if(i < details.Count) {
                    (string name, string value) info = details[i];

                    _buttons[i].SetText(info.name, info.value);
                }
                else
                    _buttons[i].ClearText();
            }
        }

        public void Clear() {
            foreach(var button in _buttons)
                button.ClearText();
        }
    }
}