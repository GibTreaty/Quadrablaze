using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test PickerInt.
	/// </summary>
	public class PickerIntTest : MonoBehaviour
	{
		[SerializeField]
		PickerInt PickerTemplate;

		[SerializeField]
		Text Info;

		int currentValue = 0;

		/// <summary>
		/// Run test.
		/// </summary>
		public void Test()
		{
			// create picker by template
			var picker = PickerTemplate.Template();

			// set values from template
			picker.ListView.DataSource = PickerTemplate.ListView.DataSource.ToObservableList();
			// or set new values
			//picker.ListView.DataSource = Enumerable.Range(1, 100).ToObservableList();

			// show picker
			picker.Show(currentValue, ValueSelected, Canceled);
		}

		void ValueSelected(int value)
		{
			currentValue = value;
			Debug.Log("value: " + value);
		}

		void Canceled()
		{
			Debug.Log("canceled");
		}

		/// <summary>
		/// Run test.
		/// </summary>
		public void TestShow()
		{
			// create picker by template
			var picker = PickerTemplate.Template();

			// set values from template
			//picker.ListView.DataSource = PickerTemplate.ListView.DataSource.ToObservableList();
			// or set new values
			picker.ListView.DataSource = Enumerable.Range(1, 100).ToObservableList();

			// show picker
			picker.Show(currentValue, ShowValueSelected, ShowCanceled);
		}

		void ShowValueSelected(int value)
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