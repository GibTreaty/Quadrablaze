using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class GameNetworkSelectionUI : MonoBehaviour {

        public static GameNetworkSelectionUI Current { get; private set; }

        [SerializeField]
        CanvasGroup _playMenuCanvasGroup;     

        [SerializeField]
        List<GameNetworkSelectionButton> _gameNetworkButtons;

        public List<GameNetworkSelectionButton> GameNetworkButtons {
            get { return _gameNetworkButtons; }
            set { _gameNetworkButtons = value; }
        }

        void OnEnable() {
            Current = this;

            UpdateButtonHighlight();
        }

        public void Initialize() {
            foreach(var button in GameNetworkButtons) {
                button.Button.onClick.AddListener(button.SetGameNetworkConnectionType);
                button.Button.onClick.AddListener(UpdateButtonHighlight);
            }
        }

        public void UpdateButtonHighlight() {
            foreach(var button in GameNetworkButtons)
                button.Highlight(false, 0);
        }
    }
}