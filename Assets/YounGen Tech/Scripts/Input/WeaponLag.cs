using UnityEngine;
using System.Collections;

namespace YounGenTech {
	[AddComponentMenu("YounGen Tech/Scripts/Input/Weapon Lag")]
	public class WeaponLag : MonoBehaviour {

		public float magnitude = 1;
		public float clampMagnitude = .5f;
		public float time = 1;
		Vector3 speed { get; set; }
		Vector3 startPosition { get; set; }
		Vector3 position = Vector3.zero;

		public Vector3 bobMultiplier = Vector3.one * 2;
		Vector3 bobOldPos;
		Vector3 bobPos;
		public Vector3 bobAngle;

		void Awake() {
			startPosition = transform.localPosition;
			position = startPosition;
			bobOldPos = transform.position + transform.forward;
		}

		void Update() {
			Vector3 velocity = transform.position - bobOldPos;
			Vector3 localVelocity = transform.InverseTransformDirection(velocity);

			speed = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * magnitude * Time.deltaTime;
			speed = Vector3.ClampMagnitude(speed, magnitude);
			speed = Vector3.Lerp(speed, Vector3.zero, Time.deltaTime);

			bobPos += localVelocity;
			bobAngle = new Vector3(Mathf.Sin(bobPos.x), Mathf.Cos(bobPos.y), Mathf.Sin(bobPos.z));

			position.x += speed.x;
			position.y += speed.y;

			position = Vector3.Slerp(position, startPosition + Vector3.Scale(bobMultiplier, bobAngle), Time.deltaTime * time);
			transform.localPosition = Vector3.ClampMagnitude(Vector3.Lerp(startPosition, position, Time.deltaTime * time) - startPosition, clampMagnitude) + startPosition;
		}

		void LateUpdate() {
			bobOldPos = transform.position;
		}
	}
}