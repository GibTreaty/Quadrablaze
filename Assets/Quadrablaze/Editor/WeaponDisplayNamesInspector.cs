using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze.Skills {
    [CustomEditor(typeof(WeaponDisplayNames))]
    public class WeaponDisplayNamesInspector : Editor {

        WeaponDisplayNames weaponDisplayNames;

        void OnEnable() {
            weaponDisplayNames = target as WeaponDisplayNames;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if(GUILayout.Button("Update Display Names")) {
                weaponDisplayNames.UpdateDisplayNames();
                EditorUtility.SetDirty(serializedObject.targetObject);
                serializedObject.ApplyModifiedProperties();
            }

            base.OnInspectorGUI();
        }
    }
}