using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_PageBox : MonoBehaviour
	{
		[SerializeField]
		private Button m_previousButton;
		[SerializeField]
		private Button m_nextButton;
		[SerializeField]
		private Button m_pageButton;
		
		[SerializeField]
		private int m_pageCount = 1;
		public int PageCount
		{
			get { return m_pageCount; }
			set { SetPageCount(value); }
		}

		[SerializeField]
		private int m_maxPageBtnCount = 9;
		public int MaxPageBtnCount
		{
			get { return m_maxPageBtnCount; }
			set { m_maxPageBtnCount = value; SetPageCount(PageCount); }
		}

		[SerializeField]
		private int m_selectedPage = 0;
		public int SelectedPage
		{
			get { return m_selectedPage; }
			set { SelectPage(value); }
		}

		private RectTransform m_rectTransform = null;
		public RectTransform RTransform { get{ return m_rectTransform!=null ? m_rectTransform : m_rectTransform = GetComponent<RectTransform>(); } }

		public event System.Action<int> OnPageSelected;

		private RectTransform m_pageButtonTransform;
		private RectTransform PageButtonTransform { get{ return m_pageButtonTransform!=null||m_pageButton==null ? m_pageButtonTransform : m_pageButtonTransform = m_pageButton.GetComponent<RectTransform>(); } }

		private int m_offset = 0;
		private List<Button> m_pageButtons = new List<Button>();

		public void SetPageCount(int p_newPageCount)
		{
			m_pageCount = Mathf.Max(1, (int)p_newPageCount);

			if (p_newPageCount <= 1)
			{
				gameObject.SetActive(false);
			}
			else if (m_pageButton != null)
			{
				gameObject.SetActive(true);
				UpdateUI();
			}
			else
			{
				Debug.LogError("uMyGUI_PageBox: SetPageCount: m_pageButton must be set in the inspector!");
			}
		}

		public void SelectPageAndCenterOffset(int p_selectedPage)
		{
			m_offset = Mathf.Min(m_pageCount - m_maxPageBtnCount, Mathf.Max(0, p_selectedPage - 1 - m_maxPageBtnCount / 2));
			SelectPage(p_selectedPage);
		}

		public void SelectPage(int p_selectedPage)
		{
			int nextPage = Mathf.Clamp(p_selectedPage, 0, m_pageCount);
			bool isPageChanged = nextPage != m_selectedPage;
			m_selectedPage = nextPage;
			UpdateUI();
			if (isPageChanged && OnPageSelected != null) { OnPageSelected(p_selectedPage); }
		}

		public void UpdateUI()
		{
			Clear();

			int buttonObjectCount = Mathf.Min(m_pageCount, m_maxPageBtnCount);

			// the rect width is prev btn width + next btn width + page btn width * page count
			float width = GetWidth(m_previousButton) + GetWidth(m_nextButton) + (GetWidth(m_pageButton) * buttonObjectCount);
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

			// calculate the range of the shown buttons
			int idealOffsetMin = Mathf.Max(0, m_selectedPage - m_maxPageBtnCount);
			int idealOffsetMax = Mathf.Max(0, Mathf.Min(m_pageCount - m_maxPageBtnCount, m_selectedPage - 1));
			if (idealOffsetMin - 1 >= m_offset)
			{
				m_offset = idealOffsetMin;
			}
			else if (idealOffsetMax + 1 <= m_offset)
			{
				m_offset = idealOffsetMax;
			}

			// create page buttons
			SetText(m_pageButton, (1 + m_offset).ToString());
			SetOnClick(m_pageButton, 1 + m_offset);
			for (int pageNumber = 2; pageNumber <= buttonObjectCount; pageNumber++)
			{
				Button pageBtn = (Button)Instantiate(m_pageButton);
				RectTransform rTransform = pageBtn.GetComponent<RectTransform>();
				rTransform.SetParent(PageButtonTransform.parent, true);
				rTransform.localScale = PageButtonTransform.localScale;
				rTransform.localPosition = PageButtonTransform.localPosition + Vector3.right * (pageNumber - 1) * GetWidth(m_pageButton);
				SetText(pageBtn, (pageNumber + m_offset).ToString());
				SetOnClick(pageBtn, pageNumber + m_offset);
				m_pageButtons.Add(pageBtn);
			}
			// highlight selected button
			for (int i = 0; i < m_pageButtons.Count; i++)
			{
				int pageNumber = i + 1 + m_offset;
				m_pageButtons[i].enabled = pageNumber != m_selectedPage;
			}
			// correct position of next button
			if (m_nextButton != null)
			{
				m_nextButton.GetComponent<RectTransform>().localPosition = PageButtonTransform.localPosition + Vector3.right * buttonObjectCount * GetWidth(m_pageButton);
			}
		}

		private void Start()
		{
			SetPageCount(m_pageCount);

			if (m_previousButton != null)
			{
				m_previousButton.onClick.AddListener(() =>
				{
					SelectPageAndCenterOffset(Mathf.Max(1, m_selectedPage - 1));
				});
			}

			if (m_nextButton != null)
			{
				m_nextButton.onClick.AddListener(() =>
				{
					SelectPageAndCenterOffset(m_selectedPage + 1);
				});
			}
		}

		private void SetText(Button p_button, string p_text)
		{
			Text text = p_button.GetComponentInChildren<Text>();
			if (text != null) { text.text = p_text; }

            TMP_Text textMeshProText = p_button.GetComponentInChildren<TMP_Text>();
            if(textMeshProText != null) { textMeshProText.text = p_text; }
        }

		private void SetOnClick(Button p_button, int p_pageNumber)
		{
			p_button.onClick.RemoveAllListeners();
			p_button.onClick.AddListener(() =>
			{
				SelectPage(p_pageNumber);
			});
		}

		private float GetWidth(Button p_button)
		{
			return p_button != null ? GetWidth(p_button.GetComponent<RectTransform>()) : 0;
		}

		private float GetWidth(RectTransform p_rTransform)
		{
			return p_rTransform != null ? p_rTransform.rect.xMax - p_rTransform.rect.xMin : 0;
		}

		private void Clear()
		{
			for (int i = 0; i < m_pageButtons.Count; i++)
			{
				if (m_pageButtons[i] != null && m_pageButtons[i] != m_pageButton)
				{
					Destroy(m_pageButtons[i].gameObject);
				}
			}
			m_pageButtons.Clear();
			m_pageButtons.Add(m_pageButton);
		}
	}
}
