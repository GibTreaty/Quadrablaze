using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze {
    public class CameraSound : MonoBehaviour {

        public void PlaySound(AudioClip clip) {
            CameraSoundController.Current.PlaySound(clip);
        }

        public void PlaySound(string name, bool playOverNetwork = false) {
            CameraSoundController.Current.PlaySound(name, playOverNetwork);
        }
    }
}