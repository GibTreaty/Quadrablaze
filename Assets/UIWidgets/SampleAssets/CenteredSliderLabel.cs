using UnityEngine;
using UnityEngine.UI;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// CenteredSlider with label.
	/// </summary>
	[RequireComponent(typeof(CenteredSlider))]
	public class CenteredSliderLabel : MonoBehaviour
	{
		[SerializeField]
		Text label;

		CenteredSlider slider;

		/// <summary>
		/// Init and add listeners.
		/// </summary>
		protected virtual void Start()
		{
			slider = GetComponent<CenteredSlider>();
			if (slider!=null)
			{
				slider.OnValuesChange.AddListener(ValueChanged);
				ValueChanged(slider.Value);
			}
		}
		
		/// <summary>
		/// Callback when slider value changed.
		/// </summary>
		/// <param name="value">Value.</param>
		protected virtual void ValueChanged(int value)
		{
			label.text = value.ToString();
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (slider!=null)
			{
				slider.OnValuesChange.RemoveListener(ValueChanged);
			}
		}
	}
}