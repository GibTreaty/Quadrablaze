using UnityEngine;
using System.Collections.Generic;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test combobox.
	/// </summary>
	public class TestCombobox : MonoBehaviour
	{
		[SerializeField]
		Combobox combobox;

		/// <summary>
		/// Get selected item.
		/// </summary>
		public void GetSelected()
		{
			//Get last selected index
			Debug.Log(combobox.ListView.SelectedIndex);
			
			//Get selected indices
			var indices = combobox.ListView.SelectedIndices;
			Debug.Log(string.Join(", ", indices.Convert(x => x.ToString()).ToArray()));
			
			//Get last selected string
			if (combobox.ListView.SelectedIndex!=-1)
			{
				Debug.Log(combobox.ListView.DataSource[combobox.ListView.SelectedIndex]);
			}
			
			//Get selected strings
			var selected_strings = combobox.ListView.SelectedIndices.Convert(x => combobox.ListView.DataSource[x]);
			Debug.Log(string.Join(", ", selected_strings.ToArray()));
		}

		/// <summary>
		/// Remove item.
		/// </summary>
		public void Remove()
		{
			//Deleting specified string
			var strings = combobox.ListView.DataSource;
			strings.RemoveAt(0);
		}

		/// <summary>
		/// Clear item list.
		/// </summary>
		public void Clear()
		{
			//Clear list
			combobox.Clear();
		}

		/// <summary>
		/// Add item.
		/// </summary>
		public void AddItem()
		{
			//Add string
			combobox.ListView.DataSource.Add("test string");
		}

		/// <summary>
		/// Add items.
		/// </summary>
		public void AddItems()
		{
			//Add strings
			var new_strings = new List<string>() {
				"test string 1",
				"test string 2",
				"test string 2",
			};
			combobox.ListView.DataSource.AddRange(new_strings);
			//combobox.Set("aaa");
		}

		/// <summary>
		/// Select item.
		/// </summary>
		public void UpdateSelect()
		{
			//Set selected index
			combobox.ListView.SelectedIndex = 1;
			//or
			combobox.ListView.Select(1);
		}
	}
}