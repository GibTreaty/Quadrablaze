using UnityEngine;
using UnityEngine.Networking;

public class SingleRandomForce : MonoBehaviour {

    public Vector3 force;
    public Vector3 angularForce;

    void Start() {
        if(!NetworkServer.active) return;

        Rigidbody rigidbody = GetComponent<Rigidbody>();

        if(rigidbody) {
            Vector3 outputForce = new Vector3(Random.Range(-force.x, force.x), Random.Range(-force.y, force.y), Random.Range(-force.z, force.z));
            Vector3 outputAngularForce = new Vector3(Random.Range(-angularForce.x, angularForce.x), Random.Range(-angularForce.y, angularForce.y), Random.Range(-angularForce.z, angularForce.z));

            rigidbody.AddForce(outputForce, ForceMode.Impulse);
            rigidbody.AddTorque(outputAngularForce, ForceMode.Impulse);
        }
    }
}