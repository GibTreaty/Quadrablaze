using UnityEngine;
using System.Collections.Generic;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test ComboboxIcons.
	/// </summary>
	public class TestComboboxIcons : MonoBehaviour
	{
		[SerializeField]
		ComboboxIcons comboboxIcons;
		
		[SerializeField]
		Sprite sampleIcon;

		/// <summary>
		/// Awake this instance.
		/// </summary>
		public void Awake()
		{
			comboboxIcons.Start();

			GetSelected();
		}

		/// <summary>
		/// Get selected item.
		/// </summary>
		public void GetSelected()
		{
			//Get last selected index
			Debug.Log(comboboxIcons.ListView.SelectedIndex);

			//Get last selected string
			Debug.Log(comboboxIcons.ListView.SelectedItem.Name);
		}

		/// <summary>
		/// Remove item.
		/// </summary>
		public void Remove()
		{
			//Deleting specified item
			var items = comboboxIcons.ListView.DataSource;
			items.Remove(items[0]);
		}

		/// <summary>
		/// Remove item at position.
		/// </summary>
		public void RemoveAt()
		{
			//Deleting item by index
			comboboxIcons.ListView.DataSource.RemoveAt(0);
		}

		/// <summary>
		/// Clear items list.
		/// </summary>
		public void Clear()
		{
			comboboxIcons.Clear();
		}

		/// <summary>
		/// Add item.
		/// </summary>
		public void AddItem()
		{
			var new_item = new ListViewIconsItemDescription() {
				Icon = sampleIcon,
				Name = "test item"
			};
			comboboxIcons.ListView.DataSource.Add(new_item);
		}

		/// <summary>
		/// Add items.
		/// </summary>
		public void AddItems()
		{
			var new_item = new ListViewIconsItemDescription() {
				Icon = sampleIcon,
				Name = "test item"
			};

			var new_items = new List<ListViewIconsItemDescription>() {
				new_item,
				new_item,
				new_item
			};
			comboboxIcons.ListView.DataSource.AddRange(new_items);
		}

		/// <summary>
		/// Select item.
		/// </summary>
		public void Select()
		{
			//Set selected index
			comboboxIcons.ListView.SelectedIndex = 1;
			//or
			comboboxIcons.ListView.Select(1);
		}
	}
}