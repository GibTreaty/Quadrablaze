using UnityEngine;
using System.Collections;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This event arguments are used for all WorkshopItemList related events.
	/// </summary>
	public class WorkshopItemListEventArgs : EventArgsBase
	{
		/// <summary>
		/// WorkshopItemList affected by this event.
		/// </summary>
		public WorkshopItemList ItemList { get; set; }
		public WorkshopItemListEventArgs() : base()
		{
		}
		public WorkshopItemListEventArgs(EventArgsBase p_errorEventArgs) : base(p_errorEventArgs)
		{
		}
	}
}
