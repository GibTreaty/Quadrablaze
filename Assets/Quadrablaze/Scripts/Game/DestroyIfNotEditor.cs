using UnityEngine;

public class DestroyIfNotEditor : MonoBehaviour {
    public bool destroy = true;

    void Awake() {
        if(destroy)
            if(!Application.isEditor) Destroy(gameObject);
    }
}