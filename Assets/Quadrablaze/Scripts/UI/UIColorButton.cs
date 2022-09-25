using Quadrablaze;
using UnityEngine;
using UnityEngine.UI;

public class UIColorButton : MonoBehaviour {

    [SerializeField]
    Button _colorButton;

    [SerializeField]
    Image _colorImage;

    [SerializeField]
    bool _hdrColor;

    [SerializeField]
    float _brightness;

    [SerializeField]
    PresetColorType _presetColor;

    [SerializeField]
    string _materialProperty = "";

    #region Properties
    public float Brightness {
        get { return _brightness; }
        set { _brightness = value; }
    }

    public Color ButtonColor {
        get { return _colorImage.color; }
        set {
            value.a = 1;
            _colorImage.color = value;
        }
    }

    public Button ColorButton {
        get { return _colorButton; }
    }

    public bool HDRColor {
        get { return _hdrColor; }
        set { _hdrColor = value; }
    }

    public string MaterialProperty {
        get { return _materialProperty; }
        set { _materialProperty = value; }
    }

    public PresetColorType PresetColor {
        get { return _presetColor; }
        set { _presetColor = value; }
    }
    #endregion

    public void ApplyColor(Material material) {
        ApplyColor(ButtonColor, material);
    }
    public void ApplyColor(Color color, Material material) {
        if(HDRColor) {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);

            color = Color.HSVToRGB(h, s, Brightness, true);
        }

        material.SetColor(MaterialProperty, color);
    }
    public void ApplyColor(ShipPreset preset) {
        Color color = ButtonColor;

        if(HDRColor) {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);

            color = Color.HSVToRGB(h, s, Brightness, true);
        }

        switch(PresetColor) {
            case PresetColorType.PrimaryColor: preset.PrimaryColor = color; break;
            case PresetColorType.SecondaryColor: preset.SecondaryColor = color; break;
            case PresetColorType.AccessoryPrimaryColor: preset.AccessoryPrimaryColor = color; break;
            case PresetColorType.AccessorySecondaryColor: preset.AccessorySecondaryColor = color; break;
            case PresetColorType.GlowColor: preset.GlowColor = color; break;
        }
    }

    public void SetImageColor(Color color) {
        ButtonColor = color;
    }

    public void GetColor(ShipPreset preset) {
        Color color = Color.black;

        switch(PresetColor) {
            case PresetColorType.PrimaryColor: color = preset.PrimaryColor; break;
            case PresetColorType.SecondaryColor: color = preset.SecondaryColor; break;
            case PresetColorType.AccessoryPrimaryColor: color = preset.AccessoryPrimaryColor; break;
            case PresetColorType.AccessorySecondaryColor: color = preset.AccessorySecondaryColor; break;
            case PresetColorType.GlowColor: color = preset.GlowColor; break;
        }

        ButtonColor = color;

        if(HDRColor) {
            float h, s, v;

            Color.RGBToHSV(ButtonColor, out h, out s, out v);
            Brightness = v;
        }
    }
}