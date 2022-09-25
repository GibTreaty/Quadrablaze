using UIWidgets;
using UnityEngine;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test SpinnerFloat.
	/// </summary>
	public class TestSpinnerFloat : MonoBehaviour
	{
		[SerializeField]
		SpinnerFloat spinner;

		/// <summary>
		/// Change culture.
		/// </summary>
		public void ChangeCulture()
		{
			#if !NETFX_CORE
			//Culture names https://msdn.microsoft.com/ru-ru/goglobal/bb896001.aspx
			spinner.Culture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
			#endif
		}
	}
}