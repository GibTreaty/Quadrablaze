using System;
using System.Collections;
using Quadrablaze;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AutoControllerGlyph : MonoBehaviour {

    [SerializeField]
    Image _glyphImage;

    [SerializeField]
    string _actionName = "";

    [SerializeField]
    ControllerType _deviceType = ControllerType.Keyboard;

    [SerializeField]
    Pole _axisPole = Pole.Positive;

    [SerializeField]
    AxisRange _range = AxisRange.Full;

    public GlyphEvent OnGlyphChanged;

    Controller _lastUsedController;
    Vector2 size;
    Player _rewiredPlayer = null;

    #region Properties
    public string ActionName {
        get { return _actionName; }
        set { _actionName = value; }
    }

    public Pole AxisPole {
        get { return _axisPole; }
        set { _axisPole = value; }
    }

    public ControllerType DeviceType {
        get { return _deviceType; }
        set { _deviceType = value; }
    }

    public Sprite Glyph {
        get { return GlyphImage ? _glyphImage.sprite : null; }
        set {
            if(GlyphImage && GlyphImage.sprite != Glyph) {
                GlyphImage.sprite = Glyph;

                OnGlyphChanged.InvokeEvent(Glyph);
            }
        }
    }

    public Image GlyphImage {
        get { return _glyphImage; }
        set { _glyphImage = value; }
    }

    Controller LastUsedController {
        get { return _lastUsedController; }
        set { _lastUsedController = value; }
    }

    public AxisRange Range {
        get { return _range; }
        set { _range = value; }
    }

    Player RewiredPlayer {
        get { return _rewiredPlayer != null ? _rewiredPlayer : (_rewiredPlayer = ReInput.players.GetPlayer(0)); }
    }
    #endregion

    void OnEnable() {
        if(RewiredPlayer != null)
            ManualUpdateGlyph();
    }

    void Update() {
        var checkController = RewiredPlayer.controllers.GetLastActiveController();

        if(checkController != null) {
            //UpdateGlyph(checkController);
            //return;
            var previousDevice = LastUsedController;
            LastUsedController = checkController;

            if(previousDevice != LastUsedController) {
                switch(LastUsedController.type) {
                    case ControllerType.Keyboard:
                    case ControllerType.Mouse:
                        if(previousDevice == null || (previousDevice.type != ControllerType.Keyboard && previousDevice.type != ControllerType.Mouse))
                            UpdateGlyph(LastUsedController);

                        break;

                    case ControllerType.Joystick:
                        if(previousDevice == null || previousDevice.type != ControllerType.Joystick)
                            UpdateGlyph(LastUsedController);

                        break;
                }
            }
        }
    }

    public void ClearGlyph() {
        AssignSprite(null);
    }

    public void UpdateGlyph(Controller controller) {
        ActionElementMap actionElementMap = null;
        Sprite sprite = null;

        foreach(var elementMap in RewiredPlayer.controllers.maps.ElementMapsWithAction(DeviceType, ActionName, true))
            if(elementMap.axisContribution == AxisPole) {
                actionElementMap = elementMap;
                break;
            }

        if(actionElementMap != null && controller != null) {
            switch(controller.type) {
                case ControllerType.Joystick:
                    if(DeviceType == ControllerType.Joystick) {
                        var elementIdentifier = ReInput.mapping.GetFirstJoystickTemplateElementIdentifier(controller as Joystick, actionElementMap.elementIdentifierId);

                        sprite = ButtonGlyphManager.GetJoystickGlyph((controller as Joystick).hardwareTypeGuid, actionElementMap.elementIdentifierId, elementIdentifier.id, Range);
                        //sprite = ButtonGlyphManager.GetJoystickGlyph((controller as Joystick).hardwareTypeGuid, actionElementMap.elementIdentifierId, elementIdentifier.id, AxisRange.Full);
                    }
                    break;

                case ControllerType.Keyboard:
                case ControllerType.Mouse:
                    if(DeviceType == ControllerType.Keyboard)
                        sprite = ButtonGlyphManager.GetGlyph(DeviceType, (int)actionElementMap.keyboardKeyCode, Range);
                    else if(DeviceType == ControllerType.Mouse)
                        sprite = ButtonGlyphManager.GetGlyph(DeviceType, actionElementMap.elementIdentifierId, Range);

                    break;
            }
        }

        AssignSprite(sprite);
    }

    public void AssignSprite(Sprite sprite) {
        _glyphImage.sprite = sprite;
        _glyphImage.color = sprite ? Color.white : Color.clear;

        var layoutElement = GetComponent<LayoutElement>();

        if(layoutElement)
            layoutElement.ignoreLayout = sprite == null;
    }

    [ContextMenu("Manually Update Glyph", true)]
    bool CanUseManualUpdate() {
        return Application.isPlaying;
    }

    [ContextMenu("Manually Update Glyph")]
    public void ManualUpdateGlyph() {
        UpdateGlyph(RewiredPlayer.controllers.GetLastActiveController());
    }

    [Serializable]
    public class GlyphEvent : UnityEvent<Sprite> { }
}