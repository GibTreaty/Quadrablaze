using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Callbacks;
using System.IO;

namespace Quadrablaze.Menu {
    public class MenuTreeEditor : EditorWindow {

        ScriptableMenuTree activeMenuTree = null;
        Vector2 viewPosition;

        GUIStyle nodeStyle;
        GUIStyle inputStyle;
        GUIStyle outputStyle;

        static void Init(ScriptableMenuTree menuTree) {
            var editor = GetWindow<MenuTreeEditor>("Menu Tree Editor");

            editor.activeMenuTree = menuTree;
            editor.autoRepaintOnSceneChange = true;

            editor.nodeStyle = new GUIStyle();
            editor.nodeStyle.alignment = TextAnchor.UpperCenter;
            editor.nodeStyle.contentOffset = new Vector2(0, 10);
            editor.nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            editor.nodeStyle.border = new RectOffset(12, 12, 12, 12);

            editor.inputStyle = new GUIStyle();
            editor.inputStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            editor.inputStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            editor.inputStyle.border = new RectOffset(4, 4, 12, 12);

            editor.outputStyle = new GUIStyle();
            editor.outputStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            editor.outputStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            editor.outputStyle.border = new RectOffset(4, 4, 12, 12);
        }

        void OnGUI() {
            if(activeMenuTree == null) {
                Close();

                return;
            }

            if(activeMenuTree.Nodes.Contains(null))
                while(activeMenuTree.Nodes.Contains(null))
                    activeMenuTree.Nodes.Remove(null);
            //activeMenuTree.Nodes.RemoveAll(s => s == null);


            DisplayCurrentMenuTree();
            DisplayNodeEditor();
        }

        void DisplayCurrentMenuTree() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.ObjectField(activeMenuTree, typeof(ScriptableMenuTree), false, GUILayout.ExpandWidth(false));
            }
            EditorGUILayout.EndHorizontal();
        }

        void DisplayNodeEditor() {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button("+ Node", GUILayout.Width(200)))
                        NewNode();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginScrollView(Vector2.zero, GUI.skin.box);
                {
                    foreach(var node in activeMenuTree.Nodes)
                        if(node != null)
                            node.DrawGUI(nodeStyle, inputStyle, outputStyle);
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        void NewNode() {
            var node = CreateInstance<ScriptableMenuNode>();

            node.name = "Node";

            var currentAssetPath = AssetDatabase.GetAssetPath(activeMenuTree);
            currentAssetPath = currentAssetPath.Substring(0, currentAssetPath.Length - Path.GetFileName(currentAssetPath).Length);

            var newAssetPath = Path.Combine(currentAssetPath, node.name + ".asset");
            newAssetPath = AssetDatabase.GenerateUniqueAssetPath(newAssetPath);

            activeMenuTree.Nodes.Add(node);

            //AssetDatabase.AddObjectToAsset(node, activeMenuTree);
            EditorUtility.SetDirty(activeMenuTree);
            AssetDatabase.CreateAsset(node, newAssetPath);
            AssetDatabase.SaveAssets();
        }

        [OnOpenAsset]
        public static bool CheckDoubleClick(int instanceID, int line) {
            if(Selection.activeObject is ScriptableMenuTree) {
                Init(Selection.activeObject as ScriptableMenuTree);

                return true;
            }

            return false;
        }
    }
}