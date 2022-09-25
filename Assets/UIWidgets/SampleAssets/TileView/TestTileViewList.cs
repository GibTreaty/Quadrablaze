using UnityEngine;
using UIWidgets;
using System;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test TileViewList.
	/// </summary>
	public class TestTileViewList : MonoBehaviour
	{
		[SerializeField]
		TextAsset sourceFile;

		[SerializeField]
		TileViewList TileView;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			LoadData();
		}

		/// <summary>
		/// Load data.
		/// </summary>
		protected virtual void LoadData()
		{
			var files = sourceFile.text.Split(new string[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);

			var items = new ObservableList<ListViewIconsItemDescription>();
			foreach (var file in files)
			{
				items.Add(new ListViewIconsItemDescription(){Name = file.Trim()});
			}
			TileView.DataSource = items;
		}
	}
}