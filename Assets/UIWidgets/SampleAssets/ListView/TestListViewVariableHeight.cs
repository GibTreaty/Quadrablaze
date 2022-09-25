using UnityEngine;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test ListViewVariableHeight.
	/// </summary>
	public class TestListViewVariableHeight : MonoBehaviour
	{
		[SerializeField]
		ListViewVariableHeight list;

		/// <summary>
		/// Add item.
		/// </summary>
		public void Add()
		{
			//list.DataSource.Clear();
			list.DataSource.Add(new ListViewVariableHeightItemDescription(){
				Name = "Added",
				Text = "Test\nTest\ntest3",
			});
		}
	}
}