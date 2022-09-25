using UnityEngine;
using System.Collections;

using Steamworks;

namespace LapinerTools.Steam.Data
{
	[System.Serializable]
	/// <summary>
	/// Defines a certain item list sort mode. See SteamWorkshopMain.Sorting for more details.
	/// </summary>
	public class WorkshopSortMode
	{
		[SerializeField]
		/// <summary>
		/// The Steam native EUGCQuery sort mode. See SteamWorkshopMain.Sorting for more details.
		/// </summary>
		public EUGCQuery MODE = EUGCQuery.k_EUGCQuery_RankedByVote;

		[SerializeField]
		/// <summary>
		/// Limits the returned items, e.g. EWorkshopSource.OWNED - only items created by this Steam user.
		/// </summary>
		public EWorkshopSource SOURCE = EWorkshopSource.PUBLIC;

		public WorkshopSortMode()
		{
		}

		public WorkshopSortMode(EUGCQuery p_mode)
		{
			MODE = p_mode;
		}

		public WorkshopSortMode(EWorkshopSource p_source)
		{
			SOURCE = p_source;
		}

		public WorkshopSortMode(EUGCQuery p_mode, EWorkshopSource p_source)
		{
			MODE = p_mode;
			SOURCE = p_source;
		}

		public override bool Equals(object p_other)
		{
			return p_other != null && p_other is WorkshopSortMode && p_other.GetHashCode() == this.GetHashCode();
		}

		public override int GetHashCode ()
		{
			return ((int)MODE + 100*(int)SOURCE).GetHashCode();
		}
	}
}
