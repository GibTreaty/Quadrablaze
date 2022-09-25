using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Quadrablaze;
using System.Collections;

public class ButtonGlyphTester : MonoBehaviour {

    [SerializeField]
    Image _buttonImage;

    [SerializeField]
    string _actionName = "Shockwave";

    [SerializeField]
    ControllerType _deviceType = ControllerType.Keyboard;

    #region Properties
    public string ActionName {
        get { return _actionName; }
        set { _actionName = value; }
    }

    public ControllerType DeviceType {
        get { return _deviceType; }
        set { _deviceType = value; }
    }

    Player RewiredPlayer { get; set; }
    #endregion

    void Awake() {
        RewiredPlayer = ReInput.players.GetPlayer(0);
    }

    void OnEnable() {
        GlobalInput.OnChangedPCorConsoleController.AddListener(UpdateGlyph);
        StartCoroutine("DelayedOnEnable");
    }

    void OnDisable() {
        GlobalInput.OnChangedPCorConsoleController.RemoveListener(UpdateGlyph);
    }

    IEnumerator DelayedOnEnable() {
        yield return new WaitForEndOfFrame();
        UpdateGlyph(ReInput.controllers.GetControllers(ControllerType.Keyboard)[0]);
    }

    public void UpdateGlyph(Controller controller) {
        var actionElementMap = RewiredPlayer.controllers.maps.GetFirstElementMapWithAction(DeviceType, ActionName, true);
        Sprite sprite = null;

        if(actionElementMap != null)
            switch(DeviceType) {
                case ControllerType.Joystick:
                    if(controller.type == ControllerType.Joystick) {
                        var elementIdentifier = ReInput.mapping.GetFirstJoystickTemplateElementIdentifier(controller as Joystick, actionElementMap.elementIdentifierId);

                        sprite = ButtonGlyphManager.GetJoystickGlyph((controller as Joystick).hardwareTypeGuid, actionElementMap.elementIdentifierId, elementIdentifier.id, actionElementMap.axisRange);
                    }

                    break;

                case ControllerType.Keyboard:
                    if(controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
                        sprite = ButtonGlyphManager.GetGlyph(DeviceType, (int)actionElementMap.keyboardKeyCode, actionElementMap.axisRange);

                    break;

                case ControllerType.Mouse:
                    if(controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
                        sprite = ButtonGlyphManager.GetGlyph(DeviceType, actionElementMap.elementIdentifierId, actionElementMap.axisRange);

                    break;
            }

        AssignSprite(sprite);
    }

    public void AssignSprite(Sprite sprite) {
        _buttonImage.sprite = sprite;
        _buttonImage.color = _buttonImage.sprite ? Color.white : Color.clear;
    }
}