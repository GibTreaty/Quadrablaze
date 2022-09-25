using UnityEngine;
using System.Collections.Generic;

public class ColliderList : MonoBehaviour {

    [SerializeField]
    List<Collider> _colliders;

    public List<Collider> Colliders {
        get { return _colliders; }
    }

    [ContextMenu("Get All Colliders")]
    void GetColliders() {
        _colliders = new List<Collider>(GetComponentsInChildren<Collider>());
    }
}