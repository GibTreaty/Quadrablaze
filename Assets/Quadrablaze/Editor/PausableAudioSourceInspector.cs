using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze {
    [CustomEditor(typeof(GameAudioSource)), CanEditMultipleObjects]
    public class PausableAudioSourceInspector : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
}