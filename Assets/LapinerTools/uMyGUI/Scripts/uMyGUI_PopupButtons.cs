using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_PopupButtons : uMyGUI_Popup
	{
		[SerializeField]
		protected RectTransform[] m_buttons =  new RectTransform[0];
		[SerializeField]
		protected string[] m_buttonNames = new string[0];
		[SerializeField]
		protected bool m_improveNavigationFocus = true;

		protected Dictionary<string, System.Action> m_onBtnClickCallbacks = new Dictionary<string, System.Action>();

		protected AudioSource[] m_audioSources = new AudioSource[0];
		protected bool m_isClosing = false;
		protected bool m_isCloseCanceled = false;

		public override void Show()
		{
			if (m_isClosing)
			{
				Hide();
				m_isCloseCanceled = true;
			}
			base.Show();
		}

		public override void Hide()
		{
			base.Hide();
			// hide all buttons
			for (int i = 0; i < m_buttons.Length; i++)
			{
				if (m_buttons[i] != null)
				{
					m_buttons[i].gameObject.SetActive(false);
				}
			}
			// remove all button callbacks
			m_onBtnClickCallbacks.Clear();
		}

		public virtual uMyGUI_PopupButtons ShowButton(string p_buttonName)
		{
			return ShowButton(p_buttonName, null);
		}

		public virtual uMyGUI_PopupButtons ShowButton(string p_buttonName, System.Action p_callback)
		{
			// search button
			for (int i = 0; i < m_buttons.Length; i++)
			{
				if (m_buttons[i] != null && m_buttonNames[i] == p_buttonName)
				{
					// show button
					m_buttons[i].gameObject.SetActive(true);
					// focus
					if (m_improveNavigationFocus)
					{
						Selectable selectable = m_buttons[i].GetComponentInChildren<Selectable>();
						if (selectable != null)
						{
							selectable.Select();
						}
					}
					// save callback
					if (p_callback != null)
					{
						m_onBtnClickCallbacks.Add(p_buttonName, p_callback);
					}
					return this;
				}
			}
			Debug.LogError("uMyGUI_PopupButtons: ShowButton: could not find button with name '" + p_buttonName + "'!");
			return this;
		}

		public virtual void OnButtonClick(RectTransform p_btn)
		{
			m_isClosing = true;
			m_isCloseCanceled = false;
			for (int i = 0; i < m_buttons.Length; i++)
			{
				if (m_buttons[i] == p_btn)
				{
					// invoke callback
					System.Action callback;
					if (m_onBtnClickCallbacks.TryGetValue(m_buttonNames[i], out callback))
					{
						callback();
					}
					break;
				}
			}
			m_isClosing = false;
			if (!m_isCloseCanceled)
			{
				Hide();
			}
		}

		protected override void Start()
		{
			base.Start();
			if (m_buttons.Length != m_buttonNames.Length)
			{
				Debug.LogError("uMyGUI_PopupButtons: m_buttons and m_buttonNames must have the same length ("+m_buttons.Length+"!="+m_buttonNames.Length+")!");
			}

			// unparent audio sources (otherwise no sound when popup is not active)
			m_audioSources = GetComponentsInChildren<AudioSource>();
			for (int i = 0; i < m_audioSources.Length; i++)
			{
				m_audioSources[i].transform.parent = transform.parent;
				m_audioSources[i].name = name + "_" + m_audioSources[i].name;
			}
		}

		protected void OnDestroy()
		{
			for (int i = 0; i < m_audioSources.Length; i++)
			{
				if (m_audioSources[i] != null)
				{
					Destroy(m_audioSources[i].gameObject);
				}
			}
		}
	}
}
