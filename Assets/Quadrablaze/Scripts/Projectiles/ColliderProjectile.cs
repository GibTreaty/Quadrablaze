using System.Collections.Generic;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class ColliderProjectile : ProjectileBase {
        public static bool ignoreCollisions = true;

        [SerializeField]
        string _impactPoolName = "";

        [SerializeField]
        string _impactPoolPrefabName = "";

        Collider _collider;

        HashSet<Collider> _ignoringColliders = new HashSet<Collider>();

        #region Properties
        public Collider AttachedCollider {
            get { return _collider; }
        }
        #endregion

        void CreateImpactEffect(Vector3 position, Quaternion rotation) {
            if(!string.IsNullOrEmpty(_impactPoolName) && !string.IsNullOrEmpty(_impactPoolPrefabName)) {
                var pool = PoolManager.GetPool(_impactPoolName);

                if(pool)
                    pool.Spawn(pool.IndexFromPrefabName(_impactPoolPrefabName), position, rotation);
            }
        }

        public void IgnoreCollider(Collider ignoreCollider, bool ignore = true) {
            if(!ignoreCollisions) return;

            if(!gameObject.activeInHierarchy)
                throw new GameObjectInactiveException();

            if(ignoreCollider && ignoreCollider.gameObject)
                if(ignoreCollider.gameObject.activeInHierarchy) {
                    if(ignore)
                        _ignoringColliders.Add(ignoreCollider);

                    Physics.IgnoreCollision(_collider, ignoreCollider, ignore);
                }
        }

        public void IgnoreCollider(IEnumerable<Collider> ignoreColliders, bool ignore = true) {
            foreach(var collider in ignoreColliders)
                try {
                    IgnoreCollider(collider, ignore);
                }
                catch(GameObjectInactiveException) {
                    return;
                }
        }

        void OnDisable() {
            if(_ignoringColliders.Count > 0) {
                if(_collider)
                    foreach(var collider in _ignoringColliders)
                        if(collider)
                            Physics.IgnoreCollision(_collider, collider, false);

                _ignoringColliders.Clear();
            }
        }

        void OnEnable() {
            if(!_collider)
                _collider = GetComponent<Collider>();
        }

        protected virtual void OnCollisionEnter(Collision hit) {
            DoDamage(hit.collider);
            DoDestroy();
            FireHitEvent(hit.collider);

            CreateImpactEffect(hit.contacts[0].point, Quaternion.LookRotation(hit.contacts[0].normal));
        }

        class GameObjectInactiveException : System.Exception {
            public override string Message {
                get { return "GameObject inactive. Can't ignore collisions."; }
            }
        }
    }
}