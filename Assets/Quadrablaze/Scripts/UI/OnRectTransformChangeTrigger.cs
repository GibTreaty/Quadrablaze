using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnRectTransformChangeTrigger : UIBehaviour {

    [SerializeField]
    UnityEvent _onDimensionsChanged;

    public UnityEvent OnDimensionsChanged {
        get { return _onDimensionsChanged; }
        private set { _onDimensionsChanged = value; }
    }

    void Awake() {
        if(OnDimensionsChanged == null) OnDimensionsChanged = new UnityEvent();
    }

    protected override void OnRectTransformDimensionsChange() {
        if(OnDimensionsChanged != null) OnDimensionsChanged.Invoke();
    }
}