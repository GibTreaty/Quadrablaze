using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze {
    [CustomEditor(typeof(RandomizeAudioPitch)), CanEditMultipleObjects]
    public class RandomizeAudioPitchInspector : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
}