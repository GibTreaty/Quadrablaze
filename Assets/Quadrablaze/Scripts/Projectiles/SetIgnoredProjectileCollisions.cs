using System.Collections.Generic;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class SetIgnoredProjectileCollisions : MonoBehaviour {

        ColliderList _collisionList = null;

        ColliderList CollisionList {
            get {
                if(_collisionList == null) CollisionList = GetComponentInParent<ColliderList>();

                return _collisionList;
            }
            set { _collisionList = value; }
        }

        public void IgnorePooledProjectile(GameObject gameObject) {
            IgnorePooledProjectile(gameObject, CollisionList.Colliders);
        }
        public void IgnorePooledProjectile(GameObject gameObject, IEnumerable<Collider> colliders) {
            var projectile = gameObject.GetComponent<ColliderProjectile>();

            if(projectile)
                foreach(Collider collider in colliders)
                    if(collider.gameObject.activeInHierarchy)
                        projectile.IgnoreCollider(collider);
        }
    }
}