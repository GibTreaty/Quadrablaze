using UnityEngine;
using System;
using System.Collections;
using YounGenTech.ComponentInterface;

namespace YounGenTech.BulletPhysics {
	/// <summary>
	/// Super simple bullet physics
	/// 
	/// <b>Note: </b>Can't detect moving colliders if the collider moves past the bullet further than the bullet's raycast range. 
	/// This can be 'fixed' by making the bullet go faster. The longer the bullet's speed, the longer its raycast ray and the longer its detection range
	/// <hr />
	/// <h2>SendMessage Events</h2>
	/// 
	/// OnBulletCollided(<see cref="BulletHit"/> hit) - Called on the <see cref="Bullet"/>s GameObject when the <see cref="Bullet"/> collides<br>
	/// OnBulletSleep(<see cref="Bullet"/> bullet) - Called on the <see cref="Bullet"/>s GameObject when the <see cref="Bullet"/> goes to sleep<br>
	/// OnHitByBullet(<see cref="BulletHit"/> hit) - Called on the Collider (or the Collider's attached Rigidbody if it has one) when the <see cref="Bullet"/> hits it
	/// </summary>
	[AddComponentMenu("YounGen Tech/Bullet Physics/Bullet")]
	public class Bullet : MonoBehaviour {
		/// <summary>
		/// Total amount of <see cref="Bullet"/>s in the scene
		/// </summary>
		public static int bulletCount { get; private set; }

		/// <summary>
		/// Called when a <see cref="Bullet"/> hits a <see cref="Collider"/>. The parameters are the <see cref="BulletHit"/> hit information and the speed/magnitude
		/// </summary>
		public event Action<BulletHit, float> OnBulletCollided;

		/// <summary>
		/// Called when the bullet goes to sleep. Sleep happens when velocity.sqrMagnitude lower than (<see cref="Physics.sleepVelocity"/> * 2)
		/// </summary>
		public event Action<Bullet> OnBulletSleep;

		/// <summary>
		/// Called each FixedUpdate and adds <see cref="Time.deltaTime"/> onto its totalTime
		/// </summary>
		public event Action<Bullet> OnBulletAddTotalTime;

		/// <summary>
		/// Called when moving the <see cref="Bullet"/> and adds onto its totalDistance
		/// </summary>
		public event Action<Bullet> OnBulletAddTotalDistance;

		/// <summary>
		/// Current movement velocity
		/// </summary>
		public Vector3 velocity;
		/// <summary>
		/// A mysterious force
		/// </summary>
		public Vector3 gravity = Physics.gravity;

		/// <summary>
		/// Weight
		/// </summary>
		public float mass = 1;

		/// <summary>
		/// Slow-down amount
		/// </summary>
		public float drag;

		/// <summary>
		/// Ricochet/Bounce
		/// </summary>
		[Range(0, 1)]
		public float bounciness;
		/// <summary>
		/// How the bounce should be calculated
		/// </summary>
		public PhysicMaterialCombine bounceCombine = PhysicMaterialCombine.Average;

		/// <summary>
		/// Apply such a mysterious force?
		/// </summary>
		public bool useGravity;
		/// <summary>
		/// Automatically rotate along the forward velocity axis
		/// </summary>
		public bool useAutoRotation;

		/// <summary>
		/// Only collide with <see cref="Collider"/>s in these layers
		/// </summary>
		public LayerMask collisionMask = -1;

		/// <summary>
		/// When your bullet technically  gets "stuck" in a collider, have it parent to that transform
		/// </summary>
		public bool parentOnZeroVelocityHit;

		/// <summary>
		/// Is this <see cref="Bullet"/> sleeping?
		/// </summary>
		public bool isSleeping { get; private set; }
		/// <summary>
		/// Total distance traveled
		/// </summary>
		public float totalDistance { get; private set; }
		/// <summary>
		/// Total time existed
		/// </summary>
		public float totalTime { get; private set; }

		/// <summary>
		/// Max distance this <see cref="Bullet"/> can travel
		/// </summary>
		public float maxDistance;

		/// <summary>
		/// Max time this <see cref="Bullet"/> can live
		/// </summary>
		public float maxTime;

		/// <summary>
		/// The delay before this <see cref="Bullet"/> is destroyed upon reaching the maxDistance or maxTime
		/// </summary>
		public float destroyDelay;

		void OnEnable() {
			bulletCount++;
		}

		void OnDisable() {
			bulletCount--;
		}

		void FixedUpdate() {
			totalTime += Time.deltaTime;
			if(OnBulletAddTotalTime != null) OnBulletAddTotalTime(this);

			if(maxDistance > 0 && !float.IsInfinity(maxDistance) && !float.IsNaN(maxDistance)) {
				if(totalDistance >= maxDistance)
					GameObject.Destroy(gameObject, destroyDelay);
			}
			else if(maxTime > 0 && !float.IsInfinity(maxTime) && !float.IsNaN(maxTime))
				if(totalTime >= maxTime)
					GameObject.Destroy(gameObject, destroyDelay);

			if(isSleeping) return;

			velocity = velocity.SimulateVelocity(useGravity ? gravity : Vector3.zero, drag);
			StartCoroutine(PhysicsUpdate());
		}

