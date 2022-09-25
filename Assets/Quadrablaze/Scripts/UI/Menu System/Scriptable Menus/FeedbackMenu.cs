using UnityEngine;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Feedback")]
    public class FeedbackMenu : ScriptableMenu {

        public override void Close(MenuItem item, UIManager manager) {
            manager.SkipInputFrame = true;
            manager.onFeedbackPanelState.Invoke(false);

            base.Close(item, manager);
        }

        //public override string GetLowerMenuName(MenuItem item, UIManager manager) {
        //    return RoundManager.RoundInProgress ? "" : item.ParentMenuName;
        //}

        public override void Open(MenuItem item, UIManager manager) {
            base.Open(item, manager);

            manager.onFeedbackPanelState.Invoke(true);
        }
    }
}