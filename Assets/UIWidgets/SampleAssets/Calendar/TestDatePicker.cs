using UnityEngine;
using UnityEngine.UI;
using System;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test DatePicker.
	/// </summary>
	public class TestDatePicker : MonoBehaviour
	{
		[SerializeField]
		DatePicker PickerTemplate;

		[SerializeField]
		Text Info;

		DateTime currentValue = DateTime.Today;

		/// <summary>
		/// Open picker and log selected value.
		/// </summary>
		public void Test()
		{
			// create picker by template
			var picker = PickerTemplate.Template();

			// show picker
			picker.Show(currentValue, ValueSelected, Canceled);
		}

		void ValueSelected(DateTime value)
		{
			currentValue = value;
			Debug.Log("value: " + value);
		}

		void Canceled()
		{
			Debug.Log("canceled");
		}

		/// <summary>
		/// Open picker and display selected value.
		/// </summary>
		public void TestShow()
		{
			// create picker by template
			var picker = PickerTemplate.Template();

			// show picker
			picker.Show(currentValue, ShowValueSelected, ShowCanceled);
		}

		void ShowValueSelected(DateTime value)
		{
			currentValue = value;
			Info.text = "Value: " + value;
		}

		void ShowCanceled()
		{
			Info.text = "Canceled";
		}
	}
}