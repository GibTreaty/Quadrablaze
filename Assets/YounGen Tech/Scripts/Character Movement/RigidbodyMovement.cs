using UnityEngine;
using System.Collections;

namespace YounGenTech {
	[RequireComponent(typeof(Rigidbody)), AddComponentMenu("YounGen Tech/Scripts/Character Movement/Rigidbody Movement")]
	public class RigidbodyMovement : MonoBehaviour {

		public Vector3 movementSpeed = Vector3.one;
		public float maxSpeed = 20;
		Vector3 input = Vector3.zero;
		Vector3 moveDir = Vector3.zero;

		public bool canJump = false;
		public float jumpPressed = 0;
		public float jumpForce = 1;

		void Update() {
			input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			jumpPressed = canJump ? Input.GetAxis("Jump") : 0;

			if(!canJump) {
				moveDir = Vector3.Lerp(moveDir, Vector3.zero, Time.deltaTime * 2);
			}

			moveDir = Vector3.Lerp(moveDir, transform.TransformDirection(Vector3.Scale(Vector3.ClampMagnitude(input, 1), movementSpeed)), Time.deltaTime * 4);

			float speed = GetComponent<Rigidbody>().velocity.magnitude;
			if(speed > maxSpeed) {
				moveDir.x /= speed;//maxSpeed;
				moveDir.z /= speed;//maxSpeed;
			}
		}

		void FixedUpdate() {
			GetComponent<Rigidbody>().AddForce(moveDir, ForceMode.Force);
			GetComponent<Rigidbody>().AddRelativeForce(0, jumpPressed * jumpForce, 0, ForceMode.Impulse);

			canJump = false;
		}

		void OnCollisionStay(Collision hit) {
			bool hitAngle = false;

			foreach(ContactPoint a in hit.contacts) {
				if(a.normal.y > .7f) { hitAngle = true; break; }
			}

			if(hitAngle) {
				canJump = true;
			}
		}
	}
}