using UnityEngine;
using System.Collections;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This event arguments are used for all WorkshopItemUpdate related events.
	/// </summary>
	public class WorkshopItemUpdateEventArgs : EventArgsBase
	{
		/// <summary>
		/// WorkshopItemUpdate affected by this event.
		/// </summary>
		public WorkshopItemUpdate Item { get; set; }
		public WorkshopItemUpdateEventArgs() : base()
		{
		}
		public WorkshopItemUpdateEventArgs(EventArgsBase p_errorEventArgs) : base(p_errorEventArgs)
		{
		}
	}
}
