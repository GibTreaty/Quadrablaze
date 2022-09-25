using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class GameConsoleWindow : EditorWindow {

    public bool autoScroll = true;

    Texture2D pixelTexture;

    GameDebugMessage selectedMessage;
    Vector2 selectedMessagePreview;
    Color selectedColor;

    [MenuItem("Window/Game Console")]
    static void Init() {
        GetWindow<GameConsoleWindow>("Game Console").autoRepaintOnSceneChange = true;
    }

    void OnEnable() {
        Setup();
    }

    void Setup() {
        if(!pixelTexture) {
            pixelTexture = new Texture2D(1, 1);
            pixelTexture.wrapMode = TextureWrapMode.Repeat;
            pixelTexture.SetPixel(0, 0, Color.white);
            pixelTexture.Apply();
        }

        ColorUtility.TryParseHtmlString("#3e5f96", out selectedColor);
    }

    void Update() {
        Repaint();
    }

    void OnGUI() {
        Setup();

        string[] tagArray = GameDebug.GetTags();

        GUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                GameDebug.LogTagFilter = "";

            GameDebug.LogTagFilter = EditorGUILayout.TextField("Filter", GameDebug.LogTagFilter);

            int selected = EditorGUILayout.Popup(-1, tagArray, GUILayout.MaxWidth(100));

            if(selected > -1)
                GameDebug.LogTagFilter = tagArray[selected];
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            autoScroll = GUILayout.Toggle(autoScroll, "Auto Scroll");

            if(GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                GameDebug.Clear();

            //if(GUILayout.Button("Add Message", GUILayout.ExpandWidth(false)))
            //    GameDebug.Log("Test Message\nTurtles", "Tag1, Alpha,Diggidy2");

            //if(GUILayout.Button("Add Message2", GUILayout.ExpandWidth(false)))
            //    GameDebug.Log("Test Message 2", "Tag2,Beta,0123,  Xor,Alpha");
        }
        GUILayout.EndHorizontal();

        if(autoScroll) {
            var position = GameDebug.ScrollPosition;
            position.y = float.MaxValue;
            GameDebug.ScrollPosition = position;
        }

        GameDebug.ScrollPosition = GUILayout.BeginScrollView(GameDebug.ScrollPosition);
        {
            if(string.IsNullOrEmpty(GameDebug.LogTagFilter.Trim()))
                DisplayMessages(GameDebug.LogMessages);
            else
                DisplayMessages(GameDebug.GetFilteredList());
        }
        GUILayout.EndScrollView();

        GUILayout.BeginVertical(GUILayout.Height(100));
        selectedMessagePreview = GUILayout.BeginScrollView(selectedMessagePreview, GUI.skin.box);
        {
            if(selectedMessage.message != null)
                if(!string.IsNullOrEmpty(selectedMessage.tag))
                    GUILayout.Label("Tags: " + selectedMessage.tag);

            GUILayout.Label(selectedMessage.stackTraceInfo);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void DisplayMessages(List<GameDebugMessage> list) {
        for(int i = 0; i < list.Count; i++) {
            GameDebugMessage debugMessage = list[i];

            GUILayout.BeginVertical();
            {
                bool selected = debugMessage.message != null && selectedMessage.message == debugMessage.message;
                Rect messageRect = GUILayoutUtility.GetRect(debugMessage.message, GUI.skin.label);

                GUI.color = selected ? selectedColor : (i % 2 == 0 ? new Color(.23529f, .23529f, .23529f, 1) : Color.clear);
                GUI.DrawTexture(messageRect, pixelTexture);
                GUI.color = Color.white;

                if(GUI.Button(messageRect, debugMessage.message, GUI.skin.label))
                    if(!selected) {
                        selectedMessage = debugMessage;

                        if(selectedMessage.context)
                            EditorGUIUtility.PingObject(selectedMessage.context);
                    }
                    else {
                        selectedMessage = default(GameDebugMessage);
                    }
            }
            GUILayout.EndVertical();
        }
    }
}