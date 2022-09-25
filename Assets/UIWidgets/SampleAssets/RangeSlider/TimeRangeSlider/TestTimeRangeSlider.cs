using UnityEngine;
using UnityEngine.UI;
using System;

namespace UIWidgetsSamples
{
	/// <summary>
	/// Test TimeRangeSlider.
	/// </summary>
	[RequireComponent(typeof(TimeRangeSlider))]
	public class TestTimeRangeSlider : MonoBehaviour
	{
		[SerializeField]
		Text startText;

		[SerializeField]
		Text endText;

		[SerializeField]
		string format = "hh:mm tt";

		/// <summary>
		/// Current slider.
		/// </summary>
		[HideInInspector]
		protected TimeRangeSlider Slider;

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected virtual void Start()
		{
			Slider = GetComponent<TimeRangeSlider>();
			if (Slider!=null)
			{
				Slider.OnChange.AddListener(SliderChanged);

				SetRange1();

				Slider.StartTime = Slider.MinTime;
				Slider.EndTime = Slider.MaxTime;

				SliderChanged(Slider.StartTime, Slider.EndTime);
			}
		}

		/// <summary>
		/// Set slider range.
		/// </summary>
		public void SetRange1()
		{
			Slider.MinTime = new DateTime(2017, 1, 1, 9, 0, 0);
			Slider.MaxTime = new DateTime(2017, 1, 1, 18, 0, 0);

			SliderChanged(Slider.StartTime, Slider.EndTime);
		}

		/// <summary>
		/// Set another range.
		/// </summary>
		public void SetRange2()
		{
			Slider.MinTime = new DateTime(2017, 1, 1, 12, 0, 0);
			Slider.MaxTime = new DateTime(2017, 1, 1, 15, 0, 0);

			SliderChanged(Slider.StartTime, Slider.EndTime);
		}

		/// <summary>
		/// Handle slider value changed event.
		/// </summary>
		/// <param name="start">Start time.</param>
		/// <param name="end">End time.</param>
		protected virtual void SliderChanged(DateTime start, DateTime end)
		{
			if (startText!=null)
			{
				startText.text = start.ToString(format);
			}
			if (endText!=null)
			{
				endText.text = end.ToString(format);
			}
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (Slider!=null)
			{
				Slider.OnChange.RemoveListener(SliderChanged);
			}
		}
	}
}