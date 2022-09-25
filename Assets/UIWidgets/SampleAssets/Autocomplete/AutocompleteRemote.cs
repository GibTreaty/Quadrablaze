using UnityEngine;
using System.Collections;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Autocomplete for ListViewIcons.
	/// </summary>
	[RequireComponent(typeof(ListViewIcons))]
	public class AutocompleteRemote : AutocompleteCustom<ListViewIconsItemDescription,ListViewIconsItemComponent,ListViewIcons>
	{
		/// <summary>
		/// The minimum number of characters a user must type before a search is performed.
		/// </summary>
		[SerializeField]
		public int MinLength = 3;

		/// <summary>
		/// The delay in seconds between when a keystroke occurs and when a search is performed.
		/// </summary>
		[SerializeField]
		public float Delay = 0.5f;

		/// <summary>
		/// Coroutine to load data.
		/// </summary>
		protected IEnumerator Coroutine = null;

		/// <summary>
		/// Get data with specified string.
		/// </summary>
		/// <param name="input">Input string.</param>
		protected override void ApplyFilter(string input)
		{
			SetInput();

			if (Input==PrevInput)
			{
				return ;
			}
			PrevInput = Input;

			if (Input.Length < MinLength)
			{
				DisplayListView.DataSource.Clear();
				HideOptions();
				return ;
			}

			DisplayListView.Start();
			DisplayListView.Multiple = false;

			if (Coroutine!=null)
			{
				StopCoroutine(Coroutine);
			}
			Coroutine = LoadData(input);
			StartCoroutine(Coroutine);
		}

		/// <summary>
		/// Load data from web.
		/// </summary>
		/// <param name="search">Search string.</param>
		/// <returns>Yield instruction.</returns>
		protected virtual IEnumerator LoadData(string search)
		{
			yield return new WaitForSeconds(Delay);

			WWW www = new WWW("http://example.com/?search=" + WWW.EscapeURL(search));
			yield return www;

			DisplayListView.DataSource = Text2List(www.text);

			www.Dispose();

			if (DisplayListView.DataSource.Count > 0)
			{
				ShowOptions();
				DisplayListView.SelectedIndex = 0;
			}
			else
			{
				HideOptions();
			}
		}

		/// <summary>
		/// Convert raw data to list.
		/// </summary>
		/// <param name="text">Raw data.</param>
		/// <returns>Data list.</returns>
		static protected ObservableList<ListViewIconsItemDescription> Text2List(string text)
		{
			var result = new ObservableList<ListViewIconsItemDescription>();

			//convert text to items and add items to list
			foreach (var line in text.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None))
			{
				result.Add(new ListViewIconsItemDescription() { Name = line.TrimEnd() });
			}

			return result;
		}

		/// <summary>
		/// Determines whether the beginnig of value matches the Input.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if beginnig of value matches the Input; otherwise, false.</returns>
		public override bool Startswith(ListViewIconsItemDescription value)
		{
			if (CaseSensitive)
			{
				return value.Name.StartsWith(Input) || (value.LocalizedName!=null && value.LocalizedName.StartsWith(Input));
			}
			return value.Name.ToLower().StartsWith(Input.ToLower()) || (value.LocalizedName!=null && value.LocalizedName.ToLower().StartsWith(Input.ToLower()));
		}

		/// <summary>
		/// Returns a value indicating whether Input occurs within specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>true if the Input occurs within value parameter; otherwise, false.</returns>
		public override bool Contains(ListViewIconsItemDescription value)
		{
			if (CaseSensitive)
			{
				return value.Name.Contains(Input) || (value.LocalizedName!=null && value.LocalizedName.Contains(Input));
			}
			return value.Name.ToLower().Contains(Input.ToLower()) || (value.LocalizedName!=null && value.LocalizedName.ToLower().Contains(Input.ToLower()));
		}

		/// <summary>
		/// Convert value to string.
		/// </summary>
		/// <returns>The string value.</returns>
		/// <param name="value">Value.</param>
		protected override string GetStringValue(ListViewIconsItemDescription value)
		{
			return value.LocalizedName ?? value.Name;
		}
	}
}