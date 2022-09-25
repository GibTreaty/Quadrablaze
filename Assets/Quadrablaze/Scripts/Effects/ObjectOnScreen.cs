using UnityEngine;

public class ObjectOnScreen : MonoBehaviour {

    public static GameObjectEvent OnObjectEnteredScreen = new GameObjectEvent();
    public static GameObjectEvent OnObjectExitedScreen = new GameObjectEvent();

    [SerializeField]
    bool _updateBoundsEachFrame;

    bool _onScreen;
    Bounds _bounds;

    #region Properties
    public bool OnScreen {
        get { return _onScreen; }
        private set {
            if(_onScreen == value) return;

            _onScreen = value;

            CallVisibilityEvent(OnScreen);
        }
    }
    #endregion

    void OnEnable() {
        CalculateBounds();
        _onScreen = IsVisible();

        CallVisibilityEvent(_onScreen);
    }

    void OnDisable() {
        OnScreen = false;
    }

    //void Awake() {
    //    CalculateBounds();
    //}

    //void Start() {
    //    CallVisibilityEvent(IsVisible());
    //    Debug.Log(IsVisible());
    //}

    void Update() {
        if(_updateBoundsEachFrame) CalculateBounds();
        else _bounds.center = transform.position;

        OnScreen = IsVisible();
    }

    //void OnDrawGizmos() {
    //    if(Application.isPlaying)
    //        Gizmos.DrawWireCube(_bounds.center, _bounds.size);
    //}

    void CalculateBounds() {
        _bounds = new Bounds(transform.position, Vector3.zero);

        foreach(Renderer renderer in GetComponentsInChildren<Renderer>())
            _bounds.Encapsulate(renderer.bounds);
    }

    void CallVisibilityEvent(bool visible) {
        if(visible) {
            if(OnObjectEnteredScreen != null) OnObjectEnteredScreen.Invoke(gameObject);
        }
        else {
            if(OnObjectExitedScreen != null) OnObjectExitedScreen.Invoke(gameObject);
        }
    }

    bool IsVisible() {
        return CameraFrustumPlanes.Current && CameraFrustumPlanes.Current.TestAABB(_bounds) && gameObject.activeInHierarchy;
    }
}