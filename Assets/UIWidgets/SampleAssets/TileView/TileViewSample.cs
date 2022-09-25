using UnityEngine;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// TileViewSample.
	/// </summary>
	public class TileViewSample : TileView<TileViewComponentSample,TileViewItemSample>
	{
		bool isStartedTileViewSample = false;

		//Comparison<TileViewItemSample> itemsComparison = (x, y) => x.Name.CompareTo(y.Name);

		/// <summary>
		/// Awake this instance.
		/// </summary>
		protected override void Awake()
		{
			Start();
			//OnSelect.AddListener(ItemSelected);
			//OnSelectObject.AddListener(ItemSelected);
		}

		void ItemSelected(int index, ListViewItem component)
		{

			if (component!=null)
			{
				//(component as TileViewComponentSample).DoSomething();
			}
			Debug.Log(index);
			Debug.Log(DataSource[index].Name);
		}

		void ItemSelected(int index)
		{
			Debug.Log(index);
			Debug.Log(DataSource[index].Name);
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (isStartedTileViewSample)
			{
				return ;
			}
			isStartedTileViewSample = true;
			
			base.Start();
			//DataSource.Comparison = itemsComparison;
		}
	}
}