using UnityEngine;

public class RotateTransform : MonoBehaviour {

    public void PointAt(Vector3 point) {
        PointInDirection(point - transform.position);
    }
    public void PointInDirection(Vector3 direction) {
        direction.y = 0;

        if(direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    public void SetRotation(Vector3 eulerAngles) {
        transform.eulerAngles = eulerAngles;
    }

    public void SetLocalRotation(Vector3 eulerAngles) {
        transform.localEulerAngles = eulerAngles;
    }

    public void Rotate(Vector3 eulerAngles) {
        transform.Rotate(eulerAngles);
    }

    public void RotateLocal(Vector3 eulerAngles) {
        transform.Rotate(eulerAngles, Space.Self);
    }
}