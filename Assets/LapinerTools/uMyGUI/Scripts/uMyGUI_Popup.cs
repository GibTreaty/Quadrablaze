using UnityEngine;
using System.Collections;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_Popup : MonoBehaviour
	{
		public event System.Action OnShow;
		public event System.Action OnHide;

		public virtual bool IsShown
		{
			get
			{
				return gameObject.activeSelf;
			}
		}

		public virtual bool DestroyOnHide { get; set; }

		public virtual void Show()
		{
			gameObject.transform.SetAsLastSibling(); // bring to front
			gameObject.SetActive(true);

			if (OnShow != null) { OnShow(); }
		}

		public virtual void Hide()
		{
			gameObject.SetActive(false);

			if (OnHide != null) { OnHide(); }

			// skip first hide call (will be called during initialization)
			if (DestroyOnHide && m_createFrame != Time.frameCount && uMyGUI_PopupManager.IsInstanceSet)
			{
				// destroy on hide
				uMyGUI_PopupManager.Instance.StartCoroutine(DestroyOnEndOfFrame());
			}
		}

		protected int m_createFrame = 0;
		
		protected virtual void Awake()
		{
			m_createFrame = Time.frameCount;
		}

		protected virtual void Start()
		{
		}
		
		protected IEnumerator DestroyOnEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			if (uMyGUI_PopupManager.IsInstanceSet)
			{
				uMyGUI_PopupManager.Instance.RemovePopup(this);
				Destroy(gameObject);
			}
		}
	}
}
