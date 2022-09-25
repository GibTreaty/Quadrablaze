using TMPro;
using UnityEngine;

public class Nametag : MonoBehaviour {

    [SerializeField]
    Canvas _nametagCanvas;

    [SerializeField]
    TMP_Text _nametagText;

    Quaternion _rotation;

    public Canvas NametagCanvas {
        get { return _nametagCanvas; }
    }

    public TMP_Text NametagText {
        get { return _nametagText; }
    }

    void Awake() {
        _rotation = transform.rotation;
    }

    public void SetNametagActive(bool enabled) {
        _nametagCanvas.gameObject.SetActive(enabled);
    }

    public void SetNametagText(string value) {
        _nametagText.text = value;
    }

    void LateUpdate() {
        transform.rotation = _rotation;
    }
}