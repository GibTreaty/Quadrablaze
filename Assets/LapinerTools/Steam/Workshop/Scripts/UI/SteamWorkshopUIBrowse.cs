using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections.Generic;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

using Steamworks;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// This class manages the uGUI of the Steam Workshop browse items menu.
	/// It registers to events of SteamWorkshopMain class, e.g. SteamWorkshopMain.OnItemListLoaded.
	/// SteamWorkshopUIBrowse also receives UI callbacks from uGUI buttons and other elements.
	/// You can replace this class with your own UI e.g. NGUI.
	/// In this case you need to take care of registering to events and calling methods of the SteamWorkshopMain class from your new implementation.
	/// </summary>
	public class SteamWorkshopUIBrowse : MonoBehaviour
	{
		[System.Serializable]
		/// <summary>
		/// Defines the available item list sorting modes. Used to generate the sort modes dropdown. See SteamWorkshopMain.Sorting for more details.
		/// </summary>
		public class SortingConfig
		{
			[System.Serializable]
			/// <summary>
			/// Defines a certain item list sort mode. Used to generate a sort mode dropdown entry. See SteamWorkshopMain.Sorting for more details.
			/// </summary>
			public class Option
			{
				[SerializeField]
				/// <summary>
				/// The Data.WorkshopSortMode later used by the SteamWorkshopMain class. See SteamWorkshopMain.Sorting for more details.
				/// </summary>
				public WorkshopSortMode MODE = new WorkshopSortMode();
				[SerializeField]
				/// <summary>
				/// The text displayed in the sort mode dropdown entry.
				/// </summary>
				public string DISPLAY_TEXT = "Votes";
			}

			[SerializeField]
			/// <summary>
			/// A reference to the uMyGUI.uMyGUI_Dropdown component, which will be initialized with SteamWorkshopUIBrowse.DEFAULT_SORT_MODE and SteamWorkshopUIBrowse.OPTIONS.
			/// </summary>
			public uMyGUI_Dropdown DROPDOWN = null;
			[SerializeField]
			/// <summary>
			/// The index of the default selected sort mode.
			/// </summary>
			public int DEFAULT_SORT_MODE = 0;
			[SerializeField]
			/// <summary>
			/// This array contains all available sort mode options, which will be visible in the sort mode selection dropdown.
			/// </summary>
			public Option[] OPTIONS = new Option[0];
		}

		protected static SteamWorkshopUIBrowse s_instance;
		/// <summary>
		/// You can use the static Instance property to access the SteamWorkshopUIBrowse class from wherever you need it in your code.
		/// Use this property only if you know that you have a static SteamWorkshopUIBrowse uGUI in your scene.
		/// If you use the SteamWorkshopPopupBrowse, then there is no guarantee that the SteamWorkshopUIBrowse was already created.
		/// </summary>
		public static SteamWorkshopUIBrowse Instance
		{
			get
			{
				// try to find an existing instance
				if (s_instance == null)
				{
					s_instance = FindObjectOfType<SteamWorkshopUIBrowse>();
				}
				return s_instance;
			}
		}

		/// <summary>
		/// Invoked when the sort mode was changed.
		/// </summary>
		public event System.Action<WorkshopSortModeEventArgs> OnSortModeChanged;
		/// <summary>
		/// Invoked when the search button is clicked or the search text is committed.
		/// </summary>
		public event System.Action<string> OnSearchButtonClick;
		/// <summary>
		/// Invoked when the item list page was changed.
		/// </summary>
		public event System.Action<int> OnPageChanged;

		/// <summary>
		/// Invoked when the play button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnPlayButtonClick;
		/// <summary>
		/// Invoked when the vote up button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnVoteUpButtonClick;
		/// <summary>
		/// Invoked when the vote down button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnVoteDownButtonClick;
		/// <summary>
		/// Invoked when the subscribe button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnSubscribeButtonClick;
		/// <summary>
		/// Invoked when the unsubscribe button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnUnsubscribeButtonClick;
		/// <summary>
		/// Invoked when the add to favorites button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnAddFavoriteButtonClick;
		/// <summary>
		/// Invoked when the remove from favorites button of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is clicked.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnRemoveFavoriteButtonClick;

		/// <summary>
		/// Invoked when the data of the WorkshopItemListEntry prefab (SteamWorkshopItemNode class) is updated.
		/// You can use this event to initialize fields of your custom UI. For example, you could add a highscore text to the WorkshopItemListEntry prefab.
		/// Then you would search the highscore uGUI text object and set the value when this event is triggered.
		/// </summary>
		public event System.Action<SteamWorkshopItemNode.ItemDataSetEventArgs> OnItemDataSet;

		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnPlayButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnPlayButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnPlayButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }
		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnVoteUpButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnVoteUpButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnVoteUpButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }
		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnVoteDownButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnVoteDownButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnVoteDownButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }
		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnSubscribeButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnSubscribeButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnSubscribeButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }
		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnUnsubscribeButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnUnsubscribeButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnUnsubscribeButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }
		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnAddFavoriteButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnAddFavoriteButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnAddFavoriteButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }
		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnRemoveFavoriteButtonClick event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnRemoveFavoriteButtonClick(WorkshopItem p_clickedItem) { InvokeEventHandlerSafely(OnRemoveFavoriteButtonClick, new WorkshopItemEventArgs(p_clickedItem)); }

		/// <summary>
		/// Internal method triggering the SteamWorkshopUIBrowse.OnItemDataSet event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnItemDataSet(WorkshopItem p_itemData, SteamWorkshopItemNode p_itemUI) { InvokeEventHandlerSafely(OnItemDataSet, new SteamWorkshopItemNode.ItemDataSetEventArgs() { ItemData = p_itemData, ItemUI = p_itemUI }); }

		[SerializeField]
		protected uMyGUI_TreeBrowser ITEM_BROWSER = null;
		[SerializeField]
		protected uMyGUI_PageBox PAGE_SELCTOR = null;
		[SerializeField]
		protected SortingConfig SORTING = null;
		[SerializeField]
		protected InputField SEARCH_INPUT = null;
		[SerializeField]
		protected Button SEARCH_BUTTON = null;
		[SerializeField, Tooltip("If true, then the first page will be loaded on MonoBehaviour.OnStart")]
		protected bool m_loadOnStart = true;
		[SerializeField]
		protected bool m_improveNavigationFocus = true;
		
		protected Dictionary<uMyGUI_TreeBrowser.Node, WorkshopItem> m_uiNodeToSteamItem = new Dictionary<uMyGUI_TreeBrowser.Node, WorkshopItem>();

		/// <summary>
		/// Call SetItems to refresh the item selection UI.
		/// Calling this method will remove all currently visible items and replace them with those passed in the p_itemList argument.
		/// </summary>
		/// <param name="p_itemList">list of items to be visualized.</param>
		public void SetItems(WorkshopItemList p_itemList)
		{
			if (ITEM_BROWSER != null)
			{
				m_uiNodeToSteamItem.Clear();
				ITEM_BROWSER.Clear();
				ITEM_BROWSER.BuildTree(ConvertItemsToNodes(p_itemList.Items.ToArray()));
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: SetItems: ITEM_BROWSER is not set in inspector!");
			}

			if (PAGE_SELCTOR != null)
			{
				PAGE_SELCTOR.OnPageSelected -= SetPage;
				PAGE_SELCTOR.SetPageCount((int)p_itemList.PagesItems);
				PAGE_SELCTOR.SelectPage((int)p_itemList.Page);
				PAGE_SELCTOR.OnPageSelected += SetPage;
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: SetItems: PAGE_SELCTOR is not set in inspector!");
			}

			if (m_improveNavigationFocus && ITEM_BROWSER != null && ITEM_BROWSER.transform.childCount > 0 && ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>() != null)
			{
				// select the first list item if available
				ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>().Select();
			}
		}

		/// <summary>
		/// The same as SteamWorkshopMain.GetItemList with an additional loading popup.
		/// </summary>
		/// <param name="p_page">item list page to load, starts with 1.</param>
		public void LoadItems(int p_page)
		{
			// show loading popup while items are loading
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			SteamWorkshopMain.Instance.GetItemList((uint)p_page, (WorkshopItemListEventArgs p_itemListArgs) =>
			{
				// list is loaded -> hide loading popup
				uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
			});
		}

		/// <summary>
		/// Search items containing p_searchText. Will reload the item list if the search text has changed.
		/// </summary>
		/// <param name="p_searchText">text to search.</param>
		public void Search(string p_searchText)
		{
			bool isSearchTextChanged = p_searchText != SteamWorkshopMain.Instance.SearchText;
			bool isSearchTextValidBefore = SteamWorkshopMain.Instance.SearchText != null && !string.IsNullOrEmpty(SteamWorkshopMain.Instance.SearchText);
			bool isSearchTextValidAfter = p_searchText != null && !string.IsNullOrEmpty(p_searchText.Trim());
			SteamWorkshopMain.Instance.SearchText = p_searchText;
			if (isSearchTextChanged && (isSearchTextValidAfter || (isSearchTextValidBefore && !isSearchTextValidAfter)))
			{
				InvokeEventHandlerSafely(OnSearchButtonClick, p_searchText);
				LoadItems(1);
			}
		}

		protected void SetPage(int p_page)
		{
			InvokeEventHandlerSafely(OnPageChanged, p_page);
			LoadItems(p_page);
		}

		protected virtual void Start()
		{
			// initialize item sorting UI
			InitSorting();
			// initialize item filtering/searching
			InitSearch();

			// refresh item list as soon as it is loaded
			SteamWorkshopMain.Instance.OnItemListLoaded += SetItems;
			// show error popups when something goes wrong
			SteamWorkshopMain.Instance.OnError += ShowErrorMessage;

			// load initial item list
			if (m_loadOnStart)
			{
				LoadItems(1);
			}
		}

		protected virtual void LateUpdate()
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
						else if (ITEM_BROWSER != null && ITEM_BROWSER.transform.childCount > 0 && ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>() != null)
						{
							// select the first list item if available
							ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>().Select();
						}
						else if (SEARCH_INPUT != null)
						{
							// select the search input as fallback (probably no items exist, because the search text was too specific)
							SEARCH_INPUT.Select();
						}
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (SteamWorkshopMain.IsInstanceSet)
			{
				SteamWorkshopMain.Instance.OnItemListLoaded -= SetItems;
				SteamWorkshopMain.Instance.OnError -= ShowErrorMessage;
			}
		}

		protected virtual void ShowErrorMessage(ErrorEventArgs p_errorArgs)
		{
			// hide the loading popup if visible
			uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
			// show the error message in a new popup
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Steam Error", p_errorArgs.ErrorMessage)
				.ShowButton(uMyGUI_PopupManager.BTN_OK);
		}

		protected virtual void SetItems(WorkshopItemListEventArgs p_itemListArgs)
		{
			if (!p_itemListArgs.IsError)
			{
				SetItems(p_itemListArgs.ItemList);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: SetItems: Steam Error: " + p_itemListArgs.ErrorMessage);
			}
		}

		/// <summary>
		/// This method will convert the gives Steam items to UI nodes, which can be passed to the item browser.
		/// </summary>
		protected virtual uMyGUI_TreeBrowser.Node[] ConvertItemsToNodes(WorkshopItem[] p_items)
		{
			uMyGUI_TreeBrowser.Node[] nodes = new uMyGUI_TreeBrowser.Node[p_items.Length];
			for (int i = 0; i < p_items.Length; i++)
			{
				if (p_items[i] != null)
				{
					SteamWorkshopItemNode.SendMessageInitData data = new SteamWorkshopItemNode.SendMessageInitData()
					{
						Item = p_items[i]
					};
					uMyGUI_TreeBrowser.Node node = new uMyGUI_TreeBrowser.Node(data, null);
					nodes[i] = node;
					m_uiNodeToSteamItem.Add(node, p_items[i]);
				}
				else
				{
					Debug.LogError("SteamWorkshopUIBrowse: ConvertItemsToNodes: item at index '" + i + "' is null!");
				}
			}
			return nodes;
		}

		protected virtual void InitSorting()
		{
			// initialize item sorting UI
			if (SORTING != null && SORTING.DROPDOWN != null)
			{
				string[] sortEntries = new string[SORTING.OPTIONS.Length];
				for (int i = 0; i < sortEntries.Length; i++)
				{
					if (SORTING.OPTIONS[i] != null)
					{
						sortEntries[i] = SORTING.OPTIONS[i].DISPLAY_TEXT;
					}
					else
					{
						sortEntries[i] = "NULL";
					}
				}
				SORTING.DROPDOWN.Entries = sortEntries;
				SORTING.DROPDOWN.Select(Mathf.Clamp(SORTING.DEFAULT_SORT_MODE, 0, sortEntries.Length - 1));
				SORTING.DROPDOWN.OnSelected += (int p_selectedSortIndex) => 
				{
					// update sort mode when selected in UI
					if (p_selectedSortIndex >= 0 && p_selectedSortIndex < SORTING.OPTIONS.Length)
					{
						WorkshopSortMode newSortMode = SORTING.OPTIONS[p_selectedSortIndex].MODE;
						bool isSortModeChanged = SteamWorkshopMain.Instance.Sorting != newSortMode;
						SteamWorkshopMain.Instance.Sorting = newSortMode;
						if (isSortModeChanged)
						{
							InvokeEventHandlerSafely(OnSortModeChanged, new WorkshopSortModeEventArgs(newSortMode));
							LoadItems(1);
						}
					}
				};
				// apply default sort mode
				if (SORTING.DEFAULT_SORT_MODE >= 0 && SORTING.DEFAULT_SORT_MODE < SORTING.OPTIONS.Length)
				{
					SteamWorkshopMain.Instance.Sorting = SORTING.OPTIONS[SORTING.DEFAULT_SORT_MODE].MODE;
				}
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: SORTING.DROPDOWN is not set in inspector!");
			}
		}

		protected virtual void InitSearch()
		{
			// initialize item filtering/searching UI

			if (SEARCH_INPUT != null)
			{
				SEARCH_INPUT.onEndEdit.AddListener(Search);	
				
				if (SEARCH_BUTTON != null)
				{
					SEARCH_BUTTON.onClick.AddListener(() =>
					{
						if (SEARCH_INPUT != null)
						{
							Search(SEARCH_INPUT.text);
						}
					});
				}
				else
				{
					Debug.LogError("SteamWorkshopUIBrowse: SEARCH_BUTTON is not set in inspector!");
				}
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: SEARCH_INPUT is not set in inspector!");
			}
		}

		protected virtual void InvokeEventHandlerSafely<T>(System.Action<T> p_handler, T p_data)
		{
			try
			{
				if (p_handler != null) { p_handler(p_data); }
			}
			catch (System.Exception ex)
			{
				Debug.LogError("SteamWorkshopUIBrowse: your event handler ("+p_handler.Target+" - System.Action<"+typeof(T)+">) has thrown an excepotion!\n" + ex);
			}
		}
	}
}