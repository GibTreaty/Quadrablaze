using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(HealthLevel)), CanEditMultipleObjects]
public class HealthLevelInspector : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}