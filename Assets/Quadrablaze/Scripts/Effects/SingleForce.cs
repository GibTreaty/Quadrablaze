using UnityEngine;
using UnityEngine.Networking;

public class SingleForce : MonoBehaviour {

    public Vector3 force;
    public Vector3 angularForce;

    void Start() {
        //if(!NetworkServer.active) return;

        Push();
    }

    public void Push() {
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        if(rigidbody) {
            rigidbody.AddForce(force, ForceMode.Impulse);
            rigidbody.AddTorque(angularForce, ForceMode.Impulse);
        }
    }
}