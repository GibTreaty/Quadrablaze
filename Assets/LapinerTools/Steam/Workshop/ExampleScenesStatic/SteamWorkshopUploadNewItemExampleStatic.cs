using UnityEngine;

using System.IO;
using System.Collections;

using LapinerTools.Steam;
using LapinerTools.Steam.UI;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

public class SteamWorkshopUploadNewItemExampleStatic : MonoBehaviour
{
	private void Start()
	{
		// enable debug log
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;

		// check if this scene contains a SteamWorkshopUIUpload instance
		if (SteamWorkshopUIUpload.Instance == null)
		{
			string errorMessage = "SteamWorkshopUploadNewItemExampleStatic: you have no SteamWorkshopUIUpload in this scene! Please drag an drop the 'SteamWorkshopItemUpload' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(errorMessage);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Error", errorMessage);
			return;
		}

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

		// set upload data in Steam Workshop item upload UI
		SteamWorkshopUIUpload.Instance.SetItemData(createNewItemUsingGivenFolder);
	}
}
