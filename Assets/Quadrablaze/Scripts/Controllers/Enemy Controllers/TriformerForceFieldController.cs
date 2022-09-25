using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class TriformerForceFieldController : MonoBehaviour {

        public static TriformerForceFieldController Current { get; private set; }

        [SerializeField]
        ResizableForceField forceField1;

        [SerializeField]
        ResizableForceField forceField2;

        [SerializeField]
        ResizableForceField forceField3;

        public Transform Hook1 { get; set; }
        public Transform Hook2 { get; set; }
        public Transform Hook3 { get; set; }

        void Awake() {
            Current = this;

            EnableForceField(false);
        }

        public void EnableForceField(bool enable) {
            if(NetworkServer.active) {
                TriformerForceFieldMessage message;

                message = enable ?
                    new TriformerForceFieldMessage(enable, Hook1.gameObject, Hook2.gameObject, Hook3.gameObject) :
                    new TriformerForceFieldMessage(enable);

                NetworkServer.SendToAll(NetMessageType.Client_SetTriformerForceField, message);
            }
            else {
                forceField1.EnableForceField(enable);
                forceField2.EnableForceField(enable);
                forceField3.EnableForceField(enable);
            }
        }

        void LateUpdate() {
            if(Hook1 && Hook2 && Hook3)
                UpdateHookPositions();
        }

        void UpdateHookPositions() {
            forceField3.Hook2Position = forceField1.Hook1Position = Hook1.position;
            forceField1.Hook2Position = forceField2.Hook1Position = Hook2.position;
            forceField2.Hook2Position = forceField3.Hook1Position = Hook3.position;
        }

        public static void SetForceField(NetworkMessage netMessage) {
            var message = netMessage.ReadMessage<TriformerForceFieldMessage>();

            Current.Hook1 = message.enable ? message.triformerA?.transform : null;
            Current.Hook2 = message.enable ? message.triformerB?.transform : null;
            Current.Hook3 = message.enable ? message.triformerC?.transform : null;

            Current.forceField1.EnableForceField(message.enable);
            Current.forceField2.EnableForceField(message.enable);
            Current.forceField3.EnableForceField(message.enable);
        }
    }

    [Serializable]
    public class TriformerForceFieldMessage : MessageBase {
        public bool enable;
        public GameObject triformerA;
        public GameObject triformerB;
        public GameObject triformerC;

        public TriformerForceFieldMessage() {
            enable = false;
            triformerA = null;
            triformerB = null;
            triformerC = null;
        }
        public TriformerForceFieldMessage(bool enable) : this(enable, null, null, null) { }
        public TriformerForceFieldMessage(bool enable, GameObject a, GameObject b, GameObject c) {
            this.enable = enable;
            triformerA = a;
            triformerB = b;
            triformerC = c;
        }
    }
}