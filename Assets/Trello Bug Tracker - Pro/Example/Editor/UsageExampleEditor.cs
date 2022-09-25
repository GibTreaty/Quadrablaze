using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace DG
{
    [CustomEditor(typeof(UsageExample))]
    public class UsageExampleEditor : Editor
    {

        private SerializedProperty keyProperty;
        private SerializedProperty tokenProperty;
        private SerializedProperty boardProperty;

        private SerializedProperty newCardsOnTopProperty;

        private SerializedProperty inProgressUIProperty;
        private SerializedProperty successUIProperty;
        private SerializedProperty fillInFormMessageUIProperty;

        private void OnEnable()
        {
            keyProperty = serializedObject.FindProperty("yourKey");
            tokenProperty = serializedObject.FindProperty("yourToken");
            boardProperty = serializedObject.FindProperty("currentBoard");

            newCardsOnTopProperty = serializedObject.FindProperty("newCardsOnTop");

            inProgressUIProperty = serializedObject.FindProperty("inProgressUI");
            successUIProperty = serializedObject.FindProperty("successUI");
            fillInFormMessageUIProperty = serializedObject.FindProperty("fillInFormMessageUI");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UsageExample usageExample = (UsageExample)target;

            GUILayout.Space(15);

            if (GUILayout.Button("Generate Key"))
            {
                Application.OpenURL("https://trello.com/1/appKey/generate");
            }

            GUIStyle boldGuiStyle = new GUIStyle {fontStyle = FontStyle.Bold};
            EditorGUILayout.LabelField("Trello Account Information", boldGuiStyle);
            keyProperty.stringValue = EditorGUILayout.TextField("Your Key", keyProperty.stringValue);
            tokenProperty.stringValue = EditorGUILayout.TextField("Your Token", tokenProperty.stringValue);
            boardProperty.stringValue = EditorGUILayout.TextField("Your Current Board", boardProperty.stringValue);

            GUILayout.Space(30);

            EditorGUILayout.LabelField("Places new uploaded cards on top of the list", boldGuiStyle);
            newCardsOnTopProperty.boolValue = EditorGUILayout.ToggleLeft("New Cards On Top", newCardsOnTopProperty.boolValue);

            GUILayout.Space(30);

            EditorGUILayout.LabelField("Report types to appear in the form", boldGuiStyle);

            var reportTypes = usageExample.reportTypes;

            if (reportTypes.Count == 0)
            {
                reportTypes.Add(new Dropdown.OptionData("Bug"));
            }

            for (int i = 0; i < reportTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();


                reportTypes[i].text = EditorGUILayout.TextField(reportTypes[i].text);
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    reportTypes.Insert(i + 1, new Dropdown.OptionData());
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    reportTypes.RemoveAt(reportTypes.Count - 1);
                }
                if (GUILayout.Button("∧", GUILayout.Width(20)))
                {
                    if (i != 0)
                    {
                        var temp = reportTypes[i - 1];
                        reportTypes[i - 1] = reportTypes[i];
                        reportTypes[i] = temp;
                    }
                }
                if (GUILayout.Button("∨", GUILayout.Width(20)))
                {
                    if (i != reportTypes.Count - 1)
                    {
                        var temp = reportTypes[i + 1];
                        reportTypes[i + 1] = reportTypes[i];
                        reportTypes[i] = temp;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.CopyFromSerializedProperty(new SerializedObject(usageExample).FindProperty("reportTypes"));


            GUILayout.Space(30);

            inProgressUIProperty.objectReferenceValue = EditorGUILayout.ObjectField("In Progress UI", inProgressUIProperty.objectReferenceValue, typeof(GameObject), true);
            successUIProperty.objectReferenceValue = EditorGUILayout.ObjectField("Success UI", successUIProperty.objectReferenceValue, typeof(GameObject), true);
            fillInFormMessageUIProperty.objectReferenceValue = EditorGUILayout.ObjectField("Fill In Form Message UI", fillInFormMessageUIProperty.objectReferenceValue, typeof(GameObject), true);

            GUILayout.Space(30);

            EditorGUILayout.HelpBox("Use, optionally, to verify your connection in play mode", MessageType.Info);
            if (GUILayout.Button("Check Connection") && EditorApplication.isPlaying)
            {
                usageExample.StartCoroutine(usageExample.Start());
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}