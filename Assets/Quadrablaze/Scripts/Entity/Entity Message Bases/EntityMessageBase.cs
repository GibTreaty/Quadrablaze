using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class EntityMessageBase : MessageBase {

        public string messageKey;
        public uint id;

        public EntityMessageBase() { }
        public EntityMessageBase(string messageKey, uint id) {
            this.messageKey = messageKey;
            this.id = id;
        }
        public EntityMessageBase(NetworkMessageDelegate method, uint id) : this(QuadrablazeSteamNetworking.GenerateNetworkMessageKey(method), id) { }

        public override void Deserialize(NetworkReader reader) {
            messageKey = reader.ReadString();
            id = reader.ReadUInt32();
        }

        public override void Serialize(NetworkWriter writer) {
            writer.Write(messageKey);
            writer.Write(id);
        }
    }
}