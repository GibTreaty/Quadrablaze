using UnityEngine;

using System.IO;
using System.Collections;
using System.Collections.Generic;

using LapinerTools.Steam;
using LapinerTools.Steam.UI;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

public class SteamWorkshopUpdateItemFromFolderExampleStatic : MonoBehaviour
{
	private void Start()
	{
		// enable debug log
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;

		// check if this scene contains a SteamWorkshopUIUpload instance
		if (SteamWorkshopUIUpload.Instance == null)
		{
			string errorMessage = "SteamWorkshopUpdateItemFromFolderExampleStatic: you have no SteamWorkshopUIUpload in this scene! Please drag an drop the 'SteamWorkshopItemUpload' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(errorMessage);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Error", errorMessage);
			return;
		}

		// get items which were uploaded before from disk, this will allow us to select an item for the update
		List<WorkshopItemUpdate> itemsAvailableForUpdate = new List<WorkshopItemUpdate>();
		string uploadRootFolderPath = Application.persistentDataPath;
		foreach (string itemFolderPath in System.IO.Directory.GetDirectories(uploadRootFolderPath))
		{
			WorkshopItemUpdate itemUpdate = SteamWorkshopMain.Instance.GetItemUpdateFromFolder(itemFolderPath);
			if (itemUpdate != null)
			{
				itemsAvailableForUpdate.Add(itemUpdate);
			}
		}

		if (itemsAvailableForUpdate.Count > 0)
		{
			// generate a list of item names
			string[] dropdownEntries = new string[itemsAvailableForUpdate.Count];
			for (int i = 0; i < dropdownEntries.Length; i++)
			{
				dropdownEntries[i] = itemsAvailableForUpdate[i].Name;
			}

			// select the item, which you want to update
			((uMyGUI_PopupDropdown)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_DROPDOWN))
				.SetEntries(dropdownEntries)
				.SetOnSelected((int p_selectedIndex) => OnExistingItemSelectedForUpdate(itemsAvailableForUpdate[p_selectedIndex]))
				.SetText("Select Item", "Select the item, which you want to update");
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("No Items Found", "Cannot find any items to update, it seems that you haven't uploaded any items yet.")
				.ShowButton(uMyGUI_PopupManager.BTN_OK);
		}
	}

	private void OnExistingItemSelectedForUpdate(WorkshopItemUpdate p_updateExistingItem)
	{
		uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_DROPDOWN);

		string itemContentFile = Path.Combine(p_updateExistingItem.ContentPath, "ItemData.txt");
		if (File.Exists(itemContentFile))
		{
			// update the content of your item
			File.AppendAllText(itemContentFile, "\nUpdate - " + System.DateTime.Now);
			
			// set upload data in Steam Workshop item upload UI
			SteamWorkshopUIUpload.Instance.SetItemData(p_updateExistingItem);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Content File Is Missing", "Have you changed this item's data?!")
				.ShowButton(uMyGUI_PopupManager.BTN_OK);
		}
	}
}
