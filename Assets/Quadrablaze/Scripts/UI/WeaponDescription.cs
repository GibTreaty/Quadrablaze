using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Quadrablaze.Skills {
    public class WeaponDescription : MonoBehaviour, ISelectHandler, ISubmitHandler, IPointerEnterHandler, IPointerClickHandler {

        public WeaponDescriptionPanel panel;
        public UISkillButton skillButton;

        public void OnPointerEnter(PointerEventData eventData) {
            Select();

            skillButton.toggleSkill.Select();
            EventSystem.current.SetSelectedGameObject(eventData.selectedObject);
        }

        public void OnSelect(BaseEventData eventData) {
            Select();
        }

        public void OnSubmit(BaseEventData eventData) {
            panel.UpgradeWeapon(this);
        }

        public void Select() {
            panel.CurrentWeapon = this;
            
            var element = skillButton.skillLayoutElement;

            if(element == null || !(element.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor)) return;

            var weaponIndex = element.ElementTypeIndex;
            
            var pivot = WeaponUpgradeShipRenderController.Current.currentShipRenderObject.GetComponent<ShipInfoObject>().WeaponPivots[weaponIndex];
            var localWeaponPosition = pivot.position;
            var localWeaponRotation = pivot.rotation;
            
            WeaponUpgradeShipRenderController.Current.MoveIconToWeapon(localWeaponPosition, localWeaponRotation);
        }

        public void ToggleSelect(bool enable) {
            panel.CurrentWeapon = enable ? this : null;
        }

        public void OnPointerClick(PointerEventData eventData) {
            panel.UpgradeWeapon(this);
        }
    }
}