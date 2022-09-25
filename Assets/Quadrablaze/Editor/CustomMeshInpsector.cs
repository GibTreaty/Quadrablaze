using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MeshFilter)), CanEditMultipleObjects]
public class CustomMeshInpsectorInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
	}
}