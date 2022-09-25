using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public static class TelegraphStateHandler {
        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_SetTelegraphState, SetTelegraphState);
        }

        public static void SendTelegraphState(GameObject gameObject, bool enable, byte extraData = 0) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_SetTelegraphState);
            writer.Write(gameObject);
            writer.Write(enable);
            writer.Write(extraData);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
        }

        static void SetTelegraphState(NetworkMessage message) {
            var reader = message.reader;

            var gameObject = reader.ReadGameObject();

            if(gameObject) {
                var telegraphHandlers = gameObject.GetComponents<ITelegraphHandler>();
                var state = reader.ReadBoolean();
                var extraData = reader.ReadByte();

                foreach(var telegraphHandler in telegraphHandlers)
                    telegraphHandler.SetTelegraphState(state, extraData);
            }
        }
    }
}