		IEnumerator PhysicsUpdate() {
			yield return new WaitForFixedUpdate();

			if(isSleeping) yield break;

			RaycastHit hit;
			float magnitude = velocity.magnitude * Time.deltaTime;

			//When bouncing off of things, it continues to move forward, bouncing off of other walls until
			//it goes the distance that it is meant to go rather than stopping it's movement on the first hit
			while(magnitude > 0) {
				if(Physics.Raycast(transform.position, velocity, out hit, magnitude, collisionMask)) {
					Vector3 pos = hit.point + hit.normal * .0001f;
					float distance = Vector3.Distance(transform.position, pos);
					float bounce = BounceCombine(hit.collider.material);
					BulletHit bulletHit = new BulletHit(this, hit, bounce);

					totalDistance += distance;
					if(OnBulletAddTotalDistance != null) OnBulletAddTotalDistance(this);

					if(hit.rigidbody && Time.deltaTime != 0)
						hit.rigidbody.AddForceAtPosition(velocity / Time.deltaTime, hit.point);

					SendMessage("OnBulletCollided", bulletHit, SendMessageOptions.DontRequireReceiver);

					GameObject sendTo = hit.collider.attachedRigidbody ? hit.collider.attachedRigidbody.gameObject : hit.collider.gameObject;
					sendTo.SendMessage("OnHitByBullet", bulletHit, SendMessageOptions.DontRequireReceiver);

					if(velocity.sqrMagnitude > Physics.bounceThreshold)
						velocity = Vector3.Reflect(velocity, hit.normal) * bounce;
					else
						velocity = Vector3.zero;

					if(OnBulletCollided != null)
						OnBulletCollided(bulletHit, magnitude);

					Vector3 oldPos = transform.position;
					transform.position = hit.point + hit.normal * .0001f;
					if(useAutoRotation && transform.position - oldPos != Vector3.zero)
						transform.rotation = Quaternion.LookRotation(transform.position - oldPos);

					magnitude = velocity.magnitude * Time.deltaTime;

					if(velocity.sqrMagnitude < Physics.sleepVelocity * 2) {
						isSleeping = true;

						if(parentOnZeroVelocityHit)
							transform.parent = hit.transform;

						SendMessage("OnBulletSleep", this, SendMessageOptions.DontRequireReceiver);
						if(OnBulletSleep != null) OnBulletSleep(this);
						yield break;
					}
				}
				else {
					totalDistance += magnitude;
					if(OnBulletAddTotalDistance != null) OnBulletAddTotalDistance(this);

					Vector3 oldPos = transform.position;
					transform.Translate(velocity.normalized * magnitude, Space.World);

					if(useAutoRotation && transform.position - oldPos != Vector3.zero)
						transform.rotation = Quaternion.LookRotation(transform.position - oldPos);

					magnitude = 0;
					break;
				}
			}
		}

		/// <summary>
		/// Add a force to move this <see cref="Bullet"/>
		/// </summary>
		/// <param name="force">Push power</param>
		/// <param name="forceMode">Force type</param>
		public void AddForce(Vector3 force, ForceMode forceMode) {
			isSleeping = false;

			velocity = velocity.AddForce(force, mass, forceMode);
		}

		/// <summary>
		/// Stop sleeping
		/// </summary>
		public void WakeUp() {
			isSleeping = true;
		}

		float BounceCombine(PhysicMaterial material) {
			switch(bounceCombine) {
				default: //Average
					return (bounciness + material.bounciness) * .5f;
				case PhysicMaterialCombine.Maximum:
					return Mathf.Max(bounciness, material.bounciness);
				case PhysicMaterialCombine.Minimum:
					return Mathf.Min(bounciness, material.bounciness);
				case PhysicMaterialCombine.Multiply:
					return bounciness * material.bounciness;
			}
		}

		void OnComponentWasEnabled(Component component) {
			this.ConnectComponentEventTo(component);
		}

		void OnComponentWasDisabled(Component component) {
			this.DisconnectComponentEventFrom(component);
		}
	}

	public struct BulletHit {
		/// <summary>
		/// The bullet that caused the collision
		/// </summary>
		public Bullet bullet { get; set; }
		/// <summary>
		/// The object that was hit
		/// </summary>
		public RaycastHit hit { get; set; }
		/// <summary>
		/// The bounce amount that "occured" on the hit
		/// </summary>
		public float bounce { get; set; }

		public BulletHit(Bullet bullet, RaycastHit hit, float bounce) {
			this.bullet = bullet;
			this.hit = hit;
			this.bounce = bounce;
		}
	}
}