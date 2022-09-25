using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// GroupedListView
	/// </summary>
	public class GroupedListView : ListViewCustomHeight<GroupedListViewComponent,IGroupedListItem>
	{
		/// <summary>
		/// Grouped data.
		/// </summary>
		public GroupedItems GroupedData = new GroupedItems();

		bool isStartedGroupedListView;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (isStartedGroupedListView)
			{
				return ;
			}
			isStartedGroupedListView = true;

			base.Start();

			GroupedData.GroupComparison = (x, y) => x.Name.CompareTo(y.Name);
			GroupedData.Data = DataSource;
		}
	}
}