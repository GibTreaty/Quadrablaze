using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_Dropdown : MonoBehaviour
	{
		[SerializeField]
		private Button m_button = null;
		[SerializeField]
		private Text m_text = null;

		[SerializeField]
		private RectTransform m_entriesRoot = null;
		[SerializeField]
		private RectTransform m_entriesBG = null;
		[SerializeField]
		private Scrollbar m_entriesScrollbar = null;
		[SerializeField]
		private Button m_entryButton = null;
		[SerializeField]
		private int m_entrySpacing = 5;

		[SerializeField]
		private string m_staticText = "";
		[SerializeField]
		private string m_nothingSelectedText = "";
		[SerializeField]
		protected bool m_improveNavigationFocus = true;

		[SerializeField]
		private string[] m_entries = new string[0];
		public string[] Entries
		{
			get { return m_entries; }
			set { m_entries = value; HideEntries(); }
		}

		[SerializeField]
		private int m_selectedIndex = -1;
		public int SelectedIndex
		{
			get { return m_selectedIndex; }
			set { m_selectedIndex = Mathf.Clamp(value, -1, m_entries.Length - 1); }
		}
		
		public event System.Action<int> OnSelected;

		public void Select(int p_selectedIndex)
		{
			int newIndex = Mathf.Clamp(p_selectedIndex, -1, m_entries.Length - 1);
			bool isChanged = newIndex != m_selectedIndex;
			m_selectedIndex = newIndex;
			UpdateText();
			if (isChanged && OnSelected != null) { OnSelected(m_selectedIndex); }
		}

		private void Start()
		{
			if (m_text != null)
			{
				UpdateText();
			}
			else
			{
				Debug.LogError("uMyGUI_Dropdown: m_text must be set in the inspector!");
			}

			if (m_button != null)
			{
				m_button.onClick.AddListener(OnClick);
			}
			else
			{
				Debug.LogError("uMyGUI_Dropdown: m_button must be set in the inspector!");
			}

			if (m_entriesRoot != null && m_entriesBG != null)
			{
				HideEntries();
			}
			else
			{
				Debug.LogError("uMyGUI_Dropdown: m_entriesRoot and m_entriesBG must be set in the inspector!");
			}
		}

		private void LateUpdate()
		{
			if (m_improveNavigationFocus)
			{
				EventSystem eventSys = EventSystem.current;
				if (eventSys != null)
				{
					if (eventSys.currentSelectedGameObject == null || !eventSys.currentSelectedGameObject.activeInHierarchy)
					{
						// if selection is lost, then ...
						if (eventSys.lastSelectedGameObject != null && eventSys.lastSelectedGameObject.activeInHierarchy)
						{
							// select last selected if it is still active
							eventSys.SetSelectedGameObject(eventSys.lastSelectedGameObject);
						}
						else if (m_entriesRoot != null)
						{
							if (m_entriesRoot.gameObject.activeSelf && m_entriesRoot.childCount > 0 && m_entriesRoot.GetChild(0).GetComponentInChildren<Button>() != null)
							{
								// select first entry if entries are shown
								m_entriesRoot.GetChild(0).GetComponentInChildren<Button>().Select();
							}
							else if (m_button != null)
							{
								// select dropdown root button if no entries are shown
								m_button.Select();
							}
						}
					}
				}
			}
		}

		private void OnClick()
		{
			if (m_entriesRoot != null)
			{
				if (m_entriesRoot.gameObject.activeSelf)
				{
					HideEntries();
				}
				else
				{
					ShowEntries();
				}
			}
		}

		private void ShowEntries()
		{
			if (m_entriesRoot != null && m_entriesBG != null && m_entryButton != null)
			{
				m_entriesRoot.gameObject.SetActive(true);
				ClearEntries();

				// the rect height is (entry btn height + entry spacing) * entries count + entry spacing
				float height = (GetHeight(m_entryButton) + m_entrySpacing) * m_entries.Length + m_entrySpacing;
				m_entriesBG.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

				// create entry buttons
				RectTransform rTransformTarget = m_entryButton.GetComponent<RectTransform>();
				SetText(m_entryButton, m_entries[0]);
				SetOnClick(m_entryButton, 0);
				m_entryButton.interactable = 0 != m_selectedIndex;
				for (int i = 1; i < m_entries.Length; i++)
				{
					Button entryBtn = (Button)Instantiate(m_entryButton);
					entryBtn.interactable = i != m_selectedIndex;
					RectTransform rTransform = entryBtn.GetComponent<RectTransform>();
					rTransform.SetParent(rTransformTarget.parent, true);
					rTransform.localScale = rTransformTarget.localScale;
					rTransform.offsetMin = rTransformTarget.offsetMin;
					rTransform.offsetMax = rTransformTarget.offsetMax;
					rTransform.localPosition = rTransformTarget.localPosition + Vector3.down * i * (GetHeight(m_entryButton) + m_entrySpacing);
					SetText(entryBtn, m_entries[i]);
					SetOnClick(entryBtn, i);
				}

				// hide or show scroll
				if (m_entriesScrollbar != null)
				{
					StartCoroutine(UpdateScrollBarVisibility());
				}
			}
		}

		private void HideEntries()
		{
			if (m_entriesRoot != null)
			{
				ClearEntries();
				m_entriesRoot.gameObject.SetActive(false);
			}
		}

		private void ClearEntries()
		{
			if (m_entriesBG != null && m_entryButton != null)
			{
				for (int i = 0; i < m_entriesBG.childCount; i++)
				{
					if (m_entriesBG.GetChild(i) != m_entryButton.transform)
					{
						Destroy(m_entriesBG.GetChild(i).gameObject);
					}
				}
			}
		}

		private void UpdateText()
		{
			if (m_text != null)
			{
				bool isSelectedIndexValid = m_selectedIndex >= 0 && m_selectedIndex < m_entries.Length;
				m_text.text = m_staticText + (isSelectedIndexValid ? m_entries[m_selectedIndex] : m_nothingSelectedText);
			}
		}
		
		private void SetOnClick(Button p_button, int p_selectedIndex)
		{
			p_button.onClick.RemoveAllListeners();
			p_button.onClick.AddListener(() =>
			{
				Select(p_selectedIndex);
				HideEntries();
			});
		}

		private void SetText(Button p_button, string p_text)
		{
			Text text = p_button.GetComponentInChildren<Text>();
			if (text != null) { text.text = p_text; }
		}

		private float GetHeight(Button p_button)
		{
			return p_button != null ? GetHeight(p_button.GetComponent<RectTransform>()) : 0;
		}
		
		private float GetHeight(RectTransform p_rTransform)
		{
			return p_rTransform != null ? p_rTransform.rect.yMax - p_rTransform.rect.yMin : 0;
		}

		private IEnumerator UpdateScrollBarVisibility()
		{
			yield return new WaitForEndOfFrame();

			if (m_entriesScrollbar != null)
			{
				m_entriesScrollbar.gameObject.SetActive(m_entriesScrollbar.size < 0.985f);
			}
		}
	}
}
