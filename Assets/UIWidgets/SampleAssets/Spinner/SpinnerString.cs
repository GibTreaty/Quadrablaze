using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Spinner with string values.
	/// Display strings instead numeric value.
	/// </summary>
	public class SpinnerString : MonoBehaviour
	{
		// Spinner.TextComponent size changed to 0
		[SerializeField]
		Spinner Spinner;

		// Spinner.TextComponent, used to display option instead number
		[SerializeField]
		Text Text;

		// Options list
		[SerializeField]
		List<string> Options = new List<string>();

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Spinner.Min = -1;
			Spinner.Max = Options.Count;
			Spinner.Step = 1;

			// add callback
			Spinner.onValueChangeInt.AddListener(Changed);

			// display initial option
			Changed(Spinner.Value);
		}

		/// <summary>
		/// Handle change event.
		/// </summary>
		/// <param name="value">Value.</param>
		protected virtual void Changed(int value)
		{
			if (value==-1)
			{
				Spinner.Value = Options.Count - 1;
			}
			else if (value==Options.Count)
			{
				Spinner.Value = 0;
			}
			else
			{
				// display option
				Text.text = Options[Spinner.Value];
			}
		}
	}
}