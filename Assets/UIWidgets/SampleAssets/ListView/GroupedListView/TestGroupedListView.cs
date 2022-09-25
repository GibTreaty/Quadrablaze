using System;
using UnityEngine;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test GroupedListView.
	/// </summary>
	public class TestGroupedListView : MonoBehaviour
	{
		[SerializeField]
		TextAsset sourceFile;

		[SerializeField]
		GroupedListView GroupedListView;

		/// <summary>
		/// Load data.
		/// </summary>
		protected virtual void Start()
		{
			LoadData();
		}

		/// <summary>
		/// Load data.
		/// </summary>
		protected virtual void LoadData()
		{
			GroupedListView.Start();

			GroupedListView.GroupedData.BeginUpdate();

			var lines = sourceFile.text.Split(new string[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);

			foreach (var line in lines)
			{
				var l = line.Trim();
				if (string.IsNullOrEmpty(l) || l[0]=='#')
				{
					continue ;
				}
				GroupedListView.GroupedData.Add(new GroupedListItem(){Name = l});
			}

			GroupedListView.GroupedData.EndUpdate();
		}
	}
}