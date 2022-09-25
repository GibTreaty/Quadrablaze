using UnityEngine;

public class RotateToVelocity : MonoBehaviour {

    [SerializeField]
    float _turnSpeed = 1;

    Vector3 _lastVelocity;

    #region Properties
    public Vector3 LastVelocity {
        get { return _lastVelocity; }
        set {
            if(value != Vector3.zero) _lastVelocity = value;
        }
    }

    Rigidbody RigidbodyComponent { get; set; }

    public float TurnSpeed {
        get { return _turnSpeed; }
        set { _turnSpeed = value; }
    }
    #endregion

    void Awake() {
        RigidbodyComponent = GetComponent<Rigidbody>();

        if(!RigidbodyComponent) {
            enabled = false;
            Debug.LogError("No Rigidbody found. Disabling GameObject.", this);
        }
    }

    void Update() {
        //LastVelocity = Vector3.Scale(RigidbodyComponent.velocity, new Vector3(1, 0, 1));
        var velocity = RigidbodyComponent.velocity;

        velocity.y = 0;
        LastVelocity = velocity;

        if(LastVelocity != Vector3.zero) {
            RigidbodyComponent.rotation = Quaternion.RotateTowards(RigidbodyComponent.rotation, Quaternion.LookRotation(LastVelocity), Time.deltaTime * TurnSpeed);
            //transform.forward = Vector3.MoveTowards(transform.forward, LastVelocity, Time.deltaTime * TurnSpeed);
        }
    }
}