using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze.Skills {
    public class WeaponListCreator : Editor {

        static string defaultSavePath = "Assets/";

        [UnityEditor.MenuItem("Assets/Create Weapon List")]
        static void Create() {
            CreateWeaponList();
        }

        static void CreateWeaponList() {
            string path = EditorUtility.SaveFilePanelInProject("Save Weapon List", "Default Weapon List", "asset", "um what", defaultSavePath);

            Debug.Log("Saved Asset at = " + path);
            if(!string.IsNullOrEmpty(path)) {
                defaultSavePath = path;
                WeaponUpgradeList upgradeList = CreateInstance<WeaponUpgradeList>();

                AssetDatabase.CreateAsset(upgradeList, path);
                AssetDatabase.SaveAssets();
            }
        }
    }
}