using UIWidgets;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Tabbed")]
    public class TabbedMenu : ScriptableMenu {

        public virtual bool CanChangeTabs(MenuItem item, UIManager manager) {
            return true;
        }

        public virtual void ChangeTab(TabDirection direction, MenuItem item, UIManager manager) {
            if(item.MainGameObject) {
                var tabs = item.MainGameObject.GetComponentInChildren<Tabs>(true);

                switch(direction) {
                    case TabDirection.Left: tabs.SelectPreviousTab(false); break;
                    case TabDirection.Right: tabs.SelectNextTab(false); break;
                }

                var selected = EventSystem.current.currentSelectedGameObject;

                if(selected == null || !selected.activeInHierarchy)
                    SelectFirstElement(item, manager);

                OnChangedTabs(item, manager, tabs, tabs.SelectedTab);
            }
        }

        protected virtual void OnChangedTabs(MenuItem item, UIManager manager, Tabs tabsComponent, Tab currentTab) { }

        public enum TabDirection {
            Left,
            Right
        }
    }
}