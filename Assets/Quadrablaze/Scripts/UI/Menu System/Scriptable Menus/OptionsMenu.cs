using UIWidgets;
using UnityEngine;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Options")]
    public class OptionsMenu : TabbedMenu {

        protected override void OnClose(MenuItem item, UIManager manager) {
            UIManager.Current.EnableKeybindings(false);
        }

        protected override void OnOpen(MenuItem item, UIManager manager) {
            UIManager.Current.EnableKeybindings(false);
        }

        protected override void OnChangedTabs(MenuItem item, UIManager manager, Tabs tabsComponent, Tab currentTab) {
            UIManager.Current.EnableKeybindings(false);
        }
    }
}