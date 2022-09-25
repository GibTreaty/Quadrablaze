using UIWidgets;

namespace UIWidgetsSamples.Tasks
{
	/// <summary>
	/// Task Autocomplete.
	/// </summary>
	public class TaskAutocomplete : AutocompleteCustom<Task,TaskComponent,TaskView>
	{
		/// <summary>
		/// Check if task name starts with input string.
		/// </summary>
		/// <param name="value">Task.</param>
		/// <returns>true if task name starts with input string; otherwise, false.</returns>
		public override bool Startswith(Task value)
		{
			if (CaseSensitive)
			{
				return value.Name.StartsWith(Input);
			}
			return value.Name.ToLower().StartsWith(Input.ToLower());
		}

		/// <summary>
		/// Check if task name contains input string.
		/// </summary>
		/// <param name="value">Task.</param>
		/// <returns>true if task name contains input string; otherwise, false.</returns>
		public override bool Contains(Task value)
		{
			if (CaseSensitive)
			{
				return value.Name.Contains(Input);
			}
			return value.Name.ToLower().Contains(Input.ToLower());
		}

		/// <summary>
		/// Convert task to string to display in InputField.
		/// </summary>
		/// <param name="value">Task.</param>
		/// <returns>String to display in InputField.</returns>
		protected override string GetStringValue(Task value)
		{
			return value.Name;
		}
	}
}