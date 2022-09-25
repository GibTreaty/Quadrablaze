using UnityEngine;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test Progressbar.
	/// </summary>
	public class TestProgressbar : MonoBehaviour
	{
		[SerializeField]
		Progressbar bar;

		[SerializeField]
		Spinner spinner;

		/// <summary>
		/// Toggle progressbar.
		/// </summary>
		public void Toggle()
		{
			if (bar.IsAnimationRun)
			{
				bar.Stop();
			}
			else
			{
				if (bar.Value==0)
				{
					bar.Animate(bar.Max);
				}
				else
				{
					bar.Animate(0);
				}
			}
		}

		/// <summary>
		/// Set progressbar value.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetValue(int value)
		{
			bar.Animate(value);
		}

		/// <summary>
		/// Set value from spinner.
		/// </summary>
		public void SetFromSpinner()
		{
			SetValue(spinner.Value);
		}
	}
}