using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// This is the Workshop browser popup. It wraps the SteamWorkshopUIBrowse class, which can be accessed through SteamWorkshopPopupBrowse.BrowseUI.
	/// This class is attached to the popup_steam_ugc_browse_root prefab.
	/// Trigger this popup like this:<br />
	/// <c>uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_browse");</c>
	/// </summary>
	public class SteamWorkshopPopupBrowse : uMyGUI_Popup
	{
		[SerializeField]
		protected SteamWorkshopUIBrowse m_browseUI;
		/// <summary>
		/// Use this property to access the SteamWorkshopUIBrowse class.
		/// </summary>
		public SteamWorkshopUIBrowse BrowseUI { get{ return m_browseUI; } }

		public SteamWorkshopPopupBrowse()
		{
			DestroyOnHide = true;
		}
	}
}
