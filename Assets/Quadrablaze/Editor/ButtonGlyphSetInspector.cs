using UnityEngine;
using UnityEditor;
using System.Collections;
using Rewired;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(ButtonGlyphSet))]
public class ButtonGlyphSetInspector : Editor {

    ButtonGlyphSet buttonGlyphSet;

    SerializedProperty buttonGlyphs;
    SerializedProperty deviceType;
    SerializedProperty joystick;
    SerializedProperty joystickTemplate;

    void OnEnable() {
        buttonGlyphSet = target as ButtonGlyphSet;

        buttonGlyphs = serializedObject.FindProperty("_buttonGlyphs");
        deviceType = serializedObject.FindProperty("_deviceType");
        joystick = serializedObject.FindProperty("_joystick");
        joystickTemplate = serializedObject.FindProperty("_joystickTemplate");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(deviceType);

        if(deviceType.enumValueIndex == (int)ControllerType.Joystick) {
            EditorGUILayout.PropertyField(joystick);
            EditorGUILayout.PropertyField(joystickTemplate);
        }

        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Default Glyph List", GUILayout.ExpandWidth(false)))
                if(EditorUtility.DisplayDialog("Creating default glyph list", "Are you sure you want to set the list to default? This will attempt to merge the current list and with the new.", "Yes", "No"))
                    switch(deviceType.enumValueIndex) {
                        case (int)ControllerType.Joystick: CreateDefaultJoystickList(); break;
                        case (int)ControllerType.Keyboard: CreateDefaultKeyboardList(); break;
                        case (int)ControllerType.Mouse: CreateDefaultMouseList(); break;
                    }

            if(joystickTemplate.objectReferenceValue != null)
                if(deviceType.enumValueIndex == (int)ControllerType.Joystick)
                    if(GUILayout.Button("Default Glyph List From Template", GUILayout.ExpandWidth(false)))
                        if(EditorUtility.DisplayDialog("Creating default glyph list from the Joystick Template", "Are you sure you want to set the list to default? This will attempt to merge the current list and with the new.", "Yes", "No"))
                            CreateDefaultJoystickTemplateList();
        }
        EditorGUILayout.EndHorizontal();

        if(buttonGlyphs.arraySize > 0) {
            if(GUILayout.Button(new GUIContent("Assign Selected Sprites", "Assigned the selected sprites as icons to the elements with similar names."), GUILayout.ExpandWidth(false))) {
                SetIconsToSelection();
            }
        }

