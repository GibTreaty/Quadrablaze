using UnityEngine;
using System.Collections;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This event arguments are used for all WorkshopItem related events.
	/// </summary>
	public class WorkshopItemEventArgs : EventArgsBase
	{
		/// <summary>
		/// WorkshopItem affected by this event.
		/// </summary>
		public WorkshopItem Item { get; set; }
		public WorkshopItemEventArgs() : base()
		{
		}
		public WorkshopItemEventArgs(WorkshopItem p_item) : base()
		{
			Item = p_item;
		}
		public WorkshopItemEventArgs(EventArgsBase p_errorEventArgs) : base(p_errorEventArgs)
		{
		}
	}
}
