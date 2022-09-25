using System;
using System.Collections;
using System.Collections.Generic;
using Quadrablaze;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Use RegisterClientHandlers() in NetworkManager.OnClientConnect
/// Use RegisterServerHandlers() in NetworkManager.OnServerStart
/// 
/// NetworkDownloadHandler.SendData() to send data (either as client to server or server to client)
/// </summary>
public class NetworkDownloadHandler : MonoBehaviour {

    const short ServerReceiver = 10000;
    const short ClientReceiver = 10001;
    const short ClientNotifier = 10002;
    const short ServerNotifier = 10003;
    static int defaultBufferSize = 1024; //max ethernet MTU is ~1400

    static NetworkDownloadHandler _current = null;

    static List<Guid> transmissionIds = new List<Guid>();
    static Dictionary<Guid, Data> transmissionData = new Dictionary<Guid, Data>();

    public static event Action<Guid, Data> OnCompleteReceive;
    public static event Action<Guid, Data> OnCompleteSend;
    public static event Action<Guid, Data> OnPartialReceive;
    public static event Action<Guid, Data> OnPartialSend;

    #region Properties
    public static int TransmissionIDCount {
        get { return transmissionIds.Count; }
    }

    static NetworkDownloadHandler Current {
        get {
            if(_current == null) CreateNetworkDownloadHandlerObject();

            return _current;
        }
    }
    #endregion

    public static void ClearDownloadedData() {
        transmissionIds.Clear();
        transmissionData.Clear();
    }
    public static void ClearDownloadedData(Func<KeyValuePair<Guid, Data>, bool> requirements) {
        List<Guid> clearData = new List<Guid>();

        foreach(var pair in transmissionData)
            if(requirements(pair))
                clearData.Add(pair.Key);

        foreach(var clear in clearData) {
            transmissionIds.Remove(clear);
            transmissionData.Remove(clear);
        }
    }

    IEnumerator ClientSendDataRoutine(Guid id, byte[] data, Action<Data> onCompleteSend) {
        while(transmissionIds.Count > 0)
            yield return null;

        NotifyServerOfDataBeingSent(id, data.Length);

        yield return null;

        transmissionIds.Add(id);

        var transmitData = new Data(data);
        var bufferSize = defaultBufferSize;

        while(transmitData.currentIndex < transmitData.data.Length - 1) {
            if(!NetworkClient.active || ClientScene.readyConnection == null) break;

            var remaining = transmitData.data.Length - transmitData.currentIndex;

            if(remaining < bufferSize)
                bufferSize = remaining;

            var bufferedData = new byte[bufferSize];

            Array.Copy(transmitData.data, transmitData.currentIndex, bufferedData, 0, bufferSize);

            SendDataToServer(id, bufferedData);
            transmitData.currentIndex += bufferSize;
            transmitData.LastTransmissionTime = Time.realtimeSinceStartup;

            yield return null;

            OnPartialSend?.Invoke(id, transmitData);
        }

        transmissionIds.Remove(id);

        onCompleteSend?.Invoke(transmitData);
        OnCompleteSend?.Invoke(id, transmitData);
    }

    public static Guid ClientSendData(byte[] data, Action<Data> onCompleteSend) {
        var id = Guid.NewGuid();

        ClientSendData(id, data, onCompleteSend);

        return id;
    }
    public static void ClientSendData(Guid id, byte[] data, Action<Data> onCompleteSend) {
        Current.StartCoroutine(Current.ClientSendDataRoutine(id, data, onCompleteSend));
    }

    static void CreateNetworkDownloadHandlerObject() {
        var gameObject = new GameObject("Network Download Handler", typeof(NetworkDownloadHandler));

        _current = gameObject.GetComponent<NetworkDownloadHandler>();
    }

    NetworkWriter GetDataWriter(short messageType, Guid id, byte[] data) {
        var writer = new NetworkWriter();

        writer.StartMessage(messageType);
        writer.WriteBytesFull(id.ToByteArray());
        writer.WriteBytesFull(data);
        writer.FinishMessage();

        return writer;
    }

    public static Data GetStoredData(Guid id) {
        Data data;

        if(transmissionData.TryGetValue(id, out data))
            return data;

        return null;
    }

    public static Guid GetTransmissionID(int index) {
        return transmissionIds[index];
    }

    void NotifyClientsOfDataBeingSent(Guid id, int dataLength, IEnumerable<NetworkConnection> sendToConnections) {
        if(!NetworkServer.active || sendToConnections == null) return;

        var writer = new NetworkWriter();

        writer.StartMessage(ClientNotifier);
        writer.WriteBytesFull(id.ToByteArray());
        writer.Write(dataLength);
        writer.FinishMessage();

        foreach(var connection in sendToConnections)
            if(connection.hostId > -1)
                connection.SendWriter(writer, Channels.DefaultReliable);
    }

    void NotifyServerOfDataBeingSent(Guid id, int dataLength) {
        if(!NetworkClient.active || ClientScene.readyConnection == null) return;

        var writer = new NetworkWriter();

        writer.StartMessage(ServerNotifier);
        writer.WriteBytesFull(id.ToByteArray());
        writer.Write(dataLength);
        writer.FinishMessage();

        ClientScene.readyConnection.SendWriter(writer, Channels.DefaultReliable);
    }

