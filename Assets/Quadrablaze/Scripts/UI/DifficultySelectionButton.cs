using System;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class DifficultySelectionButton : MonoBehaviour {

        [SerializeField]
        ButtonTypes _buttonType;

        [SerializeField]
        Button _button;

        [SerializeField]
        Image _highlightImage;

        #region Properties
        public Button Button {
            get { return _button; }
            set { _button = value; }
        }

        public ButtonTypes ButtonType {
            get { return _buttonType; }
            set { _buttonType = value; }
        }

        public Image HighlightImage {
            get { return _highlightImage; }
            set { _highlightImage = value; }
        }
        #endregion

        public void Close() {
            UIManager.Current.GoToParentMenu();
        }

        public void Highlight(bool enable, float time) {
            if(HighlightImage) HighlightImage.CrossFadeAlpha(enable ? 1 : 0, time, true);
        }

        public void NewGame() {
            GameManager.Current.NewGame();
        }

        public enum ButtonTypes {
            NewGame=0,
            Close=1
        }
    }
}