using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Manages your health and sends messages when certain health events happen
/// 
/// - To send a message to a GameObject you can use SendMessage or call them directly -
/// 
/// targetGameObject.SendMessage("Heal", new HealthEvent(gameObject, amount));
/// targetGameObject.SendMessage("Damage", new HealthEvent(gameObject, amount));
/// 
/// targetHealth.Heal(new HealthEvent(gameObject, amount));
/// targetHealth.Damage(new HealthEvent(gameObject, amount));
/// 
/// 
/// - These functions you can add to your script to receive the events -
/// 
/// void OnHealed(HealthEvent event) - Triggered on the object whose health has gone up
/// void OnCausedHeal(HealthEvent event) - Triggered on the object who has caused another object's health to go down
/// 
/// void OnDamaged(HealthEvent event) - Triggered on the object whose health has gone down
/// void OnCausedDamage(HealthEvent event) - Triggered on the object who has caused another object's health to go up
/// 
/// void OnDeath(HealthEvent event) - Triggered on the object whose health has gone on or below zero
/// void OnCausedDeath(HealthEvent event) - Triggered on the object who has caused another object's health to be at or below zero
/// </summary>
[AddComponentMenu("Health/Health")]
public class Health : MonoBehaviour, IStat {

#if UNITY_EDITOR
    float editor_value;
#endif
    [SerializeField]
    float _value = 100;
    public float Value {
        get { return _value; }
        set {
            if(disableHealthChange) return;

            float oldValue = _value;

            _value = capHealth ? Mathf.Clamp(value, 0, _maxValue) : value;

            if(!Mathf.Approximately(_value, oldValue)) {
                if(OnChangedHealth != null) OnChangedHealth.Invoke(_value);
            }
        }
    }

#if UNITY_EDITOR
    float editor_maxValue;
#endif
    [SerializeField]
    float _maxValue = 100;
    public float MaxValue {
        get { return _maxValue; }
        set {
            float oldValue = _maxValue;

            _maxValue = Mathf.Max(value, 0);

            if(!Mathf.Approximately(_maxValue, oldValue)) {
                if(OnChangedMaxHealth != null) OnChangedMaxHealth.Invoke(_maxValue);

                if(capHealth)
                    Value = Mathf.Clamp(Value, 0, _maxValue);
            }
        }
    }

    public float NormalizedValue {
        get { return Mathf.InverseLerp(0, MaxValue, Value); }
        set { Value = Mathf.Lerp(0, MaxValue, value); }
    }

    [Tooltip("Don't let the health go lower than 0 or higher than the max health")]
    public bool capHealth = true;

    /// <summary>
    /// Disables health being changed from the Heal, Damage and ChangeHealth functions
    /// </summary>
    public bool disableHealthChange = false;

    [SerializeField, Tooltip("Disable damage")]
    bool _invincible;
    public bool Invincible {
        get { return _invincible; }
        set { _invincible = value; }
    }

    [SerializeField, Tooltip("Disable healing")]
    bool _incurable;
    public bool Incurable {
        get { return _incurable; }
        set { _incurable = value; }
    }

    public HealthChangeEvent OnChangedHealth;
    public HealthChangeEvent OnChangedMaxHealth;

    [Header("Received Health Events")]
    [Tooltip("Health went up")]
    public UnityHealthEvent OnHealed;
    [Tooltip("Health is at or above max")]
    public UnityHealthEvent OnRestored;
    [Tooltip("Health went down")]
    public UnityHealthEvent OnDamaged;
    [Tooltip("Health is at or below zero")]
    public UnityHealthEvent OnDeath;

    [Header("Caused Health Events")]
    public UnityHealthEvent OnCausedHeal;
    public UnityHealthEvent OnCausedRestoration;
    public UnityHealthEvent OnCausedDamage;
    public UnityHealthEvent OnCausedDeath;

    protected virtual void Awake() {
        if(OnChangedHealth != null) OnChangedHealth.Invoke(Value);
        if(OnChangedMaxHealth != null) OnChangedMaxHealth.Invoke(MaxValue);
    }

