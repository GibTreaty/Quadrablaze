using UnityEngine;

namespace YounGenTech.PoolGen {
    [AddComponentMenu("YounGen Tech/PoolGen/Test/Projectile Test")]
    /// <summary>An example raycast projectile component</summary>
    public class ProjectileTest : MonoBehaviour, IPoolInstantiate {

        /// <summary>How much force should be applied to the hit Rigidbody</summary>
        public float hitPower = 1;

        /// <summary>Velocity of the projectile</summary>
        public Vector3 velocity;

        RaycastHit hit;

        #region Properties
        public Collider IgnoreCollider { get; set; }

        PoolUser UserComponent { get; set; }
        #endregion

        public void PoolInstantiate(PoolUser user) {
            UserComponent = user;

            UserComponent.OnSpawn.AddListener(ResetProjectile);
        }

        void FixedUpdate() {
            var deltaVelocity = velocity * Time.deltaTime;

            if(Physics.Raycast(transform.position, velocity, out hit, deltaVelocity.magnitude))
                if(!IgnoreCollider || hit.collider != IgnoreCollider) {
                    if(hit.rigidbody && hitPower > 0) hit.rigidbody.AddForce(velocity * hitPower);

                    UserComponent.Despawn();
                }

            transform.position += deltaVelocity;
        }

        void LateUpdate() {
            transform.forward = velocity;
        }

        void ResetProjectile() {
            velocity = Vector3.zero;
            IgnoreCollider = null;
        }
    }
}