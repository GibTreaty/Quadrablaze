using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze.Skills {
    public class UIAbilityBarIcon : MonoBehaviour {

        [SerializeField]
        Image _iconImage;

        [SerializeField]
        Slider _cooldownSlider;

        [SerializeField]
        TMPro.TMP_Text _cooldownTimerText;

        [SerializeField]
        SkillLayoutElement _element;

        [SerializeField]
        Color _availableIconColor = Color.white;

        [SerializeField]
        Color _unavailableIconColor = new Color(1, 1, 1, .4f);

        [SerializeField]
        AutoControllerGlyph[] _buttonGlyphs;

        #region Properties
        ICooldownTimer SourceCooldownTimer { get; set; }

        public SkillLayoutElement Element {
            get { return _element; }
            private set { _element = value; }
        }
        #endregion

        public void Initialize(SkillLayoutElement element) {
            if(SourceCooldownTimer != null)
                SourceCooldownTimer.CooldownTimer.OnChangeTime.RemoveListener(OnCooldownChange);

            if(element != null) {
                if(element.CurrentExecutor is ICooldownTimer cooldownTimer) {
                    SourceCooldownTimer = cooldownTimer;
                    SourceCooldownTimer.CooldownTimer.OnChangeTime.AddListener(OnCooldownChange);
                }   
                else {
                    SourceCooldownTimer = null;
                    return;
                }
            }
            else SourceCooldownTimer = null;

            Element = element;
            _iconImage.sprite = Element.OriginalLayoutElement.SkillIcon;

            foreach(var buttonGlyph in _buttonGlyphs) {
                buttonGlyph.ActionName = "";
                buttonGlyph.ManualUpdateGlyph();
            }
        }

        void OnCooldownChange(float time) {
            RefreshCooldown();
        }

        public void RefreshCooldown() {
            _iconImage.color = SourceCooldownTimer != null && SourceCooldownTimer.CooldownTimer.HasElapsed ? _availableIconColor : _unavailableIconColor;
            _cooldownSlider.normalizedValue = SourceCooldownTimer != null ? SourceCooldownTimer.CooldownTimer.NormalizedTime : 0;

            _cooldownTimerText.text = SourceCooldownTimer != null ?
                (SourceCooldownTimer.CooldownTimer.CurrentTime > 0 && SourceCooldownTimer.CooldownTimer.Active ? Mathf.Ceil(SourceCooldownTimer.CooldownTimer.CurrentTime).ToString() : "")
                : "";
        }

        public void UpdateActionGlyph(int skillIndex) {
            string skillId = "";
            //int skillIndex = UIManager.Current.abilityWheel.GetSkillIndex(Element.SkillId);

            //Debug.Log("UpdateActionGlyph SkillIndex = " + skillIndex);

            if(skillIndex > -1)
                skillId = "Action " + (skillIndex + 1);

            foreach(var buttonGlyph in _buttonGlyphs) {
                buttonGlyph.ActionName = skillId;

                if(skillId == "")
                    buttonGlyph.ClearGlyph();
                else
                    buttonGlyph.ManualUpdateGlyph();
            }
        }
    }
}