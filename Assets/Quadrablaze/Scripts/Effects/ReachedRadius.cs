using UnityEngine;
using UnityEngine.Events;

public class ReachedRadius : MonoBehaviour {

    [SerializeField]
    float _radius = 1;

    [SerializeField]
    Vector3 _center;

    public UnityEvent onReachedRadius;

    #region Properties
    public Vector3 Center {
        get { return _center; }
        set { _center = value; }
    }

    public float Radius {
        get { return _radius; }
        set { _radius = value; }
    }
    #endregion

    void Update() {
        if((transform.position - Center).sqrMagnitude >= Radius * Radius)
            if(onReachedRadius != null) onReachedRadius.Invoke();
    }
}