using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_TexturePicker : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_texturePrefab = null;
		public GameObject TexturePrefab
		{
			get{ return m_texturePrefab; }
			set{ m_texturePrefab = value; }
		}
		[SerializeField]
		private GameObject m_selectionPrefab = null;
		public GameObject SelectionPrefab
		{
			get{ return m_selectionPrefab; }
			set{ m_selectionPrefab = value; }
		}
		[SerializeField]
		private float m_offsetStart = 0f;
		public float OffsetStart
		{
			get{ return m_offsetStart; }
			set{ m_offsetStart = value; }
		}
		[SerializeField]
		private float m_offsetEnd = 0f;
		public float OffsetEnd
		{
			get{ return m_offsetEnd; }
			set{ m_offsetEnd = value; }
		}
		[SerializeField]
		private float m_padding = 4f;
		public float Padding
		{
			get{ return m_padding; }
			set{ m_padding = value; }
		}
		[SerializeField]
		private System.Action<int> m_buttonCallback = null;
		public System.Action<int> ButtonCallback
		{
			get{ return m_buttonCallback; }
			set{ m_buttonCallback = value; }
		}

		private Texture2D[] m_textures = new Texture2D[0];
		public Texture2D[] Textures
		{
			get{ return m_textures; }
			set{ m_textures = value; }
		}

		private RectTransform m_rectTransform = null;
		private RectTransform RTransform { get{ return m_rectTransform!=null ? m_rectTransform : m_rectTransform = GetComponent<RectTransform>(); } }

		private float m_elementSize = 1f;
		private GameObject m_selectionInstance = null;
		private GameObject[] m_instances = new GameObject[0];
		public GameObject[] Instances { get{ return m_instances; } }

		public void SetSelection(int p_selectionIndex)
		{
			if (m_selectionPrefab != null)
			{
				// remove selection if index is out of bounds
				if (p_selectionIndex < 0 || p_selectionIndex >= m_instances.Length)
				{
					Destroy(m_selectionInstance);
					m_selectionInstance = null;
					return;
				}

				if (m_selectionInstance == null)
				{
					// instantiate selection if it is not already there
					m_selectionInstance = (GameObject)Instantiate(m_selectionPrefab);
				}
				else
				{
					// reset selection position if it was already there
					RectTransform selectionTransform = m_selectionInstance.GetComponent<RectTransform>();
					Vector2 pos = selectionTransform.anchoredPosition;
					pos.x = m_selectionPrefab.GetComponent<RectTransform>().anchoredPosition.x;
					selectionTransform.anchoredPosition = pos;
				}

				// update selections position
				SetRectTransformPosition(m_selectionInstance.GetComponent<RectTransform>(), p_selectionIndex, m_elementSize);
			}
			else
			{
				Debug.LogError("uMyGUI_TexturePicker: SetSelection: you have passed a non negative selection index '"+p_selectionIndex+"', but the SelectionPrefab was not provided in the inspector or via script!");
			}
		}

		public void SetTextures(Texture2D[] p_textures, int p_selectedIndex)
		{
			if (m_texturePrefab != null)
			{
				m_textures = p_textures;
				// delete all existing texture instances
				Destroy(m_selectionInstance);
				for (int i = 0; i < m_instances.Length; i++)
				{
					Destroy(m_instances[i]);
				}
				// create new texture prefab instances
				m_instances = new GameObject[p_textures.Length];
				float maxX = 0;
				for (int i = 0; i < p_textures.Length; i++)
				{
					// instantiate texture prefab
					m_instances[i] = (GameObject)Instantiate(m_texturePrefab);
					RectTransform instanceTransform = m_instances[i].GetComponent<RectTransform>();
					m_elementSize = instanceTransform.rect.width;
					SetRectTransformPosition(instanceTransform, i, m_elementSize);
					// apply texture
					RawImage image = TryFindComponent<RawImage>(m_instances[i]);
					if (image != null)
					{
						image.texture = p_textures[i];
					}
					else
					{
						Debug.LogError("uMyGUI_TexturePicker: SetTextures: TexturePrefab must have a RawImage component attached (can be in children).");
					}
					// link button callback
					if (m_buttonCallback != null)
					{
						Button btn = TryFindComponent<Button>(m_instances[i]);
						if (btn != null)
						{
							int indexCopy = i;
							btn.onClick.AddListener(()=>{ m_buttonCallback(indexCopy); });
						}
					}
					// calculate max x position to resize this rect transform
					maxX = instanceTransform.anchoredPosition.x + m_elementSize;
					// add selection prefab if needed
					if (i==p_selectedIndex)
					{
						if (m_selectionPrefab != null)
						{
							// instantiate selection prefab
							m_selectionInstance = (GameObject)Instantiate(m_selectionPrefab);
							SetRectTransformPosition(m_selectionInstance.GetComponent<RectTransform>(), i, m_elementSize);
						}
						else
						{
							Debug.LogError("uMyGUI_TexturePicker: SetTextures: you have passed a non negative selection index '"+p_selectedIndex+"', but the SelectionPrefab was not provided in the inspector or via script!");
						}
					}
				}
				// resize rect transform (e.g. to allow scrolling if scroll rect is the parent)
				RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,  maxX - RTransform.rect.xMin + m_offsetEnd);
			}
			else
			{
				Debug.LogError("uMyGUI_TexturePicker: SetTextures: you must provide the TexturePrefab in the inspector or via script!");
			}
		}

		private void OnDestroy()
		{
			m_buttonCallback = null;
		}

		private void SetRectTransformPosition(RectTransform p_transform, int p_positionIndex, float p_size)
		{
			// parent and move prefab
			p_transform.SetParent(RTransform, false);
			Vector2 pos = p_transform.anchoredPosition;
			pos.x += m_offsetStart + p_positionIndex*(p_size+m_padding);
			p_transform.anchoredPosition = pos;
		}

		private T TryFindComponent<T>(GameObject p_object) where T : Component
		{
			// get at current object (this call is fast)
			T component = p_object.GetComponent<T>();
			if (component == null)
			{
				// try find in children including inactive (this call is slow)
				T[] components = p_object.GetComponentsInChildren<T>(true);
				if (components.Length > 0)
				{
					component = components[0];
				}
			}
			return component;
		}
	}
}
