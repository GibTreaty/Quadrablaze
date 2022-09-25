using System;
using UnityEngine;

public class TriggerBehaviour : MonoBehaviour {

    public event Action<Collider2D> OnTrigger2DEntered;
    public event Action<Collider> OnTrigger3DEntered;

    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.isTrigger)
            OnTrigger2DEntered?.Invoke(collider);
    }

    void OnTriggerEnter(Collider collider) {
        if(!collider.isTrigger)
            OnTrigger3DEntered?.Invoke(collider);
    }
}