using UnityEngine;
using System.Collections;

namespace YounGenTech {
	[AddComponentMenu("YounGen Tech/Scripts/Input/Input Event")]
	public class InputEvent : MonoBehaviour {

		public string axisName = "Fire1";
		public string messageToSend = "";
		public GameObject objectToSendTo;

		public ButtonPressType buttonPressType = ButtonPressType.Hold;
		public SendMessageType messageType = SendMessageType.SendMessage;

		void Update() {
			if(messageToSend != "" && axisName != "") {
				if(objectToSendTo == null) objectToSendTo = gameObject;

				bool onButton = false;

				switch(buttonPressType) {
					case ButtonPressType.Hold: onButton = Input.GetButton(axisName); break;
					case ButtonPressType.Tap: onButton = Input.GetButtonDown(axisName); break;
					case ButtonPressType.Release: onButton = Input.GetButtonUp(axisName); break;
				}

				if(onButton) {
					switch(messageType) {
						case SendMessageType.SendMessage: objectToSendTo.SendMessage(messageToSend, SendMessageOptions.DontRequireReceiver); break;
						case SendMessageType.SendMessageUpwards: objectToSendTo.SendMessageUpwards(messageToSend, SendMessageOptions.DontRequireReceiver); break;
						case SendMessageType.BroadcastMessage: objectToSendTo.BroadcastMessage(messageToSend, SendMessageOptions.DontRequireReceiver); break;
					}
				}
			}
		}

		public enum ButtonPressType {
			Hold, Tap, Release
		}

		public enum SendMessageType {
			SendMessage, SendMessageUpwards, BroadcastMessage
		}
	}
}