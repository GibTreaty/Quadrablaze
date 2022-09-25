using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_SliderSynchronizer : MonoBehaviour
	{
		[SerializeField]
		private Slider[] m_sliders = new Slider[0];
		public Slider[] Sliders
		{
			get{ return m_sliders; }
			set{ m_sliders = value; }
		}

		[SerializeField]
		private bool m_isSynchronizeOnStart = true;
		public bool IsSynchronizeOnStart
		{
			get{ return m_isSynchronizeOnStart; }
		}

		[SerializeField]
		private float m_value;
		public float Value
		{
			get{ return m_value; }
		}

		private void Start()
		{
			if (m_sliders.Length > 0)
			{
				m_value = m_sliders[0].value;
			}
			for (int i = 0; i < m_sliders.Length; i++)
			{
				m_sliders[i].onValueChanged.AddListener(OnSliderChanged);
			}
			if (m_isSynchronizeOnStart)
			{
				OnSliderChanged(m_value);
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < m_sliders.Length; i++)
			{
				m_sliders[i].onValueChanged.RemoveListener(OnSliderChanged);
			}
		}

		private void OnSliderChanged(float p_value)
		{
			m_value = p_value;
			for (int i = 0; i < m_sliders.Length; i++)
			{
				if (m_sliders[i].value != m_value)
				{
					m_sliders[i].value = m_value;
				}
			}
		}
	}
}
