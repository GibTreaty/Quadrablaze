using UnityEngine;
using UnityEngine.UI;

namespace LapinerTools.uMyGUI
{
	/// <summary>
	/// Fix for a nasty Unity bug:
	/// Unity Forum: http://forum.unity3d.com/threads/vertical-scroll-bar-handle-top-bottom-becomes-nan-with-scroll-view.285490/
	/// Unity Issue Tracker: http://issuetracker.unity3d.com/issues/m-transforminfo-dot-localaabb-dot-isvalid-when-reimporting-the-example-project
	/// </summary>
	public class uMyGUI_ScrollbarHandleUnityFix : MonoBehaviour
	{
		[SerializeField]
		private Vector2 m_anchorMin = new Vector2(0.8f, 0f);
		[SerializeField]
		private Vector2 m_anchorMax = new Vector2(1f, 1f);
		[SerializeField]
		private Vector2 m_pivot = new Vector2(0.5f, 0.5f);
		[SerializeField]
		private Vector2 m_offsetMin = new Vector2(-10f, -10f);
		[SerializeField]
		private Vector2 m_offsetMax = new Vector2(10f, 10f);
		
		public void Awake()
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			// needed to set anchoredPosition3D if not executed anchoredPosition3D will not be changed due to NaN
			rectTransform.localPosition = Vector3.zero;
			// remove NaN values
			rectTransform.anchoredPosition3D = Vector3.zero;
			// reset broken values to default
			rectTransform.anchorMin = m_anchorMin;
			rectTransform.anchorMax = m_anchorMax;
			rectTransform.pivot = m_pivot;
			rectTransform.offsetMin = m_offsetMin;
			rectTransform.offsetMax = m_offsetMax;
		}
	}
}
