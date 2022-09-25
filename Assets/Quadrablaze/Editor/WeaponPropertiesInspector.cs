using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Quadrablaze.Skills {
    [CustomEditor(typeof(WeaponProperties))]
    public class WeaponPropertiesInspector : Editor {

        WeaponProperties weaponPropertyTarget;
        SerializedProperty weaponProperties;

        void OnEnable() {
            weaponPropertyTarget = target as WeaponProperties;
            weaponProperties = serializedObject.FindProperty("_weaponProperties");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            //EditorGUILayout.IntField(WeaponProperties.instances.Count);

            EditorGUI.indentLevel++;
            {
                if(EditorGUILayout.PropertyField(weaponProperties)) {
                    EditorGUI.indentLevel++;

                    int oldSize = weaponProperties.arraySize;
                    weaponProperties.arraySize = EditorGUILayout.IntField("Size", weaponProperties.arraySize);

                    if(oldSize != weaponProperties.arraySize)
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.BeginVertical();
                    {
                        int weaponPropertyIndex = 0;
                        foreach(SerializedProperty weaponProperty in weaponProperties) {
                            SerializedProperty displayName = weaponProperty.FindPropertyRelative("_displayName");

                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    weaponProperty.isExpanded = EditorGUILayout.Foldout(weaponProperty.isExpanded, !weaponProperty.isExpanded ? string.IsNullOrEmpty(displayName.stringValue) ? "None" : displayName.stringValue : "");

                                    if(weaponProperty.isExpanded) {
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            {
                                                EditorGUILayout.PropertyField(displayName, GUIContent.none);
                                            }
                                            EditorGUILayout.EndHorizontal();

                                            if(WeaponDisplayNames.Current && WeaponDisplayNames.Current.displayNames.Count > 0) {
                                                EditorGUILayout.BeginHorizontal();
                                                {
                                                    int index = EditorGUILayout.Popup(-1, WeaponDisplayNames.Current.displayNames.ToArray(), GUILayout.Width(150));

                                                    if(index > -1)
                                                        displayName.stringValue = WeaponDisplayNames.Current.displayNames[index];

                                                    GUILayout.Label("Used");
                                                }
                                                EditorGUILayout.EndHorizontal();
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                if(weaponProperty.isExpanded) {
                                    EditorGUI.indentLevel++;
                                    {
                                        SerializedProperty propertyName = weaponProperty.FindPropertyRelative("_propertyName");
                                        SerializedProperty targetObject = weaponProperty.FindPropertyRelative("_target");

                                        //EditorGUILayout.BeginHorizontal(GUI.skin.box);
                                        //{
                                        //    EditorGUILayout.BeginHorizontal();
                                        //    {
                                        //        EditorGUILayout.PropertyField(displayName);
                                        //    }
                                        //    EditorGUILayout.EndHorizontal();

                                        //    if(WeaponDisplayNames.Current && WeaponDisplayNames.Current.displayNames.Length > 0) {
                                        //        EditorGUILayout.BeginHorizontal();
                                        //        {
                                        //            int index = EditorGUILayout.Popup(-1, WeaponDisplayNames.Current.displayNames, GUILayout.Width(150));

                                        //            if(index > -1)
                                        //                displayName.stringValue = WeaponDisplayNames.Current.displayNames[index];

                                        //            GUILayout.Label("Used");
                                        //        }
                                        //        EditorGUILayout.EndHorizontal();
                                        //    }
                                        //}
                                        //EditorGUILayout.EndHorizontal();

                                        EditorGUILayout.PropertyField(targetObject);

                                        if(targetObject.objectReferenceValue) {
                                            PropertyList propertyList = GetProperties(targetObject.objectReferenceValue);
                                            int selectedProperty = 0;

                                            for(int i = 1; i < propertyList.content.Length; i++)
                                                if(propertyList.properties[i - 1].Name == propertyName.stringValue) {
                                                    selectedProperty = i; break;
                                                }

                                            EditorGUILayout.BeginHorizontal(GUI.skin.box);
                                            {
                                                int oldSelectedProperty = selectedProperty;

                                                selectedProperty = EditorGUILayout.Popup(selectedProperty, propertyList.content, GUILayout.Width(200));

                                                if(oldSelectedProperty != selectedProperty)
                                                    propertyName.stringValue = selectedProperty > 0 ? propertyList.properties[selectedProperty - 1].Name : "";

                                                GUILayout.FlexibleSpace();

                                                EditorGUILayout.BeginHorizontal();
                                                {
                                                    EditorGUILayout.LabelField("Value = " + weaponPropertyTarget.GetValue(weaponPropertyIndex));
                                                }
                                                EditorGUILayout.EndHorizontal();
                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                    EditorGUI.indentLevel--;
                                }
                            }
                            EditorGUILayout.EndVertical();

                            weaponPropertyIndex++;
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        PropertyList GetProperties(Object source) {
            if(!source) return new PropertyList();

            List<GUIContent> propertyList = new List<GUIContent>();
            PropertyInfo[] properties = source.GetType().GetProperties();

            propertyList.Add(new GUIContent("None"));

            foreach(PropertyInfo property in properties) {
                propertyList.Add(new GUIContent(property.PropertyType.Name + " " + property.Name));
            }

            return new PropertyList(properties, propertyList.ToArray());
        }

        [System.Serializable]
        public struct PropertyList {
            public PropertyInfo[] properties;
            public GUIContent[] content;

            public PropertyList(PropertyInfo[] properties, GUIContent[] content) {
                this.properties = properties;
                this.content = content;
            }
        }
    }
}