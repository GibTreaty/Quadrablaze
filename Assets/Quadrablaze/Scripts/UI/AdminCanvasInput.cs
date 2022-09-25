using UnityEngine;
using UnityEngine.UI;

public class AdminCanvasInput : MonoBehaviour {

    public Toggle toggle;

    void Update() {
        if(Input.GetKeyUp(KeyCode.BackQuote))
            toggle.isOn = !toggle.isOn;
    }
}