using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// The WorkshopItemList contains the Steam Workshop items fetched from Steamworks.
	/// Also, contains all items favorited or voted by this Steam user.
	/// </summary>
	public class WorkshopItemList
	{
		/// <summary>
		/// The page number of the WorkshopItemList.Items property, starts with 1.
		/// </summary>
		public uint Page { get; set; }
		/// <summary>
		/// The overall page count in Steam Workshop.
		/// </summary>
		public uint PagesItems { get; set; }
		/// <summary>
		/// The items of the WorkshopItemList.Page.
		/// </summary>
		public List<WorkshopItem> Items { get; set; }
		/// <summary>
		/// The page count of user's favorites in Steam Workshop.
		/// </summary>
		public uint PagesItemsFavorited { get; set; }
		/// <summary>
		/// This Steam user's favorites list.
		/// </summary>
		public List<WorkshopItem> ItemsFavorited { get; set; }
		/// <summary>
		/// The page count of user's votes in Steam Workshop.
		/// </summary>
		public uint PagesItemsVoted { get; set; }
		/// <summary>
		/// All items voted by this Steam user.
		/// </summary>
		public List<WorkshopItem> ItemsVoted { get; set; }

		public WorkshopItemList()
		{
			Page = 1;
			PagesItems = 1;
			Items = new List<WorkshopItem>();
			PagesItemsFavorited = 1;
			ItemsFavorited = new List<WorkshopItem>();
			PagesItemsVoted = 1;
			ItemsVoted = new List<WorkshopItem>();
		}
	}
}
