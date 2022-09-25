using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.Data.Mapping;

[CreateAssetMenu(fileName = "New Button Glyph Set", menuName = "Input/Button Glyph Set")]
public class ButtonGlyphSet : ScriptableObject {

    [SerializeField]
    ControllerType _deviceType;

    [SerializeField]
    HardwareJoystickMap _joystick;

    [SerializeField]
    HardwareJoystickTemplateMap _joystickTemplate;

    [SerializeField]
    List<ButtonGlyph> _buttonGlyphs = new List<ButtonGlyph>();

    #region Properties
    public List<ButtonGlyph> ButtonGlyphs {
        get { return _buttonGlyphs; }
        set { _buttonGlyphs = value; }
    }

    public ControllerType DeviceType {
        get { return _deviceType; }
        set { _deviceType = value; }
    }

    public HardwareJoystickMap Joystick {
        get { return _joystick; }
        set { _joystick = value; }
    }

    public HardwareJoystickTemplateMap JoystickTemplate {
        get { return _joystickTemplate; }
        set { _joystickTemplate = value; }
    }
    #endregion

    public Sprite GetGlyph(int elementIdentifierId, AxisRange axisRange = AxisRange.Full) {
        for(int i = 0; i < ButtonGlyphs.Count; i++)
            if(ButtonGlyphs[i].elementIdentifierId == elementIdentifierId)
                return ButtonGlyphs[i].GetGlyph(axisRange);

        return null;
    }

    public void SetGlyph(int index, ButtonGlyph glyph) {
        _buttonGlyphs[index] = glyph;
    }

    public void SetGlyphIcon(int index, Sprite icon) {
        var glyph = _buttonGlyphs[index];

        glyph.icon = icon;
        _buttonGlyphs[index] = glyph;
    }

    [System.Serializable]
    public struct ButtonGlyph {

        public string name;

        public int elementIdentifierId;
        public List<string> alternateElementIdentifierNames;

        public Sprite icon;
        public Sprite iconPositive;
        public Sprite iconNegative;

        public ButtonGlyph(string name, int elementIdentifierId, List<string> alternateElementIdentifierNames) {
            this.name = name;
            this.elementIdentifierId = elementIdentifierId;
            this.alternateElementIdentifierNames = alternateElementIdentifierNames;

            icon = null;
            iconPositive = null;
            iconNegative = null;
        }

        public bool CompareNames(string name) {
            return this.name == name || alternateElementIdentifierNames.Contains(name);
        }

        public Sprite GetGlyph(AxisRange axisRange) {
            switch(axisRange) {
                case AxisRange.Full: return icon;
                case AxisRange.Positive: return iconPositive != null ? iconPositive : icon;
                case AxisRange.Negative: return iconNegative != null ? iconNegative : icon;
            }

            return null;
        }
    }
}