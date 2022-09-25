using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Collections;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// The SteamWorkshopUIBrowse class will use the SteamWorkshopItemNode class to display single items in the list.
	/// The WorkshopItemListEntry prefab has this script attached. The WorkshopItemListEntry prefab is referenced by the SteamWorkshopItemBrowser prefab.
	/// There are two options to customize the item UI:<br />1. Listen to SteamWorkshopUIBrowse.OnItemDataSet event and modify UI when it is triggered.<br />2.
	/// Derive from this class to customize the item UI. Keep in mind to change the script component of the WorkshopItemListEntry prefab to your deriving class.
	/// Override SteamWorkshopItemNode.uMyGUI_TreeBrowser_InitNode to apply your customization, e.g. new entries such as highscores.
	/// </summary>
	public class SteamWorkshopItemNode : MonoBehaviour, IScrollHandler
	{
		/// This is the argument of the SteamWorkshopUIBrowse.OnItemDataSet event.
		/// Use the ItemDataSetEventArgs.ItemUI property to find children of your customized UI e.g. highscores.
		/// Use the ItemDataSetEventArgs.ItemData property to get Steam data of the item.
		public class ItemDataSetEventArgs : EventArgsBase
		{
			/// <summary>
			/// The Steam data of the item.
			/// </summary>
			public WorkshopItem ItemData { get; set; }

			/// <summary>
			/// The uGUI object reference of the item. Can be used to find children of customized UI e.g. highscores.
			/// </summary>
			/// <value>The item U.</value>
			public SteamWorkshopItemNode ItemUI { get; set; }
		}

		/// Internal class used by SteamWorkshopUIBrowse to pass Steam item data to the SteamWorkshopItemNode class.
		public class SendMessageInitData
		{
			/// <summary>
			/// The Steam data of the item.
			/// </summary>
			public WorkshopItem Item { get; set; }
		}

		[SerializeField]
		protected Text m_textName;

		[SerializeField]
		protected Text m_textDescription;

		[SerializeField]
		protected Text m_textVotes;
		
		[SerializeField]
		protected Button m_btnVotesUp;
		
		[SerializeField]
		protected Button m_btnVotesUpActive;
		
		[SerializeField]
		protected Button m_btnVotesDown;
		
		[SerializeField]
		protected Button m_btnVotesDownActive;

		[SerializeField]
		protected Text m_textFavorites;

		[SerializeField]
		protected Button m_btnFavorites;

		[SerializeField]
		protected Button m_btnFavoritesActive;

		[SerializeField]
		protected Text m_textSubscriptions;
		
		[SerializeField]
		protected Text m_textDownloadProgress;
		
		[SerializeField]
		protected Button m_btnSubscriptions;
		
		[SerializeField]
		protected Button m_btnSubscriptionsActive;

		[SerializeField]
		protected RawImage m_image;
		public RawImage Image { get{ return m_image; } }

		[SerializeField]
		protected Image m_selectionImage;

		[SerializeField]
		protected Button m_btnDownload;

		[SerializeField]
		protected Button m_btnPlay;

		[SerializeField]
		protected Button m_btnDelete;
		
		[SerializeField]
		protected bool m_useExplicitNavigation = true;
		
		[SerializeField]
		protected bool m_improveNavigationFocus = true;

		protected SendMessageInitData m_data = null;
		protected ScrollRect m_parentScroller = null;
		protected WWW m_pendingImageDownload = null;
		protected bool isDestroyed = false;

		/// <summary>
		/// Called from the SteamWorkshopUIBrowse class to initialze the item UI and everytime the item data is updated.
		/// </summary>
		/// <param name="p_data">is of type SendMessageInitData. Contains the Steam data of the item.</param>
		public virtual void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (p_data is SendMessageInitData)
			{
				SteamWorkshopMain.Instance.OnInstalled -= OnItemInstalled; // needed to prevent double registration
				SteamWorkshopMain.Instance.OnInstalled += OnItemInstalled;

				m_data = (SendMessageInitData)p_data;
				// preview image
				if (m_image != null && m_image.texture == null && m_pendingImageDownload == null) { StartCoroutine(DownloadPreview(m_data.Item.PreviewImageURL)); }
				// item name and stats
				if (m_textName != null) { m_textName.text = m_data.Item.Name; }
				if (m_textDescription != null) { m_textDescription.text = m_data.Item.Description; }
				if (m_textVotes != null) { m_textVotes.text = m_data.Item.VotesUp + " / " + m_data.Item.VotesDown; }
				if (m_textFavorites != null) { m_textFavorites.text = m_data.Item.Favorites.ToString(); }
				if (m_textSubscriptions != null) { m_textSubscriptions.text = m_data.Item.Subscriptions.ToString(); }
				// icon states and callbacks
				if (m_btnFavorites != null && m_btnFavoritesActive != null)
				{
					m_btnFavorites.gameObject.SetActive(!m_data.Item.IsFavorited);
					m_btnFavoritesActive.gameObject.SetActive(m_data.Item.IsFavorited);
				}
				if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
				{
					m_btnSubscriptions.gameObject.SetActive(!m_data.Item.IsSubscribed);
					m_btnSubscriptionsActive.gameObject.SetActive(m_data.Item.IsSubscribed);
				}
				if (m_btnVotesUp != null && m_btnVotesUpActive != null)
				{
					m_btnVotesUp.gameObject.SetActive(!m_data.Item.IsVotedUp);
					m_btnVotesUpActive.gameObject.SetActive(m_data.Item.IsVotedUp);
				}
				if (m_btnVotesDown != null && m_btnVotesDownActive != null)
				{
					m_btnVotesDown.gameObject.SetActive(!m_data.Item.IsVotedDown);
					m_btnVotesDownActive.gameObject.SetActive(m_data.Item.IsVotedDown);
				}
				// button states and callbacks
				if (m_btnDownload != null)
				{
					m_btnDownload.gameObject.SetActive(!m_data.Item.IsInstalled && !m_data.Item.IsDownloading);
				}
				if (m_btnPlay != null)
				{
					m_btnPlay.gameObject.SetActive(m_data.Item.IsInstalled && !m_data.Item.IsDownloading);
				}
				if (m_btnDelete != null)
				{
					m_btnDelete.gameObject.SetActive(m_data.Item.IsSubscribed);
				}
				// improve button and icon navigation
				if (m_useExplicitNavigation)
				{
					SetNavigationTargetsHorizontal(new Selectable[]{ m_btnDelete, m_btnVotesUp, m_btnVotesUpActive, m_btnVotesDown, m_btnVotesDownActive, m_btnFavorites, m_btnFavoritesActive, m_btnSubscriptions, m_btnSubscriptionsActive, m_btnPlay, m_btnDownload });
					StartCoroutine(SetNavigationTargetsVertical());
				}
				// download progress
				if (m_textDownloadProgress != null)
				{
					m_textDownloadProgress.gameObject.SetActive(m_data.Item.IsDownloading);
				}
				if (m_data.Item.IsDownloading)
				{
					StartCoroutine(ShowDownloadProgress());
				}

				// invoke event
				SteamWorkshopUIBrowse.Instance.InvokeOnItemDataSet(m_data.Item, this);
			}
			else
			{
				Debug.LogError("SteamWorkshopItemNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a SteamWorkshopItemNode.SendMessageInitData! p_data: " + p_data);
			}
		}

		/// <summary>
		/// Internal method implementing the IScrollHandler interface. Required for mouse wheel scrolling of the item list.
		/// </summary>
		/// <param name="data">mouse wheel event data.</param>
		public virtual void OnScroll(PointerEventData data)
		{
			// try to find the parent ScrollRect
			if (m_parentScroller == null)
			{
				m_parentScroller = GetComponentInParent<ScrollRect>();
			}

			// cannot do anything without a parent ScrollRect -> return
			if (m_parentScroller == null)
			{
				return;
			}

			// forward the scroll event data to the parent
			m_parentScroller.OnScroll(data);
		}

		/// <summary>
		/// Selects the download or the play button. Nothing is selected when the download is active. In this case the play button will be selected when the download is finished.
		/// </summary>
		public virtual void Select()
		{
			if (m_btnDownload != null && m_btnDownload.gameObject.activeSelf)
			{
				m_btnDownload.Select();
			}
			else if (m_btnPlay != null && m_btnPlay.gameObject.activeSelf)
			{
				m_btnPlay.Select();
			}
		}

		protected virtual void Start()
		{
			// link button clicks
			if (m_btnFavorites != null && m_btnFavoritesActive != null)
			{
				m_btnFavorites.onClick.AddListener(AddFavorite);
				m_btnFavoritesActive.onClick.AddListener(RemovedFavorite);
			}
			if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
			{
				m_btnSubscriptions.onClick.AddListener(Subscribe);
				m_btnSubscriptionsActive.onClick.AddListener(Unsubscribe);
			}
			if (m_btnVotesUp != null && m_btnVotesUpActive != null)
			{
				m_btnVotesUp.onClick.AddListener(VoteUp);
			}
			if (m_btnVotesDown != null && m_btnVotesDownActive != null)
			{
				m_btnVotesDown.onClick.AddListener(VoteDown);
			}
			if (m_btnDownload != null)
			{
				m_btnDownload.onClick.AddListener(Subscribe);
			}
			if (m_btnPlay != null)
			{
				m_btnPlay.onClick.AddListener(OnPlayBtn);
			}
			if (m_btnDelete != null)
			{
				m_btnDelete.onClick.AddListener(Unsubscribe);
			}
		}

		protected virtual void OnDestroy()
		{
			isDestroyed = true;
			if (m_image != null)
			{
				Destroy(m_image.texture); // release image space
			}
			if (m_pendingImageDownload != null)
			{
				m_pendingImageDownload.Dispose(); // cancel download and release memory
				m_pendingImageDownload = null;
			}
			if (SteamWorkshopMain.IsInstanceSet)
			{
				SteamWorkshopMain.Instance.OnInstalled -= OnItemInstalled;
			}
		}

		protected virtual void OnPlayBtn()
		{
			if (m_data != null)
			{
				SteamWorkshopUIBrowse.Instance.InvokeOnPlayButtonClick(m_data.Item);
			}
		}

		protected virtual void Subscribe()
		{
			if (m_data != null)
			{
				SteamWorkshopMain.Instance.Subscribe(m_data.Item, OnItemUpdated(m_btnSubscriptionsActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnSubscribeButtonClick(m_data.Item);
			}
		}

		protected virtual void Unsubscribe()
		{
			if (m_data != null)
			{
				SteamWorkshopMain.Instance.Unsubscribe(m_data.Item, OnItemUpdated(m_btnSubscriptions));
				SteamWorkshopUIBrowse.Instance.InvokeOnUnsubscribeButtonClick(m_data.Item);
			}
		}

		protected virtual void AddFavorite()
		{
			if (m_data != null)
			{
				SteamWorkshopMain.Instance.AddFavorite(m_data.Item, OnItemUpdated(m_btnFavoritesActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnAddFavoriteButtonClick(m_data.Item);
			}
		}
		
		protected virtual void RemovedFavorite()
		{
			if (m_data != null)
			{
				SteamWorkshopMain.Instance.RemoveFavorite(m_data.Item, OnItemUpdated(m_btnFavorites));
				SteamWorkshopUIBrowse.Instance.InvokeOnRemoveFavoriteButtonClick(m_data.Item);
			}
		}

		protected virtual void VoteUp()
		{
			if (m_data != null)
			{
				SteamWorkshopMain.Instance.Vote(m_data.Item, true, OnItemUpdated(m_btnVotesUpActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnVoteUpButtonClick(m_data.Item);
			}
		}

		protected virtual void VoteDown()
		{
			if (m_data != null)
			{
				SteamWorkshopMain.Instance.Vote(m_data.Item, false, OnItemUpdated(m_btnVotesDownActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnVoteDownButtonClick(m_data.Item);
			}
		}
		
		protected virtual void OnItemInstalled(WorkshopItemEventArgs p_itemArgs)
		{
			OnItemUpdated(m_btnPlay).Invoke(p_itemArgs);
		}
		
		protected virtual System.Action<WorkshopItemEventArgs> OnItemUpdated(Selectable p_focusWhenDone)
		{
			return (WorkshopItemEventArgs p_itemArgs) =>
			{
				if (!isDestroyed && m_data != null && !p_itemArgs.IsError && m_data.Item.SteamNative.m_nPublishedFileId == p_itemArgs.Item.SteamNative.m_nPublishedFileId)
				{
					uMyGUI_TreeBrowser_InitNode(new SendMessageInitData(){ Item = p_itemArgs.Item });
					if (m_improveNavigationFocus && p_focusWhenDone != null)
					{
						p_focusWhenDone.Select();
					}
				}
			};
		}

		protected virtual void SetNavigationTargetsHorizontal(Selectable[] p_horizontalNavOrder)
		{
			for (int currI = 0; currI < p_horizontalNavOrder.Length; currI++)
			{
				Selectable current = p_horizontalNavOrder[currI];
				if (current != null)
				{
					Navigation nav = current.navigation;
					nav.mode = Navigation.Mode.Explicit;
					for (int i = currI-1; i >= 0; i--)
					{
						Selectable leftTarget = p_horizontalNavOrder[i];
						if (leftTarget != null && leftTarget.gameObject.activeSelf)
						{
							nav.selectOnLeft = leftTarget;
							break;
						}
					}
					for (int i = currI + 1; i < p_horizontalNavOrder.Length; i++)
					{
						Selectable rightTarget = p_horizontalNavOrder[i];
						if (rightTarget != null && rightTarget.gameObject.activeSelf)
						{
							nav.selectOnRight = rightTarget;
							break;
						}
					}
					current.navigation = nav;
				}
			}
		}

		protected virtual void SetNavigationTargetsVertical(Selectable p_current, Selectable[] p_verticalNavOrder)
		{
			if (p_current == null || !p_current.gameObject.activeSelf) { return; }

			for (int currI = 0; currI < p_verticalNavOrder.Length; currI++)
			{
				Selectable current = p_verticalNavOrder[currI];
				if (current != null && currI >= 0)
				{
					Navigation nav = current.navigation;
					nav.mode = Navigation.Mode.Explicit;
					for (int i = currI-1; i >= 0; i--)
					{
						Selectable upTarget = p_verticalNavOrder[i];
						if (upTarget != null && upTarget.gameObject.activeSelf)
						{
							nav.selectOnUp = upTarget;
							break;
						}
					}
					for (int i = currI + 1; i < p_verticalNavOrder.Length; i++)
					{
						Selectable downTarget = p_verticalNavOrder[i];
						if (downTarget != null && downTarget.gameObject.activeSelf)
						{
							nav.selectOnDown = downTarget;
							break;
						}
					}
					current.navigation = nav;
				}
			}
		}

		protected virtual IEnumerator SetNavigationTargetsVertical()
		{
			yield return new WaitForEndOfFrame(); // wait until all nodes of the list have been loaded

			if (transform.parent != null)
			{
				// find the neighbours
				SteamWorkshopItemNode[] allNodes = transform.parent.GetComponentsInChildren<SteamWorkshopItemNode>();
				int selfIndex = System.Array.IndexOf(allNodes, this);
				if (selfIndex >= 0)
				{
					SteamWorkshopItemNode self = allNodes[selfIndex];
					SteamWorkshopItemNode up = selfIndex > 0 ? allNodes[selfIndex - 1] : null;
					SteamWorkshopItemNode down = selfIndex < allNodes.Length - 1 ? allNodes[selfIndex + 1] : null;
					SetNavigationTargetsVertical(self.m_btnDelete, new Selectable[]{
						up ? up.m_btnDelete : null,
						self.m_btnDelete,
						down ? down.m_btnDelete : null });
					SetNavigationTargetsVertical(self.m_btnVotesUp, new Selectable[]{
						up ? up.m_btnVotesUp : null, up ? up.m_btnVotesUpActive : null,
						self.m_btnVotesUp,
						down ? down.m_btnVotesUp : null, down ? down.m_btnVotesUpActive : null });
					SetNavigationTargetsVertical(self.m_btnVotesUpActive, new Selectable[]{
						up ? up.m_btnVotesUp : null, up ? up.m_btnVotesUpActive : null,
						self.m_btnVotesUpActive,
						down ? down.m_btnVotesUp : null, down ? down.m_btnVotesUpActive : null });
					SetNavigationTargetsVertical(self.m_btnVotesDown, new Selectable[]{
						up ? up.m_btnVotesDown : null, up ? up.m_btnVotesDownActive : null,
						self.m_btnVotesDown,
						down ? down.m_btnVotesDown : null, down ? down.m_btnVotesDownActive : null });
					SetNavigationTargetsVertical(self.m_btnVotesDownActive, new Selectable[]{
						up ? up.m_btnVotesDown : null, up ? up.m_btnVotesDownActive : null,
						self.m_btnVotesDownActive,
						down ? down.m_btnVotesDown : null, down ? down.m_btnVotesDownActive : null });
					SetNavigationTargetsVertical(self.m_btnFavorites, new Selectable[]{
						up ? up.m_btnFavorites : null, up ? up.m_btnFavoritesActive : null,
						self.m_btnFavorites,
						down ? down.m_btnFavorites : null, down ? down.m_btnFavoritesActive : null });
					SetNavigationTargetsVertical(self.m_btnFavoritesActive, new Selectable[]{
						up ? up.m_btnFavorites : null, up ? up.m_btnFavoritesActive : null,
						self.m_btnFavoritesActive,
						down ? down.m_btnFavorites : null, down ? down.m_btnFavoritesActive : null });
					SetNavigationTargetsVertical(self.m_btnSubscriptions, new Selectable[]{
						up ? up.m_btnSubscriptions : null, up ? up.m_btnSubscriptionsActive : null,
						self.m_btnSubscriptions,
						down ? down.m_btnSubscriptions : null, down ? down.m_btnSubscriptionsActive : null });
					SetNavigationTargetsVertical(self.m_btnSubscriptionsActive, new Selectable[]{
						up ? up.m_btnSubscriptions : null, up ? up.m_btnSubscriptionsActive : null,
						self.m_btnSubscriptionsActive,
						down ? down.m_btnSubscriptions : null, down ? down.m_btnSubscriptionsActive : null });
					SetNavigationTargetsVertical(self.m_btnPlay, new Selectable[]{
						up ? up.m_btnPlay : null, up ? up.m_btnDownload : null,
						self.m_btnPlay,
						down ? down.m_btnPlay : null, down ? down.m_btnDownload : null });
					SetNavigationTargetsVertical(self.m_btnDownload, new Selectable[]{
						up ? up.m_btnPlay : null, up ? up.m_btnDownload : null,
						self.m_btnDownload,
						down ? down.m_btnPlay : null, down ? down.m_btnDownload : null });
					// automatic navigation in the start and in the end of the list
					if (selfIndex == 0 || selfIndex == allNodes.Length - 1)
					{
						yield return new WaitForEndOfFrame();

						SetAutomaticNavigation(m_btnDelete);
						SetAutomaticNavigation(m_btnVotesUp);
						SetAutomaticNavigation(m_btnVotesUpActive);
						SetAutomaticNavigation(m_btnVotesDown);
						SetAutomaticNavigation(m_btnVotesDownActive);
						SetAutomaticNavigation(m_btnFavorites);
						SetAutomaticNavigation(m_btnFavoritesActive);
						SetAutomaticNavigation(m_btnSubscriptions);
						SetAutomaticNavigation(m_btnSubscriptionsActive);
						SetAutomaticNavigation(m_btnPlay);
						SetAutomaticNavigation(m_btnDownload);
					}
				}
			}
		}

		protected virtual void SetAutomaticNavigation(Selectable p_selectable)
		{
			if (p_selectable != null)
			{
				Navigation nav = p_selectable.navigation;
				nav.mode = Navigation.Mode.Automatic;
				p_selectable.navigation = nav;
			}
		}
		
		protected virtual IEnumerator ShowDownloadProgress()
		{
			while (m_data != null && m_data.Item.IsDownloading)
			{
				if (m_textDownloadProgress != null)
				{
					m_textDownloadProgress.gameObject.SetActive(true);
					m_textDownloadProgress.text = (int)(SteamWorkshopMain.Instance.GetDownloadProgress(m_data.Item) * 100f) + "%";
				}
				yield return new WaitForSeconds(0.4f);
			}
		}

		protected virtual IEnumerator DownloadPreview(string p_URL)
		{
			if (!string.IsNullOrEmpty(p_URL))
			{
				m_pendingImageDownload = new WWW(p_URL);
				yield return m_pendingImageDownload;
				if (m_pendingImageDownload != null)
				{
					if (m_pendingImageDownload.isDone && string.IsNullOrEmpty(m_pendingImageDownload.error))
					{
						if (m_image != null)
						{
							m_image.texture = m_pendingImageDownload.texture;
						}
					}
					else
					{
						Debug.LogError("SteamWorkshopItemNode: DownloadPreview: could not load preview image at '" + p_URL + "'\n" + m_pendingImageDownload.error);
					}
					m_pendingImageDownload = null;
				}
			}
		}
	}
}
