using UnityEngine;
using System.Collections;


namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// Supported data sources used to display sorted items.<br />
	/// PUBLIC: community items available in Steam Workshop<br />
	/// SUBSCRIBED: subscribed items<br />
	/// OWNED: items created by this Steam user
	/// </summary>
	public enum EWorkshopSource
	{
		PUBLIC,
		SUBSCRIBED,
		OWNED
	}
}