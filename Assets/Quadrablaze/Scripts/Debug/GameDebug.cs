using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class GameDebug {

    #region Properties
    public static string LogTagFilter {
        get { return DebugObject.LogTagFilter; }
        set { DebugObject.LogTagFilter = value; }
    }

    public static List<GameDebugMessage> LogMessages {
        get { return DebugObject.LogMessages; }
        set { DebugObject.LogMessages = value; }
    }

    public static Vector3 ScrollPosition {
        get { return DebugObject.ScrollPosition; }
        set { DebugObject.ScrollPosition = value; }
    }

    public static GameDebugObject DebugObject {
        get { return GameDebugObject.GetDebugObject(); }
    }
    #endregion

    public static void Clear() {
        LogMessages.Clear();
    }

    public static int CountTags() {
        int count = 0;

        //for(int i = 0; i < LogMessages.Count; i++) {
        //    if(!string.IsNullOrEmpty(LogMessages[i].tag.Trim())) {

        //    }
        //}

        return count;
    }

    public static List<GameDebugMessage> GetFilteredList() {
        return LogMessages.Where(s => s.tag.Contains(LogTagFilter)).ToList();
    }

    public static string[] GetTags() {
        List<string> tags = new List<string>();

        for(int i = 0; i < LogMessages.Count; i++) {
            string[] tagArray = LogMessages[i].Tags;

            for(int n = 0; n < tagArray.Length; n++)
                if(!tags.Contains(tagArray[n])) tags.Add(tagArray[n]);
        }

        return tags.OrderBy(s => s).ToArray();
    }

    public static void Log(string message) {
        LogMessages.Add(new GameDebugMessage(message, null));
    }
    public static void Log(string message, string tag) {
        LogMessages.Add(new GameDebugMessage(message, tag, null));
    }
    public static void Log(string message, string tag, Object context) {
        LogMessages.Add(new GameDebugMessage(message, tag, context));
    }

    public static void Log(GUIContent message) {
        LogMessages.Add(new GameDebugMessage(message, null));
    }
    public static void Log(GUIContent message, string tag) {
        LogMessages.Add(new GameDebugMessage(message, tag, null));
    }
    public static void Log(GUIContent message, string tag, Object context) {
        LogMessages.Add(new GameDebugMessage(message, tag, context));
    }
}

public class GameDebugObject : ScriptableObject {

    static GameDebugObject DebugObject { get; set; }

    [SerializeField]
    List<GameDebugMessage> _log = new List<GameDebugMessage>();

    [SerializeField]
    string _logTagFilter = "";

    [SerializeField]
    Vector3 _scrollPosition;

    #region Properties
    public string LogTagFilter {
        get { return _logTagFilter; }
        set { _logTagFilter = value; }
    }

    public List<GameDebugMessage> LogMessages {
        get { return _log; }
        set { _log = value; }
    }

    public Vector3 ScrollPosition {
        get { return _scrollPosition; }
        set { _scrollPosition = value; }
    }
    #endregion

    public static GameDebugObject GetDebugObject() {
        if(!DebugObject) {
            DebugObject = CreateInstance<GameDebugObject>();
            DebugObject.hideFlags = HideFlags.HideAndDontSave;
        }

        return DebugObject;
    }
}

[System.Serializable]
public struct GameDebugMessage {

    public GUIContent message;
    public string tag;
    public Object context;
    public string stackTraceInfo;

    public string[] Tags {
        get {
            //return tag.Split(',').Select(s => s.Trim().OrderBy(s => s)).ToArray<string>();

            string[] splitString = tag.Split(',');

            for(int i = 0; i < splitString.Length; i++)
                splitString[i] = splitString[i].Trim();

            return splitString.OrderBy(s => s).ToArray();

            //return from s in tag.Split(',')
            //       let s = s.Trim()
            //       order by s;
        }
    }

    public GameDebugMessage(string message, Object context) : this(new GUIContent(message), "", context) { }
    public GameDebugMessage(string message, string tag, Object context) : this(new GUIContent(message), tag, context) { }

    public GameDebugMessage(GUIContent message, Object context) : this(message, "", context) { }
    public GameDebugMessage(GUIContent message, string tag, Object context) {
        this.message = message;
        this.tag = tag;
        this.context = context;

        stackTraceInfo = StackTraceUtility.ExtractStackTrace();

        stackTraceInfo = stackTraceInfo.Remove(0, stackTraceInfo.IndexOf("\n") + 1);
        stackTraceInfo = stackTraceInfo.Remove(0, stackTraceInfo.IndexOf("\n") + 1);
        stackTraceInfo = stackTraceInfo.Remove(0, stackTraceInfo.IndexOf("\n") + 1);
    }
}