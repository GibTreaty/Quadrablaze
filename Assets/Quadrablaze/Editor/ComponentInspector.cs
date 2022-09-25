using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;

[CustomEditor(typeof(Component), true)]
public class ComponentInspector : Editor {

    static GUIContent[] moveComponentToolbar = new GUIContent[] { new GUIContent("ᴧ", "Move Component Up"), new GUIContent("v", "Move Component Down") };

    public override void OnInspectorGUI() {
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            int selected = GUILayout.Toolbar(-1, moveComponentToolbar, GUILayout.ExpandWidth(false));

            switch(selected) {
                case 0: ComponentUtility.MoveComponentUp(target as Component); break;
                case 1: ComponentUtility.MoveComponentDown(target as Component); break;
            }
        }
        GUILayout.EndHorizontal();

        base.OnInspectorGUI();
    }
}