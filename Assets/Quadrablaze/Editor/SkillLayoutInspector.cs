using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;

namespace Quadrablaze {
    [CustomEditor(typeof(ScriptableSkillLayout))]
    public class SkillLayoutInspector : Editor {
        SerializedProperty elements;
        ReorderableList elementsList;

        void ElementCallback(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty property) {
            rect.xMin += 8;
            EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), true);
        }

        void HeaderCallback(Rect rect, string title) {
            GUI.Label(rect, "Skill Layout Elements");
        }

        float HeightCallback(int index, SerializedProperty property) {
            return EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)) + 10;
        }

        void OnEnable() {
            elements = serializedObject.FindProperty("_elements");
            elementsList = new ReorderableList(serializedObject, elements, true, true, true, true);
            elementsList.elementHeightCallback += index => HeightCallback(index, elements);
            elementsList.drawHeaderCallback += rect => HeaderCallback(rect, "Elements");
            elementsList.drawElementCallback += (rect, index, isActive, isFocused) => ElementCallback(rect, index, isActive, isFocused, elements);
        }


        public override void OnInspectorGUI() {
            serializedObject.Update();
            elementsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}