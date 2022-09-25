using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test PickerBool.
	/// </summary>
	public class TestPickerBool : MonoBehaviour
	{
		[SerializeField]
		PickerBool PickerTemplate;

		[SerializeField]
		Text Info;

		bool currentValue;

		/// <summary>
		/// Show picker and log selected value.
		/// </summary>
		public void Test()
		{
			// create picker by template
			var picker = PickerTemplate.Template();

			picker.SetMessage("Confirmation text");

			// show picker
			picker.Show(currentValue, ValueSelected, Canceled);
		}

		void ValueSelected(bool value)
		{
			currentValue = value;
			Debug.Log("value: " + value);
		}

		void Canceled()
		{
			Debug.Log("canceled");
		}

		/// <summary>
		/// Show picker and display selected value.
		/// </summary>
		public void TestShow()
		{
			// create picker by template
			var picker = PickerTemplate.Template();

			picker.SetMessage("Confirmation text");
			
			// show picker
			picker.Show(currentValue, ShowValueSelected, ShowCanceled);
		}

		void ShowValueSelected(bool value)
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