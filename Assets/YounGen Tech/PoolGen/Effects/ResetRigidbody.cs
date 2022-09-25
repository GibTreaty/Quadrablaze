using UnityEngine;

namespace YounGenTech.PoolGen.Effects {
    [AddComponentMenu("YounGen Tech/PoolGen/Effects/Reset Rigidbody")]
    /// <summary>Resets the attached rigidbody's velocity and angular velocity when spawned</summary>
    public class ResetRigidbody : MonoBehaviour, IPoolInstantiate {
        
        Rigidbody RigidbodyComponent { get; set; }

        /// <summary>Hooks to the <see cref="PoolUser"/>'s <see cref="PoolUser.OnSpawn"/> event</summary>
        public void PoolInstantiate(PoolUser user) {
            RigidbodyComponent = GetComponent<Rigidbody>();
            user.OnSpawn.AddListener(ResetVelocities);
        }

        /// <summary>Resets the Rigidbody's velocity and angularVelocity</summary>
        public void ResetVelocities() {
            RigidbodyComponent.velocity = Vector3.zero;
            RigidbodyComponent.angularVelocity = Vector3.zero;
        }
    }
}