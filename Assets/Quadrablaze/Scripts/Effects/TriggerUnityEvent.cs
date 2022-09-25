using UnityEngine;
using UnityEngine.Networking;

public class TriggerUnityEvent : MonoBehaviour {

    [SerializeField]
    LayerMask _triggerMask = -1;

    [SerializeField]
    float _delay;

    [SerializeField]
    bool _onEnter = true, _onStay = true, _onExit = true;

    public ColliderEvent onTrigger;
    public ColliderEvent onTriggerExit;

    float _timer;
    bool triggered;

    #region Properties
    public float Delay {
        get { return _delay; }
        set { _delay = value; }
    }

    public float NormalizedTime {
        get { return Timer / Delay; }
    }

    public float Timer {
        get { return _timer; }
        set { _timer = value; }
    }

    public LayerMask TriggerMask {
        get { return _triggerMask; }
        set { _triggerMask = value; }
    }
    #endregion

    void Update() {
        if(!NetworkServer.active) return;

        if(triggered) {
            DelayDamageDelayTimer();

            triggered = false;
        }

        if(Timer > 0)
            Timer = Mathf.Max(Timer - Time.deltaTime, 0);
    }

    void OnTriggerEnter(Collider hit) {
        if(NetworkServer.active && _onEnter)
            OnHit(hit);
    }

    void OnTriggerExit(Collider hit) {
        if(NetworkServer.active && _onExit)
            OnExit(hit);
    }

    void OnTriggerStay(Collider hit) {
        if(NetworkServer.active && _onStay)
            OnHit(hit);
    }

    void OnHit(Collider hit) {
        if(!triggered)
            if((TriggerMask & (1 << hit.gameObject.layer)) > 0)
                if(Timer == 0) {
                    triggered = true;

                    if(onTrigger != null) onTrigger.Invoke(hit);
                }
    }

    void OnExit(Collider hit) {
        if((TriggerMask & (1 << hit.gameObject.layer)) > 0)
            if(onTriggerExit != null) onTriggerExit.Invoke(hit);
    }

    public void ResetDamageDelayTimer() {
        Timer = 0;
    }

    public void DelayDamageDelayTimer() {
        Timer = Delay;
    }

}