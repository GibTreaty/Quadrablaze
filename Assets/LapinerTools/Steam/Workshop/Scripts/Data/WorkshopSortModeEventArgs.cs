using UnityEngine;
using System.Collections;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This event arguments are used for all WorkshopSortMode related events.
	/// </summary>
	public class WorkshopSortModeEventArgs : EventArgsBase
	{
		/// <summary>
		/// WorkshopSortMode affected by this event.
		/// </summary>
		public WorkshopSortMode SortMode { get; set; }
		public WorkshopSortModeEventArgs() : base()
		{
		}
		public WorkshopSortModeEventArgs(WorkshopSortMode p_sortMode) : base()
		{
			SortMode = p_sortMode;
		}
		public WorkshopSortModeEventArgs(EventArgsBase p_errorEventArgs) : base(p_errorEventArgs)
		{
		}
	}
}
