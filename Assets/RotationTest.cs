using UnityEngine;

public class RotationTest : MonoBehaviour {

    public Vector2 inputAxis = new Vector2(0, 1);

    public Transform orientation;
    public Vector3 gravity = Physics.gravity;

    void OnDrawGizmos() {
        Vector3 axis = new Vector3(-inputAxis.y, 0, inputAxis.x);
        Quaternion rotation = orientation.rotation;
        //Vector3 direction = GetDirection(axis);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, GetDirection(new Vector3(-1, 0, 0)));

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GetDirection(new Vector3(0, 0, 1)));

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, gravity.normalized);
    }

    Vector3 GetDirection(Vector3 axis) {
        Quaternion rotation = orientation.rotation * Quaternion.LookRotation(Vector3.forward, gravity);

        return Quaternion.LookRotation(rotation * Vector3.right) * axis;
    }
}