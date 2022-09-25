using UnityEngine;

namespace YounGenTech.PoolGen {
    /// <summary>An example of an object that shoots using a pool for projectiles</summary>
    [AddComponentMenu("YounGen Tech/PoolGen/Test/Single Shooter Test")]
    public class SingleShooterTest : MonoBehaviour, IPoolInstantiate {

        public string poolName;

        public Transform shootingPoint;

        public float shootSpeed = 1;
        public float shootDelay = .2f;

        float shootTimer;
        PoolManager projectilePool;

        #region Properties
        Collider ColliderComponent { get; set; }

        PoolUser UserComponent { get; set; }
        #endregion

        public void PoolInstantiate(PoolUser user) {
            ColliderComponent = GetComponent<Collider>();
            UserComponent = user;
            projectilePool = PoolManager.GetPool(poolName);

            if(UserComponent)
                if(!projectilePool) UserComponent.Despawn();
                else {
                    UserComponent.OnSpawn.AddListener(RandomizeDirection);

                    RandomizeDirection(); //Call it now because the OnSpawn event would have already happened by now
                }
        }

        void Update() {
            if(shootTimer > 0)
                shootTimer = Mathf.Max(shootTimer - Time.deltaTime, 0);

            if(shootTimer == 0)
                Shoot();
        }

        void RandomizeDirection() {
            var constantForce = GetComponent<ConstantForce>();

            constantForce.relativeForce = new Vector3(0, 0, Random.Range(5f, 10f));
            constantForce.relativeTorque = new Vector3(0, (Random.Range(0, 2) == 0) ? -.5f : .5f, 0);
        }

        void Shoot() {
            var user = projectilePool.Spawn(shootingPoint.position, shootingPoint.rotation);

            if(user) {
                var projectileTest = user.GetComponent<ProjectileTest>();

                projectileTest.velocity = transform.forward * shootSpeed;
                projectileTest.IgnoreCollider = ColliderComponent;
            }

            shootTimer = shootDelay;
        }
    }
}