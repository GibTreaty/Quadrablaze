using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Quadrablaze.SkillExecutors;

namespace Quadrablaze.Skills {
    public class UpgradeSkillPanel : MonoBehaviour {

        [SerializeField]
        UpgradeSkillDescription _currentSkill;

        public TextMeshProUGUI upgradeTitle;

        public TextMeshProUGUI inputDescription;
        public TextMeshProUGUI currentUpgradeDescription;
        public TextMeshProUGUI nextUpgradeDescription;

        public AutoControllerGlyph[] skillButtonGlyphs;

        #region Properties
        public UpgradeSkillDescription CurrentSkill {
            get { return _currentSkill; }
            set {
                _currentSkill = value;

                SetupInfo();
            }
        }
        #endregion

        void OnEnable() {
            Refresh();

            if(RoundManager.RoundInProgress)
                if(CurrentSkill) {
                    CurrentSkill.skillButton.selectSkillButton.Select();
                    CurrentSkill.skillButton.selectSkillButton.OnSelect(null);
                }
                else {
                    UpgradeSkillDescription description;

                    try {
                        description = GetComponentsInChildren<UpgradeSkillDescription>()[0];
                    }
                    catch(System.IndexOutOfRangeException) {
                        Debug.LogError("Skills haven't been set to player :(");
                        return;
                    }

                    description.skillButton.selectSkillButton.Select();
                    description.skillButton.selectSkillButton.OnSelect(null);
                }
        }

        public void Refresh() {
            SetupInfo();
        }

        void SetupInfo() {
            var element = CurrentSkill ? CurrentSkill.skillButton.skillLayoutElement : null;

            if(element != null) {
                upgradeTitle.text = element.OriginalLayoutElement.DisplayName + (" <size=50%>" + element.CurrentLevel + "/" + element.OriginalLayoutElement.LevelCap + "</size>");
                currentUpgradeDescription.text = element.GetLevelDescription(element.CurrentLevel);
                nextUpgradeDescription.text = element.GetLevelDescription(element.CurrentLevel + 1);
            }
            else {
                upgradeTitle.text = "Upgrade";
                currentUpgradeDescription.text = "";
                nextUpgradeDescription.text = "";
            }

            // TODO Make this use the Ability Wheel

            //return;

            foreach(var buttonGlyph in skillButtonGlyphs) {
                if(element != null && !element.Passive && !(element.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor)) {
                    string skillId = "";
                    int skillIndex = UIManager.Current.abilityWheel.GetSkillIndex(element.OriginalLayoutElement.name);

                    if(skillIndex > -1)
                        skillId = "Action " + (skillIndex + 1);

                    buttonGlyph.ActionName = skillId;
                }

                buttonGlyph.ManualUpdateGlyph();
            }

            //foreach(var buttonGlyph in skillButtonGlyphs) {
            //    buttonGlyph.ActionName = (element != null && !element.Passive) ? element.SkillName : "";
            //    buttonGlyph.ManualUpdateGlyph();
            //}
        }

        public void UpgradeCurrent() {
            Upgrade(CurrentSkill);
        }

        public void Upgrade(UpgradeSkillDescription skill) {
            if(skill) {
                skill.skillButton.UpgradeSkill();
                Refresh();
            }
        }
    }
}