using UnityEngine;

using System.IO;
using System.Collections;

using LapinerTools.Steam;
using LapinerTools.Steam.UI;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

public class SteamWorkshopUpdateOwnedItemExamplePopup : MonoBehaviour
{
	private void Start()
	{
		// enable debug log
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;

		// get all items created by the user, this will allow us to select an item for the update
		uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
		SteamWorkshopMain.Instance.Sorting = new WorkshopSortMode(EWorkshopSource.OWNED);
		SteamWorkshopMain.Instance.GetItemList(1, OnOwnedItemListLoaded);
	}

	private void OnOwnedItemListLoaded(WorkshopItemListEventArgs p_itemListArgs)
	{
		uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);

		if (p_itemListArgs.IsError) { return; }

		if (p_itemListArgs.ItemList.Items.Count > 0)
		{
			// generate a list of item names
			string[] dropdownEntries = new string[p_itemListArgs.ItemList.Items.Count];
			for (int i = 0; i < dropdownEntries.Length; i++)
			{
				dropdownEntries[i] = p_itemListArgs.ItemList.Items[i].Name;
			}

			// select the item, which you want to update
			((uMyGUI_PopupDropdown)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_DROPDOWN))
				.SetEntries(dropdownEntries)
				.SetOnSelected((int p_selectedIndex) => OnOwnedItemSelectedForUpdate(p_itemListArgs.ItemList.Items[p_selectedIndex]))
				.SetText("Select Item", "Select the item, which you want to update");
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("No Items Found", "Cannot find any items to update, it seems that you haven't uploaded any items yet.")
				.ShowButton(uMyGUI_PopupManager.BTN_OK);
		}
	}

	private void OnOwnedItemSelectedForUpdate(WorkshopItem p_item)
	{
		uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_DROPDOWN);

		// only installed items can be updated
		if (!p_item.IsInstalled)
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Not Installed", "This item is not installed. Please subscribe this item first!")
				.ShowButton(uMyGUI_PopupManager.BTN_OK, Start); // retry on OK button
			return;
		}

		// generate a WorkshopItemUpdate instance
		WorkshopItemUpdate updateExistingItem = new WorkshopItemUpdate(p_item);

		string itemContentFile = Path.Combine(updateExistingItem.ContentPath, "ItemData.txt");
		if (File.Exists(itemContentFile))
		{
			// update the content of your item
			File.AppendAllText(itemContentFile, "\nUpdate - " + System.DateTime.Now);
			
			// show the Steam Workshop item upload popup
			((SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload")).UploadUI.SetItemData(updateExistingItem);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Not Installed", "This item is subscribed, but not installed. Please sync local files in Steam!")
				.ShowButton(uMyGUI_PopupManager.BTN_OK, Start); // retry on OK button
		}
	}
}
