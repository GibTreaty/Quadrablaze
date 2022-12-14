using UnityEngine;
using UIWidgets;
#if NETFX_CORE
using System.Threading.Tasks;
#else
using System.Threading;
#endif

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test ListViewIcons with threads.
	/// </summary>
	public class TestListViewIconsThread : MonoBehaviour
	{
		[SerializeField]
		ListViewIcons ListView;

		/// <summary>
		/// Test adding items from other thread.
		/// </summary>
		public void TestAdd()
		{
#if NETFX_CORE
			var t = Task.Run(() => AddInForeground());
#else
			var th = new Thread(AddInForeground);
			th.Start();
#endif
		}

		/// <summary>
		/// Test setting new items list from other thread.
		/// </summary>
		public void TestSet()
		{
#if NETFX_CORE
			var t = Task.Run(() => SetInForeground());
#else
			var th = new Thread(SetInForeground);
			th.Start();
#endif
		}

		/// <summary>
		/// Select and scroll to specified index.
		/// </summary>
		/// <param name="i">Index.</param>
		public void Scroll(int i)
		{
			ListView.Select(i);
			ListView.ScrollTo(i);
		}

		void AddInForeground()
		{
			var items = ListView.DataSource;

			items.BeginUpdate();

			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 1"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 2"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 3"});

			items.EndUpdate();
		}

		void SetInForeground()
		{
			var items = new ObservableList<ListViewIconsItemDescription>();

			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 1"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 2"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 3"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 4"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 5"});
			items.Add(new ListViewIconsItemDescription(){Name ="Added from thread 6"});

			ListView.DataSource = items;
		}
	}
}