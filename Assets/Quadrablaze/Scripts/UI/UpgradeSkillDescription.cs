using UnityEngine;
using UnityEngine.EventSystems;

namespace Quadrablaze.Skills {
    public class UpgradeSkillDescription : MonoBehaviour, ISelectHandler, ISubmitHandler, IPointerEnterHandler {

        public UpgradeSkillPanel panel;
        public UISkillButton skillButton;

        public void DoUpgrade() {
            panel.Upgrade(this);
            panel.Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Select();

            skillButton.selectSkillButton.Select();
            EventSystem.current.SetSelectedGameObject(eventData.selectedObject);
        }

        public void OnSelect(BaseEventData eventData) {
            Select();
        }

        public void OnSubmit(BaseEventData eventData) {
            DoUpgrade();
        }

        public void Select() {
            panel.CurrentSkill = this;
        }

        public void ToggleSelect(bool enable) {
            panel.CurrentSkill = enable ? this : null;
        }
    }
}