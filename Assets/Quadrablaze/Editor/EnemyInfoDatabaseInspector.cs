using UnityEngine;
using UnityEditor;
using System.Collections;
using YounGenTech.PoolGen;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using System;

namespace Quadrablaze {
    [CustomEditor(typeof(EnemyInfoDatabase))]
    public class EnemyInfoDatabaseInspector : Editor {

        PoolManager poolManager;

        EnemyInfoDatabase enemyInfoDatabase;

        SerializedProperty entities;

        ReorderableList entityList;

        void OnEnable() {
            enemyInfoDatabase = target as EnemyInfoDatabase;

            entities = serializedObject.FindProperty("_entities");

            entityList = new ReorderableList(serializedObject, entities);
            entityList.drawElementCallback += (rect, index, isActive, isFocused) => OnDrawEntityElement(rect, index, isActive, isFocused, entities);
            entityList.drawHeaderCallback += (rect) => OnDrawEntityElementHeader(rect, "Scriptable Minion Entities");
            entityList.elementHeightCallback += OnElementHeight;
        }

        float OnElementHeight(int index) {
            return EditorGUI.GetPropertyHeight(entities.GetArrayElementAtIndex(index)) + 70;
        }

        void OnDrawEntityElementHeader(Rect rect, string title) {
            GUI.Label(rect, title);
        }

        void OnDrawEntityElement(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty list) {
            var item = list.GetArrayElementAtIndex(index);
            var content = new GUIContent();

            if(item.objectReferenceValue != null) {
                Texture2D icon = null;

                if(item.objectReferenceValue is EnemyEntity entity)
                    icon = AssetPreview.GetAssetPreview(entity.Prefab);
                else if(item.objectReferenceValue is Entities.ScriptableMinionEntity minionEntity)
                    icon = AssetPreview.GetAssetPreview(minionEntity.OriginalGameObject);

                content.image = icon;
            }

            var previewRect = rect;
            previewRect.width = 96;
            GUI.Box(previewRect, content);

            var objectFieldRect = rect;
            objectFieldRect.width -= previewRect.width;
            objectFieldRect.x += previewRect.width;
            objectFieldRect.y += 32;
            objectFieldRect.height -= 64;

            EditorGUI.ObjectField(objectFieldRect, item, GUIContent.none);
        }

        void OnDrawDropEntity<T>(ReorderableList reorderableList, SerializedProperty list) {
            var currentEvent = Event.current;
            var dropArea = GUILayoutUtility.GetLastRect();

            dropArea.yMin += reorderableList.headerHeight;

            if(dropArea.Contains(currentEvent.mousePosition)) {
                if(DragAndDrop.visualMode == DragAndDropVisualMode.Copy) {
                    var boxContent = new GUIContent($"Drag n' Drop to add new {typeof(T).Name}");

                    GUI.Box(dropArea, boxContent);
                }

                switch(currentEvent.type) {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if(currentEvent.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag();

                            foreach(var item in DragAndDrop.objectReferences)
                                if(item is T)
                                    AddItemToList(item);
                        }

                        break;
                }
            }

            GUILayout.Space(40);

            void AddItemToList(UnityEngine.Object obj) {
                list.InsertArrayElementAtIndex(list.arraySize);
                var newItem = list.GetArrayElementAtIndex(list.arraySize - 1);

                newItem.objectReferenceValue = obj;
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            entityList.DoLayoutList();
            EditorGUILayout.EndVertical();
            OnDrawDropEntity<Entities.ScriptableMinionEntity>(entityList, entities);

            serializedObject.ApplyModifiedProperties();
        }
    }
}