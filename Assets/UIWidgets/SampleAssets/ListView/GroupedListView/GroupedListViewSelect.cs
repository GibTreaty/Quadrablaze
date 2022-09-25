using System;
using System.Linq;
using UIWidgets;
using UnityEngine;

namespace UIWidgetsSamples
{
	/// <summary>
	/// GroupedListViewSelect.
	/// </summary>
	[RequireComponent(typeof(GroupedListView))]
	public class GroupedListViewSelect : MonoBehaviour
	{
		GroupedListView ListView;

		[SerializeField]
		bool selectGroupItems;

		/// <summary>
		/// Add listeners.
		/// </summary>
		protected virtual void Start()
		{
			ListView = GetComponent<GroupedListView>();
			ListView.OnSelect.AddListener(ProcessSelected);
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (ListView != null)
			{
				ListView.OnSelect.RemoveListener(ProcessSelected);
			}
		}

		/// <summary>
		/// Process selected item.
		/// Select all items from group on group click.
		/// </summary>
		/// <param name="index">Selected item index.</param>
		/// <param name="component">Selected item component.</param>
		protected virtual void ProcessSelected(int index, ListViewItem component)
		{
			if (!(ListView.DataSource[index] is GroupedListGroup))
			{
				return ;
			}
			ListView.Deselect(index);
			if (selectGroupItems)
			{
				var end_index = ListView.DataSource.FindIndex(index + 1, x => x is GroupedListGroup);
				if (end_index==-1)
				{
					end_index = ListView.DataSource.Count - 1;
				}
				else
				{
					end_index -= 1;
				}
				Enumerable.Range(index + 1, end_index - index).ForEach(ListView.Toggle);
			}
		}
	}
}