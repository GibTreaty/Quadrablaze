using UnityEngine;

namespace Quadrablaze {
    public class RandomizeAudioPitch : MonoBehaviour {

        public AnimationCurve pitchCurve = AnimationCurve.Linear(0, .9f, 1, 1.1f);

        AudioSource audioSource;

        void Awake() {
            audioSource = GetComponent<AudioSource>();

            RandomizePitch();
        }

        public void RandomizePitch() {
            audioSource.pitch = pitchCurve.Evaluate(Random.value);
        }
    }
}