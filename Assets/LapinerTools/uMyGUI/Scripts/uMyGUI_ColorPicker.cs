using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_ColorPicker : MonoBehaviour
	{
		public class ColorEventArgs : System.EventArgs { public readonly Color Value; public ColorEventArgs(Color p_value){ Value = p_value; } }

		[SerializeField]
		private Slider m_redSlider = null;
		public Slider RedSlider
		{
			get{ return m_redSlider; }
			set{ m_redSlider = value; }
		}

		[SerializeField]
		private Slider m_greenSlider = null;
		public Slider GreenSlider
		{
			get{ return m_greenSlider; }
			set{ m_greenSlider = value; }
		}

		[SerializeField]
		private Slider m_blueSlider = null;
		public Slider BlueSlider
		{
			get{ return m_blueSlider; }
			set{ m_blueSlider = value; }
		}

		[SerializeField]
		private Color m_pickedColor = Color.gray;
		public Color PickedColor
		{
			get{ return m_pickedColor; }
			set
			{
				if (m_pickedColor != value)
				{
					m_pickedColor = value;
					UpdateColor();
					if (m_onChanged != null) { m_onChanged(this, new ColorEventArgs(m_pickedColor)); }
				}
			}
		}

		[SerializeField]
		private Graphic m_colorPreview = null;
		public Graphic ColorPreview
		{
			get{ return m_colorPreview; }
			set{ m_colorPreview = value; }
		}

		public System.EventHandler<ColorEventArgs> m_onChanged;

		private void Start()
		{
			if (m_redSlider == null || m_greenSlider == null || m_blueSlider == null)
			{
				Debug.LogError("uMyGUI_ColorPicker: all three sliders (RGB) must be set in inspector!");
				enabled = false;
				return;
			}

			UpdateColor();
			m_redSlider.onValueChanged.AddListener(SetRedValue);
			m_greenSlider.onValueChanged.AddListener(SetGreenValue);
			m_blueSlider.onValueChanged.AddListener(SetBlueValue);
		}

		private void OnDestroy()
		{
			m_onChanged = null;
			m_redSlider.onValueChanged.RemoveListener(SetRedValue);
			m_greenSlider.onValueChanged.RemoveListener(SetGreenValue);
			m_blueSlider.onValueChanged.RemoveListener(SetBlueValue);
		}

		private void SetRedValue(float p_redValue)
		{
			if (m_pickedColor.r != p_redValue)
			{
				m_pickedColor.r = p_redValue;
				UpdateColor();
				if (m_onChanged != null) { m_onChanged(this, new ColorEventArgs(m_pickedColor)); }
			}
		}

		private void SetGreenValue(float p_greenValue)
		{
			if (m_pickedColor.g != p_greenValue)
			{
				m_pickedColor.g = p_greenValue;
				UpdateColor();
				if (m_onChanged != null) { m_onChanged(this, new ColorEventArgs(m_pickedColor)); }
			}
		}

		private void SetBlueValue(float p_blueValue)
		{
			if (m_pickedColor.b != p_blueValue)
			{
				m_pickedColor.b = p_blueValue;
				UpdateColor();
				if (m_onChanged != null) { m_onChanged(this, new ColorEventArgs(m_pickedColor)); }
			}
		}

		private void UpdateColor()
		{
			m_redSlider.value = m_pickedColor.r;
			m_greenSlider.value = m_pickedColor.g;
			m_blueSlider.value = m_pickedColor.b;
			if (m_colorPreview != null)
			{
				m_colorPreview.color = m_pickedColor;
			}
		}
	}
}
