using UnityEngine;
using UnityEngine.Networking;

public class BooleanMessage : MessageBase {

    public bool value;

    public override void Deserialize(NetworkReader reader) {
        value = reader.ReadBoolean();
    }

    public override void Serialize(NetworkWriter writer) {
        writer.Write(value);
    }
}