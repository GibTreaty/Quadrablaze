using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_Draggable : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public class DragEvent : System.EventArgs { public readonly PointerEventData m_event; public DragEvent(PointerEventData p_event) { m_event = p_event; } }

		[SerializeField]
		private bool m_isResetRotationWhenDragged = false;
		public bool IsResetRotationWhenDragged
		{
			get{ return m_isResetRotationWhenDragged; }
			set{ m_isResetRotationWhenDragged = value; }
		}

		[SerializeField]
		private bool m_isSnapBackOnEndDrag = false;
		public bool IsSnapBackOnEndDrag
		{
			get{ return m_isSnapBackOnEndDrag; }
			set{ m_isSnapBackOnEndDrag = value; }
		}

		[SerializeField]
		private bool m_isTopInHierarchyWhenDragged = true;
		public bool IsTopInHierarchyWhenDragged
		{
			get{ return m_isTopInHierarchyWhenDragged; }
			set{ m_isTopInHierarchyWhenDragged = value; }
		}

		[SerializeField]
		private CanvasGroup m_disableBlocksRaycastsOnDrag = null;
		public CanvasGroup DisableBlocksRaycastsOnDrag
		{
			get{ return m_disableBlocksRaycastsOnDrag; }
			set{ m_disableBlocksRaycastsOnDrag = value; }
		}

		private bool m_isDragged = false;
		public bool IsDragged { get{ return m_isDragged; } }

		public System.EventHandler<DragEvent> m_onBeginDrag;
		public System.EventHandler<DragEvent> m_onDrag;
		public System.EventHandler<DragEvent> m_onEndDrag;

		private RectTransform m_initialParentTransform = null;
		private RectTransform m_canvasTransform = null;
		private RectTransform m_transform = null;
		private int m_initialSiblingIndex = 0;
		private Vector3 m_initialPosition;
		private Quaternion m_initialRotation;

		private Vector3 m_dragOffset = Vector3.zero;

		public void OnBeginDrag(PointerEventData p_event)
		{
			// save state
			m_isDragged = true;
			m_initialParentTransform = m_transform.parent as RectTransform;
			m_initialPosition = m_transform.position;
			m_initialRotation = m_transform.rotation;
			// calculate and save the offset between mouse pointer and pivot
			Vector3 newPos;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasTransform, p_event.position, p_event.pressEventCamera, out newPos))
			{
				m_dragOffset = m_transform.position - newPos;
			}
			else
			{
				m_dragOffset = Vector3.zero;
			}
			// rotate towards camera if needed
			if (m_isResetRotationWhenDragged)
			{
				m_transform.rotation = Quaternion.identity;
			}
			// make last element of canvas if needed
			if (m_isTopInHierarchyWhenDragged)
			{
				m_initialSiblingIndex = m_transform.GetSiblingIndex();
				m_transform.SetParent(m_canvasTransform, true);
				m_transform.SetAsLastSibling();
			}
			// disable block raycasts
			if (m_disableBlocksRaycastsOnDrag != null)
			{
				m_disableBlocksRaycastsOnDrag.blocksRaycasts = false;
			}
			// notify listeners
			if (m_onBeginDrag != null)
			{
				m_onBeginDrag(this, new DragEvent(p_event));
			}
		}

		public void OnDrag(PointerEventData p_event)
		{
			Vector3 newPos;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasTransform, p_event.position, p_event.pressEventCamera, out newPos))
			{
				m_transform.position = newPos + m_dragOffset;
			}

			if (m_onDrag != null)
			{
				m_onDrag(this, new DragEvent(p_event));
			}
		}

		public void OnEndDrag(PointerEventData p_event)
		{
			// OnEndDrag can get called without OnBeginDrag -> check if is dragged first (see http://issuetracker.unity3d.com/issues/onenddrag-is-called-when-you-click-a-ui-object)
			if (m_isDragged)
			{
				// save state
				m_isDragged = false;
				// snap back if needed
				if (m_isSnapBackOnEndDrag)
				{
					m_transform.position = m_initialPosition;
					m_transform.rotation = m_initialRotation;
				}
				// restore parent relationship if it was changed
				if (m_isTopInHierarchyWhenDragged)
				{
					m_transform.SetParent(m_initialParentTransform, true);
					m_transform.SetSiblingIndex(m_initialSiblingIndex);
				}
				// restore block raycasts
				if (m_disableBlocksRaycastsOnDrag != null)
				{
					m_disableBlocksRaycastsOnDrag.blocksRaycasts = true;
				}
				// notify listeners
				if (m_onEndDrag != null)
				{
					m_onEndDrag(this, new DragEvent(p_event));
				}
			}
		}

		private void Start ()
		{
			Canvas canvas = GetComponentInParent<Canvas>();
			if (canvas != null)
			{
				m_canvasTransform = canvas.GetComponent<RectTransform>();
			}
			else
			{
				Debug.LogError("uMyGUI_Draggable: no Canvas component was found in parent!");
				enabled = false;
			}
			m_transform = GetComponent<RectTransform>();
		}
	}
}
