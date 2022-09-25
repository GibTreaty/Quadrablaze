using UIWidgets;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class OptionsUIManager : MonoBehaviour {

        [Header("Options")]
        public Tabs optionsTabs;
        public UIControlOptions uiControlList;
        public UIAudioOptions uiAudioOptions;
        public UIVideoOptions uiVideoOptions;
        public UIGameOptions uiGameOptions;
        public Button controlRemapperCalibrateButton;
        
        public void Initialize() {
            SetupOptionsMenuNavigation();
        }

        public void SetupOptionsMenuNavigation() {
            gameObject.SetActive(true);
            gameObject.SetActive(false);

            uiControlList.SetupMenuNavigation();
            uiAudioOptions.SetupMenuNavigation();
            uiVideoOptions.SetupMenuNavigation();
            uiGameOptions.SetupMenuNavigation();
        }
    }
}