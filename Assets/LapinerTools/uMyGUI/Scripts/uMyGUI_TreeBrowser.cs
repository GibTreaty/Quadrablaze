using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_TreeBrowser : MonoBehaviour
	{
		public class Node
		{
			public readonly object SendMessageData;
			public readonly Node[] Children;
			public Node(object p_sendMessageData, Node[] p_children) { SendMessageData = p_sendMessageData; Children = p_children; }
		}

		public class NodeClickEventArgs : System.EventArgs
		{
			public readonly Node ClickedNode; public NodeClickEventArgs(Node p_clickedNode) { ClickedNode = p_clickedNode; }
		}

		public class NodeInstantiateEventArgs : System.EventArgs
		{
			public readonly Node Node;
			public readonly GameObject Instance;
			public NodeInstantiateEventArgs(Node p_node, GameObject p_instance) { Node = p_node; Instance = p_instance; }
		}

		private class InternalNode
		{
			public readonly Node m_node;
			public GameObject m_instance;
			public int m_indentLevel = 0;
			public RectTransform m_transform;
			public bool m_isFoldout = false;
			public float m_minY = 0f;
			public InternalNode(Node p_node, GameObject p_instance, int p_indentLevel)
			{
				m_node = p_node;
				m_instance = p_instance;
				m_indentLevel = p_indentLevel;
				m_transform = m_instance.GetComponent<RectTransform>();
			}
		}

		[SerializeField]
		private GameObject m_innerNodePrefab = null;
		public GameObject InnerNodePrefab
		{
			get{ return m_innerNodePrefab; }
			set{ m_innerNodePrefab = value; }
		}

		[SerializeField]
		private GameObject m_leafNodePrefab = null;
		public GameObject LeafNodePrefab
		{
			get{ return m_leafNodePrefab; }
			set{ m_leafNodePrefab = value; }
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
		private float m_indentSize = 20f;
		public float IndentSize
		{
			get{ return m_indentSize; }
			set{ m_indentSize = value; }
		}

		[SerializeField]
		private float m_forcedEntryHeight = 0;
		public float ForcedEntryHeight
		{
			get{ return m_forcedEntryHeight; }
			set{ m_forcedEntryHeight = value; }
		}

		[SerializeField]
		private bool m_useExplicitNavigation = false;
		public bool UseExplicitNavigation
		{
			get{ return m_useExplicitNavigation; }
			set{ m_useExplicitNavigation = value; }
		}

		[SerializeField]
		private float m_navScrollSpeed = 200f;
		public float NavScrollSpeed
		{
			get{ return m_navScrollSpeed; }
			set{ m_navScrollSpeed = value; }
		}

		[SerializeField]
		private float m_navScrollSmooth = 20f;
		public float NavScrollSmooth
		{
			get{ return m_navScrollSmooth; }
			set{ m_navScrollSmooth = value; }
		}

		private ScrollRect m_parentScroller = null;
		public ScrollRect ParentScroller
		{
			get{ return m_parentScroller; }
		}

		public System.EventHandler<NodeClickEventArgs> OnInnerNodeClick;
		public System.EventHandler<NodeClickEventArgs> OnLeafNodeClick;
		public System.EventHandler<NodeClickEventArgs> OnLeafNodePointerDown;
		public System.EventHandler<NodeInstantiateEventArgs> OnNodeInstantiate;

		private RectTransform m_rectTransform = null;
		private RectTransform RTransform { get{ return m_rectTransform!=null ? m_rectTransform : m_rectTransform = GetComponent<RectTransform>(); } }

		private List<InternalNode> m_nodes = new List<InternalNode>();

		private GameObject m_lastSelectedGO = null;

		public void BuildTree(Node[] p_rootNodes)
		{
			BuildTree(p_rootNodes, 0, 0);
		}

		public void BuildTree(Node[] p_rootNodes, int p_insertAt, int p_indentLevel)
		{
			if (m_innerNodePrefab != null && m_leafNodePrefab != null)
			{
				List<InternalNode> new_nodes = new List<InternalNode>();
				float minY = 0;
				float currY = m_nodes.Count >= p_insertAt && p_insertAt > 0 ? m_nodes[p_insertAt-1].m_minY : -m_offsetStart;
				for (int i = 0; i < p_rootNodes.Length; i++)
				{
					if (p_rootNodes[i] != null)
					{
						GameObject nodeGO;
						bool isInnerNode = p_rootNodes[i].Children != null && p_rootNodes[i].Children.Length > 0;
						// instantiate prefab
						if (isInnerNode)
						{
							// inner node
							nodeGO = (GameObject)Instantiate(m_innerNodePrefab);
						}
						else
						{
							// leaf node
							nodeGO = (GameObject)Instantiate(m_leafNodePrefab);
						}
						// get element size
						RectTransform nodeTransform = nodeGO.GetComponent<RectTransform>();
						if (m_forcedEntryHeight != 0)
						{
							nodeTransform.sizeDelta = new Vector2(nodeTransform.sizeDelta.x, m_forcedEntryHeight);
						}
						float elementSize = nodeTransform.rect.height;
						// propagate data via SendMessage (must be done before parenting to guarantee that the object is active)
						if (p_rootNodes[i].SendMessageData != null)
						{
							// check if object is active
							if (!nodeGO.activeInHierarchy)
							{
								Debug.LogError("uMyGUI_TreeBrowser: BuildTree: node has SendMessageData set, but instance is inactive! SendMessage call will fail! Make your prefab active!");
							}
							nodeGO.SendMessage("uMyGUI_TreeBrowser_InitNode", p_rootNodes[i].SendMessageData);
						}
						// save in local structure
						InternalNode node = new InternalNode(p_rootNodes[i], nodeGO, p_indentLevel);
						new_nodes.Add(node);
						// setup foldout behaviour of inner nodes
						if (isInnerNode) { SetupInnerNode(node); } else { SetupLeafNode(node); }
						// move to the right position
						currY = SetRectTransformPosition(nodeTransform, currY, elementSize, p_indentLevel);
						// calculate max y position to resize this rect transform
						node.m_minY = nodeTransform.anchoredPosition.y - elementSize;
						minY = node.m_minY;

						// notify listeners
						if (OnNodeInstantiate != null) { OnNodeInstantiate(this, new NodeInstantiateEventArgs(p_rootNodes[i], nodeGO)); }
					}
				}
				// move nodes behind the insert index
				if (p_insertAt < m_nodes.Count)
				{
					float moveDist;
					if (p_insertAt == 0)
					{
						moveDist = minY;
					}
					else
					{
						moveDist = minY - m_nodes[p_insertAt-1].m_minY;
					}
					UpdateNodePosition(p_insertAt, moveDist);
				}
				// add new nodes to list
				if (p_insertAt < m_nodes.Count)
				{
					m_nodes.InsertRange(p_insertAt, new_nodes);
				}
				else
				{
					m_nodes.AddRange(new_nodes);
				}
				// resize rect transform (e.g. to allow scrolling if scroll rect is the parent)
				if (m_nodes.Count > 0)
				{
					RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  Mathf.Abs(m_nodes[m_nodes.Count-1].m_minY - RTransform.rect.yMax - m_offsetEnd));
				}
				// improve controller and keyboard navigation by setting explicit targets
				if (m_useExplicitNavigation)
				{
					SetExplicitNavigationTargets();
				}
			}
			else
			{
				Debug.LogError("uMyGUI_TreeBrowser: BuildTree: you must provide the InnerNodePrefab and LeafNodePrefab in the inspector or via script!");
			}
		}

		public void Clear()
		{
			for (int i = 0; i < m_nodes.Count; i++)
			{
				Destroy(m_nodes[i].m_instance);
			}
			m_nodes.Clear();
		}

		private void Start()
		{
			m_parentScroller = GetComponentInParent<ScrollRect>();
		}

		private void LateUpdate()
		{
			// scroll to selected entry if focus is changed (e.g. through controller navigation)
			if (m_parentScroller == null) { return; }

			GameObject currSelected;
			EventSystem eventSys = EventSystem.current;

			if (eventSys != null && // uGUI is used with an EventSystem
			    (currSelected = eventSys.currentSelectedGameObject) != null && // something is selected
			    currSelected.transform.IsChildOf(transform)) // the selected object is an entry of this TreeBrowser
			{
				if (currSelected != m_lastSelectedGO) // selection has changed or scroll is required
				{
					m_lastSelectedGO = currSelected;
					Transform itemRoot = currSelected.transform;
					// find root of the item
					while(itemRoot.parent != transform && itemRoot.parent != null)
					{
						itemRoot = itemRoot.parent;
					}
					// calculate position of selected item
					RectTransform rect = itemRoot.GetComponent<RectTransform>();
					if (rect == null) { return; }

					Vector3[] corners = new Vector3[4];
					rect.GetWorldCorners(corners);
					for (int i = 0; i < corners.Length; i++)
					{
						corners[i] = m_parentScroller.transform.InverseTransformPoint(corners[i]);
					}
					Vector3 selectedMin = Vector3.Min(Vector3.Min(corners[0], corners[1]), Vector3.Min(corners[2], corners[3]));
					Vector3 selectedMax = Vector3.Max(Vector3.Max(corners[0], corners[1]), Vector3.Max(corners[2], corners[3]));

					// calculate visible rect
					m_parentScroller.GetComponent<RectTransform>().GetLocalCorners(corners);
					Vector3 scrollMin = Vector3.Min(Vector3.Min(corners[0], corners[1]), Vector3.Min(corners[2], corners[3]));
					Vector3 scrollMax = Vector3.Max(Vector3.Max(corners[0], corners[1]), Vector3.Max(corners[2], corners[3]));

					// scroll if needed
					if (selectedMin.y < scrollMin.y)
					{
						if (m_parentScroller.verticalNormalizedPosition >= 1) { m_parentScroller.verticalNormalizedPosition = 0.999f; } // fixes scrollbar getting stuck
						m_parentScroller.velocity = Vector3.up * Mathf.Max(5f, m_navScrollSpeed * (m_navScrollSmooth != 0 ? (scrollMin.y - selectedMin.y) / m_navScrollSmooth : 1));
						m_lastSelectedGO = null; // continue scrolling
					}
					else if (selectedMax.y > scrollMax.y)
					{
						if (m_parentScroller.verticalNormalizedPosition <= 0) { m_parentScroller.verticalNormalizedPosition = 0.001f; } // fixes scrollbar getting stuck
						m_parentScroller.velocity = Vector3.down * Mathf.Max(5f, m_navScrollSpeed * (m_navScrollSmooth != 0 ? (selectedMax.y - scrollMax.y) / m_navScrollSmooth : 1));
						m_lastSelectedGO = null; // continue scrolling
					}
				}
			}
			else
			{
				m_lastSelectedGO = null;
			}
		}

		private void OnDestroy()
		{
			OnInnerNodeClick = null;
			OnLeafNodeClick = null;
			OnNodeInstantiate = null;
		}

		private void SetExplicitNavigationTargets()
		{
			if (m_nodes.Count > 2)
			{
				Selectable[] prevSelectables, currSelectables, nextSelectables;
				RectTransform prev, curr, next;
				curr = m_nodes[0].m_transform;
				next = m_nodes[1].m_transform;
				currSelectables = curr.GetComponentsInChildren<Selectable>();
				nextSelectables = next.GetComponentsInChildren<Selectable>();
				SetAutomaticNavigation(m_nodes[0].m_transform); // this will allow to exit the list at the top
				SetAutomaticNavigation(m_nodes[m_nodes.Count-1].m_transform); // this will allow to exit the list at the bottom
				for (int i = 1; i < m_nodes.Count-1; i++)
				{
					prev = curr;
					curr = next;
					next = m_nodes[i+1].m_transform;
					prevSelectables = currSelectables;
					currSelectables = nextSelectables;
					nextSelectables = next.GetComponentsInChildren<Selectable>();

					if (prev != null && curr != null && next != null)
					{
						if (prevSelectables.Length == currSelectables.Length && nextSelectables.Length == currSelectables.Length)
						{
							for (int j = 0; j < currSelectables.Length; j++)
							{
								Navigation nav = currSelectables[j].navigation;
								nav.mode = Navigation.Mode.Explicit;
								nav.selectOnUp = prevSelectables[j];
								nav.selectOnDown = nextSelectables[j];
								currSelectables[j].navigation = nav;
							}
						}
					}
				}
			}
		}

		private void SetAutomaticNavigation(RectTransform p_nodeTransform)
		{
			if (p_nodeTransform != null)
			{
				Selectable[] selectables = p_nodeTransform.GetComponentsInChildren<Selectable>();
				for (int i = 0; i < selectables.Length; i++)
				{
					Navigation nav = selectables[i].navigation;
					nav.mode = Navigation.Mode.Automatic;
					selectables[i].navigation = nav;
				}
			}
		}

		private float SetRectTransformPosition(RectTransform p_transform, float p_currY, float p_size, int p_indentLevel)
		{
			// parent and move prefab
			p_transform.SetParent(RTransform, false);
			Vector2 pos = p_transform.anchoredPosition;
			pos.x += p_indentLevel*m_indentSize;
			pos.y += p_currY;
			p_currY -= m_padding + p_size;
			p_transform.anchoredPosition = pos;
			return p_currY;
		}

		private void UpdateNodePosition(int p_startIndex, float p_moveDist)
		{
			for (int i = p_startIndex; i < m_nodes.Count; i++)
			{
				Vector2 pos = m_nodes[i].m_transform.anchoredPosition;
				pos.y += p_moveDist;
				m_nodes[i].m_transform.anchoredPosition = pos;
				m_nodes[i].m_minY = pos.y - m_nodes[i].m_transform.rect.height;
			}
		}

		private void SetupInnerNode(InternalNode p_node)
		{
			// register to the click method
			if (p_node.m_instance.GetComponent<Button>() != null)
			{
				p_node.m_instance.GetComponent<Button>().onClick.AddListener(()=>ToggleInnerNodeFoldout(p_node));
			}
			else if (p_node.m_instance.GetComponent<Toggle>() != null)
			{
				p_node.m_instance.GetComponent<Toggle>().onValueChanged.AddListener((bool p_isOn)=>ToggleInnerNodeFoldout(p_node));
			}
			else
			{
				Debug.LogError("uMyGUI_TreeBrowser: BuildTree: the inner node prefabs must have either a Button or a Toggle script attached to the root. Otherwise they cannot fold out!");
			}
		}

		private void SetupLeafNode(InternalNode p_node)
		{
			// register to the click method
			if (p_node.m_instance.GetComponent<Button>() != null)
			{
				p_node.m_instance.GetComponent<Button>().onClick.AddListener(()=>SafeCallOnLeafNodeClick(p_node));
			}
			else if (p_node.m_instance.GetComponent<Toggle>() != null)
			{
				p_node.m_instance.GetComponent<Toggle>().onValueChanged.AddListener((bool p_isOn)=>SafeCallOnLeafNodeClick(p_node));
			}
			// register to the pointer down method
			EventTrigger trigger = p_node.m_instance.GetComponent<EventTrigger>();
			if (trigger == null) { trigger = p_node.m_instance.AddComponent<EventTrigger>(); }
			EventTrigger.Entry onPointerDown = new EventTrigger.Entry();
			onPointerDown.eventID = EventTriggerType.PointerDown;
			EventTrigger.TriggerEvent callback = new EventTrigger.TriggerEvent();
			callback.AddListener((BaseEventData p_downEvent)=>SafeCallOnLeafNodePointerDown(p_node));
			onPointerDown.callback = callback;
			if (trigger.triggers == null)
			{
				trigger.triggers = new List<EventTrigger.Entry>();
			}
			trigger.triggers.Add(onPointerDown);
		}

		private void ToggleInnerNodeFoldout(InternalNode p_node)
		{
			int nodeIndex = m_nodes.IndexOf(p_node);
			p_node.m_isFoldout = !p_node.m_isFoldout;
			if (p_node.m_isFoldout)
			{
				BuildTree(p_node.m_node.Children, nodeIndex+1, p_node.m_indentLevel + 1);
			}
			else
			{
				// delete all children of this node from list and calculate the move distance
				float moveDist = 0;
				for (int i = 0; i < p_node.m_node.Children.Length; i++)
				{
					int removeIndex = nodeIndex+p_node.m_node.Children.Length-i;
					InternalNode removeNode = m_nodes[removeIndex];
					moveDist += removeNode.m_transform.rect.height;
					if (i+1 < p_node.m_node.Children.Length)
					{
						moveDist += m_padding;
					}
					// check if this node was foldout and remove children if needed
					if (removeNode.m_isFoldout)
					{
						ToggleInnerNodeFoldout(removeNode);
					}
					m_nodes.RemoveAt(removeIndex);
					Destroy(removeNode.m_instance);
				}
				// move all elements up that were not deleted and after the clicked node
				UpdateNodePosition(nodeIndex+1, moveDist);
				// resize rect transform (e.g. to allow scrolling if scroll rect is the parent)
				RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RTransform.sizeDelta.y - moveDist);
			}
			// notify listeners
			if (OnInnerNodeClick != null)
			{
				OnInnerNodeClick(this, new NodeClickEventArgs(p_node.m_node));
			}
		}

		private void SafeCallOnLeafNodePointerDown(InternalNode p_node)
		{
			// notify listeners
			if (OnLeafNodePointerDown != null)
			{
				OnLeafNodePointerDown(this, new NodeClickEventArgs(p_node.m_node));
			}
		}

		private void SafeCallOnLeafNodeClick(InternalNode p_node)
		{
			// notify listeners
			if (OnLeafNodeClick != null)
			{
				OnLeafNodeClick(this, new NodeClickEventArgs(p_node.m_node));
			}
		}
	}
}
