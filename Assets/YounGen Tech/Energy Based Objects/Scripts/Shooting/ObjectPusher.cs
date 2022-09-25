using UnityEngine;
using YounGenTech.BulletPhysics;

namespace YounGenTech.EnergyBasedObjects {
    /// <summary>
    /// Pushes Rigidbodies and <see cref="Bullet"/>s
    /// </summary>
    [AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Pusher")]
    public class ObjectPusher : ObjectShootingPoint {

        /// <summary>
        /// These can be set manually from another script or from the inspector
        /// </summary>
        public GameObject[] pushProjectiles;

        [SerializeField]
        /// <summary>
        /// The amount of force used to push the projectiles in the direction of this object (Force direction is local to this object)
        /// </summary>
        private Vector3 force = Vector3.forward;

        [SerializeField]
        /// <summary>
        /// Random force applied to force (Random force is local to this object)
        /// </summary>
        private Vector3 randomForce;
        /// <summary>
        /// The force mode that will be used to push the projectiles
        /// </summary>
        public ForceMode forceMode = ForceMode.Acceleration;

        /// <summary>
        /// Multiplies the force by Time.deltaTime. Use this if you are using the force modes Force and Acceleration.
        /// </summary>
        public bool multiplyDeltaTime;

        /// <summary>
        /// Use the forceCurve to decide how much power should be applied at certain distances
        /// </summary>
        public bool useForceCurve;
        /// <summary>
        /// X = Distance, Y = Power Multiplier
        /// </summary>
        public AnimationCurve forceCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        /// <summary>
        /// Use AddForceAtPosition using worldPushPoint as the position to push
        /// </summary>
        public bool pushAtWorldPoint;
        /// <summary>
        /// Push rigidbodies at this position with AddForceAtPosition
        /// </summary>
        public Vector3 worldPushPoint;

        /// <summary>
        /// Use Rigidbody.AddExplosionForce. This setting does not effect the <see cref="Bullet"/> component
        /// </summary>
        public bool useExplosionForce;
        /// <summary>
        /// Reverse the explosion force
        /// </summary>
        public bool implosion;
        /// <summary>
        /// The radius of the explosion
        /// </summary>
        public float explosionRadius = Mathf.Infinity;

        /// <summary>
        /// isKinematic is turned off on projectiles when this weapon is shot
        /// </summary>
        public bool notKinematicOnShoot;
        /// <summary>
        /// Unparent the projectiles when this weapon is shot
        /// </summary>
        public bool unparentOnShoot;

        public Vector3 RandomForce {
            get {
                return randomForce;
            }

            set {
                randomForce = value;
            }
        }

        public Vector3 Force {
            get { return force; }
            set { force = value; }
        }

        public float ForwardSpeed {
            get { return Force.z; }
        }

        public float HorizontalSpeed {
            get { return Force.x; }
        }

        public float VerticalSpeed {
            get { return Force.y; }
        }

        void Awake() {
            OnWeaponShoot_NoParameters += Push;
        }

        /// <summary>
        /// Adds force to the projectiles that have a Rigidbody or <see cref="Bullet"/> component
        /// </summary>
        public void Push() {
            foreach(GameObject pushProjectile in pushProjectiles)
                if(pushProjectile) {
                    Bullet bullet = pushProjectile.GetComponent<Bullet>();
                    if(!(pushProjectile.GetComponent<Rigidbody>() || bullet)) continue;

                    Vector3 forceDirection = transform.TransformDirection(force + GetRandomDirection(randomForce));

                    if(multiplyDeltaTime)
                        forceDirection *= Time.deltaTime;

                    if(unparentOnShoot)
                        pushProjectile.transform.parent = null;

                    //Force curve is positive or negative - Defaults to positive if curve isn't used
                    float sign = 1;

                    if(useForceCurve) {
                        float distance = Vector3.Distance(pushAtWorldPoint ? worldPushPoint : transform.position, pushProjectile.transform.position);
                        float forceCurveOutput = forceCurve.Evaluate(distance);
                        Vector3 direction = (worldPushPoint - pushProjectile.transform.position).normalized * forceDirection.magnitude;

                        sign = Mathf.Sign(forceCurveOutput);

                        if(pushAtWorldPoint)
                            forceDirection = direction * forceCurveOutput;
                        else
                            forceDirection *= forceCurveOutput;
                    }

                    if(pushProjectile.GetComponent<Rigidbody>()) {
                        if(notKinematicOnShoot)
                            pushProjectile.GetComponent<Rigidbody>().isKinematic = false;

                        if(pushAtWorldPoint) {
                            if(useExplosionForce)
                                pushProjectile.GetComponent<Rigidbody>().AddExplosionForce((implosion ? -forceDirection.magnitude : forceDirection.magnitude) * sign, worldPushPoint, explosionRadius, 0, forceMode);
                            else
                                pushProjectile.GetComponent<Rigidbody>().AddForceAtPosition(forceDirection, worldPushPoint, forceMode);
                        }
                        else {
                            if(useExplosionForce)
                                pushProjectile.GetComponent<Rigidbody>().AddExplosionForce((implosion ? -forceDirection.magnitude : forceDirection.magnitude) * sign, transform.position, explosionRadius, 0, forceMode);
                            else
                                pushProjectile.GetComponent<Rigidbody>().AddForce(forceDirection, forceMode);
                        }

                        if(forceDirection != Vector3.zero)
                            pushProjectile.transform.rotation = Quaternion.LookRotation(forceDirection);
                    }

                    if(bullet)
                        bullet.AddForce(forceDirection, forceMode);
                }
        }

        /// <summary>
        /// Resets the pushProjectiles array
        /// </summary>
        public void ClearProjectiles() {
            pushProjectiles = new GameObject[0];
        }

        [ContextMenu("Reset Force Curve")]
        void ResetForceCurve() {
            forceCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            if(useExplosionForce)
                Gizmos.DrawWireSphere(pushAtWorldPoint ? worldPushPoint : transform.position, explosionRadius);
        }
#endif
    }
}