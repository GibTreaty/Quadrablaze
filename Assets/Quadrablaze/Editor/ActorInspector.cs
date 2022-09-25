using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze {
    [CustomEditor(typeof(Actor)), CanEditMultipleObjects]
    public class ActorInspector : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
}