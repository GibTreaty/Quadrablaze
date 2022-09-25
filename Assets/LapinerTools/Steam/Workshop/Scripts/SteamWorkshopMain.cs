using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using LapinerTools.Steam.Data;
using LapinerTools.Steam.Data.Internal;

using Steamworks;

namespace LapinerTools.Steam
{
	/// <summary>
	/// SteamWorkshopMain is the easy to use lightweight Steam Workshop API of the Easy Steamworks Integration Unity plugin.
	/// Use this class to list, search, sort, download, subscribe, vote, favorite, upload and update Steam workshop items.
	/// </summary>
	public class SteamWorkshopMain : SteamMainBase<SteamWorkshopMain>
	{
#region Members

		// Page that was requested
		private uint m_reqPage = 0;
		// If not null, then a GetItemList request is being processed
		private WorkshopItemList m_reqItemList = null;
		// This map contains all items that were loaded until now
		private Dictionary<PublishedFileId_t, WorkshopItem> m_items = new Dictionary<PublishedFileId_t, WorkshopItem>();
		// All pending downloads
		private List<PublishedFileId_t> m_downloadingItems = new List<PublishedFileId_t>();
		// This item is currently being uploaded
		private WorkshopItemUpdate m_uploadItemData = null;
		// This is the texture that was rendered by RenderIcon
		private Texture2D m_renderedTexture = null;

#endregion


#region Events

		/// <summary>
		/// Invoked when the item list is fully loaded. See also SteamWorkshopMain.GetItemList.
		/// </summary>
		public event System.Action<WorkshopItemListEventArgs> OnItemListLoaded;
		/// <summary>
		/// Invoked when an item was successfully subscribed. See also SteamWorkshopMain.Subscribe.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnSubscribed;
		/// <summary>
		/// Invoked when an item was successfully unsubscribed. See also SteamWorkshopMain.Unsubscribe.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnUnsubscribed;
		/// <summary>
		/// Invoked when an item was successfully added to the user's favorites list. See also SteamWorkshopMain.AddFavorite.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnAddedFavorite;
		/// <summary>
		/// Invoked when an item was successfully removed from the user's favorites list. See also SteamWorkshopMain.RemoveFavorite.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnRemovedFavorite;
		/// <summary>
		/// Invoked when an item was successfully voted up or down. See also SteamWorkshopMain.Vote.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnVoted;
		/// <summary>
		/// Invoked when an item was successfully downloaded and installed. See also SteamWorkshopMain.Subscribe and SteamWorkshopMain.GetDownloadProgress.
		/// </summary>
		public event System.Action<WorkshopItemEventArgs> OnInstalled;
		/// <summary>
		/// Invoked when an item was successfully uploaded or updated. See also SteamWorkshopMain.Upload and SteamWorkshopMain.GetUploadProgress.
		/// </summary>
		public event System.Action<WorkshopItemUpdateEventArgs> OnUploaded;

#endregion


#region API

		[SerializeField, Tooltip("Controls the item list sorting. See also OnItemListLoaded and GetItemList.")]
		private WorkshopSortMode m_sorting = new WorkshopSortMode();
		/// <summary>
		/// Controls the item list sorting. See also SteamWorkshopMain.OnItemListLoaded and SteamWorkshopMain.GetItemList.
		/// </summary>
		public WorkshopSortMode Sorting
		{
			get{ return m_sorting; }
			set{ m_sorting = value; }
		}

		[SerializeField, Tooltip("This search filter is applied to the item list. See also OnItemListLoaded and GetItemList.")]
		private string m_searchText = "";
		/// <summary>
		/// This search filter is applied to the item list. See also SteamWorkshopMain.OnItemListLoaded and SteamWorkshopMain.GetItemList.
		/// </summary>
		public string SearchText
		{
			get{ return m_searchText; }
			set{ m_searchText = value; }
		}

		[SerializeField, Tooltip("This tag filter is applied to the item list. See also SearchMatchAnyTag, OnItemListLoaded and GetItemList.")]
		private List<string> m_searchTags = new List<string>();
		/// <summary>
		/// This tag filter is applied to the item list. See also SteamWorkshopMain.SearchMatchAnyTag, SteamWorkshopMain.OnItemListLoaded and SteamWorkshopMain.GetItemList.
		/// </summary>
		public List<string> SearchTags
		{
			get{ return m_searchTags; }
			set{ m_searchTags = value; }
		}

		[SerializeField, Tooltip("Should the items filtered by SearchTags just need to have one required tag (true), or all of them (false). See also OnItemListLoaded and GetItemList.")]
		private bool m_searchMatchAnyTag = true;
		/// <summary>
		/// Should the items filtered by SteamWorkshopMain.SearchTags just need to have one required tag (true), or all of them (false). See also SteamWorkshopMain.OnItemListLoaded and SteamWorkshopMain.GetItemList.
		/// </summary>
		public bool SearchMatchAnyTag
		{
			get{ return m_searchMatchAnyTag; }
			set{ m_searchMatchAnyTag = value; }
		}

		[SerializeField, Tooltip("Set this property to true if you want your UI to respond faster, but sacrifice up-to-dateness. Disabled by default.")]
		private bool m_isSteamCacheEnabled = false;
		/// <summary>
		/// Set this property to <c>true</c> if you want your UI to respond faster, but sacrifice up-to-dateness. Disabled by default.
		/// </summary>
		public bool IsSteamCacheEnabled
		{
			get{ return m_isSteamCacheEnabled; }
			set{ m_isSteamCacheEnabled = value; }
		}

		/// <summary>
		/// Loads the item list for the given page. The p_onItemListLoaded callback is invoked when done.
		/// First all favorite items of the user are fetched, then all user votes are loaded.
		/// Once this is done the given page is loaded and the prior loaded user favorites and vote states are applied.
		/// See also SteamWorkshopMain.OnItemListLoaded, SteamWorkshopMain.Sorting and SteamWorkshopMain.SearchText.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_page">item list page to load, starts with 1.</param>
		/// <param name="p_onItemListLoaded">invoked when the item list is either fully loaded or an error has occured.</param>
		public bool GetItemList(uint p_page, System.Action<WorkshopItemListEventArgs> p_onItemListLoaded)
		{
			if (p_page <= 0)
			{
				ErrorEventArgs errorArgs = new ErrorEventArgs("Page (p_page parameter) must be greater 0, but was '" + p_page + "'!");
				InvokeEventHandlerSafely(p_onItemListLoaded, new WorkshopItemListEventArgs(errorArgs));
				HandleError("GetItemList: failed! ", errorArgs);
				return false;
			}

			lock (m_lock)
			{
				if (m_reqItemList != null) { return false; } // request is already being processed

				if (SteamManager.Initialized)
				{
					// create the requested item list object to indicate that a request is being processed
					m_reqItemList = new WorkshopItemList();
					m_reqItemList.Page = p_page;
					m_reqItemList.PagesItemsFavorited = 0;
					m_reqItemList.PagesItemsVoted = 0;
					m_pendingRequests.Clear<GetUserItemVoteResult_t>();

					// subscribe to the OnItemListLoaded event and remove subscription after first result
					SetSingleShotEventHandler("OnItemListLoaded", ref OnItemListLoaded, p_onItemListLoaded);

					// start query for favorite items (all available items will be loaded when this request is finished)
					QueryFavoritedItems(1); // start on first page of voted items, then iterate the rest

					return true; // request started
				}
				else
				{
					ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
					InvokeEventHandlerSafely(p_onItemListLoaded, new WorkshopItemListEventArgs(errorArgs));
					HandleError("GetItemList: failed! ", errorArgs);
					return false; // no request, because there is no connection to steam
				}
			}
		}

