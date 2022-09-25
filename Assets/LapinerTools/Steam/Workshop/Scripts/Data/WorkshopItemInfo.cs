using UnityEngine;
using System.Collections;

namespace LapinerTools.Steam.Data.Internal
{
	/// <summary>
	/// This class is used to serialize data into an XML file in the item folder.
	/// This XML allows to update levels by using the same Steam PublishedFileId.
	/// Besides, the Name and Description are saved to prepopulate the update popup.
	/// </summary>
	public class WorkshopItemInfo
	{
		public ulong PublishedFileId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string IconFileName { get; set; }
		public string[] Tags { get; set; }
	}
}
