using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AutoControllerGlyph)), CanEditMultipleObjects]
public class AutoControllerGlyphInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
	}
}