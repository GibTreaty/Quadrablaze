using UnityEngine;
using UIWidgets;
using System.Collections.Generic;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test ListViewIcons.
	/// </summary>
	public class TestListViewIcons : MonoBehaviour
	{
		[SerializeField]
		ListViewIcons ListView;

		[SerializeField]
		Notify notifySimple;

		/// <summary>
		/// Add listeners.
		/// </summary>
		public void Start()
		{
			ListView.OnSelectObject.AddListener(Notification);
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		public void OnDestroy()
		{
			ListView.OnSelectObject.RemoveListener(Notification);
		}

		/// <summary>
		/// Show notification for specified item index.
		/// </summary>
		/// <param name="index">Item index.</param>
		public void Notification(int index)
		{
			var message = ListView.SelectedIndex==-1
				? "Nothing selected"
				: "Selected: " + ListView.SelectedItem.Name;

			notifySimple.Template().Show(
				message,
				customHideDelay: 5f
			);
		}

		[SerializeField]
		ListViewIconsItemComponent DefaultItemOriginal;

		[SerializeField]
		ListViewIconsItemComponent DefaultItemNew;

		/// <summary>
		/// Set original default item component.
		/// </summary>
		public void SetOriginal()
		{
			ListView.DefaultItem = DefaultItemOriginal;
		}

		/// <summary>
		/// Set new default item component.
		/// </summary>
		public void SetNew()
		{
			ListView.DefaultItem = DefaultItemNew;
		}
	}
}