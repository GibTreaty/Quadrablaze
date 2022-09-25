using UnityEngine;

public class DestroyOnDisable : MonoBehaviour {
    void OnDisable() {
        Destroy(gameObject);
    }
}