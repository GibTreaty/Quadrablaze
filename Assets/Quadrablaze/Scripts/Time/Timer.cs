using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
[XmlInclude(typeof(EventTimer))]
public class Timer {

    [SerializeField]
    protected bool _active;

    [SerializeField]
    protected float _length;

    [SerializeField]
    protected float _currentTime;

    [SerializeField]
    protected TimerDirection _direction = TimerDirection.Down;

    [SerializeField]
    protected bool _autoReset;

    [SerializeField]
    protected bool _autoDisable;

    #region Properties
    public bool Active {
        get { return _active; }
        set { _active = value; }
    }

    public bool AutoDisable {
        get { return _autoDisable; }
        set { _autoDisable = value; }
    }

    public bool AutoReset {
        get { return _autoReset; }
        set { _autoReset = value; }
    }

    public virtual float CurrentTime {
        get { return _currentTime; }
        set { _currentTime = value; }
    }

    public TimerDirection Direction {
        get { return _direction; }
        set { _direction = value; }
    }

    public bool HasElapsed {
        get { return Direction == TimerDirection.Down ? CurrentTime == 0 : CurrentTime == Length; }
    }

    public float Length {
        get { return _length; }
        set { _length = value; }
    }

    public float NormalizedTime {
        get { return CurrentTime / Length; }
        set { CurrentTime = Mathf.LerpUnclamped(0, Length, value); }
    }
    #endregion

    public Timer() { }
    public Timer(float length) {
        _length = length;
    }
    public Timer(float length, TimerDirection direction) {
        _length = length;
        _direction = direction;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public void Reset() {
        Reset(false);
    }
    public void Reset(bool deactivate) {
        if(deactivate) Active = false;

        if(Direction == TimerDirection.Down) SetHigh();
        else SetLow();
    }

    public void SetHigh() {
        CurrentTime = Length;
    }

    public void SetLow() {
        CurrentTime = 0;
    }

    public void Start() {
        Start(false);
    }
    public void Start(bool reset) {
        if(reset) Reset();

        Active = true;
    }
    public void Start(float time) {
        Length = time;
        Reset();
        Active = true;
    }

    public void Stop() {
        Active = false;
    }

    public TimerData ToTimerData() {
        return new TimerData(this);
    }

    public virtual void Update(bool unscaledDeltaTime = false) {
        if(!Active) return;

        if(Direction == TimerDirection.Down) {
            if(CurrentTime > 0) {
                CurrentTime = Mathf.Max(CurrentTime - (unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime), 0);

                if(CurrentTime == 0) {
                    if(AutoReset) CurrentTime = Length;
                    if(AutoDisable) Active = false;
                }
            }
            else {
                if(AutoReset) CurrentTime = Length;
                if(AutoDisable) Active = false;
            }
        }
        else {
            if(CurrentTime < Length) {
                CurrentTime = Mathf.Min(CurrentTime + (unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime), Length);

                if(CurrentTime == Length) {
                    if(AutoReset) CurrentTime = 0;
                    if(AutoDisable) Active = false;
                }
            }
        }
    }

    public static bool operator ==(Timer a, Timer b) {
        return a.CurrentTime == b.CurrentTime;
    }
    public static bool operator !=(Timer a, Timer b) {
        return !(a.CurrentTime == b.CurrentTime);
    }
    public static bool operator <(Timer a, Timer b) {
        return a.CurrentTime < b.CurrentTime;
    }
    public static bool operator >(Timer a, Timer b) {
        return a.CurrentTime > b.CurrentTime;
    }

    public static bool operator ==(Timer a, float b) {
        return a.CurrentTime == b;
    }
    public static bool operator !=(Timer a, float b) {
        return !(a.CurrentTime == b);
    }
    public static bool operator <(Timer a, float b) {
        return a.CurrentTime < b;
    }
    public static bool operator >(Timer a, float b) {
        return a.CurrentTime > b;
    }

    public enum TimerDirection {
        Down,
        Up
    }
}

[System.Serializable]
public struct TimerData {
    public bool active;
    public float length;
    public float currentTime;
    //public Timer.TimerDirection direction;
    public bool autoReset;
    public bool autoDisable;

    public TimerData(Timer timer) {
        active = timer.Active;
        length = timer.Length;
        currentTime = timer.CurrentTime;
        //direction = timer.Direction;
        autoReset = timer.AutoReset;
        autoDisable = timer.AutoDisable;
    }
}