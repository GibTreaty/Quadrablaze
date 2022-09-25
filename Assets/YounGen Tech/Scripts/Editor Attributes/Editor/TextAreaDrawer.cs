using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(TextAreaAttribute))]
public class TextAreaDrawer : PropertyDrawer {

	GUIStyle textArea = null;
	float textAreaWidth = 0;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if(property.propertyType == SerializedPropertyType.String) {
			position.height -= 30;
			EditorGUI.LabelField(position, label.text);
			position.y += GUI.skin.label.CalcHeight(label, position.width);

			property.stringValue = EditorGUI.TextArea(position, property.stringValue, TextAreaStyle());

			textAreaWidth = position.width;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return base.GetPropertyHeight(property, label) + Mathf.Max(TextAreaStyle().CalcHeight(new GUIContent(property.stringValue + "\n"), textAreaWidth), 30);
	}

	GUIStyle TextAreaStyle() {
		if(textArea == null)
			textArea = new GUIStyle(GUI.skin.textArea);

		TextAreaAttribute a = attribute as TextAreaAttribute;

		textArea.richText = a.richText;
		textArea.wordWrap = a.wordWrap;

		return textArea;
	}
}