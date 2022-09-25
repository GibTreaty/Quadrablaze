using UnityEngine;
using UnityEngine.Events;

public class CollisionTrigger : MonoBehaviour {

    [SerializeField]
    bool _triggerActive;

    [SerializeField]
    LayerMask _collisionMask = -1;

    [SerializeField]
    EventTimer _skillTimer = new EventTimer(1);

    public CollisionEvent OnCollision = new CollisionEvent();

    #region Properties
    public LayerMask CollisionMask {
        get { return _collisionMask; }
        set { _collisionMask = value; }
    }

    public EventTimer SkillTimer {
        get { return _skillTimer; }
        set { _skillTimer = value; }
    }

    public bool TriggerActive {
        get { return _triggerActive; }
        set { _triggerActive = value; }
    }
    #endregion

    void Update() {
        SkillTimer.Update();
    }

    void OnCollisionEnter(Collision collision) {
        Invoke(collision);
    }

    void OnCollisionStay(Collision collision) {
        Invoke(collision);
    }

    void Invoke(Collision collision) {
        if(SkillTimer.HasElapsed)
            if(TriggerActive) {
                SkillTimer.Reset();

                if(OnCollision != null) OnCollision.Invoke(collision);
            }
    }

    [System.Serializable]
    public class CollisionEvent : UnityEvent<Collision> { }
}