    static void ReceiveData(NetworkMessage netMsg) {
        var id = new Guid(netMsg.reader.ReadBytesAndSize());
        var bufferedData = netMsg.reader.ReadBytesAndSize();

        if(!transmissionData.ContainsKey(id))
            return;

        var storedData = transmissionData[id];
        Array.Copy(bufferedData, 0, storedData.data, storedData.currentIndex, bufferedData.Length);

        storedData.currentIndex += bufferedData.Length;
        storedData.LastTransmissionTime = Time.realtimeSinceStartup;

        storedData.OnPartialReceive?.Invoke();
        OnPartialReceive?.Invoke(id, storedData);

        if(storedData.currentIndex < storedData.data.Length - 1)
            return;

        transmissionData.Remove(id);

        storedData.OnCompleteReceive?.Invoke();
        OnCompleteReceive?.Invoke(id, storedData);
    }

    public static void RegisterClientHandlers() {
        QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(ClientReceiver, ReceiveData);
        QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(ClientNotifier, TransmissionNotifier);
    }

    public static void RegisterServerHandlers() {
        NetworkServer.RegisterHandler(ServerReceiver, ReceiveData);
        NetworkServer.RegisterHandler(ServerNotifier, TransmissionNotifier);
    }

    public static Guid SendData(byte[] data, Action<Data> onCompleteSend = null, IEnumerable<NetworkConnection> sendToConnections = null) {
        var id = Guid.NewGuid();

        SendData(id, data, onCompleteSend, sendToConnections);

        return id;
    }
    public static void SendData(Guid id, byte[] data, Action<Data> onCompleteSend = null, IEnumerable<NetworkConnection> sendToConnections = null) {
        if(NetworkServer.active) {
            ServerSendData(id, data, onCompleteSend, sendToConnections);
        }
        else if(NetworkClient.active) {
            ClientSendData(id, data, onCompleteSend);
        }
    }

    void SendDataToClient(Guid id, byte[] data, IEnumerable<NetworkConnection> sendToConnections) {
        if(sendToConnections == null) return;

        var writer = GetDataWriter(ClientReceiver, id, data);

        foreach(var connection in sendToConnections)
            if(connection.hostId > -1)
                connection.SendWriter(writer, 2);
    }

    void SendDataToServer(Guid id, byte[] data) {
        var writer = GetDataWriter(ServerReceiver, id, data);

        if(ClientScene.readyConnection != null)
            ClientScene.readyConnection.SendWriter(writer, 2);
    }

    public static Guid ServerSendData(byte[] data, Action<Data> onCompleteSend, IEnumerable<NetworkConnection> sendToConnections) {
        var id = Guid.NewGuid();

        ServerSendData(id, data, onCompleteSend, sendToConnections);

        return id;
    }
    public static void ServerSendData(Guid id, byte[] data, Action<Data> onCompleteSend, IEnumerable<NetworkConnection> sendToConnections) {
        Current.StartCoroutine(Current.ServerSendDataRoutine(id, data, onCompleteSend, sendToConnections));
    }

    IEnumerator ServerSendDataRoutine(Guid id, byte[] data, Action<Data> onCompleteSend, IEnumerable<NetworkConnection> sendToConnections) {
        if(transmissionIds.Count > 0) {
            while(transmissionIds.Count > 0)
                yield return null;

            yield return new WaitForSeconds(1);
        }

        NotifyClientsOfDataBeingSent(id, data.Length, sendToConnections);

        yield return null;

        transmissionIds.Add(id);

        var transmitData = new Data(data);
        var bufferSize = defaultBufferSize;

        while(transmitData.currentIndex < transmitData.data.Length - 1) {
            if(!NetworkServer.active) break;

            bool atLeastOneConnection = false;

            foreach(var connection in sendToConnections)
                if(connection != null && connection.hostId > -1) {
                    atLeastOneConnection = true;
                    break;
                }

            if(!atLeastOneConnection)
                break;

            var remaining = transmitData.data.Length - transmitData.currentIndex;

            if(remaining < bufferSize)
                bufferSize = remaining;

            var bufferedData = new byte[bufferSize];

            Array.Copy(transmitData.data, transmitData.currentIndex, bufferedData, 0, bufferSize);

            SendDataToClient(id, bufferedData, sendToConnections);
            transmitData.currentIndex += bufferSize;
            transmitData.LastTransmissionTime = Time.realtimeSinceStartup;

            yield return null;

            OnPartialSend?.Invoke(id, transmitData);
        }

        transmissionIds.Remove(id);

        onCompleteSend?.Invoke(transmitData);
        OnCompleteSend?.Invoke(id, transmitData);
    }

    static void TransmissionNotifier(NetworkMessage netMsg) {
        var id = new Guid(netMsg.reader.ReadBytesAndSize());
        var size = netMsg.reader.ReadInt32();

        Debug.Log("TransmissionNotifier id:" + netMsg.conn.connectionId + "\nguid" + id.ToString());

        if(transmissionData.ContainsKey(id))
            return;

        transmissionData[id] = new Data(new byte[size]);
    }

    public class Data {
        public int currentIndex;
        public byte[] data;

        public Action OnPartialReceive;
        public Action OnCompleteReceive;

        #region Properties
        /// <summary>Time since starting the download</summary>
        public float ElapsedTime {
            get { return Time.realtimeSinceStartup - TimeStarted; }
        }

        /// <summary>Last time data was sent or received</summary>
        public float LastTransmissionTime { get; set; }

        /// <summary>Time that the data was started being sent or received</summary>
        public float TimeStarted { get; private set; }

        /// <summary>Time it took to finish downloading or uploading the data</summary>
        public float TotalTransmissionTime {
            get { return LastTransmissionTime - TimeStarted; }
        }
        #endregion

        public Data(byte[] data) {
            currentIndex = 0;
            this.data = data;

            TimeStarted = Time.realtimeSinceStartup;
        }
    }
}