using UnityEngine;
using UnityEditor;
using System.Collections;

namespace YounGenTech {
	[CustomPropertyDrawer(typeof(DisabledOutOfPlayAttribute))]
	public class DisabledOutOfPlayPropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);
			{
				bool enabledBefore = GUI.enabled;

				GUI.enabled = (attribute as DisabledOutOfPlayAttribute).invert ? !Application.isPlaying : Application.isPlaying;
				EditorGUI.PropertyField(position, property);
				GUI.enabled = enabledBefore;
			}
			EditorGUI.EndProperty();
		}

	}
}