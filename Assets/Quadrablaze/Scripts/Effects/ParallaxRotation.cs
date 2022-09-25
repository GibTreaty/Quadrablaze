using UnityEngine;

public class ParallaxRotation : MonoBehaviour {

	public Transform rotateTransform;
	public float speed;

	void LateUpdate() {
		rotateTransform.rotation = Quaternion.Slerp(rotateTransform.rotation, Quaternion.Euler(transform.position.z, 0, -transform.position.x), Time.deltaTime * speed);
		//rotateTransform.rotation = Quaternion.Euler(transform.position.z, 0, -transform.position.x);
	}
}