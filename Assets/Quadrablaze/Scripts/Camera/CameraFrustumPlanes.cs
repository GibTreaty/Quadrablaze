using UnityEngine;

public class CameraFrustumPlanes : MonoBehaviour {
    public static CameraFrustumPlanes Current { get; private set; }

    Plane[] _planes;
    Camera _camera;

    void Awake() {
        Current = this;

        _camera = GetComponent<Camera>();
    }

    void Update() {
        _planes = GeometryUtility.CalculateFrustumPlanes(_camera);
    }

    public bool TestAABB(Bounds bounds) {
        return GeometryUtility.TestPlanesAABB(_planes, bounds);
    }
}