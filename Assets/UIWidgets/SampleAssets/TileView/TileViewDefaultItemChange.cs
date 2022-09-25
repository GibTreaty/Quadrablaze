using UnityEngine;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test changing TileView DefaultItem.
	/// </summary>
	public class TileViewDefaultItemChange : MonoBehaviour
	{
		[SerializeField]
		TileViewIcons TileView;

		[SerializeField]
		ListViewIconsItemComponent DefaultItemOriginal;

		[SerializeField]
		ListViewIconsItemComponent DefaultItemNew;

		/// <summary>
		/// Set original default item.
		/// </summary>
		public void SetOriginal()
		{
			TileView.DefaultItem = DefaultItemOriginal;
		}

		/// <summary>
		/// Set new default item.
		/// </summary>
		public void SetNew()
		{
			TileView.DefaultItem = DefaultItemNew;
		}
	}
}

