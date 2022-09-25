using UnityEngine;
using UnityEngine.UI;

public class UIHighlight : MonoBehaviour {

    [SerializeField]
    GameObject _highlight;

    public void EnableHighlight(bool enable) {
        _highlight.SetActive(enable);
    }
}