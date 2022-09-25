using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class QuadrablazeTransformSync : NetworkBehaviour {

        [SerializeField]
        float _sendInterval = .04f;

        [SerializeField]
        float _movementThreshold = .001f;

        [SerializeField]
        float _velocityThreshold = .001f;

        [SerializeField]
        float _snapThreshold = 5;

        [SerializeField]
        float _interpolatePosition = 1;

        [SerializeField]
        float _interpolateRotation = 1;

        Vector2 _targetSyncPosition;
        float _targetSyncRotation;
        Vector2 _targetSyncVelocity;

        Vector2 _lastPosition;
        float _lastRotation;
        Vector2 _lastVelocity;

        float _lastSyncTime;
        float _lastSendTime;

        NetworkWriter _syncWriter;

        #region Properties
        public float InterpolatePosition {
            get { return _interpolatePosition; }
            set { _interpolatePosition = value; }
        }

        public float InterpolateRotation {
            get { return _interpolateRotation; }
            set { _interpolateRotation = value; }
        }

        public float LastSendTime {
            get { return _lastSendTime; }
            protected set { _lastSendTime = value; }
        }

        public float LastSyncTime {
            get { return _lastSyncTime; }
            protected set { _lastSyncTime = value; }
        }

        public float MovementThreshold {
            get { return _movementThreshold; }
            set { _movementThreshold = value; }
        }

        Rigidbody RigidbodyComponent { get; set; }

        public float SendInterval {
            get { return _sendInterval; }
            set { _sendInterval = value; }
        }

        public float SnapThreshold {
            get { return _snapThreshold; }
            set { _snapThreshold = value; }
        }

        Vector2 ThisPosition {
            get { return new Vector2(transform.position.x, transform.position.z); }
            set {
                var position = transform.position;

                position.x = value.x;
                position.z = value.y;

                transform.position = position;
            }
        }

        Vector2 ThisRigidbodyPosition {
            get { return new Vector2(RigidbodyComponent.position.x, RigidbodyComponent.position.z); }
            set {
                var position = RigidbodyComponent.position;

                position.x = value.x;
                position.z = value.y;

                RigidbodyComponent.position = position;
            }
        }

        float ThisRigidbodyRotation {
            get { return RigidbodyComponent.rotation.eulerAngles.y; }
            set {
                var rotation = RigidbodyComponent.rotation.eulerAngles;

                rotation.y = value;

                RigidbodyComponent.rotation = Quaternion.Euler(rotation);
            }
        }

        float ThisRotation {
            get { return transform.rotation.eulerAngles.y; }
            set {
                var rotation = transform.rotation.eulerAngles;

                rotation.y = value;

                transform.rotation = Quaternion.Euler(rotation);
            }
        }

        Vector2 ThisVelocity {
            get { return new Vector2(RigidbodyComponent.velocity.x, RigidbodyComponent.velocity.z); }
            set {
                var velocity = RigidbodyComponent.velocity;

                velocity.x = value.x;
                velocity.z = value.y;

                RigidbodyComponent.velocity = velocity;
            }
        }

        public float VelocityThreshold {
            get { return _velocityThreshold; }
            set { _velocityThreshold = value; }
        }
        #endregion

        void Awake() {
            RigidbodyComponent = GetComponent<Rigidbody>();

            _lastPosition = ThisPosition;
            _lastRotation = ThisRotation;
            _lastVelocity = Vector2.zero;

            _syncWriter = new NetworkWriter();
        }

        void Deserialize(NetworkReader reader) {
            DeserializePosition(reader);
            DeserializeRotation(reader);
            DeserializeVelocity(reader);

            var position = new Vector3(_targetSyncPosition.x, RigidbodyComponent.position.y, _targetSyncPosition.y);
            var rotation = Quaternion.Euler(RigidbodyComponent.rotation.eulerAngles.x, _targetSyncRotation, RigidbodyComponent.rotation.eulerAngles.z);
            var velocity = new Vector3(_targetSyncVelocity.x, 0, _targetSyncVelocity.y);

            if(isServer)
                for(int i = 1; i < QuadrablazeSteamNetworking.Current.SteamConnections.Count; i++) {
                    var connection = QuadrablazeSteamNetworking.Current.SteamConnections[i];

                    if(!connection.clientOwnedObjects.Contains(netId)) {
                        var writer = new NetworkWriter();

                        writer.StartMessage(NetMessageType.Client_TransformSync);
                        {
                            writer.Write(netId);
                            Serialize(writer);
                        }
                        writer.FinishMessage();

                        connection.SendWriter(writer, 1);
                    }
                }

            if(isServer && !isClient) { // Only happens on a server without a client?
                RigidbodyComponent.MovePosition(position);
                RigidbodyComponent.MoveRotation(rotation);
                RigidbodyComponent.velocity = velocity;
            }
            else if(GetNetworkSendInterval() != 0) { // Happens on every client (even the host)               
                if((ThisRigidbodyPosition - _targetSyncPosition).magnitude > _snapThreshold) {
                    RigidbodyComponent.position = position;
                    ThisVelocity = _targetSyncVelocity;
                }

                if(_interpolateRotation == 0)
                    RigidbodyComponent.rotation = rotation;

                if(_interpolatePosition == 0)
                    RigidbodyComponent.position = position;

            }
            else {
                RigidbodyComponent.MovePosition(position);
                RigidbodyComponent.MoveRotation(rotation);
                RigidbodyComponent.velocity = velocity;
            }
        }

        void DeserializePosition(NetworkReader reader) {
            _targetSyncPosition = reader.ReadVector2();
        }

        void DeserializeRotation(NetworkReader reader) {
            _targetSyncRotation = reader.ReadSingle();
        }

        void DeserializeVelocity(NetworkReader reader) {
            _targetSyncVelocity = reader.ReadVector2();
        }

        void FixedUpdate() {
            if(isClient)
                FixedUpdateClient();
        }

        void FixedUpdateClient() {
            if(_lastSyncTime != 0)
                if(NetworkServer.active || NetworkClient.active)
                    if(GetNetworkSendInterval() != 0)
                        if(!hasAuthority)
                            Interpolate();
        }

        public override int GetNetworkChannel() {
            return 1;
        }

        public override float GetNetworkSendInterval() {
            return _sendInterval;
        }

        bool HasMoved() {
            if((ThisRigidbodyPosition - _lastPosition).magnitude > 0)
                return true;

            if(Mathf.Abs(ThisRigidbodyRotation - _lastRotation) > 0)
                return true;

            if((ThisVelocity - _lastVelocity).magnitude > 0)
                return true;

            return false;
        }

        void Interpolate() {
            if(_interpolatePosition != 0) {
                var targetVelocity = ((_targetSyncPosition - ThisRigidbodyPosition) * _interpolatePosition) / GetNetworkSendInterval();

                RigidbodyComponent.velocity = new Vector3(targetVelocity.x, 0, targetVelocity.y);
            }

            if(_interpolateRotation != 0)
                RigidbodyComponent.MoveRotation(Quaternion.Slerp(RigidbodyComponent.rotation, Quaternion.Euler(RigidbodyComponent.rotation.eulerAngles.x, _targetSyncRotation, RigidbodyComponent.rotation.eulerAngles.z), Time.fixedDeltaTime * _interpolateRotation));

            _targetSyncPosition += _targetSyncVelocity * Time.fixedDeltaTime * .1f;
        }

        public override void OnStartAuthority() {
            _lastPosition = ThisPosition;
            _lastRotation = ThisRotation;
            _lastVelocity = Vector2.zero;

            SendWriter();

            _lastSyncTime = 0;
        }

        public override void OnStartServer() {
            _lastPosition = ThisPosition;
            _lastRotation = ThisRotation;
            _lastVelocity = Vector2.zero;

            if(!localPlayerAuthority)
                SendWriter();

            _lastSyncTime = 0;
        }

        void Send() {
            if(HasMoved())
                SendWriter();
        }

        void SendWriter() {
            short messageType = NetworkServer.active ? NetMessageType.Client_TransformSync : NetMessageType.Server_TransformSync;

            _syncWriter.StartMessage(messageType);
            {
                _syncWriter.Write(netId);
                Serialize(_syncWriter);
            }
            _syncWriter.FinishMessage();

            if(NetworkServer.active) {
                for(int i = 1; i < QuadrablazeSteamNetworking.Current.SteamConnections.Count; i++) {
                    var connection = QuadrablazeSteamNetworking.Current.SteamConnections[i];

                    if(!connection.clientOwnedObjects.Contains(netId))
                        connection.SendWriter(_syncWriter, 1);
                }
            }
            else if(ClientScene.readyConnection != null) {
                ClientScene.readyConnection.SendWriter(_syncWriter, GetNetworkChannel());
            }
        }

        void Serialize(NetworkWriter writer) {
            SerializePosition(writer);
            SerializeRotation(writer);
            SerializeVelocity(writer);
        }

        void SerializePosition(NetworkWriter writer) {
            var value = ThisRigidbodyPosition;

            writer.Write(value);
            _lastPosition = value;
        }

        void SerializeRotation(NetworkWriter writer) {
            var value = ThisRotation;

            writer.Write(value);
            _lastRotation = value;
        }

        void SerializeVelocity(NetworkWriter writer) {
            var value = ThisVelocity;

            writer.Write(value);
            _lastVelocity = value;
        }

        void Update() {
            if(localPlayerAuthority ? hasAuthority : NetworkServer.active) // If the object can be assigned to a connection, then check for authority. Else, the Server will control the position.
                if(Time.time - _lastSendTime > GetNetworkSendInterval()) {
                    Send();
                    _lastSendTime = Time.time;
                }
        }


        static void Client_HandleTransformSyncMessage(NetworkMessage message) {
            var gameObject = message.reader.ReadGameObject();

            if(gameObject != null) {
                var transformSync = gameObject.GetComponent<QuadrablazeTransformSync>();

                if(transformSync != null) {
                    transformSync.Deserialize(message.reader);
                    transformSync.LastSyncTime = Time.time;
                }
            }
        }

        static void Server_HandleTransformSyncMessage(NetworkMessage message) {
            var gameObject = message.reader.ReadGameObject();

            if(gameObject != null) {
                var transformSync = gameObject.GetComponent<QuadrablazeTransformSync>();

                if(transformSync != null) {
                    //if(!transformSync.hasAuthority) return;
                    //if(!transformSync.localPlayerAuthority) return;
                    if(message.conn.clientOwnedObjects == null) return;
                    if(message.conn.clientOwnedObjects.Contains(transformSync.netId)) {
                        transformSync.Deserialize(message.reader);
                        transformSync.LastSyncTime = Time.time;
                    }
                }
            }
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterServerHandler(NetMessageType.Server_TransformSync, Server_HandleTransformSyncMessage);
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_TransformSync, Client_HandleTransformSyncMessage);
        }
    }
    //public class QuadrablazeTransformSync : NetworkBehaviour {

    //    [SerializeField]
    //    float _sendInterval = .04f;

    //    [SerializeField]
    //    float _movementThreshold = .001f;

    //    [SerializeField]
    //    float _velocityThreshold = .001f;

    //    [SerializeField]
    //    float _snapThreshold = 5;

    //    [SerializeField]
    //    float _interpolatePosition = 1;

    //    [SerializeField]
    //    float _interpolateRotation = 1;

    //    Vector2 _targetSyncPosition;
    //    float _targetSyncRotation;
    //    Vector2 _targetSyncVelocity;

    //    Vector2 _lastPosition;
    //    float _lastRotation;
    //    Vector2 _lastVelocity;

    //    float _lastSyncTime;
    //    float _lastSendTime;

    //    NetworkWriter _syncWriter;

    //    #region Properties
    //    public float InterpolatePosition {
    //        get { return _interpolatePosition; }
    //        set { _interpolatePosition = value; }
    //    }

    //    public float InterpolateRotation {
    //        get { return _interpolateRotation; }
    //        set { _interpolateRotation = value; }
    //    }

    //    public float LastSendTime {
    //        get { return _lastSendTime; }
    //        protected set { _lastSendTime = value; }
    //    }

    //    public float LastSyncTime {
    //        get { return _lastSyncTime; }
    //        protected set { _lastSyncTime = value; }
    //    }

    //    public float MovementThreshold {
    //        get { return _movementThreshold; }
    //        set { _movementThreshold = value; }
    //    }

    //    Rigidbody RigidbodyComponent { get; set; }

    //    public float SendInterval {
    //        get { return _sendInterval; }
    //        set { _sendInterval = value; }
    //    }

    //    public float SnapThreshold {
    //        get { return _snapThreshold; }
    //        set { _snapThreshold = value; }
    //    }

    //    Vector2 ThisPosition {
    //        get { return new Vector2(transform.position.x, transform.position.z); }
    //        set {
    //            var position = transform.position;

    //            position.x = value.x;
    //            position.z = value.y;

    //            transform.position = position;
    //        }
    //    }

    //    Vector2 ThisRigidbodyPosition {
    //        get { return new Vector2(RigidbodyComponent.position.x, RigidbodyComponent.position.z); }
    //        set {
    //            var position = RigidbodyComponent.position;

    //            position.x = value.x;
    //            position.z = value.y;

    //            RigidbodyComponent.position = position;
    //        }
    //    }

    //    float ThisRigidbodyRotation {
    //        get { return RigidbodyComponent.rotation.eulerAngles.y; }
    //        set {
    //            var rotation = RigidbodyComponent.rotation.eulerAngles;

    //            rotation.y = value;

    //            RigidbodyComponent.rotation = Quaternion.Euler(rotation);
    //        }
    //    }

    //    float ThisRotation {
    //        get { return transform.rotation.eulerAngles.y; }
    //        set {
    //            var rotation = transform.rotation.eulerAngles;

    //            rotation.y = value;

    //            transform.rotation = Quaternion.Euler(rotation);
    //        }
    //    }

    //    Vector2 ThisVelocity {
    //        get { return new Vector2(RigidbodyComponent.velocity.x, RigidbodyComponent.velocity.z); }
    //        set {
    //            var velocity = RigidbodyComponent.velocity;

    //            velocity.x = value.x;
    //            velocity.z = value.y;

    //            RigidbodyComponent.velocity = velocity;
    //        }
    //    }

    //    public float VelocityThreshold {
    //        get { return _velocityThreshold; }
    //        set { _velocityThreshold = value; }
    //    }
    //    #endregion

    //    void Awake() {
    //        RigidbodyComponent = GetComponent<Rigidbody>();

    //        _lastPosition = ThisPosition;
    //        _lastRotation = ThisRotation;
    //        _lastVelocity = Vector2.zero;

    //        //if(localPlayerAuthority)
    //        _syncWriter = new NetworkWriter();
    //    }

    //    void Deserialize(NetworkReader reader) {
    //        DeserializePosition(reader);
    //        DeserializeRotation(reader);
    //        DeserializeVelocity(reader);

    //        var position = new Vector3(_targetSyncPosition.x, RigidbodyComponent.position.y, _targetSyncPosition.y);
    //        var rotation = Quaternion.Euler(RigidbodyComponent.rotation.eulerAngles.x, _targetSyncRotation, RigidbodyComponent.rotation.eulerAngles.z);
    //        var velocity = new Vector3(_targetSyncVelocity.x, 0, _targetSyncVelocity.y);

    //        if(isServer && !isClient) { // Only happens on a server without a client?
    //            RigidbodyComponent.MovePosition(position);
    //            RigidbodyComponent.MoveRotation(rotation);
    //            RigidbodyComponent.velocity = velocity;
    //        }
    //        else if(GetNetworkSendInterval() != 0) { // Happens on every client (even the host)
    //            if((ThisRigidbodyPosition - _targetSyncPosition).magnitude > _snapThreshold) {
    //                RigidbodyComponent.position = position;
    //                ThisVelocity = _targetSyncVelocity;
    //            }

    //            if(_interpolateRotation == 0)
    //                RigidbodyComponent.rotation = rotation;

    //            if(_interpolatePosition == 0)
    //                RigidbodyComponent.position = position;
    //        }
    //        else {
    //            RigidbodyComponent.MovePosition(position);
    //            RigidbodyComponent.MoveRotation(rotation);
    //            RigidbodyComponent.velocity = velocity;
    //        }
    //    }

    //    void DeserializePosition(NetworkReader reader) {
    //        _targetSyncPosition = reader.ReadVector2();
    //    }

    //    void DeserializeRotation(NetworkReader reader) {
    //        _targetSyncRotation = reader.ReadSingle();
    //    }

    //    void DeserializeVelocity(NetworkReader reader) {
    //        _targetSyncVelocity = reader.ReadVector2();
    //    }

    //    void FixedUpdate() {
    //        if(isServer)
    //            FixedUpdateServer();

    //        if(isClient)
    //            FixedUpdateClient();
    //    }

    //    void FixedUpdateClient() {
    //        if(_lastSyncTime != 0)
    //            if(NetworkServer.active || NetworkClient.active)
    //                if(GetNetworkSendInterval() != 0)
    //                    if(!hasAuthority)
    //                        Interpolate();
    //    }

    //    void FixedUpdateServer() {
    //        if(syncVarDirtyBits == 0)
    //            if(NetworkServer.active)
    //                if(GetNetworkSendInterval() != 0) {
    //                    if((ThisPosition - _lastPosition).magnitude < _movementThreshold)
    //                        if(Mathf.Abs(ThisRotation - _lastRotation) < _movementThreshold)
    //                            if((ThisVelocity - _lastVelocity).magnitude < _velocityThreshold)
    //                                return;

    //                    SetDirtyBit(1);
    //                }
    //    }

    //    public override int GetNetworkChannel() {
    //        return 1;
    //    }

    //    public override float GetNetworkSendInterval() {
    //        return _sendInterval;
    //    }

    //    bool HasMoved() {
    //        if((ThisRigidbodyPosition - _lastPosition).magnitude > 0)
    //            return true;

    //        if(Mathf.Abs(ThisRigidbodyRotation - _lastRotation) > 0)
    //            return true;

    //        if((ThisVelocity - _lastVelocity).magnitude > 0)
    //            return true;

    //        return false;
    //    }

    //    void Interpolate() {
    //        if(_interpolatePosition != 0) {
    //            var targetVelocity = ((_targetSyncPosition - ThisRigidbodyPosition) * _interpolatePosition) / GetNetworkSendInterval();

    //            RigidbodyComponent.velocity = new Vector3(targetVelocity.x, 0, targetVelocity.y);
    //        }

    //        if(_interpolateRotation != 0)
    //            RigidbodyComponent.MoveRotation(Quaternion.Slerp(RigidbodyComponent.rotation, Quaternion.Euler(RigidbodyComponent.rotation.eulerAngles.x, _targetSyncRotation, RigidbodyComponent.rotation.eulerAngles.z), Time.fixedDeltaTime * _interpolateRotation));

    //        _targetSyncPosition += _targetSyncVelocity * Time.fixedDeltaTime * .1f;
    //    }

    //    public override void OnDeserialize(NetworkReader reader, bool initialState) {
    //        if(!isServer || !NetworkServer.localClientActive) {
    //            if(!initialState)
    //                if(reader.ReadPackedUInt32() == 0)
    //                    return;

    //            Deserialize(reader);

    //            _lastSyncTime = Time.time;
    //        }
    //    }

    //    public override bool OnSerialize(NetworkWriter writer, bool initialState) {
    //        if(!initialState) {
    //            if(syncVarDirtyBits == 0) {
    //                writer.WritePackedUInt32(0);

    //                return false;
    //            }

    //            writer.WritePackedUInt32(1);
    //        }

    //        Serialize(writer);

    //        return true;
    //    }

    //    public override void OnStartAuthority() {
    //        _lastSyncTime = 0;
    //    }

    //    public override void OnStartServer() {
    //        _lastSyncTime = 0;
    //    }

    //    [Client]
    //    void Send() {
    //        if(HasMoved() && ClientScene.readyConnection != null) {
    //            _syncWriter.StartMessage(NetMessageType.Network_TransformSync);
    //            {
    //                _syncWriter.Write(netId);
    //                Serialize(_syncWriter);
    //            }
    //            _syncWriter.FinishMessage();

    //            ClientScene.readyConnection.SendWriter(_syncWriter, GetNetworkChannel());
    //        }
    //    }

    //    void Serialize(NetworkWriter writer) {
    //        SerializePosition(writer);
    //        SerializeRotation(writer);
    //        SerializeVelocity(writer);
    //    }

    //    void SerializePosition(NetworkWriter writer) {
    //        var value = ThisRigidbodyPosition;

    //        writer.Write(value);
    //        _lastPosition = value;
    //    }

    //    void SerializeRotation(NetworkWriter writer) {
    //        var value = ThisRotation;

    //        writer.Write(value);
    //        _lastRotation = value;
    //    }

    //    void SerializeVelocity(NetworkWriter writer) {
    //        var value = ThisVelocity;

    //        writer.Write(value);
    //        _lastVelocity = value;
    //    }

    //    void Update() {
    //        if(hasAuthority)
    //            //if(localPlayerAuthority)
    //            if(!NetworkServer.active)
    //                if(Time.time - _lastSendTime > GetNetworkSendInterval()) {
    //                    Send();
    //                    _lastSendTime = Time.time;
    //                }
    //    }

    //    static void HandleTransformSyncMessage(NetworkMessage message) {
    //        var gameObject = message.reader.ReadGameObject();

    //        if(gameObject != null) {
    //            var transformSync = gameObject.GetComponent<QuadrablazeTransformSync>();

    //            if(transformSync != null) {
    //                if(!transformSync.hasAuthority) return;
    //                //if(!transformSync.localPlayerAuthority) return;
    //                if(message.conn.clientOwnedObjects == null) return;
    //                if(message.conn.clientOwnedObjects.Contains(transformSync.netId)) {
    //                    transformSync.Deserialize(message.reader);
    //                    transformSync.LastSyncTime = Time.time;
    //                }
    //            }
    //        }
    //    }

    //    public static void RegisterHandlers() {
    //        QuadrablazeSteamNetworking.RegisterServerHandler(NetMessageType.Network_TransformSync, HandleTransformSyncMessage);
    //    }
    //}
}