        if(EditorGUILayout.PropertyField(buttonGlyphs)) {
            EditorGUI.indentLevel++;

            foreach(SerializedProperty buttonGlyph in buttonGlyphs) {
                var icon = buttonGlyph.FindPropertyRelative("icon");
                var iconPositive = buttonGlyph.FindPropertyRelative("iconPositive");
                var iconNegative = buttonGlyph.FindPropertyRelative("iconNegative");
                var name = buttonGlyph.FindPropertyRelative("name");

                GUI.color = (!icon.objectReferenceValue && !iconPositive.objectReferenceValue && !iconNegative.objectReferenceValue) ? new Color(1, .5f, .5f) : Color.white;

                GUIContent buttonContent = new GUIContent(name.stringValue);

                if(icon.objectReferenceValue) buttonContent.image = (icon.objectReferenceValue as Sprite).texture;

                GUILayoutOption[] options = icon.objectReferenceValue ? new GUILayoutOption[] { GUILayout.Height(32) } : null;

                if(EditorGUILayout.PropertyField(buttonGlyph, buttonContent, options)) {
                    GUI.color = Color.white;
                    EditorGUI.indentLevel++;

                    var elementIdentifierId = buttonGlyph.FindPropertyRelative("elementIdentifierId");

                    EditorGUILayout.PropertyField(name);
                    EditorGUILayout.PropertyField(elementIdentifierId);
                    
                    switch(deviceType.enumValueIndex) {
                        case (int)ControllerType.Joystick: {
                                string[] elementNames = null;

                                if(buttonGlyphSet.Joystick != null)
                                    elementNames = buttonGlyphSet.Joystick.GetElementIdentifierNames();
                                else if(buttonGlyphSet.JoystickTemplate != null)
                                    elementNames = buttonGlyphSet.JoystickTemplate.GetElementIdentifierNames();

                                if(elementNames != null) {
                                    EditorGUI.indentLevel++;
                                    elementIdentifierId.intValue = EditorGUILayout.Popup("Joystick Element", elementIdentifierId.intValue, elementNames);
                                    EditorGUI.indentLevel--;
                                    EditorGUILayout.Separator();
                                }

                                break;
                            }

                        case (int)ControllerType.Keyboard: {
                                EditorGUI.indentLevel++;
                                elementIdentifierId.intValue = (int)(KeyboardKeyCode)EditorGUILayout.EnumPopup("KeyCode", (KeyboardKeyCode)System.Enum.Parse(typeof(KeyboardKeyCode), elementIdentifierId.intValue.ToString()));
                                EditorGUI.indentLevel--;
                                EditorGUILayout.Separator();
                            }
                            break;

                        case (int)ControllerType.Mouse: {
                                EditorGUI.indentLevel++;
                                elementIdentifierId.intValue = (int)(MouseInputElement)EditorGUILayout.EnumPopup("Mouse Input Element", (MouseInputElement)System.Enum.Parse(typeof(MouseInputElement), elementIdentifierId.intValue.ToString()));
                                EditorGUI.indentLevel--;
                                EditorGUILayout.Separator();
                            }
                            break;
                    }

                    var alternateElementIdentifierNames = buttonGlyph.FindPropertyRelative("alternateElementIdentifierNames");

                    EditorGUILayout.PropertyField(alternateElementIdentifierNames, true);
                    EditorGUILayout.Separator();

                    EditorGUILayout.PropertyField(icon);
                    EditorGUILayout.PropertyField(iconPositive);
                    EditorGUILayout.PropertyField(iconNegative);

                    EditorGUI.indentLevel--;
                }
            }

            GUI.color = Color.white;
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    List<ButtonGlyphSet.ButtonGlyph> CreateAndMergeGlyphs(string[] elementNames) {
        List<ButtonGlyphSet.ButtonGlyph> buttonGlyphs = new List<ButtonGlyphSet.ButtonGlyph>();

        for(int i = 0; i < elementNames.Length; i++) {
            var buttonGlyph = new ButtonGlyphSet.ButtonGlyph(elementNames[i], i, null);
            var existingGlyph = buttonGlyphSet.ButtonGlyphs.FirstOrDefault(s => buttonGlyph.name == s.name);

            if(!string.IsNullOrEmpty(existingGlyph.name))
                buttonGlyph = new ButtonGlyphSet.ButtonGlyph(buttonGlyph.name, buttonGlyph.elementIdentifierId, existingGlyph.alternateElementIdentifierNames) {
                    icon = existingGlyph.icon,
                    iconPositive = existingGlyph.iconPositive,
                    iconNegative = existingGlyph.iconNegative
                };

            buttonGlyphs.Add(buttonGlyph);
        }

        return buttonGlyphs;
    }

    void CreateDefaultJoystickList() {
        if(buttonGlyphSet.Joystick != null)
            buttonGlyphSet.ButtonGlyphs = CreateAndMergeGlyphs(buttonGlyphSet.Joystick.GetElementIdentifierNames());

        serializedObject.ApplyModifiedProperties();
    }

    void CreateDefaultJoystickTemplateList() {
        if(buttonGlyphSet.JoystickTemplate != null)
            buttonGlyphSet.ButtonGlyphs = CreateAndMergeGlyphs(buttonGlyphSet.JoystickTemplate.GetElementIdentifierNames());

        serializedObject.ApplyModifiedProperties();
    }

    void CreateDefaultKeyboardList() {
        buttonGlyphSet.ButtonGlyphs = CreateAndMergeGlyphs(System.Enum.GetNames(typeof(KeyboardKeyCode)));
        serializedObject.ApplyModifiedProperties();
    }

    void CreateDefaultMouseList() {
        buttonGlyphSet.ButtonGlyphs = CreateAndMergeGlyphs(System.Enum.GetNames(typeof(MouseInputElement)));
        serializedObject.ApplyModifiedProperties();
    }

    void SetIconsToSelection() {
        if(buttonGlyphs.arraySize == 0) return;

        var selectedObjects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

        foreach(var selected in selectedObjects)
            foreach(var child in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(selected))) {
                var sprite = child as Sprite;

                if(sprite) {
                    for(int i = 0; i < buttonGlyphSet.ButtonGlyphs.Count; i++)
                        if(sprite.name.EndsWith(buttonGlyphSet.ButtonGlyphs[i].name)) {
                            buttonGlyphSet.SetGlyphIcon(i, sprite);
                            break;
                        }
                }
            }
    }
}