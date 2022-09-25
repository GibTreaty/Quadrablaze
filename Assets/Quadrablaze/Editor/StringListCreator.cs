using UnityEngine;
using UnityEditor;
using System.Collections;

public class StringListCreator : Editor {

    static string defaultSavePath = "Assets/";

    [MenuItem("Assets/Create String List")]
    static void Create() {
        CreateStringList();
    }

    static void CreateStringList() {
        string path = EditorUtility.SaveFilePanelInProject("Save String List", "Default String List", "asset", "um what", defaultSavePath);

        Debug.Log("Saved Asset at = " + path);
        if(!string.IsNullOrEmpty(path)) {
            defaultSavePath = path;
            StringList upgradeList = CreateInstance<StringList>();

            AssetDatabase.CreateAsset(upgradeList, path);
            AssetDatabase.SaveAssets();
        }
    }
}