using UnityEngine;

public class HealthLevel : Health {

    [SerializeField]
    float _startHealth = 1;

    [SerializeField]
    float _modifier = 1;

    #region Properties
    public float Modifier {
        get { return _modifier; }
        set { _modifier = value; }
    }

    public float ModifiedHealthValue {
        get { return Mathf.Max((int)(StartHealth * Modifier), 1); }
    }

    public float StartHealth {
        get { return _startHealth; }
        set { _startHealth = value; }
    }
    #endregion

    public override void Reset() {
        MaxValue = ModifiedHealthValue;

        base.Reset();
    }
}