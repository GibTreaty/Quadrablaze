using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Steamworks;

using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;

public class SteamWorkshopBrowseExamplePopup : MonoBehaviour
{
	private void Start()
	{
		// enable debug log
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;

		// show the Steam Workshop browse popup
		((SteamWorkshopPopupBrowse)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_browse")).BrowseUI
			// implement your item/level loading here
			.OnPlayButtonClick += (WorkshopItemEventArgs p_itemArgs) =>
			{
				// collect names of all local files
				string filesInfoStr = "\n";
				try
				{
					string[] localFiles = Directory.GetFiles(p_itemArgs.Item.InstalledLocalFolder);
					for (int i = 0; i < localFiles.Length; i++)
					{
						filesInfoStr += localFiles[i] + "\n";
					}
				}
				catch
				{
					filesInfoStr += "not found!";
				}

				// put all together in one string and show a popup
				string itemDataStr = 
					// item name
					"Name: " + p_itemArgs.Item.Name + "\n" +
					// steam PublishedFileId
					"Published File Id: " + p_itemArgs.Item.SteamNative.m_nPublishedFileId + "\n" +
					// folder containing level data on user's hard driver
					"Local Folder: " + p_itemArgs.Item.InstalledLocalFolder + "\n" + filesInfoStr;

				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("Item Played", "Load your Steam Workshop item here (e.g. could be a new level for your game)\n" + itemDataStr)
					.ShowButton(uMyGUI_PopupManager.BTN_OK);
			};
	}
}
