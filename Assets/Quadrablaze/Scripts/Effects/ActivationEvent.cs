using UnityEngine;
using UnityEngine.Events;

public class ActivationEvent : MonoBehaviour {

    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    void OnEnable() {
        if(OnActivated != null) OnActivated.Invoke();
    }

    void OnDisable() {
        if(OnDeactivated != null) OnDeactivated.Invoke();
    }
}