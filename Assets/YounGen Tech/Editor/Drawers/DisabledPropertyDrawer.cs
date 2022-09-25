using UnityEngine;
using UnityEditor;
using System.Collections;

namespace YounGenTech {
	[CustomPropertyDrawer(typeof(DisabledAttribute))]
	public class DisabledPropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);
			{
				bool enabledBefore = GUI.enabled;

				GUI.enabled = false;
				EditorGUI.PropertyField(position, property);
				GUI.enabled = enabledBefore;
			}
			EditorGUI.EndProperty();
		}

	}
}