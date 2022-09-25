using UnityEngine;

namespace YounGenTech {
	[AddComponentMenu("YounGen Tech/Scripts/Physics/Physics Test")]
	public class PhysicsTest : MonoBehaviour {

		public Rigidbody push;

		public Vector3 velocity;
		public Vector3 force;
		public float drag;

		void Awake() {
			push.drag = drag;
			push.velocity = force;
			velocity = force;
		}

		void FixedUpdate() {
			velocity = velocity.SimulateVelocity(Vector3.zero, drag);
			transform.Translate(velocity * Time.deltaTime);
		}
	}
}