		/// <summary>
		/// Subscribe an item. If the subscription is succesfull, then the download is started.
		/// See also SteamWorkshopMain.OnSubscribed and SteamWorkshopMain.GetDownloadProgress.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_item">item to subscribe.</param>
		/// <param name="p_onSubscribed">invoked when the item is subscribed successfully or an error has occured.</param>
		public bool Subscribe(WorkshopItem p_item, System.Action<WorkshopItemEventArgs> p_onSubscribed)
		{
			return Subscribe(p_item.SteamNative.m_nPublishedFileId, p_onSubscribed);
		}

		/// <summary>
		/// Subscribe an item. If the subscription is succesfull, then the download is started.
		/// See also SteamWorkshopMain.OnSubscribed and SteamWorkshopMain.GetDownloadProgress.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_fileId">Steam PublishedFileId of the item to subscribe.</param>
		/// <param name="p_onSubscribed">invoked when the item is subscribed successfully or an error has occured.</param>
		public bool Subscribe(PublishedFileId_t p_fileId, System.Action<WorkshopItemEventArgs> p_onSubscribed)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnSubscribed" + p_fileId, ref OnSubscribed, p_onSubscribed);
				Execute<RemoteStorageSubscribePublishedFileResult_t>(SteamUGC.SubscribeItem(p_fileId), OnSubscribeCallCompleted);
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onSubscribed, new WorkshopItemEventArgs(errorArgs));
				HandleError("Subscribe: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Unsubscribe an item. Will not remove the item from disk.
		/// If the item is not used for a certain time, then the Steam client will remove it.
		/// See also SteamWorkshopMain.OnUnsubscribed.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_item">item to unsubscribe.</param>
		/// <param name="p_onUnsubscribed">invoked when the item is unsubscribed successfully or an error has occured.</param>
		public bool Unsubscribe(WorkshopItem p_item, System.Action<WorkshopItemEventArgs> p_onUnsubscribed)
		{
			return Unsubscribe(p_item.SteamNative.m_nPublishedFileId, p_onUnsubscribed);
		}
		
		/// <summary>
		/// Unsubscribe an item. Will not remove the item from disk.
		/// If the item is not used for a certain time, then the Steam client will remove it.
		/// See also SteamWorkshopMain.OnUnsubscribed.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_fileId">Steam PublishedFileId of the item to unsubscribe.</param>
		/// <param name="p_onUnsubscribed">invoked when the item is unsubscribed successfully or an error has occured.</param>
		public bool Unsubscribe(PublishedFileId_t p_fileId, System.Action<WorkshopItemEventArgs> p_onUnsubscribed)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnUnsubscribed" + p_fileId, ref OnUnsubscribed, p_onUnsubscribed);
				Execute<RemoteStorageUnsubscribePublishedFileResult_t>(SteamUGC.UnsubscribeItem(p_fileId), OnUnsubscribeCallCompleted);
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onUnsubscribed, new WorkshopItemEventArgs(errorArgs));
				HandleError("Unsubscribe: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Add an item to user's favorites list.
		/// See also SteamWorkshopMain.OnAddedFavorite.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_item">item added to favorites.</param>
		/// <param name="p_onAddedFavorite">invoked when the item is added to favorites successfully or an error has occured.</param>
		public bool AddFavorite(WorkshopItem p_item, System.Action<WorkshopItemEventArgs> p_onAddedFavorite)
		{
			return AddFavorite(p_item.SteamNative.m_nPublishedFileId, p_onAddedFavorite);
		}
		
		/// <summary>
		/// Add an item to user's favorites list.
		/// See also SteamWorkshopMain.OnAddedFavorite.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_fileId">Steam PublishedFileId of the item added to favorites.</param>
		/// <param name="p_onAddedFavorite">invoked when the item is added to favorites successfully or an error has occured.</param>
		public bool AddFavorite(PublishedFileId_t p_fileId, System.Action<WorkshopItemEventArgs> p_onAddedFavorite)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnAddedFavorite" + p_fileId, ref OnAddedFavorite, p_onAddedFavorite);
				Execute<UserFavoriteItemsListChanged_t>(SteamUGC.AddItemToFavorites(SteamUtils.GetAppID(), p_fileId), OnFavoriteChangeCallCompleted);
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onAddedFavorite, new WorkshopItemEventArgs(errorArgs));
				HandleError("AddFavorite: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}
		
		/// <summary>
		/// Remove an item from user's favorites list.
		/// See also SteamWorkshopMain.OnRemovedFavorite.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_item">item removed from favorites.</param>
		/// <param name="p_onRemovedFavorite">invoked when the item is removed from favorites successfully or an error has occured.</param>
		public bool RemoveFavorite(WorkshopItem p_item, System.Action<WorkshopItemEventArgs> p_onRemovedFavorite)
		{
			return RemoveFavorite(p_item.SteamNative.m_nPublishedFileId, p_onRemovedFavorite);
		}
		
		/// <summary>
		/// Remove an item from user's favorites list.
		/// See also SteamWorkshopMain.OnRemovedFavorite.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_fileId">Steam PublishedFileId of the item removed from favorites.</param>
		/// <param name="p_onRemovedFavorite">invoked when the item is removed from favorites successfully or an error has occured.</param>
		public bool RemoveFavorite(PublishedFileId_t p_fileId, System.Action<WorkshopItemEventArgs> p_onRemovedFavorite)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnRemovedFavorite" + p_fileId, ref OnRemovedFavorite, p_onRemovedFavorite);
				Execute<UserFavoriteItemsListChanged_t>(SteamUGC.RemoveItemFromFavorites(SteamUtils.GetAppID(), p_fileId), OnFavoriteChangeCallCompleted);
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onRemovedFavorite, new WorkshopItemEventArgs(errorArgs));
				HandleError("RemoveFavorite: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Vote an item up or down.
		/// See also SteamWorkshopMain.OnVoted.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_item">voted item.</param>
		/// <param name="p_isUpVote">if set to <c>true</c> then the item will be voted up, otherwise the item is voted down.</param>
		/// <param name="p_onVoted">invoked when the item vote is successfull or an error has occured.</param>
		public bool Vote(WorkshopItem p_item, bool p_isUpVote, System.Action<WorkshopItemEventArgs> p_onVoted)
		{
			return Vote(p_item.SteamNative.m_nPublishedFileId, p_isUpVote, p_onVoted);
		}
		
		/// <summary>
		/// Vote an item up or down.
		/// See also SteamWorkshopMain.OnVoted.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_fileId">Steam PublishedFileId of the voted item.</param>
		/// <param name="p_isUpVote">if set to <c>true</c> then the item will be voted up, otherwise the item is voted down.</param>
		/// <param name="p_onVoted">invoked when the item vote is successfull or an error has occured.</param>
		public bool Vote(PublishedFileId_t p_fileId, bool p_isUpVote, System.Action<WorkshopItemEventArgs> p_onVoted)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnVoted" + p_fileId, ref OnVoted, p_onVoted);
				Execute<SetUserItemVoteResult_t>(SteamUGC.SetUserItemVote(p_fileId, p_isUpVote), OnVoteCallCompleted);
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onVoted, new WorkshopItemEventArgs(errorArgs));
				HandleError("Vote: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Gets the download progress of an item. Use SteamWorkshopMain.Subscribe to start download.
		/// See also SteamWorkshopMain.OnInstalled.
		/// </summary>
		/// <returns>the download progress between 0 and 1.</returns>
		/// <param name="p_item">item being downloaded.</param>
		public float GetDownloadProgress(WorkshopItem p_item)
		{
			return GetDownloadProgress(p_item.SteamNative.m_nPublishedFileId);
		}

		/// <summary>
		/// Gets the download progress of an item. Use SteamWorkshopMain.Subscribe to start download.
		/// See also SteamWorkshopMain.OnInstalled.
		/// </summary>
		/// <returns>the download progress between 0 and 1.</returns>
		/// <param name="p_fileId">Steam PublishedFileId of the item being downloaded.</param>
		public float GetDownloadProgress(PublishedFileId_t p_fileId)
		{
			if (SteamManager.Initialized)
			{
				EItemState state = (EItemState)SteamUGC.GetItemState(p_fileId);
				if (IsDownloading(state))
				{
					ulong loaded, total;
					if (SteamUGC.GetItemDownloadInfo(p_fileId, out loaded, out total) && total != 0)
					{
						return (float)loaded / (float)total;
					}
					else
					{
						return 0f;
					}
				}
				else if (IsInstalled(state))
				{
					return 1f;
				}
			}
			return 0f;
		}

		/// <summary>
		/// Gets the upload progress of an item. Use SteamWorkshopMain.Upload to start upload.
		/// See also SteamWorkshopMain.OnUploaded.
		/// </summary>
		/// <returns>the upload progress between 0 and 1.</returns>
		/// <param name="p_itemUpdate">update data of the item being uploaded.</param>
		public float GetUploadProgress(WorkshopItemUpdate p_itemUpdate)
		{
			if (SteamManager.Initialized && p_itemUpdate.SteamNative.m_uploadHandle != UGCUpdateHandle_t.Invalid)
			{
				ulong loaded, total;
				EItemUpdateStatus state = (EItemUpdateStatus)SteamUGC.GetItemUpdateProgress(p_itemUpdate.SteamNative.m_uploadHandle, out loaded, out total);
				if (state != EItemUpdateStatus.k_EItemUpdateStatusInvalid)
				{
					p_itemUpdate.SteamNative.m_lastValidUpdateStatus = state;
				}

				switch (state)
				{
					case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig: // 0%
						return 0f;
					case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent: // 0% - 10%
						return (total > 0 ? (float)loaded / (float)total : 0f) * 0.1f;
					case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent: // 10% - 75%
						return (total > 0 ? (float)loaded / (float)total : 0f) * 0.65f + 0.1f;
					case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile: // 75% - 90%
						return (total > 0 ? (float)loaded / (float)total : 0f) * 0.15f + 0.75f;
					case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges: // 90% - 100%
						return (total > 0 ? (float)loaded / (float)total : 0f) * 0.1f + 0.9f;
				}

				if (p_itemUpdate.SteamNative.m_lastValidUpdateStatus != EItemUpdateStatus.k_EItemUpdateStatusInvalid)
				{
					return 1f;
				}
			}
			return 0f;
		}

		/// <summary>
		/// Upload or update the given item.
		/// A new item is created if p_itemData's WorkshopItemUpdate.SteamNative.m_nPublishedFileId is not set, otherwise
		/// the existing item is updated (new Steam PublishedFileId is stored in p_itemData).
		/// If p_itemData's WorkshopItemUpdate.ContentPath is given, then all contents at this path are uploaded or updated.
		/// Creates a WorkshopItemInfo.xml file to allow later item updates, see SteamWorkshopMain.GetItemUpdateFromFolder.
		/// See also SteamWorkshopMain.GetUploadProgress.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_itemData">update data of the item to upload.</param>
		/// <param name="p_onUploaded">invoked when the item upload is successfull or an error has occured.</param>
		public bool Upload(WorkshopItemUpdate p_itemData, System.Action<WorkshopItemUpdateEventArgs> p_onUploaded)
		{
			if (SteamManager.Initialized)
			{
				// check if item has any content attached
				bool isContentToUploadFound = false;
				if (!string.IsNullOrEmpty(p_itemData.ContentPath))
				{
					string[] filesAtContentPath = System.IO.Directory.GetFiles(p_itemData.ContentPath);
					foreach (string filePath in filesAtContentPath)
					{
						if (!System.IO.Path.Equals(filePath, p_itemData.IconPath))
						{
							// there is at least one file in addition to the item icon
							isContentToUploadFound = true;
							break;
						}
					}
				}

				if (!isContentToUploadFound)
				{
					ErrorEventArgs errorArgs = new ErrorEventArgs("No content to upload found! WorkshopItemUpdate.ContentPath is set to '"+p_itemData.ContentPath+"'!");
					InvokeEventHandlerSafely(p_onUploaded, new WorkshopItemUpdateEventArgs(errorArgs));
					HandleError("Upload: failed! ", errorArgs);
					return false; // no request, because there is nothing to upload
				}

				// check if this is an item update or item creation
				m_uploadItemData = p_itemData;
				if (m_uploadItemData.SteamNative.m_nPublishedFileId == PublishedFileId_t.Invalid)
				{
					// create new item
					SetSingleShotEventHandler("OnUploaded", ref OnUploaded, p_onUploaded);
					Execute<CreateItemResult_t>(SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity), OnCreateItemCompleted);
				}
				else
				{
					// save an XML with the most important data to allow later item updates
					if (!string.IsNullOrEmpty(m_uploadItemData.ContentPath))
					{
                        //using(var stream = System.IO.File.Open(System.IO.Path.Combine(m_uploadItemData.ContentPath, "WorkshopItemInfo.xml"), System.IO.FileMode.Create))
                        using(System.IO.FileStream stream = new System.IO.FileStream(System.IO.Path.Combine(m_uploadItemData.ContentPath, "WorkshopItemInfo.xml"), System.IO.FileMode.Create)) {
                            //using(var xmlWriter = System.Xml.XmlWriter.Create(stream))
                            using(var xmlWriter = new System.Xml.XmlTextWriter(stream, System.Text.Encoding.UTF8)) {
                                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                                xmlWriter.Indentation = 4;

                                new XmlSerializer(typeof(WorkshopItemInfo)).Serialize(xmlWriter, new WorkshopItemInfo() {
                                    PublishedFileId = m_uploadItemData.SteamNative.m_nPublishedFileId.m_PublishedFileId,
                                    Name = m_uploadItemData.Name,
                                    Description = m_uploadItemData.Description,
                                    IconFileName = !string.IsNullOrEmpty(m_uploadItemData.IconPath) ? System.IO.Path.GetFileName(m_uploadItemData.IconPath) : "",
                                    Tags = m_uploadItemData.Tags != null ? m_uploadItemData.Tags.ToArray() : new string[0]
                                });
                            }
                        }
					}

					// update existing item
					UGCUpdateHandle_t itemUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), m_uploadItemData.SteamNative.m_nPublishedFileId);
					m_uploadItemData.SteamNative.m_uploadHandle = itemUpdateHandle;
					bool isItemVisibilitySet = SteamUGC.SetItemVisibility(itemUpdateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
					bool isItemTitleSet = !string.IsNullOrEmpty(m_uploadItemData.Name) && SteamUGC.SetItemTitle(itemUpdateHandle, m_uploadItemData.Name);
					bool isItemDescriptionSet = !string.IsNullOrEmpty(m_uploadItemData.Description) && SteamUGC.SetItemDescription(itemUpdateHandle, m_uploadItemData.Description);
					bool isItemPreviewSet = !string.IsNullOrEmpty(m_uploadItemData.IconPath) && SteamUGC.SetItemPreview(itemUpdateHandle, m_uploadItemData.IconPath);
					bool isItemContentSet = !string.IsNullOrEmpty(m_uploadItemData.ContentPath) && SteamUGC.SetItemContent(itemUpdateHandle, m_uploadItemData.ContentPath);
					bool isItemTagsSet = m_uploadItemData.Tags != null && m_uploadItemData.Tags.Count > 0 && SteamUGC.SetItemTags(itemUpdateHandle, m_uploadItemData.Tags);

					if (!isItemVisibilitySet) { HandleError("Upload: ", new ErrorEventArgs("Could not set item visibility to 'public'!")); }
					if (!string.IsNullOrEmpty(m_uploadItemData.Name) && !isItemTitleSet) { HandleError("Upload: ", new ErrorEventArgs("Could not set item title to '" + m_uploadItemData.Name + "'!")); }
					if (!string.IsNullOrEmpty(m_uploadItemData.Description) && !isItemDescriptionSet) { HandleError("Upload: ", new ErrorEventArgs("Could not set item description to '" + m_uploadItemData.Description + "'!")); }
					if (!string.IsNullOrEmpty(m_uploadItemData.IconPath) && !isItemPreviewSet) { HandleError("Upload: ", new ErrorEventArgs("Could not set item icon path to '" + m_uploadItemData.IconPath + "'!")); }
					if (!string.IsNullOrEmpty(m_uploadItemData.ContentPath) && !isItemContentSet) { HandleError("Upload: ", new ErrorEventArgs("Could not set item content path to '" + m_uploadItemData.ContentPath + "'!")); }
					if (m_uploadItemData.Tags != null && m_uploadItemData.Tags.Count > 0 && !isItemTagsSet) { HandleError("Upload: ", new ErrorEventArgs("Could not set item tags!")); }

					if (IsDebugLogEnabled) { Debug.Log("Upload: starting..."); }
					SetSingleShotEventHandler("OnUploaded", ref OnUploaded, p_onUploaded);
					Execute<SubmitItemUpdateResult_t>(SteamUGC.SubmitItemUpdate(itemUpdateHandle, p_itemData.ChangeNote), OnItemUpdateCompleted);
				}
				return true;
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onUploaded, new WorkshopItemUpdateEventArgs(errorArgs));
				HandleError("Upload: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Will make a screenshot with the given resolution without rendering the uGUI overlay.
		/// </summary>
		/// <param name="p_camera">camera to render.</param>
		/// <param name="p_width">screenshot width, must be less or equal screen width.</param>
		/// <param name="p_height">screenshot height, must be less or equal screen height.</param>
		/// <param name="p_saveToFilePath">screenshot is saved as a PNG file at this path. Can be null or empty.</param>
		/// <param name="p_onRenderIconCompleted">invoked when the screenshot is rendered. A Texture2D instance containing the screenshot is passed as argument.</param>
		public void RenderIcon(Camera p_camera, int p_width, int p_height, string p_saveToFilePath, System.Action<Texture2D> p_onRenderIconCompleted)
		{
			StartCoroutine(RenderIconRoutine(p_camera, p_width, p_height, p_saveToFilePath, true, p_onRenderIconCompleted));
		}

		/// <summary>
		/// Will make a screenshot with the given resolution without rendering the uGUI overlay.
		/// </summary>
		/// <param name="p_camera">camera to render.</param>
		/// <param name="p_width">screenshot width, must be less or equal screen width.</param>
		/// <param name="p_height">screenshot height, must be less or equal screen height.</param>
		/// <param name="p_saveToFilePath">screenshot is saved as a PNG file at this path. Can be null or empty.</param>
		/// <param name="p_keepTextureReference">if set to <c>true</c> then the Texture2D will destroyed with the SteamWorkshopMain instance or when the next screenshot is taken, otherwise you must destroy the Texture2D in your code.</param>
		/// <param name="p_onRenderIconCompleted">invoked when the screenshot is rendered. A Texture2D instance containing the screenshot is passed as argument.</param>
		public void RenderIcon(Camera p_camera, int p_width, int p_height, string p_saveToFilePath, bool p_keepTextureReference, System.Action<Texture2D> p_onRenderIconCompleted)
		{
			StartCoroutine(RenderIconRoutine(p_camera, p_width, p_height, p_saveToFilePath, p_keepTextureReference, p_onRenderIconCompleted));
		}

		/// <summary>
		/// Tries to load the Data.WorkshopItemUpdate data from an item folder.
		/// This data can be used in SteamWorkshopMain.Upload to update existing items.
		/// The item update data is stored in the WorkshopItemInfo.xml file.
		/// </summary>
		/// <returns>a Data.WorkshopItemUpdate instance if the item update data was loaded successfuly, otherwise null.</returns>
		/// <param name="p_itemContentFolderPath">Path of the item folder containing a WorkshopItemInfo.xml file.</param>
		public WorkshopItemUpdate GetItemUpdateFromFolder(string p_itemContentFolderPath)
		{
			WorkshopItemUpdate itemUpdate = null;
			string infoFilePath = System.IO.Path.Combine(p_itemContentFolderPath, "WorkshopItemInfo.xml");
			if (System.IO.File.Exists(infoFilePath))
			{
				try
				{
					using (System.IO.FileStream stream = new System.IO.FileStream(infoFilePath, System.IO.FileMode.Open))
					{
						WorkshopItemInfo info = new XmlSerializer(typeof(WorkshopItemInfo)).Deserialize(stream) as WorkshopItemInfo;
						itemUpdate = new WorkshopItemUpdate(new PublishedFileId_t(info.PublishedFileId))
						{
							Name = info.Name,
							Description = info.Description,
							ContentPath = p_itemContentFolderPath,
						};
						if (!string.IsNullOrEmpty(info.IconFileName))
						{
							string iconFilePath = System.IO.Path.Combine(p_itemContentFolderPath, info.IconFileName);
							if (System.IO.File.Exists(iconFilePath))
							{
								itemUpdate.IconPath = iconFilePath;
							}
						}
					}	
				}
				catch (System.Exception ex)
				{
					Debug.LogError("SteamWorkshopMain: GetItemUpdateFromFolder: could not parse item info at '"+infoFilePath+"'!\n" + ex.Message);
				}
			}
			else
			{
				Debug.LogError("SteamWorkshopMain: GetItemUpdateFromFolder: could not find item info at '"+infoFilePath+"'!");
			}
			return itemUpdate;
		}

#endregion


#region MonoBehaviour

		protected override void LateUpdate()
		{
			lock(m_lock)
			{
				// update state of downloading items
				for (int i = m_downloadingItems.Count-1; i >= 0; i--)
				{
					WorkshopItem item;
					PublishedFileId_t fileId = m_downloadingItems[i];
					if (m_items.TryGetValue(fileId, out item))
					{
						EItemState itemState = (EItemState)SteamUGC.GetItemState(fileId);
						if (itemState != item.SteamNative.m_itemState || IsInstalled(itemState))
						{
							item.SteamNative.m_itemState = itemState;
							item.IsInstalled = IsInstalled(itemState);
							item.IsDownloading = IsDownloading(itemState);
							item.IsUpdateNeeded = IsUpdateNeeded(itemState);
							if (item.IsInstalled)
							{
								// item is now downloaded and installed -> get installation infos
								string localFolder; ulong sizeOnDisk; uint timestamp;
								System.DateTime timestampParsed = System.DateTime.MinValue;
								if (SteamUGC.GetItemInstallInfo(fileId, out sizeOnDisk, out localFolder, 260, out timestamp))
								{
									timestampParsed = new System.DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
									timestampParsed = timestampParsed.AddSeconds(timestamp).ToLocalTime();
								}
								item.InstalledLocalFolder = localFolder;
								item.InstalledSizeOnDisk = sizeOnDisk;
								item.InstalledTimestamp = timestampParsed;

								// remove from pending download list and invoke event
								m_downloadingItems.RemoveAt(i);
								if (IsDebugLogEnabled) { Debug.Log("SteamWorkshopMain: item installed " + fileId + (OnInstalled != null ? " (will notify)" : " (no listeners)")); }
								if (OnInstalled != null)
								{
									InvokeEventHandlerSafely(OnInstalled, new WorkshopItemEventArgs(item));
									ClearSingleShotEventHandlers("OnInstalled" + fileId, ref OnInstalled);
								}
							}
						}
					}
					else
					{
						m_downloadingItems.RemoveAt(i); // lost reference to item anyway
					}
				}

				// save current get user vote request count
				int pendingVoteReqCountAll = m_pendingRequests.Count<GetUserItemVoteResult_t>();
				// remove failed/skipped requests
				base.LateUpdate();
				// if all votes are now loaded or failed, start query for all available items
				int pendingVoteReqCountActive = m_pendingRequests.Count<GetUserItemVoteResult_t>();
				if (pendingVoteReqCountAll > 0 &&  pendingVoteReqCountActive == 0)
				{
					QueryAllItems();
				}

				// log pending things
				if (IsDebugLogEnabled && Time.frameCount % 300 == 0)
				{
					// log stuck downloads
					if (m_downloadingItems.Count > 0)
					{
						Debug.Log("Pending downloads left: " + m_downloadingItems.Count);
					}
				}
			}
		}
		
		private void OnDestroy()
		{
			if (m_renderedTexture != null)
			{
				Destroy(m_renderedTexture);
				m_renderedTexture = null;
			}
		}

#endregion


#region SteamResultCalls

		private void OnAvailableItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnAvailableItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref OnItemListLoaded))
			{
				lock (m_lock)
				{
					// save the items page count
					m_reqItemList.PagesItems = GetPageCount(p_callback);

					for (uint i = 0; i < p_callback.m_unNumResultsReturned; i++)
					{
						SteamUGCDetails_t itemDetails;
						if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, i, out itemDetails))
						{
							WorkshopItem item = ParseItem(p_callback.m_handle, i, itemDetails);
							if (m_sorting.SOURCE != EWorkshopSource.OWNED || item.IsOwned)
							{
								m_reqItemList.Items.Add(item);
							}
							m_items[item.SteamNative.m_nPublishedFileId] = item;
							// check if this item is included in the favorites
							item.IsFavorited = m_reqItemList.ItemsFavorited.Where(flvl => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault() != null;
							// check if this item is included in the votes
							WorkshopItem votedItem = m_reqItemList.ItemsVoted.Where(flvl => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault();
							if (votedItem != null)
							{
								item.IsVotedUp = votedItem.IsVotedUp;
								item.IsVotedDown = votedItem.IsVotedDown;
								item.IsVoteSkipped = votedItem.IsVoteSkipped;
							}
						}
					}
					if (OnItemListLoaded != null)
					{
						InvokeEventHandlerSafely(OnItemListLoaded, new WorkshopItemListEventArgs() { ItemList = m_reqItemList });
						ClearSingleShotEventHandlers("OnItemListLoaded", ref OnItemListLoaded);
						if (IsDebugLogEnabled)
						{
							Debug.Log("OnAvailableItemsCallCompleted: loaded " + 
							          m_reqItemList.Items.Count + " items from page " + m_reqItemList.Page + ", " + 
							          m_reqItemList.ItemsFavorited.Count + " favorited by user, " + 
							          m_reqItemList.ItemsVoted.Count + " voted by user");
						}
					}
					// request finished
					m_reqItemList = null;
					m_pendingRequests.Clear<GetUserItemVoteResult_t>();
				}
			}
		}

		private void OnFavoriteItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnFavoriteItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref OnItemListLoaded))
			{
				lock (m_lock)
				{
					// save the favorite items page count
					if (m_reqItemList.PagesItemsFavorited == 0)
					{
						m_reqItemList.PagesItemsFavorited = GetPageCount(p_callback);
					}

					// add all favorite items to list
					for (uint i = 0; i < p_callback.m_unNumResultsReturned; i++)
					{
						SteamUGCDetails_t itemDetails;
						if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, i, out itemDetails))
						{
							WorkshopItem item = ParseItem(p_callback.m_handle, i, itemDetails);
							m_reqItemList.ItemsFavorited.Add(item);
							m_items[item.SteamNative.m_nPublishedFileId] = item;
							// this item was received in the favorites list, it must be favorited
							item.IsFavorited = true;
						}
					}

					if (m_reqPage >= m_reqItemList.PagesItemsFavorited)
					{
						if (SteamUser.BLoggedOn())
						{
							// favorites are now loaded, start query for voted items
							// getting the vote data needs a Steam call for each item
							// hence, we need first to find out which items were voted on
							QueryVotedItems(1); // start on first page of voted items, then iterate the rest
						
						}
						else
						{
							// cannot fetch user votes when offline (the items will be loaded from cache)
							QueryAllItems();
							HandleError("OnFavoriteItemsCallCompleted: user is offline, user votes will not be loaded! ", ErrorEventArgs.Create(EResult.k_EResultNotLoggedOn));
						}
					}
					else
					{
						// load next page with favorite items
						QueryFavoritedItems(m_reqPage + 1);
					}
				}
			}
		}

		private void OnVotedItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnVotedItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref OnItemListLoaded))
			{
				lock (m_lock)
				{
					// save the voted items page count
					if (m_reqItemList.PagesItemsVoted == 0)
					{
						m_reqItemList.PagesItemsVoted = GetPageCount(p_callback);
					}

					// start loading further voted items if there are multiple pages
					if (m_reqPage < m_reqItemList.PagesItemsVoted)
					{
						QueryVotedItems(m_reqPage + 1);
					}

					// add all voted items to list and fetch the vote state of each item
					for (uint i = 0; i < p_callback.m_unNumResultsReturned; i++)
					{
						SteamUGCDetails_t itemDetails;
						if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, i, out itemDetails))
						{
							WorkshopItem item = ParseItem(p_callback.m_handle, i, itemDetails);
							m_reqItemList.ItemsVoted.Add(item);
							m_items[item.SteamNative.m_nPublishedFileId] = item;
							// check if this item is included in the favorites
							item.IsFavorited = m_reqItemList.ItemsFavorited.Where(flvl => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault() != null;
							// get the user vote for each voted item
							Execute<GetUserItemVoteResult_t>(SteamUGC.GetUserItemVote(itemDetails.m_nPublishedFileId), OnUserVoteCallCompleted);
						}
					}

					// if there are no items voted by the user, then continue and query for all available items
					if (m_pendingRequests.Count<GetUserItemVoteResult_t>() == 0)
					{
						if (IsDebugLogEnabled) { Debug.Log("OnVotedItemsCallCompleted - no user votes found"); }
						QueryAllItems();
					}
					else
					{
						if (IsDebugLogEnabled) { Debug.Log("OnVotedItemsCallCompleted - started vote requests: " + m_pendingRequests.Count<GetUserItemVoteResult_t>()); }
					}
				}
			}
		}

		private void OnUserVoteCallCompleted(GetUserItemVoteResult_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResultNoEvent<GetUserItemVoteResult_t>("OnUserVoteCallCompleted", p_callback.m_eResult, p_bIOFailure))
			{
				lock (m_lock)
				{
					WorkshopItem votedItem = m_reqItemList.ItemsVoted.Where(flvl => flvl.SteamNative.m_nPublishedFileId == p_callback.m_nPublishedFileId).FirstOrDefault();
					if (votedItem != null)
					{
						votedItem.IsVotedUp = p_callback.m_bVotedUp;
						votedItem.IsVotedDown = p_callback.m_bVotedDown;
						votedItem.IsVoteSkipped = p_callback.m_bVoteSkipped;
					}
				}
			}

			lock (m_lock)
			{
				if (m_pendingRequests.Count<GetUserItemVoteResult_t>() == 0)
				{
					// favorites and all votes are now loaded, start query for all available items
					QueryAllItems();
				}
			}
		}

		private void OnSubscribeCallCompleted(RemoteStorageSubscribePublishedFileResult_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<RemoteStorageSubscribePublishedFileResult_t, WorkshopItemEventArgs>("OnSubscribeCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnSubscribed" + p_callback.m_nPublishedFileId, ref OnSubscribed))
			{
				lock (m_lock)
				{
					WorkshopItem item;
					if (m_items.TryGetValue(p_callback.m_nPublishedFileId, out item))
					{
						// update state of the cached item
						item.IsSubscribed = true;
						EItemState itemState = (EItemState)SteamUGC.GetItemState(p_callback.m_nPublishedFileId);
						item.SteamNative.m_itemState = itemState;
						item.IsInstalled = IsInstalled(itemState);
						item.IsDownloading = IsDownloading(itemState);
						item.IsUpdateNeeded = IsUpdateNeeded(itemState);
						// start download if needed
						if ((item.IsUpdateNeeded || !item.IsInstalled) && SteamUGC.DownloadItem(p_callback.m_nPublishedFileId, true))
						{
							if (IsDebugLogEnabled) { Debug.Log("OnSubscribeCallCompleted: started download for " + p_callback.m_nPublishedFileId); }
							if (!m_downloadingItems.Contains(p_callback.m_nPublishedFileId))
							{
								m_downloadingItems.Add(p_callback.m_nPublishedFileId);
							}
							// update state again
							itemState = (EItemState)SteamUGC.GetItemState(p_callback.m_nPublishedFileId);
							item.SteamNative.m_itemState = itemState;
							item.IsInstalled = IsInstalled(itemState);
							item.IsDownloading = IsDownloading(itemState);
							item.IsUpdateNeeded = IsUpdateNeeded(itemState);
						}
						else
						{
							if (IsDebugLogEnabled) { Debug.Log("OnSubscribeCallCompleted: subscribed to already installed item " + p_callback.m_nPublishedFileId); }
						}
						// inform listeners
						if (OnSubscribed != null)
						{
							InvokeEventHandlerSafely(OnSubscribed, new WorkshopItemEventArgs(item));
							ClearSingleShotEventHandlers("OnSubscribed" + p_callback.m_nPublishedFileId, ref OnSubscribed);
						}
					}
					else
					{
						ErrorEventArgs errorArgs = new ErrorEventArgs("Could not find item!");
						HandleError("OnSubscribeCallCompleted: failed! ", errorArgs);
						if (OnSubscribed != null) { CallSingleShotEventHandlers("OnSubscribed" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnSubscribed); } // call single shot event with error
					}
				}
			}
		}
		
		private void OnUnsubscribeCallCompleted(RemoteStorageUnsubscribePublishedFileResult_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<RemoteStorageUnsubscribePublishedFileResult_t, WorkshopItemEventArgs>("OnUnsubscribeCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnUnsubscribed" + p_callback.m_nPublishedFileId, ref OnUnsubscribed))
			{
				lock (m_lock)
				{
					WorkshopItem item;
					if (m_items.TryGetValue(p_callback.m_nPublishedFileId, out item))
					{
						// update state of the cached item
						item.IsSubscribed = false;
						EItemState itemState = (EItemState)SteamUGC.GetItemState(p_callback.m_nPublishedFileId);
						item.SteamNative.m_itemState = itemState;
						item.IsInstalled = IsInstalled(itemState);
						item.IsDownloading = IsDownloading(itemState);
						item.IsUpdateNeeded = IsUpdateNeeded(itemState);
						// inform listeners
						if (OnUnsubscribed != null)
						{
							InvokeEventHandlerSafely(OnUnsubscribed, new WorkshopItemEventArgs(item));
							ClearSingleShotEventHandlers("OnUnsubscribed" + p_callback.m_nPublishedFileId, ref OnUnsubscribed);
						}
					}
					else
					{
						ErrorEventArgs errorArgs = new ErrorEventArgs("Could not find subscribed item!");
						HandleError("OnUnsubscribeCallCompleted: failed! ", errorArgs);
						if (OnUnsubscribed != null) { CallSingleShotEventHandlers("OnUnsubscribed" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnUnsubscribed); } // call single shot event with error
					}
				}
			}
		}

		private void OnFavoriteChangeCallCompleted(UserFavoriteItemsListChanged_t p_callback, bool p_bIOFailure)
		{
			WorkshopItem item;
			m_items.TryGetValue(p_callback.m_nPublishedFileId, out item);

			if (CheckAndLogResultNoEvent<UserFavoriteItemsListChanged_t>("OnFavoriteChangeCallCompleted", p_callback.m_eResult, p_bIOFailure))
			{
				lock (m_lock)
				{
					if (item != null)
					{
						item.IsFavorited = p_callback.m_bWasAddRequest;
						if (item.IsFavorited)
						{
							if (OnAddedFavorite != null)
							{
								InvokeEventHandlerSafely(OnAddedFavorite, new WorkshopItemEventArgs(item));
								ClearSingleShotEventHandlers("OnAddedFavorite" + p_callback.m_nPublishedFileId, ref OnAddedFavorite);
							}
						}
						else
						{
							if (OnRemovedFavorite != null)
							{
								InvokeEventHandlerSafely(OnRemovedFavorite, new WorkshopItemEventArgs(item));
								ClearSingleShotEventHandlers("OnRemovedFavorite" + p_callback.m_nPublishedFileId, ref OnRemovedFavorite);
							}
						}
					}
					else
					{
						ErrorEventArgs errorArgs = new ErrorEventArgs("Could not find changed item!");
						HandleError("OnFavoriteChangeCallCompleted: failed! ", errorArgs);
						if (OnAddedFavorite != null) { CallSingleShotEventHandlers("OnAddedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnAddedFavorite); } // call single shot event with error
						if (OnRemovedFavorite != null) { CallSingleShotEventHandlers("OnRemovedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnRemovedFavorite); } // call single shot event with error
					}
				}
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.Create(p_callback.m_eResult);
				if (OnAddedFavorite != null) { CallSingleShotEventHandlers("OnAddedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnAddedFavorite); } // call single shot event with error
				if (OnRemovedFavorite != null) { CallSingleShotEventHandlers("OnRemovedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnRemovedFavorite); } // call single shot event with error
			}
		}

		private void OnVoteCallCompleted(SetUserItemVoteResult_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<SetUserItemVoteResult_t, WorkshopItemEventArgs>("OnVoteCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnVoted" + p_callback.m_nPublishedFileId, ref OnVoted))
			{
				lock (m_lock)
				{
					WorkshopItem item;
					if (m_items.TryGetValue(p_callback.m_nPublishedFileId, out item))
					{
						item.IsVotedUp = p_callback.m_bVoteUp;
						item.IsVotedDown = !p_callback.m_bVoteUp;
						item.IsVoteSkipped = false;
						if (OnVoted != null)
						{
							InvokeEventHandlerSafely(OnVoted, new WorkshopItemEventArgs(item));
							ClearSingleShotEventHandlers("OnVoted" + p_callback.m_nPublishedFileId, ref OnVoted);
						}
					}
					else
					{
						ErrorEventArgs errorArgs = new ErrorEventArgs("Could not find voted item!");
						HandleError("OnVoteCallCompleted: failed! ", errorArgs);
						if (OnVoted != null) { CallSingleShotEventHandlers("OnVoted" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorArgs), ref OnVoted); } // call single shot event with error
					}
				}
			}
		}

		private void OnCreateItemCompleted(CreateItemResult_t p_callback, bool p_bIOFailure)
		{
			if (p_callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				// redirect user to the legal agreement that he needs to accept before he can create a level
				Application.OpenURL("https://steamcommunity.com/workshop/workshoplegalagreement/");
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateWorkshopLegalAgreement();
				HandleError("OnCreateItemCompleted: failed! ", errorArgs);
				if (OnUploaded != null) { CallSingleShotEventHandlers("OnUploaded", new WorkshopItemUpdateEventArgs(errorArgs), ref OnUploaded); } // call single shot event with error
			}
			else if (CheckAndLogResult<SetUserItemVoteResult_t, WorkshopItemUpdateEventArgs>("OnCreateItemCompleted", p_callback.m_eResult, p_bIOFailure, "OnUploaded", ref OnUploaded))
			{
				m_uploadItemData.SteamNative.m_nPublishedFileId = p_callback.m_nPublishedFileId;
				Upload(m_uploadItemData, null);
			}
		}

		private void OnItemUpdateCompleted(SubmitItemUpdateResult_t p_callback, bool p_bIOFailure)
		{
			if (p_callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				// redirect user to the legal agreement that he needs to accept before he can create a level
				Application.OpenURL("https://steamcommunity.com/workshop/workshoplegalagreement/");
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateWorkshopLegalAgreement();
				HandleError("OnItemUpdateCompleted: failed! ", errorArgs);
				if (OnUploaded != null) { CallSingleShotEventHandlers("OnUploaded", new WorkshopItemUpdateEventArgs(errorArgs), ref OnUploaded); } // call single shot event with error
			}
			else if (CheckAndLogResult<SetUserItemVoteResult_t, WorkshopItemUpdateEventArgs>("OnItemUpdateCompleted (" + m_uploadItemData.Name + ")", p_callback.m_eResult, p_bIOFailure, "OnUploaded", ref OnUploaded))
			{
				if (OnUploaded != null)
				{
					InvokeEventHandlerSafely(OnUploaded, new WorkshopItemUpdateEventArgs() { Item = m_uploadItemData });
					ClearSingleShotEventHandlers("OnUploaded", ref OnUploaded);
				}
			}
		}

#endregion


#region InternalLogic

		private WorkshopItem ParseItem(UGCQueryHandle_t p_handle, uint p_indexInHandle, SteamUGCDetails_t p_itemDetails)
		{
			string ownerName = SteamFriends.GetFriendPersonaName(new CSteamID(p_itemDetails.m_ulSteamIDOwner));
			ulong favorites;
			if (!SteamUGC.GetQueryUGCStatistic(p_handle, p_indexInHandle, EItemStatistic.k_EItemStatistic_NumFavorites, out favorites))
			{
				favorites = 0;
			}
			ulong subscriptions;
			if (!SteamUGC.GetQueryUGCStatistic(p_handle, p_indexInHandle, EItemStatistic.k_EItemStatistic_NumSubscriptions, out subscriptions))
			{
				subscriptions = 0;
			}
			string previewURL;
			if (!SteamUGC.GetQueryUGCPreviewURL(p_handle, p_indexInHandle, out previewURL, 1024))
			{
				previewURL = "";
			}
			bool isSubscribed = (SteamUGC.GetItemState(p_itemDetails.m_nPublishedFileId) & (uint)EItemState.k_EItemStateSubscribed) != 0;
			EItemState itemState = (EItemState)SteamUGC.GetItemState(p_itemDetails.m_nPublishedFileId);

			string localFolder;
			ulong sizeOnDisk;
			uint timestamp;
			System.DateTime timestampParsed = System.DateTime.MinValue;
			if (SteamUGC.GetItemInstallInfo(p_itemDetails.m_nPublishedFileId, out sizeOnDisk, out localFolder, 260, out timestamp))
			{
				timestampParsed = new System.DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
				timestampParsed = timestampParsed.AddSeconds(timestamp).ToLocalTime();
			}

			return new WorkshopItem()
			{
				SteamNative = new WorkshopItem.SteamNativeData(p_itemDetails.m_nPublishedFileId)
				{
					m_details = p_itemDetails,
					m_itemState = itemState
				},
				Name = p_itemDetails.m_rgchTitle,
				Description = p_itemDetails.m_rgchDescription,
				OwnerName = ownerName,
				IsOwned = p_itemDetails.m_ulSteamIDOwner == SteamUser.GetSteamID().m_SteamID,
				PreviewImageURL = previewURL,
				VotesUp = p_itemDetails.m_unVotesUp,
				VotesDown = p_itemDetails.m_unVotesDown,
				Subscriptions = subscriptions,
				Favorites = favorites,
				IsSubscribed = isSubscribed,
				IsInstalled = IsInstalled(itemState),
				IsDownloading = IsDownloading(itemState),
				IsUpdateNeeded = IsUpdateNeeded(itemState),
				InstalledLocalFolder = localFolder,
				InstalledSizeOnDisk = sizeOnDisk,
				InstalledTimestamp = timestampParsed
			};
		}

		private bool IsInstalled(EItemState p_itemState)
		{
			return
				(p_itemState & EItemState.k_EItemStateInstalled) == EItemState.k_EItemStateInstalled && // must be installed
				!((p_itemState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading) && // must not be downloading
				!((p_itemState & EItemState.k_EItemStateDownloadPending) == EItemState.k_EItemStateDownloadPending); // must not wait for download
		}

		private bool IsDownloading(EItemState p_itemState)
		{
			return
				(p_itemState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading ||
				(p_itemState & EItemState.k_EItemStateDownloadPending) == EItemState.k_EItemStateDownloadPending;
		}

		private bool IsUpdateNeeded(EItemState p_itemState)
		{
			return (p_itemState & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate;
		}

		private uint GetPageCount(SteamUGCQueryCompleted_t p_callback)
		{
			if (p_callback.m_unTotalMatchingResults != 0)
			{
				return (uint)Mathf.Ceil((float)p_callback.m_unTotalMatchingResults / 50f);
			}
			else
			{
				return 1;
			}
		}

		private void QueryFavoritedItems(uint p_page)
		{
			if (IsDebugLogEnabled) { Debug.Log("QueryFavoritedItems page " + p_page); }

			m_reqPage = p_page; // all pages will be iterated
			UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUserUGCRequest(
				SteamUser.GetSteamID().GetAccountID(),
				EUserUGCList.k_EUserUGCList_Favorited,
				EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
				EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc,
				AppId_t.Invalid,
				SteamUtils.GetAppID(),
				m_reqPage);
			if (!m_isSteamCacheEnabled) { SteamUGC.SetAllowCachedResponse(queryHandle, 0); } // don't allow cache usage
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(queryHandle), OnFavoriteItemsCallCompleted);
		}

		private void QueryVotedItems(uint p_page)
		{
			if (IsDebugLogEnabled) { Debug.Log("QueryVotedItems page " + p_page); }

			m_reqPage = p_page; // all pages will be iterated
			UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUserUGCRequest(
				SteamUser.GetSteamID().GetAccountID(),
				EUserUGCList.k_EUserUGCList_VotedOn,
				EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
				EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc,
				AppId_t.Invalid,
				SteamUtils.GetAppID(),
				m_reqPage);
			if (!m_isSteamCacheEnabled) { SteamUGC.SetAllowCachedResponse(queryHandle, 0); } // don't allow cache usage
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(queryHandle), OnVotedItemsCallCompleted);
		}

		private void QueryAllItems()
		{
			lock (m_lock)
			{
				m_reqPage = m_reqItemList.Page;
			}
			
			if (IsDebugLogEnabled) { Debug.Log("QueryAllItems from " + m_sorting.SOURCE + " page " + m_reqPage); }

			UGCQueryHandle_t queryHandle;
			if (m_sorting.SOURCE != EWorkshopSource.SUBSCRIBED)
			{
				// get all public items from SteamUGC
				queryHandle = SteamUGC.CreateQueryAllUGCRequest(
					m_sorting.MODE,
					EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
					AppId_t.Invalid,
					SteamUtils.GetAppID(),
					m_reqPage);
				if (m_searchText != null && !string.IsNullOrEmpty(m_searchText.Trim()))
				{
					SteamUGC.SetSearchText(queryHandle, m_searchText);
				}
				if (m_searchTags != null && m_searchTags.Count > 0)
				{
					SteamUGC.SetMatchAnyTag(queryHandle, m_searchMatchAnyTag);
					for (int i = 0; i < m_searchTags.Count; i++)
					{
						SteamUGC.AddRequiredTag(queryHandle, m_searchTags[i]);	
					}
				}
			}
			else
			{
				// get all subscribed items
				uint subscriptionsCount = SteamUGC.GetNumSubscribedItems();
				if (subscriptionsCount > 0)
				{
					// create a query to get subscribed items information
					PublishedFileId_t[] subscriptions = new PublishedFileId_t[subscriptionsCount];
					subscriptionsCount = System.Math.Min(subscriptionsCount, SteamUGC.GetSubscribedItems(subscriptions, subscriptionsCount));
					queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(subscriptions, subscriptionsCount);
				}
				else
				{
					// user has not subscribed to anything -> results are empty
					lock (m_lock)
					{
						m_reqItemList.PagesItems = 0;
						if (OnItemListLoaded != null)
						{
							InvokeEventHandlerSafely(OnItemListLoaded, new WorkshopItemListEventArgs() { ItemList = m_reqItemList });
							ClearSingleShotEventHandlers("OnItemListLoaded", ref OnItemListLoaded);
							if (IsDebugLogEnabled) { Debug.Log("QueryAllItems: no subscribed items"); }
						}
						// request finished
						m_reqItemList = null;
						m_pendingRequests.Clear<GetUserItemVoteResult_t>();
					}
					return;
				}
			}
			if (!m_isSteamCacheEnabled) { SteamUGC.SetAllowCachedResponse(queryHandle, 0); } // don't allow cache usage
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(queryHandle), OnAvailableItemsCallCompleted);
		}

		private IEnumerator RenderIconRoutine(Camera p_camera, int p_width, int p_height, string p_saveToFilePath, bool p_keepTextureReference, System.Action<Texture2D> p_onRenderIconCompleted)
		{
			// wait for end of frame -> don't interrupt the current process
			yield return new WaitForEndOfFrame();
			
			// render icon using the target size
			Rect originalPixelRect = p_camera.pixelRect;
			if (p_width > originalPixelRect.width || p_height > originalPixelRect.height)
			{
				Debug.LogError("SteamWorkshopUIUpload: RenderIconRoutine: cannot render icon in given resolution ("+p_width+","+p_height+"), because it exceeds the current camera's resolution ("+originalPixelRect.width+","+originalPixelRect.height+")!");
				p_width = (int)Mathf.Min(p_width, originalPixelRect.width);
				p_height = (int)Mathf.Min(p_height, originalPixelRect.height);
			}
			Rect pixelRect = new Rect(0, 0, p_width, p_height);
			p_camera.pixelRect = pixelRect;
			p_camera.Render();
			p_camera.pixelRect = originalPixelRect;
			
			// read icon texture from screen
			if (m_renderedTexture != null)
			{
				Destroy(m_renderedTexture); // destroy old icon texture
			}
			m_renderedTexture = new Texture2D(p_width, p_height, TextureFormat.RGB24, false, true);
			m_renderedTexture.ReadPixels(pixelRect, 0, 0, false);
			m_renderedTexture.Apply(false);
			
			// save icon to file
			if (!string.IsNullOrEmpty(p_saveToFilePath))
			{
				// create directory of the item if it does not exist yet
				string directory = System.IO.Path.GetDirectoryName(p_saveToFilePath);
				if (!System.IO.Directory.Exists(directory))
				{
					System.IO.Directory.CreateDirectory(directory);
				}
				// save item icon to .png file
				System.IO.File.WriteAllBytes(p_saveToFilePath, m_renderedTexture.EncodeToPNG());
				if (IsDebugLogEnabled) { Debug.Log("RenderIconRoutine saved icon to '"+p_saveToFilePath+"'"); }
			}
			
			if (p_onRenderIconCompleted != null)
			{
				p_onRenderIconCompleted(m_renderedTexture);
			}

			if (!p_keepTextureReference)
			{
				// forget about this texture -> this texture MUST BE DESTROYED somewhere else otherwise it will stay in memory -> memory leak
				m_renderedTexture = null;
			}
		}
		
#endregion
	}
}
