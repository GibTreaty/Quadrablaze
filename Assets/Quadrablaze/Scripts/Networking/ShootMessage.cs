using Quadrablaze;
using UnityEngine;
using UnityEngine.Networking;

public class ShootEnableMessage : EntityMessageBase {
    public bool shootFlag;
    public Vector3 shootDirection;
    public ShootEnableMessage() { }
    public ShootEnableMessage(NetworkMessageDelegate method, uint id, bool shootFlag, Vector3 shootDirection) : base(method, id) {
        this.shootFlag = shootFlag;
        this.shootDirection = shootDirection;
    }

    public override void Deserialize(NetworkReader reader) {
        base.Deserialize(reader);

        shootFlag = reader.ReadBoolean();
        shootDirection = reader.ReadVector3();
    }

    public override void Serialize(NetworkWriter writer) {
        base.Serialize(writer);

        writer.Write(shootFlag);
        writer.Write(shootDirection);
    }
}