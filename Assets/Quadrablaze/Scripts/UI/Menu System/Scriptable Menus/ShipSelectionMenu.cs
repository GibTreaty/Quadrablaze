using UnityEngine;
using UnityEngine.EventSystems;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Ship Selection")]
    public class ShipSelectionMenu : TabbedMenu {

        public override bool CanChangeTabs(MenuItem item, UIManager manager) {
            return !(manager.shipSelectionUIManager.ColorPickerPanel.activeSelf || manager.shipSelectionUIManager.MovementTypeListView.gameObject.activeSelf);
        }

        public override void ChangeTab(TabDirection direction, MenuItem item, UIManager manager) {
            switch(direction) {
                case TabDirection.Left: manager.shipSelectionUIManager.ChangeShipLeft(); break;
                case TabDirection.Right: manager.shipSelectionUIManager.ChangeShipRight(); break;
            }

            var selected = EventSystem.current.currentSelectedGameObject;

            if(selected == null || !selected.activeInHierarchy)
                SelectFirstElement(item, manager);
        }
    }
}