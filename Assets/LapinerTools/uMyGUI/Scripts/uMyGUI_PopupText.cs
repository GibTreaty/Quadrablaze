using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_PopupText : uMyGUI_PopupButtons
	{
		[SerializeField]
		protected Text m_header;
		[SerializeField]
		protected Text m_body;

        [SerializeField]
        protected TMPro.TMP_Text m_tExtMeshProHeader;
        [SerializeField]
        protected TMPro.TMP_Text m_textMeshProBody;

        [SerializeField]
		protected bool m_useExplicitNavigation = false;

		protected bool m_isFirstFrameShown = false;

		public virtual uMyGUI_PopupText SetText(string p_headerText, string p_bodyText)
		{
			if (m_header != null)			
				m_header.text = p_headerText;
			
			if (m_body != null)			
				m_body.text = p_bodyText;			

            if(m_tExtMeshProHeader)
                m_tExtMeshProHeader.text = p_headerText;

            if(m_textMeshProBody)
                m_textMeshProBody.text = p_bodyText;

            return this;
		}

		public override void Show ()
		{
			base.Show ();
			m_isFirstFrameShown = true;
		}

		public virtual void LateUpdate()
		{
			if (m_isFirstFrameShown)
			{
				m_isFirstFrameShown = false;

				if (m_useExplicitNavigation)
				{
					// iterate all active buttons and set explicit navigation on them
					List<Button> btns = new List<Button>();
					for (int i = 0; i < m_buttons.Length; i++)
					{
						if (m_buttons[i] != null && m_buttons[i].gameObject.activeSelf && m_buttons[i].GetComponentInChildren<Button>() != null)
						{
							btns.Add(m_buttons[i].GetComponentInChildren<Button>());
						}
					}
					for (int i = 0; i < btns.Count; i++)
					{
						Button btn = btns[i];
						Navigation btnNav = btn.navigation;
						btnNav.mode = Navigation.Mode.Explicit;
						if (i > 0)
						{
							btnNav.selectOnLeft = btns[i-1];
						}
						if (i < btns.Count - 1)
						{
							btnNav.selectOnRight = btns[i+1];
						}
						btn.navigation = btnNav;
					}
				}
			}
		}
	}
}
