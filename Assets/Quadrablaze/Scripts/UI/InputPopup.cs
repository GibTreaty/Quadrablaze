using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Quadrablaze {
    public class InputPopup : MonoBehaviour {

        [SerializeField]
        Button _doneButton;

        [SerializeField]
        Button _cancelButton;

        [SerializeField]
        TMPro.TMP_InputField _textInput;

        [SerializeField]
        StringEvent _onNameSubmitted;

        [SerializeField]
        UnityEvent _onDone;

        [SerializeField]
        UnityEvent _onCancel;

        bool initialized = false;

        #region Properties
        public Button CancelButton {
            get { return _cancelButton; }
        }

        public Button DoneButton {
            get { return _doneButton; }
        }

        public UnityEvent OnCancel {
            get { return _onCancel; }
            private set { _onCancel = value; }
        }

        public UnityEvent OnDone {
            get { return _onDone; }
            private set { _onDone = value; }
        }

        public StringEvent OnNameSubmitted {
            get { return _onNameSubmitted; }
            private set { _onNameSubmitted = value; }
        }
        #endregion

        public void Initialize() {
            if(initialized) return;
            if(OnDone == null) OnDone = new UnityEvent();
            if(OnCancel == null) OnCancel = new UnityEvent();
            if(OnNameSubmitted == null) OnNameSubmitted = new StringEvent();

            _doneButton.onClick.AddListener(OnDone.Invoke);
            _cancelButton.onClick.AddListener(OnCancel.Invoke);

            OnDone.AddListener(NameSet);
            OnCancel.AddListener(Close);

            initialized = true;
        }

        public void Close() {
            SetText("");
            //gameObject.SetActive(false);
        }

        void NameSet() {
            Debug.Log("Name Set:" + _textInput.text);
            OnNameSubmitted.Invoke(_textInput.text);
            Close();
        }

        public void Open(string startText = "") {
            Initialize();
            SetText(startText);
            _textInput.Select();

            //gameObject.SetActive(true);
        }

        public void SetText(string text) {
            _textInput.text = text;
        }
    }
}