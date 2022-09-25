using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Effect Database/Effects/Networked Effect")]
    public class NetworkedEffect : EffectBase {

        static void DoEffect(NetworkMessage message) {
            string id = message.reader.ReadString();

            bool parentFlag = message.reader.ReadBoolean();
            Transform parent = null;

            var parameters = new string[0];

            if(parentFlag) {
                var netId = message.reader.ReadNetworkIdentity();

                if(netId != null)
                    parent = netId.transform;

                var parameterCount = message.reader.ReadInt32();
                parameters = new string[parameterCount];

                for(int i = 0; i < parameterCount; i++)
                    parameters[i] = message.reader.ReadString();
            }

           var output = EffectManager.Current.Effects.Play(id, parent, null, parameters);

            if(output != null) {
                var effect = output.GetComponent<NetworkedEffectBehaviour>();

                if(effect != null)
                    effect.OnPlayEffect(parameters);
            }
        }

        public override GameObject Play(string id, Transform parent, Action<object, string[]> callback = null, params string[] parameters) {
            if(NetworkServer.active) {
                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_EffectManagerInvoke);
                writer.Write(id);

                NetworkIdentity netId = null;
                bool parentFlag = false;

                if(parent != null) {
                    netId = parent.GetComponent<NetworkIdentity>();
                    parentFlag = netId != null;
                }

                writer.Write(parentFlag);

                if(parentFlag)
                    writer.Write(netId);

                writer.Write(parameters.Length);

                for(int i = 0; i < parameters.Length; i++)
                    writer.Write(parameters[i]);

                writer.FinishMessage();

                QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
            }

            var output = CreateObject(parent);

            if(output != null) {
                var effect = output.GetComponent<NetworkedEffectBehaviour>();

                if(effect != null)
                    effect.OnPlayEffect(parameters);

                callback?.Invoke(output, parameters);
            }

            return output;
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_EffectManagerInvoke, DoEffect);
        }
    }
}