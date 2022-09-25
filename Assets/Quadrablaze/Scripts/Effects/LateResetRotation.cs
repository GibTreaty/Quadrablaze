using UnityEngine;

public class LateResetRotation : MonoBehaviour {
    void LateUpdate() {
        transform.rotation = Quaternion.identity;
    }
}