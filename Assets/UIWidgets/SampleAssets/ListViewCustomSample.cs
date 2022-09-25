using System;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// ListViewCustom sample.
	/// </summary>
	public class ListViewCustomSample : ListViewCustom<ListViewCustomSampleComponent,ListViewCustomSampleItemDescription>
	{
		bool isStartedListViewCustomSample = false;

		Comparison<ListViewCustomSampleItemDescription> itemsComparison = (x, y) => x.Name.CompareTo(y.Name);

		/// <summary>
		/// Set items comparison.
		/// </summary>
		public override void Start()
		{
			if (isStartedListViewCustomSample)
			{
				return ;
			}
			isStartedListViewCustomSample = true;

			base.Start();
			DataSource.Comparison = itemsComparison;
		}
	}
}