using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze {
    public class DifficultySelectionUI : MonoBehaviour {

        public static DifficultySelectionUI Current { get; private set; }

        [SerializeField]
        List<DifficultySelectionButton> _difficultyButtons;

        public List<DifficultySelectionButton> DifficultyButtons {
            get { return _difficultyButtons; }
            set { _difficultyButtons = value; }
        }

        void OnEnable() {
            Current = this;
        }

        void Start() {
            foreach(DifficultySelectionButton button in DifficultyButtons) {
                    switch(button.ButtonType) {
                        case DifficultySelectionButton.ButtonTypes.Close:
                            button.Button.onClick.AddListener(button.Close);
                            break;

                        case DifficultySelectionButton.ButtonTypes.NewGame:
                            button.Button.onClick.AddListener(button.NewGame);
                            break;
                    }
            }
        }
    }
}