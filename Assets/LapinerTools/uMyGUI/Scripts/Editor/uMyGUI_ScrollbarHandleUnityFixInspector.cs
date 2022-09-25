#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace LapinerTools.uMyGUI
{
	/// <summary>
	/// Fix for a nasty Unity bug:
	/// Unity Forum: http://forum.unity3d.com/threads/vertical-scroll-bar-handle-top-bottom-becomes-nan-with-scroll-view.285490/
	/// Unity Issue Tracker: http://issuetracker.unity3d.com/issues/m-transforminfo-dot-localaabb-dot-isvalid-when-reimporting-the-example-project
	/// </summary>
	/// 
	[CustomEditor(typeof(uMyGUI_ScrollbarHandleUnityFix))] 
	public class uMyGUI_ScrollbarHandleUnityFixInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("FIX")) { ((uMyGUI_ScrollbarHandleUnityFix)target).Awake(); }
		}
	}
}
#endif
