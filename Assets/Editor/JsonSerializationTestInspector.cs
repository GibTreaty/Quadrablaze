using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(JsonSerializationTest))]
public class JsonSerializationTestInspector : Editor {

    JsonSerializationTest jsonTest;

    void OnEnable() {
        jsonTest = target as JsonSerializationTest;
    }

    public override void OnInspectorGUI() {
        if(GUILayout.Button("Save To Output"))
            jsonTest.outputString = JsonUtility.ToJson(jsonTest.testStruct);

        if(GUILayout.Button("Load From Output"))
            if(!string.IsNullOrEmpty(jsonTest.outputString))
                jsonTest.testStruct = JsonUtility.FromJson<TestStruct>(jsonTest.outputString);

        base.OnInspectorGUI();
    }
}