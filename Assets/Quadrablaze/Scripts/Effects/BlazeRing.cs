using UnityEngine;

namespace Quadrablaze {
    public class BlazeRing : MonoBehaviour {

        public ParticleSystem ringParticles;

        public void SetRadius(float radius) {
            var shape = ringParticles.shape;

            shape.radius = radius;
        }
    }
}