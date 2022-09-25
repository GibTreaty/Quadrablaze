using UnityEngine;

public class InvincibilityTime : MonoBehaviour {

    [SerializeField]
    float _timer;

    #region Properties
    Health HealthComponent { get; set; }

    public float Timer {
        get { return _timer; }
        set {
            _timer = value;
            HealthComponent.Invincible = Timer > 0;
        }
    }
    #endregion

    void Awake() {
        HealthComponent = GetComponent<Health>();
    }

    void Update() {
        if(Timer > 0)
            Timer = Mathf.Max(Timer - Time.deltaTime, 0);
    }

    public void SetTimeToMax(float time) {
        Timer = Mathf.Max(Timer, time);
    }
}