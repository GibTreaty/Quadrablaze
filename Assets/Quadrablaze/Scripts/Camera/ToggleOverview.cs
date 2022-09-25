using UnityEngine;

namespace Quadrablaze {
    public class ToggleOverview {
        public static void SetOverviewStatus(bool enable) {
            GameManager.Current.OverviewCameraComponent.Status = enable;
        }

        public static void ToggleOverviewStatus() {
            GameManager.Current.OverviewCameraComponent.ToggleStatus();
        }
    }
}