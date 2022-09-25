using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Quadrablaze.Skills {
    public class WeaponPropertyButton : MonoBehaviour {

        [SerializeField]
        TextMeshProUGUI _nameText;

        [SerializeField]
        TextMeshProUGUI _valueText;

        public TextMeshProUGUI NameText {
            get { return _nameText; }
        }

        public TextMeshProUGUI ValueText {
            get { return _valueText; }
        }

        void Awake() {
            ClearText();
        }

        public void SetText(string name, string value) {
            NameText.text = name;
            ValueText.text = value;
        }

        public void SetText(WeaponProperties.Properties property) {
            SetText(property.DisplayName, property.Value);
        }

        public void ClearText() {
            NameText.text = "";
            ValueText.text = "";
        }
    }
}