using UnityEngine;

public class TimedDestroy : MonoBehaviour {

    public float time;
    public bool detachChildren;
    public bool cancelOnDisable;

    void OnEnable() {
        StartTimedDestroy(time);
    }

    void OnDisable() {
        if(cancelOnDisable) StopTimedDestroy();
    }

    public void StartTimedDestroy(float time) {
        Invoke("DoDestroy", time);
    }
    void DoDestroy() {
        if(detachChildren) transform.DetachChildren();

        Destroy(gameObject);
    }
    public void StopTimedDestroy() {
        CancelInvoke("DoDestroy");
    }
}