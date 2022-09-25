using UnityEngine;

namespace Quadrablaze {
    public class BlazeAfterburner : MonoBehaviour {

        public ParticleSystem afterburnerParticles;

        public void SetStartSpeed(float speed) {
            var data = afterburnerParticles.main;

            data.startSpeed = speed;
        }
    }
}