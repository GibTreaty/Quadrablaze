using UnityEngine;
using System.Collections.Generic;

using Steamworks;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// The WorkshopItemUpdate class stores all data required to upload a new Steam Workshop item or to update an existing item.
	/// You can get the native Steam data (e.g. PublishedFileId) from the WorkshopItem.SteamNative property.
	/// Most properties are self-explanatory and not documented in more detail.
	/// </summary>
	public class WorkshopItemUpdate
	{
		/// <summary>
		/// The WorkshopItem.SteamNativeData class contains Steam native data such as PublishedFileId, UGCUpdateHandle or ItemUpdateStatus.
		/// You can use this data to make own calls to the Steamworks.NET API.
		/// </summary>
		public class SteamNativeData
		{
			public PublishedFileId_t m_nPublishedFileId { get; set; }
			public UGCUpdateHandle_t m_uploadHandle { get; set; }
			public EItemUpdateStatus m_lastValidUpdateStatus { get; set; }
			
			public SteamNativeData()
			{
				m_nPublishedFileId = PublishedFileId_t.Invalid;
				m_uploadHandle = UGCUpdateHandle_t.Invalid;
				m_lastValidUpdateStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
			}
			public SteamNativeData(PublishedFileId_t p_nPublishedFileId)
			{
				m_nPublishedFileId = p_nPublishedFileId;
				m_uploadHandle = UGCUpdateHandle_t.Invalid;
				m_lastValidUpdateStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
			}
		}

		public string Name { get; set; }
		public string Description { get; set; }

		/// <summary>
		/// The image file path, which will be used as item icon in the Steam Workshop web page and the SteamWorkshopUIBrowse. Can be null or empty.
		/// </summary>
		public string IconPath { get; set; }
		/// <summary>
		/// Everything inside this folder will be uploaded to Steam Workshop. Can be null or empty if only the name, description and icon should be updated.
		/// </summary>
		public string ContentPath { get; set; }
		/// <summary>
		/// Optional change note, which will be visible in the Steam Workshop web page.
		/// </summary>
		public string ChangeNote { get; set; }
		/// <summary>
		/// Optional item tags, which will be visible in the Steam Workshop web page.
		/// The Steam Workshop web page will allow to browse items by tag.
		/// This item will be found by entering any of the tags in the search UI.
		/// </summary>
		public List<string> Tags { get; set; }

		/// <summary>
		/// Contains Steam native data such as PublishedFileId, UGCUpdateHandle or ItemUpdateStatus.
		/// You can use this data to make own calls to the Steamworks.NET API.
		/// </summary>
		public SteamNativeData SteamNative { get; set; }

		public WorkshopItemUpdate()
		{
			// this is a new item
			SteamNative = new SteamNativeData();
			ChangeNote = "Initial version";
			Tags = new List<string>();
		}

		public WorkshopItemUpdate(WorkshopItem p_existingItem)
		{
			if (p_existingItem.SteamNative != null)
			{
				// this is an existing item -> copy data
				Name = p_existingItem.Name;
				Description = p_existingItem.Description;
				ContentPath = p_existingItem.InstalledLocalFolder;
				SteamNative = new SteamNativeData(p_existingItem.SteamNative.m_nPublishedFileId);
				ChangeNote = "";
				Tags = new List<string>();

				// try to find icon
				if (!string.IsNullOrEmpty(ContentPath))
				{
					string possibleIconPath = System.IO.Path.Combine(ContentPath, Name + ".png");
					if (System.IO.File.Exists(possibleIconPath))
					{
						IconPath = possibleIconPath;
					}
				}
			}
			else
			{
				// this is a new item
				SteamNative = new SteamNativeData();
				ChangeNote = "Initial version";
				Tags = new List<string>();
			}
		}

		public WorkshopItemUpdate(PublishedFileId_t p_existingPublishedFileId)
		{
			// this is an existing item
			SteamNative = new SteamNativeData(p_existingPublishedFileId);
			ChangeNote = "";
			Tags = new List<string>();
		}
	}
}
