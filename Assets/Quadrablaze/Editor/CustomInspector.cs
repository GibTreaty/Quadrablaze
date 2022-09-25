using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

public class CustomInspector : EditorWindow {

    static string[] titleToolbar = new string[] { "^", "v" };

    GameObject _currentGameObject;
    List<Editor> _currentEditors = new List<Editor>();
    Dictionary<Editor, bool> _editorFoldout = new Dictionary<Editor, bool>();
    //ReorderableList reorderableList;
    Vector2 scroll;

    [MenuItem("Window/Inspector 2")]
    static void Init() {
        CustomInspector window = GetWindow<CustomInspector>("Inspector 2");
        window.autoRepaintOnSceneChange = true;
    }

    void Update() {
        if(Selection.activeGameObject != _currentGameObject) {
            _currentGameObject = Selection.activeGameObject;

            foreach(Editor editor in _currentEditors)
                DestroyImmediate(editor);

            _currentEditors.Clear();
            _editorFoldout.Clear();

            if(_currentGameObject)
                foreach(Object component in _currentGameObject.GetComponents<Object>()) {
                    Editor editor = Editor.CreateEditor(component);

                    _currentEditors.Add(editor);
                    _editorFoldout[editor] = true;
                }
        }

        if(_currentGameObject)
            Repaint();
    }

    void OnGUI() {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        {
            if(_currentEditors.Count > 0) {
                foreach(Editor editor in _currentEditors) {
                    _editorFoldout[editor] = EditorGUILayout.Foldout(_editorFoldout[editor], editor.GetPreviewTitle());

                    if(_editorFoldout[editor]) {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            int tool = -1;

                            EditorGUILayout.BeginHorizontal();
                            {
                                tool = GUILayout.Toolbar(-1, titleToolbar, GUILayout.ExpandWidth(false));

                                //GUILayout.Label(editor.GetPreviewTitle());
                            }
                            EditorGUILayout.EndHorizontal();

                            editor.OnInspectorGUI();

                            switch(tool) {
                                case 0:
                                    ComponentUtility.MoveComponentUp(editor.target as Component);
                                    break;
                                case 1:
                                    ComponentUtility.MoveComponentDown(editor.target as Component);
                                    break;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    //void DrawHeader(
}