using UnityEngine;

public class TestTriangles : MonoBehaviour {

    public Transform pointA;
    public Transform pointB;
    public Transform pointC;

    public Transform forceField;
    public Transform hook1;
    public Transform hook2;

    [ContextMenu("Move")]
    public void Move() {
        GetComponent<Rigidbody>().position += new Vector3(0,0,5);
    }

    void LateUpdate() {
        Vector3 scale = forceField.localScale;

        scale.x = Vector3.Distance(hook1.position, hook2.position);

        forceField.localScale = scale;
    }

    void DoMath() {
        //Vector3 localPosition = position - center;

        Vector3 a = pointA.position;
        Vector3 b = pointB.position;
        Vector3 c = pointC.position;

        Vector3 center = (a + b + c) / 3;

        Vector3 directionA = a - center;
        float bSide = Mathf.Sign(Vector3.Dot(Quaternion.Euler(0, 90, 0) * directionA, b - a));

        Vector3 directionB = Quaternion.Euler(0, 120 * bSide, 0) * directionA;
        Vector3 directionC = Quaternion.Euler(0, 120 * -bSide, 0) * directionA;

        Gizmos.DrawWireSphere(center, .25f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(center, directionA.normalized);
        Gizmos.DrawWireSphere(center + directionA.normalized * 3, .25f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(center, directionB.normalized);
        Gizmos.DrawWireSphere(center + directionB.normalized * 3, .25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(center, directionC.normalized);
        Gizmos.DrawWireSphere(center + directionC.normalized * 3, .25f);
        //Vector3 equilateralA = dire
    }

    //void OnDrawGizmos() {
    //    DoMath();
    //}
}