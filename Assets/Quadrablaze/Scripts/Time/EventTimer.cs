using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventTimer : Timer {

    public TimerEvent OnChangeTime;
    public UnityEvent OnElapsed;
    public UnityEvent OnLow;
    public UnityEvent OnHigh;

    public override float CurrentTime {
        set {
            if(CurrentTime == value) return;

            _currentTime = value;

            if(OnChangeTime != null) OnChangeTime.Invoke(CurrentTime);

            if(CurrentTime == 0) {
                if(OnLow != null) OnLow.Invoke();
            }
            else if(Length > 0 && CurrentTime == Length) {
                if(OnHigh != null) OnHigh.Invoke();
            }
        }
    }

    public EventTimer() { InitializeEvents(); }
    public EventTimer(float length) : base(length) { InitializeEvents(); }
    public EventTimer(float length, TimerDirection direction) : base(length, direction) { InitializeEvents(); }
    public EventTimer(Timer timer) {
        Active = timer.Active;
        AutoDisable = timer.AutoDisable;
        AutoReset = timer.AutoReset;
        CurrentTime = timer.CurrentTime;
        Direction = timer.Direction;
        Length = timer.Length;
        InitializeEvents();
    }

    void FireEvent() {
        if(OnElapsed != null) OnElapsed.Invoke();
    }

    void InitializeEvents() {
        if(OnChangeTime == null) OnChangeTime = new TimerEvent();
        if(OnElapsed == null) OnElapsed = new UnityEvent();
        if(OnLow == null) OnLow = new UnityEvent();
        if(OnHigh == null) OnHigh = new UnityEvent();
    }

    public override void Update(bool unscaledDeltaTime = false) {
        if(!Active) return;

        if(Direction == TimerDirection.Down) {
            if(CurrentTime > 0) {
                CurrentTime = Mathf.Max(CurrentTime - (unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime), 0);

                if(CurrentTime == 0) {
                    if(AutoReset) CurrentTime = Length;
                    if(AutoDisable) Active = false;

                    FireEvent();
                }
            }
            else {
                if(AutoReset) {
                    CurrentTime = Length;

                    FireEvent();
                }

                if(AutoDisable) Active = false;
            }
        }
        else {
            if(CurrentTime < Length) {
                CurrentTime = Mathf.Min(CurrentTime + (unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime), Length);

                if(CurrentTime == Length) {
                    if(AutoReset) CurrentTime = 0;
                    if(AutoDisable) Active = false;

                    FireEvent();
                }
            }
        }
    }

    [System.Serializable]
    public class TimerEvent : UnityEvent<float> { }
}