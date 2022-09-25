using UnityEngine;

namespace YounGenTech {
	public static class PhysicsSimulator {
		/// <summary>
		/// Add force to velocity
		/// </summary>
		public static Vector3 AddForce(this Vector3 velocity, Vector3 force, float mass, ForceMode forceMode = ForceMode.Force) {
			switch(forceMode) {
				case ForceMode.Force:
					velocity += (force / mass) * Time.fixedDeltaTime; break;
				case ForceMode.Acceleration:
					velocity += force * Time.fixedDeltaTime; break;
				case ForceMode.Impulse:
					velocity += force / mass; break;
				case ForceMode.VelocityChange:
					velocity += force; break;
			}

			return velocity;
		}

		/// <summary>
		/// Get just the force without adding it to velocity
		/// </summary>
		public static Vector3 GetForce(this Vector3 force, float mass, ForceMode forceMode = ForceMode.Force) {
			switch(forceMode) {
				case ForceMode.Force:
					return (force / mass) * Time.fixedDeltaTime;
				case ForceMode.Acceleration:
					return force * Time.fixedDeltaTime;
				case ForceMode.Impulse:
					return force / mass;
				case ForceMode.VelocityChange:
					return force;
			}

			return Vector3.zero;
		}

		/// <summary>
		/// Moves a position based on acceleration and drag
		/// </summary>
		public static Vector3 SimulatePosition(this Vector3 position, Vector3 velocity, Vector3 acceleration, float drag = 0) {
			velocity += acceleration * Time.deltaTime;
			velocity -= velocity * drag * Time.deltaTime;

			return position + velocity * Time.deltaTime;
		}

		/// <summary>
		/// Applies acceleration and drag to the velocity
		/// </summary>
		public static Vector3 SimulateVelocity(this Vector3 velocity, Vector3 acceleration, float drag = 0) {
			velocity += acceleration * Time.deltaTime;
			velocity -= velocity * drag * Time.deltaTime;

			return velocity;
		}
	}
}