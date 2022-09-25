using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rewired;

public class ButtonGlyphManager : MonoBehaviour {

    public static ButtonGlyphManager Current { get; private set; }

    [SerializeField]
    List<ButtonGlyphSet> _buttonGlyphSets = new List<ButtonGlyphSet>();

    #region Properties
    public List<ButtonGlyphSet> ButtonGlyphSets {
        get { return _buttonGlyphSets; }
        set { _buttonGlyphSets = value; }
    }
    #endregion

    void OnEnable() {
        Current = this;
    }

    public static Sprite GetJoystickGlyph(System.Guid joystickGuid, int elementIdentifierID, int templateElementIdentifierId, AxisRange axisRange = AxisRange.Full) {
        if(Current == null) return null;
        if(Current.ButtonGlyphSets == null) return null;

        for(int i = 0; i < Current.ButtonGlyphSets.Count; i++) {
            var buttonGlyph = Current.ButtonGlyphSets[i];

            if(buttonGlyph.DeviceType == ControllerType.Joystick)
                if(buttonGlyph.Joystick != null && buttonGlyph.Joystick.Guid == joystickGuid)
                    return buttonGlyph.GetGlyph(elementIdentifierID, axisRange);
                else if(buttonGlyph.JoystickTemplate != null)
                    return buttonGlyph.GetGlyph(templateElementIdentifierId, axisRange);
        }

        return null;
    }

    public static Sprite GetGlyph(ControllerType controllerType, int elementIdentifierId, AxisRange axisRange = AxisRange.Full, System.Guid joystickGuid = default(System.Guid)) {
        if(Current == null) return null;
        if(Current.ButtonGlyphSets == null) return null;

        for(int i = 0; i < Current.ButtonGlyphSets.Count; i++) {
            var buttonGlyph = Current.ButtonGlyphSets[i];

            if(buttonGlyph.DeviceType == controllerType)
                return buttonGlyph.GetGlyph(elementIdentifierId, axisRange);
        }

        return null;
    }
}