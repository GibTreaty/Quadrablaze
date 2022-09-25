using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class UIInputElement : MonoBehaviour {

    public Text inputNameText;
    public Button primaryInputButton;
    public Text primaryInputText;
    public Text secondaryInputText;

    bool _useAlternateName;
    int _controlIndex;

    #region Properties
    public int ControlIndex {
        get { return _controlIndex; }
        set {
            _controlIndex = value;
            RefreshText();
        }
    }

    public bool UseAlternateName {
        get { return _useAlternateName; }
        set { _useAlternateName = value; }
    }
    #endregion

    public void SetText(string inputName, string primaryInput, string secondaryInput) {
        inputNameText.text = inputName;
        primaryInputText.text = primaryInput;
        secondaryInputText.text = secondaryInput;
    }

    public void RefreshText() {
        //Rewired.ReInput.players.GetPlayer(0).controllers.polling.PollControllerForFirstAxis(Rewired.ControllerType.Keyboard,0);
        //if(cInput.GetText(ControlIndex, 1) == "Escape")
        //	primaryInputButton.interactable = false;

        //string name1 = ReInput.players.GetPlayer(0).controllers.maps.GetFirstButtonMapWithAction(ControllerType.Keyboard, ControlIndex, false).elementIdentifierName;

        //ReInput.players.GetPlayer(0).controllers.maps.ElementMapsWithAction(ControllerType.Keyboard, ControlIndex, true);
        //SetText(ReInput.mapping.Actions[ControlIndex].name, name1, ReInput.mapping.Actions[ControlIndex].name);

        InputAction inputAction = ReInput.mapping.GetAction(ControlIndex);
        Player.ControllerHelper.MapHelper maps = ReInput.players.GetPlayer(0).controllers.maps;

        ActionElementMap keyboardElement = maps.GetFirstElementMapWithAction(ControllerType.Keyboard, ControlIndex, false);
        ActionElementMap mouseElement = maps.GetFirstElementMapWithAction(ControllerType.Mouse, ControlIndex, false);
        ActionElementMap gamepadElement = maps.GetFirstElementMapWithAction(ControllerType.Joystick, ControlIndex, false);
        
        string keyboardMouseInputText = "";

        if(keyboardElement != null) keyboardMouseInputText = keyboardElement.elementIdentifierName;
        else if(mouseElement != null) keyboardMouseInputText = mouseElement.elementIdentifierName;

        string gamepadInputText = "";

        if(gamepadElement != null) gamepadInputText = gamepadElement.elementIdentifierName;

        if(inputAction.type == InputActionType.Button) {
            if(keyboardElement != null) keyboardMouseInputText = keyboardElement.elementIdentifierName;
            else if(mouseElement != null) keyboardMouseInputText = mouseElement.elementIdentifierName;

            if(gamepadElement != null) gamepadInputText = gamepadElement.elementIdentifierName;

            SetText(inputAction.name, keyboardMouseInputText, gamepadInputText);
        }
        else {
            if(UseAlternateName)
                SetText(inputAction.positiveDescriptiveName, keyboardMouseInputText, gamepadInputText);
            else
                SetText(inputAction.negativeDescriptiveName, keyboardMouseInputText, gamepadInputText);
        }
    }

    public void PollPrimaryInput() {
        //if(!cInput.scanning && !string.IsNullOrEmpty(inputNameText.text))
        //	cInput.ChangeKey(inputNameText.text, 1);
    }

    public void PollSecondaryInput() {
        //if(!cInput.scanning && !string.IsNullOrEmpty(inputNameText.text))
        //	cInput.ChangeKey(inputNameText.text, 2);
    }
}