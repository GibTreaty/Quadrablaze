using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YounGenTech.Entities.Weapon;
using Quadrablaze.SkillExecutors;

namespace Quadrablaze.Skills {
    public class WeaponDescriptionPanel : MonoBehaviour {

        [SerializeField]
        WeaponDescription _currentWeapon;

        [SerializeField]
        WeaponDescription _nextWeapon;

        [SerializeField]
        StringList _emptyWeapons;

        public TextMeshProUGUI currentWeaponName;
        public TextMeshProUGUI nextWeaponName;

        public WeaponPropertyListUI currentWeaponPropertyList;
        public WeaponPropertyListUI nextWeaponPropertyList;

        public AutoControllerGlyph[] weaponButtonGlyphs;

        public TextMeshProUGUI descriptionText;

        [SerializeField]
        GameObject fakeNextWeapon;

        #region Properties
        public WeaponDescription CurrentWeapon {
            get { return _currentWeapon; }
            set {
                _currentWeapon = value;

                SetupProperties();
            }
        }

        WeaponDescription NextWeapon {
            get { return _currentWeapon; }
            set { _nextWeapon = value; }
        }
        #endregion

        void OnEnable() {
            if(RoundManager.RoundInProgress)
                if(CurrentWeapon) {
                    CurrentWeapon.skillButton.toggleSkill.Select();
                    CurrentWeapon.skillButton.toggleSkill.OnSelect(null);
                }
                else {
                    var descriptions = GetComponentsInChildren<WeaponDescription>();
                    var description = descriptions[0];

                    description.skillButton.toggleSkill.Select();
                    description.skillButton.toggleSkill.OnSelect(null);
                }

            foreach(var buttonGlyph in weaponButtonGlyphs)
                buttonGlyph.ManualUpdateGlyph();

            OnGlyphChanged();
        }

        void Awake() {
            foreach(var buttonGlyph in weaponButtonGlyphs)
                buttonGlyph.OnGlyphChanged.AddListener(s => OnGlyphChanged());
        }

        void Start() {
            OnGlyphChanged();
        }

        string GetRandomName() {
            return _emptyWeapons.GetRandom((System.DateTime.UtcNow.Hour + 1) * System.DateTime.UtcNow.Day * System.DateTime.UtcNow.Month);
        }

        void OnGlyphChanged() {
            bool allNull = true;

            foreach(var buttonGlyph in weaponButtonGlyphs)
                if(buttonGlyph.Glyph) {
                    allNull = false;
                    break;
                }

            descriptionText.text = allNull ? "'Submit' button not assigned" : "to upgrade";
        }

        public void Refresh() {
            SetupProperties();
        }

        void SetupProperties() {
            SetCurrentWeaponDefaultName();
            currentWeaponPropertyList.Clear();

            Destroy(fakeNextWeapon);
            SetNextWeaponDefaultName();
            nextWeaponPropertyList.SetWeaponProperties(null);

            if(!CurrentWeapon) return;

            var element = CurrentWeapon.skillButton.skillLayoutElement;
            var playerInfo = CurrentWeapon.skillButton.playerInfo;

            if(playerInfo)
                if(element.CurrentExecutor is Weapon executor) {
                    var scriptableWeapon = executor.OriginalSkillExecutor.List.Weapons[element.CurrentLevel - 1];

                    currentWeaponName.text = $"{scriptableWeapon.name} <size=50%>{element.CurrentLevel}/{Mathf.Min(element.OriginalLayoutElement.LevelCap, executor.OriginalSkillExecutor.List.Weapons.Length)}</size>";
                    currentWeaponPropertyList.SetWeaponProperties(executor);
                }

            SetupNextWeapon();
        }

        void SetupNextWeapon() {
            if(CurrentWeapon) {
                var element = CurrentWeapon.skillButton.skillLayoutElement;
                var playerUpgradeSet = CurrentWeapon.skillButton.playerInfo.AttachedEntity.CurrentUpgradeSet;

                if(element.CurrentExecutor is Weapon executor)
                    if(element.CurrentLevel < element.OriginalLayoutElement.LevelCap) {
                        var scriptableWeapon = executor.OriginalSkillExecutor.List.Weapons[element.CurrentLevel - 1];

                        nextWeaponName.text = scriptableWeapon.name;
                        nextWeaponPropertyList.SetWeaponProperties(executor);
                    }
            }
        }

        void SetCurrentWeaponDefaultName() {
            currentWeaponName.text = GetRandomName();
        }

        void SetNextWeaponDefaultName() {
            nextWeaponName.text = GetRandomName();
        }

        public void UpgradeCurrentWeapon() {
            UpgradeWeapon(CurrentWeapon);
        }

        public void UpgradeFully(int weaponIndex) {
            var weapon = GetComponentsInChildren<WeaponDescription>()[weaponIndex];

            if(weapon)
                while(weapon.skillButton.CanAcquire)
                    weapon.skillButton.UpgradeSkill();
        }

        public void UpgradeToLaser(int weaponIndex) {
            var weapon = GetComponentsInChildren<WeaponDescription>()[weaponIndex];

            if(weapon)
                while(weapon.skillButton.CanAcquire && weapon.skillButton.skillLayoutElement.CurrentLevel < 8)
                    weapon.skillButton.UpgradeSkill();
        }

        public void UpgradeWeapon(WeaponDescription weapon) {
            if(weapon) {
                weapon.skillButton.UpgradeSkill();
                Refresh();
            }
        }
    }
}