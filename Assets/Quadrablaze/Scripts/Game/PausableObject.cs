using System;
using Quadrablaze;
using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.Entities;

public class PausableObject : MonoBehaviour, IActorEntityObjectInitialize {

    public BooleanEvent onPauseChange;
    public BooleanEvent onPauseChangeInverted;

    bool subscribed;

    public void ActorEntityObjectInitialize(ActorEntity entity) {
        if(onPauseChange == null) onPauseChange = new BooleanEvent();
        if(onPauseChangeInverted == null) onPauseChangeInverted = new BooleanEvent();

        Subscribe();
    }

    void OnDisable() {
        Unsubscribe();
    }

    void Subscribe() {
        if(!subscribed && PauseManager.Current) {
            PauseManager.Current.Subscribe(onPauseChange.Invoke);
            PauseManager.Current.Subscribe(PauseInverted);

            subscribed = true;
        }
    }

    void Unsubscribe() {
        if(PauseManager.Current) {
            PauseManager.Current.onPauseChange.RemoveListener(onPauseChange.Invoke);
            PauseManager.Current.onPauseChange.RemoveListener(PauseInverted);
        }

        subscribed = false;
    }

    void PauseInverted(bool paused) {
        onPauseChangeInverted.Invoke(!paused);
    }
}