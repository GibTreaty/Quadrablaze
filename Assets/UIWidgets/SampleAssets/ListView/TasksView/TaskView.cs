using UIWidgets;

namespace UIWidgetsSamples.Tasks
{
	/// <summary>
	/// TaskView.
	/// </summary>
	public class TaskView : ListViewCustom<TaskComponent,Task>
	{
		/// <summary>
		/// Tasks comparison.
		/// </summary>
		public static System.Comparison<Task> ItemsComparison = (x, y) => x.Name.CompareTo(y.Name);

		bool isStartedTaskView = false;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (isStartedTaskView)
			{
				return ;
			}
			isStartedTaskView = true;

			base.Start();
			DataSource.Comparison = ItemsComparison;
		}
	}
}