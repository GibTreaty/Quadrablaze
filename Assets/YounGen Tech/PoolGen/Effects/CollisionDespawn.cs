using System;
using UnityEngine;

namespace YounGenTech.PoolGen.Effects {
    [AddComponentMenu("YounGen Tech/PoolGen/Effects/Collision Despawn")]
    /// <summary>Despawns the object when it collides</summary>
    public class CollisionDespawn : MonoBehaviour, IPoolInstantiate {

        [SerializeField]
        LayerMask collisionLayer = -1;

        PoolUser UserComponent { get; set; }

        public void PoolInstantiate(PoolUser user) {
            UserComponent = user;
        }

        void OnCollisionEnter(Collision hit) {
            if((hit.collider.gameObject.layer & collisionLayer) == 0)
                UserComponent.Despawn();
        }
    }
}