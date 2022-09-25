using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// This is the Workshop item upload popup. It wraps the SteamWorkshopUIUpload class, which can be accessed through SteamWorkshopPopupUpload.UploadUI.
	/// This class is attached to the popup_steam_ugc_upload_root prefab.
	/// Trigger this popup like this:<br />
	/// <c>uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload");</c>
	/// </summary>
	public class SteamWorkshopPopupUpload : uMyGUI_Popup
	{
		[SerializeField]
		protected SteamWorkshopUIUpload m_uploadUI;
		/// <summary>
		/// Use this property to access the SteamWorkshopUIUpload class.
		/// </summary>
		public SteamWorkshopUIUpload UploadUI { get{ return m_uploadUI; } }

		public SteamWorkshopPopupUpload()
		{
			DestroyOnHide = true;
		}

		/// <summary>
		/// Will register for the SteamWorkshopUIUpload.OnFinishedUpload event to hide/destroy itself when the work is done.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			if (m_uploadUI != null)
			{
				m_uploadUI.OnFinishedUpload += (WorkshopItemUpdateEventArgs p_args) => Hide();
			}
		}
	}
}
