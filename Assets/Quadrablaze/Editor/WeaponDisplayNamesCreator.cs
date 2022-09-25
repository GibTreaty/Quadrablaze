using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze.Skills {
    public class WeaponDisplayNamesCreator : Editor {

        static string defaultSavePath = "Assets/";

        [UnityEditor.MenuItem("Assets/Weapon Display Names")]
        static void Create() {
            CreateWeaponDisplayNames();
        }

        static void CreateWeaponDisplayNames() {
            string path = EditorUtility.SaveFilePanelInProject("Save Weapon Display Names", "Default Weapon Display Names", "asset", "um what", defaultSavePath);

            Debug.Log("Saved Asset at = " + path);
            if(!string.IsNullOrEmpty(path)) {
                defaultSavePath = path;
                WeaponDisplayNames upgradeList = CreateInstance<WeaponDisplayNames>();

                AssetDatabase.CreateAsset(upgradeList, path);
                AssetDatabase.SaveAssets();
            }
        }
    }
}