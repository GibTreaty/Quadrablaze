using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Steamworks;

using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;

public class SteamWorkshopTagsExample : MonoBehaviour
{
	[SerializeField]
	public string[] TAGS = new string[]{ "TAG1", "TAG2", "TAG3", "TAG4" };

	private List<string> m_tagsToUse = new List<string>();

	private void Start()
	{
		// enable debug log
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;

		// set tag match mode
		SteamWorkshopMain.Instance.SearchMatchAnyTag = true;
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0,Screen.height-28, Screen.width, 28));
		GUILayout.BeginHorizontal();
		
		// BUTTONS
		if (GUILayout.Button("Browse With Tags", GUILayout.Height(28)))
		{
			// show the Steam Workshop browse popup
			if (SteamWorkshopUIBrowse.Instance != null)
			{
				// popup is shown already => let it reload
				SteamWorkshopUIBrowse.Instance.LoadItems(1);
			}
			else
			{
				// popup is not shown => bring it to the front (it will automatically load the first page)
				((SteamWorkshopPopupBrowse)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_browse")).BrowseUI
					// implement your item/level loading here
					.OnPlayButtonClick += (WorkshopItemEventArgs p_itemArgs) =>
					{
						((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
							.SetText("Item Played", "Item Name: " + p_itemArgs.Item.Name + "\nFor further item details check SteamWorkshopBrowseExamplePopup or SteamWorkshopBrowseExampleStatic classes.")
							.ShowButton(uMyGUI_PopupManager.BTN_OK);
					};
			}


		}

		if (GUILayout.Button("Upload With Tags", GUILayout.Height(40)))
		{
			// everything inside this folder will be uploaded with your item
			string dummyItemContentFolder = Path.Combine(Application.persistentDataPath, "DummyItemContentFolder" + System.DateTime.Now.Ticks); // use DateTime.Now.Ticks to create a unique folder for each upload
			if (!Directory.Exists(dummyItemContentFolder)) { Directory.CreateDirectory(dummyItemContentFolder); }
			
			// create dummy content to upload
			string dummyItemContentStr =
				"Save your item/level/mod data here.\n" +
				"It does not need to be a text file. Any file type is supported (binary, images, etc...).\n" + 
				"You can save multiple files, Steam items are folders (not single files).\n";
			File.WriteAllText(Path.Combine(dummyItemContentFolder, "ItemData.txt"), dummyItemContentStr);
			
			// tell which folder you want to upload
			WorkshopItemUpdate createNewItemUsingGivenFolder = new WorkshopItemUpdate();
			createNewItemUsingGivenFolder.ContentPath = dummyItemContentFolder;
			createNewItemUsingGivenFolder.Tags = m_tagsToUse; // apply tags to the item
			
			// show the Steam Workshop item upload popup with a custom popup after successful upload
			SteamWorkshopPopupUpload uploadPopup = (SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload");
			uploadPopup.UploadUI.SetItemData(createNewItemUsingGivenFolder);
			uploadPopup.UploadUI.OnFinishedUpload += ((WorkshopItemUpdateEventArgs p_args) =>
			{
				if (!p_args.IsError && p_args.Item != null)
				{
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
						.SetText("Item Uploaded", "Item '" + p_args.Item.Name +"' was successfully uploaded!\nTags: " + p_args.Item.Tags.Aggregate((tag1, tag2) => tag1 + ", " + tag2) +
						         "\nIt can take a long time for this new level to arrive in the Steam Workshop listing, sometimes longer than an hour! Be patient...")
						.ShowButton(uMyGUI_PopupManager.BTN_OK);
				}
			});
		}

		// TAG SELECTION
		for (int i = 0; i < TAGS.Length; i++)
		{
			bool isTagSetInScript = m_tagsToUse.Contains(TAGS[i]);
			bool isTagSetInToggle = GUILayout.Toggle(isTagSetInScript, TAGS[i]);
			// add tag
			if (isTagSetInToggle && !isTagSetInScript)
			{
				m_tagsToUse.Add(TAGS[i]);
			}
			// remove tag
			if (!isTagSetInToggle && isTagSetInScript)
			{
				m_tagsToUse.Remove(TAGS[i]);
			}
			// apply tags for the item browser search
			SteamWorkshopMain.Instance.SearchTags = m_tagsToUse;
		}

		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
