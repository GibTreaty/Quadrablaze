using UnityEngine;
using UnityEditor;

namespace YounGenTech.PoolGen {
    [CustomEditor(typeof(PoolManager))]
    public class PoolManagerInspector : Editor {

        static Texture2D emptyAssetPreview;

        PoolManager poolManager;

        SerializedProperty poolName;
        SerializedProperty hideSpawnedObjects;
        SerializedProperty poolGenPrefabs;

        SerializedProperty OnPool;
        SerializedProperty OnSpawn;
        SerializedProperty OnDespawn;

        Texture2D pixelTexture;

        Vector2 prefabSlider;
        Vector2 prefabSlider2;
        int prefabSelected = -1;

        void OnEnable() {
            poolManager = target as PoolManager;

            poolName = serializedObject.FindProperty("_poolName");
            hideSpawnedObjects = serializedObject.FindProperty("_hideSpawnedObjects");
            poolGenPrefabs = serializedObject.FindProperty("_poolGenPrefabs");

            OnPool = serializedObject.FindProperty("_onPool");
            OnSpawn = serializedObject.FindProperty("_onSpawn");
            OnDespawn = serializedObject.FindProperty("_onDespawn");

            pixelTexture = new Texture2D(1, 1);
            pixelTexture.wrapMode = TextureWrapMode.Repeat;
            pixelTexture.SetPixel(0, 0, Color.white);
            pixelTexture.Apply();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(poolName);
            EditorGUILayout.PropertyField(hideSpawnedObjects);

            int addSelected = -1;
            int removeSelected = -1;

            if(prefabSelected >= poolGenPrefabs.arraySize)
                prefabSelected = -1;

            if(!emptyAssetPreview) emptyAssetPreview = AssetPreview.GetAssetPreview(null);

            int viewHeight = 137;
            Rect prefabScrollRect = GUILayoutUtility.GetRect(0, viewHeight + 24, GUILayout.ExpandWidth(true));
            Vector2 wholePrefabButtonSize = new Vector2(68, viewHeight);
            Vector2 addRemoveButtonSize = new Vector2(21, (viewHeight / 2) + 2);
            float spacing = 4;

            float totalWidth = (wholePrefabButtonSize.x * poolManager.PrefabCount) + (addRemoveButtonSize.x * 2) + (spacing * 2);

            Rect prefabScrollViewRect = new Rect(0, 0, totalWidth, viewHeight);

            GUI.Box(prefabScrollRect, "");

            prefabSlider2 = GUI.BeginScrollView(prefabScrollRect, prefabSlider2, prefabScrollViewRect, true, false);
            {
                #region Add/Remove Buttons
                { //Left Add/Remove Buttons
                    Rect addButtonRect = new Rect(0, 0, addRemoveButtonSize.x, addRemoveButtonSize.y);
                    DrawSquare(addButtonRect, new Color(.5f, .8f, .5f));
                    if(GUI.Button(addButtonRect, new GUIContent("", "Add prefab to beginning"), GUI.skin.label)) {
                        addSelected = 0;
                        EditorGUI.FocusTextInControl("");
                    }
                    DrawSquare(new Rect(addButtonRect.center.x - 6, addButtonRect.center.y - 2, 11, 3), new Color(.2f, .2f, .2f));
                    DrawSquare(new Rect(addButtonRect.center.x - 2, addButtonRect.center.y - 6, 3, 11), new Color(.2f, .2f, .2f));

                    Rect removeButtonRect = new Rect(0, addButtonRect.yMax + 2, addRemoveButtonSize.x, addRemoveButtonSize.y);
                    DrawSquare(removeButtonRect, new Color(.8f, .5f, .5f));
                    if(GUI.Button(removeButtonRect, new GUIContent("", "Remove first prefab"), GUI.skin.label)) {
                        removeSelected = 0;
                        EditorGUI.FocusTextInControl("");
                    }
                    DrawSquare(new Rect(removeButtonRect.center.x - 6, removeButtonRect.center.y - 2, 11, 3), new Color(.2f, .2f, .2f));
                }
                #endregion

                float totalWeight = GetTotalWeight();
                Rect testScrollRect = new Rect(0, 0, prefabScrollRect.width, prefabScrollRect.height);

                for(int prefabIndex = 0; prefabIndex < poolGenPrefabs.arraySize; prefabIndex++) {
                    SerializedProperty prefabInfo = poolGenPrefabs.GetArrayElementAtIndex(prefabIndex);
                    SerializedProperty prefabObject = prefabInfo.FindPropertyRelative("_prefab");
                    SerializedProperty prefabID = prefabInfo.FindPropertyRelative("_id");
                    SerializedProperty prefabInitializationType = prefabInfo.FindPropertyRelative("_initializationType");
                    SerializedProperty active = prefabInfo.FindPropertyRelative("_active");

                    Rect wholePrefabButtonRect = new Rect(0, spacing, wholePrefabButtonSize.x, wholePrefabButtonSize.y);
                    wholePrefabButtonRect.x = addRemoveButtonSize.x + (wholePrefabButtonRect.width * prefabIndex) + spacing;

                    Rect prefabRect = new Rect(wholePrefabButtonRect.x + 1, wholePrefabButtonRect.y + 10, wholePrefabButtonRect.width - 2, wholePrefabButtonRect.width - 2);
                    Rect transformedPrefabRect = wholePrefabButtonRect;
                    transformedPrefabRect.x -= prefabSlider2.x;

                    bool isVisible = testScrollRect.Overlaps(transformedPrefabRect);

                    if(isVisible) {
                        #region Prefab Button
                        GameObject prefab = prefabObject.objectReferenceValue as GameObject;
                        bool previousActiveState = prefab && prefab.activeSelf;
                        if(prefab && !previousActiveState) prefab.SetActive(true);

                        Texture2D texture = prefabObject.objectReferenceValue ? AssetPreview.GetAssetPreview(prefabObject.objectReferenceValue) : emptyAssetPreview;
                        GUIContent buttonContent = new GUIContent(texture, prefabObject.objectReferenceValue ? prefabObject.objectReferenceValue.name : null);

                        if(prefab && !previousActiveState) prefab.SetActive(previousActiveState);

                        if(prefabSelected == prefabIndex)
                            DrawSquare(new Rect(prefabRect.x - 1, prefabRect.y - 1, prefabRect.width + 2, prefabRect.height + 1), new Color(0, 1, 1, .8f));
                        //DrawSquare(new Rect(wholePrefabButtonRect.x, wholePrefabButtonRect.y, wholePrefabButtonRect.width, wholePrefabButtonRect.width), new Color(0, 1, 1, .8f));

                        GUI.backgroundColor = Color.clear;

                        if(GUI.Button(prefabRect, buttonContent, GUI.skin.label)) {
                            prefabSelected = prefabSelected == prefabIndex ? -1 : prefabIndex;
                            EditorGUI.FocusTextInControl("");
                        }
                        #endregion

                        #region Weight Indicator
                        Rect weightIndicator = prefabRect;
                        float percentage = prefabInfo.FindPropertyRelative("_weight").floatValue / totalWeight;
                        weightIndicator.height = Mathf.Clamp01(percentage) * prefabRect.height;
                        weightIndicator.y += prefabRect.height - weightIndicator.height;

                        DrawSquare(weightIndicator, new Color(0, 0, 1, .2f));
                        #endregion

                        #region Active Indicator
                        Rect activeIndicator = new Rect(0, 0, 0, 0);
                        activeIndicator.width = 30;
                        activeIndicator.height = 12;
                        activeIndicator.center = new Vector2(wholePrefabButtonRect.center.x, activeIndicator.center.y);

                        DrawSquare(activeIndicator, active.boolValue ? Color.green : Color.red);
                        if(GUI.Button(activeIndicator, new GUIContent("", active.boolValue ? "Deactivate" : "Activate"), GUI.skin.label))
                            active.boolValue = !active.boolValue;
                        #endregion

                        #region Add/Remove Buttons
                        float halfWidth = wholePrefabButtonRect.width / 2;

                        Rect addButtonRect = new Rect(wholePrefabButtonRect.center.x, prefabRect.yMax + 1, 0, 15);
                        addButtonRect.xMax += halfWidth - 3;
                        DrawSquare(addButtonRect, new Color(.5f, .8f, .5f));
                        if(GUI.Button(addButtonRect, new GUIContent("", "Add prefab after this"), GUI.skin.label)) {
                            addSelected = prefabIndex + 1;
                            EditorGUI.FocusTextInControl("");
                        }
                        DrawSquare(new Rect(addButtonRect.center.x - 6, addButtonRect.center.y - 2, 11, 3), new Color(.2f, .2f, .2f));
                        DrawSquare(new Rect(addButtonRect.center.x - 2, addButtonRect.center.y - 6, 3, 11), new Color(.2f, .2f, .2f));

                        Rect removeButtonRect = new Rect(wholePrefabButtonRect.center.x, prefabRect.yMax + 1, 0, 15);
                        removeButtonRect.xMin -= halfWidth - 3;
                        DrawSquare(removeButtonRect, new Color(.8f, .5f, .5f));
                        if(GUI.Button(removeButtonRect, new GUIContent("", "Remove this prefab"), GUI.skin.label)) {
                            removeSelected = prefabIndex;
                            EditorGUI.FocusTextInControl("");
                        }
                        DrawSquare(new Rect(removeButtonRect.center.x - 6, removeButtonRect.center.y - 2, 11, 3), new Color(.2f, .2f, .2f));
                        #endregion

                        #region ID
                        Rect idRect = new Rect(wholePrefabButtonRect.center.x, removeButtonRect.yMax, 0, 15);
                        idRect.xMin -= halfWidth - 3;
                        idRect.xMax += halfWidth - 3;
                        DrawSquare(idRect, new Color(.4f, .4f, .4f, .8f));
                        GUI.Label(idRect, "ID:" + prefabID.intValue);
                        #endregion

                        #region Initialization Type
                        Rect initializationRect = new Rect(wholePrefabButtonRect.center.x, idRect.yMax + 1, 0, 16);
                        initializationRect.xMin -= halfWidth - 3;
                        initializationRect.xMax += halfWidth - 3;
                        DrawSquare(initializationRect, new Color(.4f, .4f, .4f, .8f));
                        prefabInitializationType.enumValueIndex = EditorGUI.Popup(initializationRect, prefabInitializationType.enumValueIndex, prefabInitializationType.enumDisplayNames);
                        #endregion
                    }

                    GUI.backgroundColor = Color.white;
                }

                #region Add/Remove Buttons
                if(poolGenPrefabs.arraySize > 0) { //Right Add/Remove Buttons
                    float x = Mathf.Max(prefabScrollViewRect.xMax, prefabScrollRect.xMax - ((Mathf.Ceil(addRemoveButtonSize.x * .5f)) + 2)) - addRemoveButtonSize.x;
                    //x += spacing;

                    Rect addButtonRect = new Rect(x, 0, addRemoveButtonSize.x, addRemoveButtonSize.y);
                    DrawSquare(addButtonRect, new Color(.5f, .8f, .5f));
                    if(GUI.Button(addButtonRect, new GUIContent("", "Add prefab to end"), GUI.skin.label)) {
                        addSelected = poolGenPrefabs.arraySize;
                        EditorGUI.FocusTextInControl("");
                    }
                    DrawSquare(new Rect(addButtonRect.center.x - 6, addButtonRect.center.y - 2, 11, 3), new Color(.2f, .2f, .2f));
                    DrawSquare(new Rect(addButtonRect.center.x - 2, addButtonRect.center.y - 6, 3, 11), new Color(.2f, .2f, .2f));

                    Rect removeButtonRect = new Rect(x, addButtonRect.yMax + 2, addRemoveButtonSize.x, addRemoveButtonSize.y);
                    DrawSquare(removeButtonRect, new Color(.8f, .5f, .5f));
                    if(GUI.Button(removeButtonRect, new GUIContent("", "Remove last prefab"), GUI.skin.label)) {
                        removeSelected = poolGenPrefabs.arraySize - 1;
                        EditorGUI.FocusTextInControl("");
                    }
                    DrawSquare(new Rect(removeButtonRect.center.x - 6, removeButtonRect.center.y - 2, 11, 3), new Color(.2f, .2f, .2f));
                }
                #endregion
                GUI.color = Color.white;
            }
            GUI.EndScrollView();

            if(prefabSelected > -1)
                for(int prefabIndex = 0; prefabIndex < poolGenPrefabs.arraySize; prefabIndex++) {
                    SerializedProperty prefabInfo = poolGenPrefabs.GetArrayElementAtIndex(prefabIndex);

                    prefabInfo.isExpanded = prefabIndex == prefabSelected;
                }

            if(addSelected > -1) {
                if(prefabSelected > addSelected) prefabSelected++;

                poolGenPrefabs.InsertArrayElementAtIndex(addSelected);
                serializedObject.ApplyModifiedProperties();

                SerializedProperty prefabInfo = poolGenPrefabs.GetArrayElementAtIndex(addSelected);
                SerializedProperty active = prefabInfo.FindPropertyRelative("_active");
                SerializedProperty prefabID = prefabInfo.FindPropertyRelative("_id");
                SerializedProperty weight = prefabInfo.FindPropertyRelative("_weight");
                SerializedProperty prefab = prefabInfo.FindPropertyRelative("_prefab");
                SerializedProperty poolSize = prefabInfo.FindPropertyRelative("_poolSize");
                SerializedProperty canExpand = prefabInfo.FindPropertyRelative("_canExpand");
                SerializedProperty initializationType = prefabInfo.FindPropertyRelative("_initializationType");
                SerializedProperty timeToDespawn = prefabInfo.FindPropertyRelative("_timeToDespawn");
                SerializedProperty reuseSpawned = prefabInfo.FindPropertyRelative("_reuseSpawned");
                SerializedProperty isNetworked = prefabInfo.FindPropertyRelative("_isNetworked");

                prefabID.intValue = PoolPrefab.GenerateUniqueID(poolManager.PoolGenPrefabs);

                active.boolValue = true;
                weight.floatValue = 1;
                prefab.objectReferenceValue = null;
                poolSize.intValue = 1;

                var enumArray = System.Enum.GetNames(typeof(PoolManager.PoolInitializationType));

                for(int i = 0; i < enumArray.Length; i++)
                    if(enumArray[i] == PoolManager.PoolInitializationType.Start.ToString()) {
                        initializationType.enumValueIndex = i;
                        break;
                    }

                canExpand.boolValue = false;
                timeToDespawn.floatValue = 0;
                reuseSpawned.intValue = 0;
                isNetworked.boolValue = false;
            }

            if(removeSelected > -1 && poolGenPrefabs.arraySize > removeSelected) {
                if(prefabSelected >= removeSelected) prefabSelected--;

                poolGenPrefabs.DeleteArrayElementAtIndex(removeSelected);
                serializedObject.ApplyModifiedProperties();
            }

            if(prefabSelected > -1) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    SerializedProperty prefabInfo = poolGenPrefabs.GetArrayElementAtIndex(prefabSelected);

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.enabled = Application.isPlaying;
                        if(poolManager.IsObjectContainerInitialized(poolManager.PoolGenPrefabs[prefabSelected].ID)) {
                            if(GUILayout.Button("Spawn", GUILayout.ExpandWidth(false)))
                                poolManager.Spawn(prefabSelected);

                            var objectContainer = poolManager.GetObjectPool(poolManager.PoolGenPrefabs[prefabSelected].ID);

                            GUI.enabled = objectContainer.SpawnedObjects.Count > 0;

                            if(GUILayout.Button("Despawn All", GUILayout.ExpandWidth(false)))
                                poolManager.DespawnAll(prefabSelected);
                        }
                        else {
                            if(GUILayout.Button("Initialize Pool", GUILayout.ExpandWidth(false)))
                                poolManager.InitializePool(prefabSelected);
                        }

                        GUI.enabled = !Application.isPlaying;
                        if(GUILayout.Button("Generate Unique ID", GUILayout.ExpandWidth(false))) {
                            SerializedProperty prefabID = prefabInfo.FindPropertyRelative("_id");

                            prefabID.intValue = PoolPrefab.GenerateUniqueID(poolManager.PoolGenPrefabs);
                        }
                        GUI.enabled = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    SerializedProperty reuseSpawned = prefabInfo.FindPropertyRelative("_reuseSpawned");
                    SerializedProperty isNetworked = prefabInfo.FindPropertyRelative("_isNetworked");

                    foreach(SerializedProperty properties in prefabInfo) {
                        EditorGUILayout.PropertyField(properties, true);
                        bool richText = EditorStyles.helpBox.richText;
                        EditorStyles.helpBox.richText = true;
                        if(properties.name == reuseSpawned.name && properties.boolValue && isNetworked.boolValue) {
                            EditorGUILayout.HelpBox("It is not recommended to have <b>Reuse Spawned</b> and <b>Is Networked</b> on at the same time.\nNetworked objects that are still being used may end up being despawned on the client but not on the server.\nUse at your own risk.", MessageType.Warning);
                        }
                        EditorStyles.helpBox.richText = richText;
                    }

                    //if(reuseSpawned.boolValue) {
                    //    //if(prefabInfo.FindPropertyRelative("_reuseSpawned").boolValue && prefabInfo.FindPropertyRelative("_isNetworked").boolValue) {
                    //    EditorGUILayout.HelpBox("It is not recommended to have Reuse Spawned on with a networked pool. Networked Objects that are being used may end up being despawned on the client but not on the server. Use at your own risk.", MessageType.Warning);
                    //}
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.PropertyField(OnPool);
            EditorGUILayout.PropertyField(OnSpawn);
            EditorGUILayout.PropertyField(OnDespawn);

            serializedObject.ApplyModifiedProperties();

            if(poolManager.ObjectContainers != null && poolManager.ObjectContainers.Count > 0) {
                Repaint();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    foreach(var container in poolManager.ObjectContainers.Values)
                        if(container.PoolPrefab.Prefab)
                            EditorGUILayout.LabelField(container.ToString());
                }
                EditorGUILayout.EndVertical();
            }
        }

        void DrawSquare(Rect rect) {
            GUI.DrawTexture(rect, pixelTexture);
        }
        void DrawSquare(Rect rect, Color color) {
            DrawSquare(rect, color, Color.white);
        }
        void DrawSquare(Rect rect, Color color, Color switchBackTo) {
            GUI.color = color;
            DrawSquare(rect);
            GUI.color = switchBackTo;
        }

        float GetTotalWeight() {
            float weight = 0;

            foreach(SerializedProperty prefabInfo in poolGenPrefabs)
                weight += prefabInfo.FindPropertyRelative("_weight").floatValue;

            return weight;
        }
    }
}