using UnityEngine;

public class ShooterCamera : MonoBehaviour {

    [SerializeField]
    Transform _target;

    public Transform Target {
        get { return _target; }
        set { _target = value; }
    }

    public Vector3 TargetPosition { get; set; }

    public float radius = 1;
    public float speed = 1;
    public float smoothness = 1;

    void OnEnable() {
        TargetPosition = transform.position;
    }

    void FixedUpdate() {
        if(Target) {
            Vector3 tempTargetPosition = new Vector3(Target.position.x, 0, Target.position.z);
            Vector3 tempPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 direction = tempTargetPosition - tempPosition;
            float directionMagnitude = direction.magnitude;

            if(directionMagnitude > radius) {
                Vector3 radiusPosition = tempTargetPosition - direction.normalized * radius;
                Vector3 radiusDirection = radiusPosition - tempPosition;

                TargetPosition = transform.position + Vector3.ClampMagnitude(radiusDirection, 1) * Time.deltaTime * speed;
            }
        }

        transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * 60 * smoothness);
    }

    //void OnDrawGizmos() {
    //    Gizmos.DrawWireSphere(transform.position, radius);

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(new Vector3(transform.position.x, 0, transform.position.z), TargetPosition);
    //}

    public void InstantMove() {
        if(Target)
            transform.position = new Vector3(Target.position.x, 0, Target.position.z);

        TargetPosition = transform.position;
    }
    public void InstantMove(Vector3 position) {
        transform.position = position;
        TargetPosition = position;
    }
}