    public float ChangeHealth(HealthEvent healthEvent) {
        if(!disableHealthChange) {
            if(healthEvent.healthEventType == HealthEvent.EventType.Percent)
                healthEvent.amount *= MaxValue;

            float newHealth = Mathf.Clamp(Value + healthEvent.amount, 0, MaxValue);
            float healthChange = newHealth - Value;

            if(healthChange > 0) {
                if(Incurable) return Value;
            }
            else if(healthChange < 0) {
                if(Invincible) return Value;
            }
            else return Value;

            Value = newHealth;

            HealthEvent received = new HealthEvent(healthEvent.originGameObject, gameObject, healthChange, healthEvent.description);
            HealthEvent caused = new HealthEvent(gameObject, healthEvent.originGameObject, healthChange, healthEvent.description);

            //Quadrablaze.HealthProxy.OnChangedHealth.Invoke(received);

            if(healthChange > 0) { //Healed
                if(OnHealed != null) OnHealed.Invoke(received);

                //Quadrablaze.HealthProxy.OnHealed.Invoke(received);

                if(healthEvent.originGameObject)
                    if(healthEvent.originGameObject.GetComponentInParent<Health>() is Health causedEvent)
                        causedEvent.OnCausedHeal?.Invoke(caused);
            }
            else if(healthChange < 0) { //Damaged
                if(OnDamaged != null) OnDamaged.Invoke(received);

                //Quadrablaze.HealthProxy.OnDamaged.Invoke(received);

                if(healthEvent.originGameObject)
                    if(healthEvent.originGameObject.GetComponentInParent<Health>() is Health causedEvent)
                        causedEvent.OnCausedDamage?.Invoke(caused);
            }

            if(Value <= 0) {
                if(healthChange < 0) { //Death
                    if(OnDeath != null) OnDeath.Invoke(received);

                    //Quadrablaze.HealthProxy.OnDeath.Invoke(received);

                    if(healthEvent.originGameObject)
                        if(healthEvent.originGameObject.GetComponentInParent<Health>() is Health causedEvent)
                            causedEvent.OnCausedDeath?.Invoke(caused);
                }
            }
            else if(Value >= MaxValue) { //Restored
                if(healthChange > 0) {
                    if(OnRestored != null) OnRestored.Invoke(received);

                    //Quadrablaze.HealthProxy.OnRestored.Invoke(received);

                    if(healthEvent.originGameObject)
                        if(healthEvent.originGameObject.GetComponentInParent<Health>() is Health causedEvent)
                            causedEvent.OnCausedRestoration?.Invoke(caused);
                }
            }
        }

        return Value;
    }

    /// <summary>
    /// Subtracts health
    /// </summary>
    /// <param name="healthEvent"></param>
    public bool Damage(HealthEvent healthEvent) {
        if(!disableHealthChange) {
            //healthEvent.amount = -Mathf.Abs(healthEvent.amount);
            if(healthEvent.amount > 0) healthEvent.amount = -healthEvent.amount;

            return Value > ChangeHealth(healthEvent);
        }

        return false;
    }

    /// <summary>
    /// Adds health
    /// </summary>
    /// <param name="healthEvent"></param>
    public bool Heal(HealthEvent healthEvent) {
        if(!disableHealthChange) {
            //healthEvent.amount = Mathf.Abs(healthEvent.amount);
            if(healthEvent.amount < 0) healthEvent.amount = -healthEvent.amount;

            return Value < ChangeHealth(healthEvent);
        }

        return false;
    }

    /// <summary>
    /// Resets health
    /// </summary>
    public virtual void Reset() {
        //ChangeHealth(new HealthEvent(null, MaxValue));
        Value = MaxValue;
    }

    void OnValidate() {
#if UNITY_EDITOR
        if(editor_value != Value)
            if(OnChangedHealth != null) OnChangedHealth.Invoke(Value);

        if(editor_maxValue != MaxValue)
            if(OnChangedMaxHealth != null) OnChangedMaxHealth.Invoke(MaxValue);
#endif

        Value = _value;
        MaxValue = _maxValue;

#if UNITY_EDITOR
        editor_value = Value;
        editor_maxValue = MaxValue;
#endif
    }

    [Serializable]
    public class HealthChangeEvent : UnityEvent<float> { }
    [Serializable]
    public class UnityHealthEvent : UnityEvent<HealthEvent> { }
}

public struct HealthEvent {
    /// <summary>
    /// The GameObject that caused the health event to happen
    /// </summary>
    public GameObject originGameObject;

    /// <summary>
    /// The GameObject that was effected by this health event
    /// </summary>
    public GameObject targetGameObject;

    /// <summary>
    /// The amount of health to change
    /// </summary>
    public float amount;

    /// <summary>
    /// Describes the nature of the event
    /// </summary>
    public string description;

    public EventType healthEventType;

    public HealthEvent(GameObject originGameObject, float amount, EventType eventType = EventType.Normal) {
        this.originGameObject = originGameObject;
        targetGameObject = null;
        this.amount = amount;
        description = "";
        healthEventType = eventType;
    }
    public HealthEvent(GameObject originGameObject, float amount, string description, EventType eventType = EventType.Normal) {
        this.originGameObject = originGameObject;
        targetGameObject = null;
        this.amount = amount;
        this.description = description;
        healthEventType = eventType;
    }

    public HealthEvent(GameObject originGameObject, GameObject targetGameObject, float amount, EventType eventType = EventType.Normal) {
        this.originGameObject = originGameObject;
        this.targetGameObject = targetGameObject;
        this.amount = amount;
        description = "";
        healthEventType = eventType;
    }
    public HealthEvent(GameObject originGameObject, GameObject targetGameObject, float amount, string description, EventType eventType = EventType.Normal) {
        this.originGameObject = originGameObject;
        this.targetGameObject = targetGameObject;
        this.amount = amount;
        this.description = description;
        healthEventType = eventType;
    }

    public override string ToString() {
        string message = "";

        message += "Origin(" + (originGameObject ? originGameObject.name : "null") + ")";
        message += "\nEffected(" + (targetGameObject ? targetGameObject.name : "null") + ")";
        message += "\nAmount(" + amount + ")";

        if(!string.IsNullOrEmpty(description))
            message += "\n" + description;

        return message;
    }

    public enum EventType {
        Normal,
        Percent
    }
}