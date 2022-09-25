using Quadrablaze.Entities;
using Quadrablaze.Skills;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using YounGenTech.PoolGen;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class QuadrablazeSteamNetworking : MonoBehaviour {

        public const string GAME_ID = "quadrablaze-661050";
        public const ELobbyType DefaultLobbyType = ELobbyType.k_ELobbyTypePrivate;

        public const string LobbyData_Title = "lobbyTitle";
        public const string LobbyData_Status = "lobbyStatus";
        public const string LobbyData_Version = "gameVersion";
        public const string LobbyData_GameMode = "gameMode";

        public static QuadrablazeSteamNetworking Current { get; private set; }

        public static event Action<NetworkClient> OnClientStarted;
        public static Dictionary<short, NetworkMessageDelegate> OnClientRegisterHandler;
        public static Dictionary<short, NetworkMessageDelegate> OnServerRegisterHandler;
        public static Dictionary<short, HashSet<NetworkMessageDelegate>> OnClientRegisterEventHandler = new Dictionary<short, HashSet<NetworkMessageDelegate>>();
        public static Dictionary<short, HashSet<NetworkMessageDelegate>> OnServerRegisterEventHandler = new Dictionary<short, HashSet<NetworkMessageDelegate>>();
        internal static Dictionary<short, Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader>> downloadHandlers = new Dictionary<short, Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader>>();
        internal static Dictionary<short, Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader>> downloadPartialHandlers = new Dictionary<short, Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader>>();

        static List<Type> TypeLookup;

        /// <summary>Key: Type, Value: List<MethodInfo></summary>
        static Dictionary<ushort, List<MethodInfo>> MethodLookup;

        public static Dictionary<(Type typeName, ushort id), (MethodInfo methodInfo, ushort id)> TypeMethodLookup;
        public static Dictionary<string, (MethodInfo methodInfo, NetworkMessageDelegate staticMethod, Dictionary<object, NetworkMessageDelegate> listeners)> GameNetworkListeners;
        public static Dictionary<string, MethodInfo> EntityNetworkListeners;

        [SerializeField]
        GameObject testPrefab;

        [SerializeField]
        GameObject playerPrefab;

        [SerializeField]
        List<GameObject> networkedPrefabs = new List<GameObject>();

        [SerializeField]
        GameParty _currentParty;

        [SerializeField]
        float _joinPingFrequency = 1;

        float lastJoinPing;

        #region Properties
        public List<ChannelQOS> Channels {
            get { return GetHostTopology().DefaultConfig.Channels; }
        }

        public GameParty CurrentParty {
            get { return _currentParty; }
            protected set { _currentParty = value; }
        }

        public bool IsFindingQuickMatch {
            get { return isFindingQuickMatch; }
            private set { isFindingQuickMatch = value; }
        }

        public NetworkClient MyClient {
            get { return _myClient; }
            private set { _myClient = value; }
        }

        public NetworkIdentity MyNetworkIdentity { get; set; }

        public GamePlayerInfo MyPlayerInfo { get; set; }

        public UnityEvent OnConnect {
            get { return _onConnect; }
            private set { _onConnect = value; }
        }

        public HashSet<GameParty.Player> Players {
            get { return CurrentParty.Players; }
        }

        public CSteamID SteamLobbyId {
            get { return _steamLobbyId; }
            set { _steamLobbyId = value; }
        }

        public List<NetworkConnection> SteamConnections {
            get { return _steamConnections; }
            private set { _steamConnections = value; }
        }

        //public bool WasHelpMenuShown { get; set; }
        #endregion

        Callback<P2PSessionRequest_t> _steamInviteCallback;
        Callback<LobbyCreated_t> _lobbyCreated;
        Callback<LobbyEnter_t> _lobbyEntered;
        Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;
        Callback<LobbyChatUpdate_t> _lobbyChatUpdate;
        Callback<P2PSessionRequest_t> _P2PSessionRequested;
        Callback<P2PSessionConnectFail_t> _P2PSessionConnectFail;
        CallResult<LobbyMatchList_t> _lobbyMatchList;
        Callback<LobbyDataUpdate_t> _lobbyDataUpdate;
        Callback<LobbyChatMsg_t> _lobbyChatMessage;

        CSteamID _steamLobbyId;
        CSteamID _hostId;
        NetworkClient _myClient;
        HostTopology _hostTopology;
        List<NetworkConnection> _steamConnections = new List<NetworkConnection>();
        UnityEvent _onConnect;

        Coroutine connectingRoutine = null;
        bool hasRequestedLobbyData = false;
        bool isFindingQuickMatch = false;

        public void AddConnection(NetworkConnection connection) {
            SteamConnections.Add(connection);
        }

        void ConnectToUnetServerForSteam(CSteamID hostSteamId) {
            Debug.Log("Connecting to UNET server");

            // Create connection to host player's steam ID
            var conn = new SteamNetworkConnection(hostSteamId);
            var mySteamClient = new SteamNetworkClient(conn);

            MyClient = mySteamClient;

            mySteamClient.RegisterHandler(MsgType.Connect, OnSteamConnect);
            mySteamClient.SetNetworkConnectionClass<SteamNetworkConnection>();
            mySteamClient.Configure(GetHostTopology());
            mySteamClient.Connect();
        }

        public void CreateLobby() {
            SteamMatchmaking.CreateLobby(DefaultLobbyType, 4);
        }

        public void CreateP2PConnectionWithPeer(CSteamID peer) {
            Debug.Log("Sending P2P acceptance message and creating remote client reference for UNET server");
            SteamNetworking.SendP2PPacket(peer, null, 0, EP2PSend.k_EP2PSendReliable);

            // create new connnection for this client and connect them to server
            var newConnection = new SteamNetworkConnection(peer);

            newConnection.ForceInitialize(GetHostTopology());
            NetworkServer.AddExternalConnection(newConnection);
            AddConnection(newConnection);

            var player = CurrentParty.AddPlayer(MyClient.connection, newConnection, false, (ulong)peer, SteamFriends.GetFriendPersonaName(peer));
            //GameLobbyManager.Current.LobbyUI.AddPlayer(player, newConnection);
        }

        IEnumerator DelayedShipSend(NetworkConnection connection, GameParty.Player player) {
            yield return new WaitForEndOfFrame();

            if(Players.Count <= 1) yield break;

            Debug.Log("DelayedShipSend sending ships to id:" + connection.connectionId);

            //if(connection.connectionId == 2) {
            //    GetPlayer(0).playerInfo.ShareShip.SendData(new NetworkConnection[] { connection });
            //}
            //else {
            foreach(var p in Players)
                if(p != player)
                    if(p.playerInfo.IsShipDownloaded) {
                        yield return null;

                        while(NetworkDownloadHandler.TransmissionIDCount > 0)
                            yield return null;

                        p.playerInfo.ShareShip.SendData(new NetworkConnection[] { connection });
                    }
            //}
        }

        public void DestroyPlayer(NetworkConnection connection) {
            if(connection == null) return;

            var items = FindObjectsOfType<NetworkIdentity>();

            for(int i = 0; i < items.Length; i++)
                if(items[i].clientAuthorityOwner != null && items[i].clientAuthorityOwner.connectionId == connection.connectionId)
                    NetworkServer.Destroy(items[i].gameObject);
        }

        public NetworkConnection GetConnection(CSteamID steamId) {
            if(steamId.m_SteamID == SteamUser.GetSteamID().m_SteamID)
                if(NetworkServer.active && NetworkServer.connections.Count > 0)
                    return NetworkServer.connections[0];

            for(int i = 0; i < SteamConnections.Count; i++) {
                var steamConnection = SteamConnections[i] as SteamNetworkConnection;

                if(steamConnection != null && steamConnection.steamId.m_SteamID == steamId.m_SteamID)
                    return steamConnection;
            }

            return null;
        }

        public HostTopology GetHostTopology() {
            if(_hostTopology == null) {
                var connectionConfig = new ConnectionConfig();

                connectionConfig.AddChannel(QosType.Reliable);
                connectionConfig.AddChannel(QosType.Unreliable);
                connectionConfig.AddChannel(QosType.Reliable);

                _hostTopology = new HostTopology(connectionConfig, 4);
            }

            return _hostTopology;
        }

        public GameParty.Player GetPlayer(Func<GameParty.Player, bool> requirements) {
            return Players.FirstOrDefault(requirements);
        }
        public GameParty.Player GetPlayer(int playerId) {
            return CurrentParty.GetPlayer(playerId);
        }
        public GameParty.Player GetPlayer(NetworkConnection connection) {
            return CurrentParty.GetPlayer(player => player.serverToClientConnection.connectionId == connection.connectionId);
        }
        public GameParty.Player GetPlayer(PlayerEntity entity) {
            if(entity == null) return null;

            return GetPlayer(player => player.playerInfo.AttachedEntity == entity);
        }
        public GameParty.Player GetPlayer(CSteamID steamId) {
            return CurrentParty.GetPlayer(steamId.m_SteamID);
        }

        public CSteamID GetSteamIDForConnection(NetworkConnection connection) {
            if(NetworkServer.connections.Count >= 1 && connection == NetworkServer.connections[0])
                return SteamUser.GetSteamID();

            if(MyClient != null && connection == MyClient.connection)
                return SteamUser.GetSteamID();

            for(int i = 0; i < SteamConnections.Count; i++) {
                if(SteamConnections[i] != connection)
                    continue;

                var steamConn = SteamConnections[i] as SteamNetworkConnection;

                if(steamConn != null)
                    return steamConn.steamId;
                else
                    Debug.LogError("Client is not a SteamNetworkConnection");
            }

            Debug.LogError("Could not find Steam ID");

            return CSteamID.Nil;
        }

        void HandleNetworkData(short messageType, Guid id, NetworkDownloadHandler.Data data, MemoryStream stream, BinaryReader reader, bool partial) {
            Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader> downloadHandler;

            //if(!partial)
            //    Debug.LogError(string.Format("HandleNetworkData({0},{1})", messageType, id.ToString()));

            if(partial ? downloadPartialHandlers.TryGetValue(messageType, out downloadHandler) : downloadHandlers.TryGetValue(messageType, out downloadHandler))
                downloadHandler.Invoke(data, stream, reader);
        }

        void HandlePartiallyReceivedData(Guid id, NetworkDownloadHandler.Data dataInfo) {
            byte[] bytes = dataInfo.data;

            using(var stream = new MemoryStream(bytes))
            using(var reader = new BinaryReader(stream)) {
                var messageType = reader.ReadInt16();

                HandleNetworkData(messageType, id, dataInfo, stream, reader, true);
            }
        }

        void HandleQuadrablazeChatMessage(byte messageType, BinaryReader reader) {
            Debug.LogError($"Message received {messageType}");
            switch(messageType) {
                case ChatMessageType.Message:
                    break;

                case ChatMessageType.Kick:
                    var steamId = (CSteamID)reader.ReadUInt64();

                    Debug.LogError($" Kick message: {steamId}\nMy id:{SteamUser.GetSteamID()}");

                    if(steamId == SteamUser.GetSteamID()) {
                        if(SteamLobbyId.IsValid())
                            Stop();
                        //SteamMatchmaking.LeaveLobby(SteamLobbyId);
                    }

                    break;
            }
        }

        void HandleReceivedData(Guid id, NetworkDownloadHandler.Data dataInfo) {
            byte[] bytes = dataInfo.data;

            using(var stream = new MemoryStream(bytes))
            using(var reader = new BinaryReader(stream)) {
                var messageType = reader.ReadInt16();

                HandleNetworkData(messageType, id, dataInfo, stream, reader, false);
            }
        }

        public void Initialize() {
            Current = this;

            //LogFilter.currentLogLevel = LogFilter.Debug;

            NetworkRegistrationHelper.InvokeRegisterNetworkHandlers();
            //Boss.BossController.RegisterHandlers();
            //BossSpawner.RegisterHandlers();
            //BossInfoUI.RegisterHandlers();
            //Actor.RegisterHandlers();
            //PlayerActor.RegisterHandlers();
            //PlayerInput.RegisterHandlers();
            //Boss.TriClone.RegisterTriCloneHandlers();
            //QuadrablazeTransformSync.RegisterHandlers();
            //ShieldDrainer.RegisterHandlers();
            //BarrageSkillExecutor.RegisterHandlers();
            //SkillExecutor.RegisterHandlers();
            //TurretSkillExecutor.RegisterHandlers();

            //TelegraphStateHandler.RegisterHandlers();
            //HitManager.RegisterHandlers();

            //NetworkedEffect.RegisterHandlers();

            _steamInviteCallback = Callback<P2PSessionRequest_t>.Create(InviteCallback);
            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            _lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            _P2PSessionRequested = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequested);
            _P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
            _lobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
            _lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            _lobbyChatMessage = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);

            if(OnConnect == null) OnConnect = new UnityEvent();

            NetworkDownloadHandler.OnPartialReceive += HandlePartiallyReceivedData;
            NetworkDownloadHandler.OnCompleteReceive += HandleReceivedData;

            RegisterDownloadHandler(DownloadMessageType.ShipDownloaded, OnShipFullyReceived);
            RegisterPartialDownloadHandler(DownloadMessageType.ShipDownloaded, OnShipPartiallyReceived);
            RegisterGameNetworkMethods();
            RegisterEntityNetworkMethods();
        }

        public void InviteFriend() {
            SteamFriends.ActivateGameOverlayInviteDialog(SteamLobbyId);
        }

        void InviteCallback(P2PSessionRequest_t pCallback) {
            Debug.Log("Steam_InviteCallback");
        }

        bool IsLobbyDataValid(CSteamID lobbyId) {
            var gameVersion = SteamMatchmaking.GetLobbyData(lobbyId, LobbyData_Version);
            var status = SteamMatchmaking.GetLobbyData(lobbyId, LobbyData_Status);

            if(gameVersion == GameManager.Current.GameVersion && status != "inRound")
                return true;

            return false;
        }

        public bool IsMemberInSteamLobby(CSteamID steamUser) {
            if(SteamManager.Initialized) {
                int numMembers = SteamMatchmaking.GetNumLobbyMembers(SteamLobbyId);

                for(int i = 0; i < numMembers; i++) {
                    var member = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobbyId, i);

                    if(member.m_SteamID == steamUser.m_SteamID)
                        return true;
                }
            }

            return false;
        }

        [ContextMenu("Join Developer")]
        public void JoinDeveloper() {
            var call = SteamMatchmaking.RequestLobbyList();
            _lobbyMatchList.Set(call, OnLobbyMatchList);
        }

        public void JoinSteamLobby(CSteamID lobbyId) {
            if(SteamLobbyId.IsValid())
                if(SteamLobbyId == lobbyId) {
                    Debug.Log("Cannot join the same lobby that you are in");
                    return;
                }
                else {
                    Stop();
                }

            Debug.Log("JoinSteamLobby");

            var options = GameManager.Current.Options;

            options.GameNetworkConnectionType = GameNetworkType.Join;

            GameManager.Current.Options = options;

            GameManager.Current.NewGame();
            SteamMatchmaking.JoinLobby(lobbyId);
        }

        public void QuickMatch() {
            IsFindingQuickMatch = true;
            RequestMatchList();
        }

        void OnClientConnect(NetworkConnection connection) {
            Debug.Log("OnClientConnect");
            NetworkDownloadHandler.RegisterClientHandlers();

            MyClient.RegisterHandler(NetMessageType.Network_GameNetMessage, ReceiveNetworkGameMessage);
            MyClient.RegisterHandler(NetMessageType.Network_EntityNetMessage, ReceiveNetworkEntityMessage);
            MyClient.RegisterHandler(NetMessageType.Client_DebugMessage, NetworkDebugMessage);
            MyClient.RegisterHandler(NetMessageType.Client_ReceiveErrorMessage, ReceiveErrorMessage);
            //MyClient.RegisterHandler(NetMessageType.Client_SuccessfulVerification, SuccessfulVerification);
            //MyClient.RegisterHandler(NetMessageType.SendSteamID, SendSteamID);
            MyClient.RegisterHandler(MsgType.Disconnect, OnDisconnect);

            // TODO: See if it's possible to use OnClientRegisterEventHandler for these

            MyClient.RegisterHandler(NetMessageType.Client_OpenMultiplayerLobby, msg => UIManager.Current.EnableMultiplayerLobby());
            MyClient.RegisterHandler(NetMessageType.Client_StartJoinGame, msg => GameManager.Current.StartJoinGame());

            MyClient.RegisterHandler(NetMessageType.Client_WeaponShoot, WeaponObject.NetworkWeaponShoot);
            MyClient.RegisterHandler(NetMessageType.Client_SetTriformerForceField, TriformerForceFieldController.SetForceField);
            MyClient.RegisterHandler(NetMessageType.Client_SkyboxHue, SkyboxEffects.SetCurrentHue);
            MyClient.RegisterHandler(NetMessageType.Client_PlayCameraSound, CameraSoundController.NetworkPlayCameraSound);

            MyClient.RegisterHandler(NetMessageType.Client_RailRiderState, Boss.RailRider.NetworkRailRiderState);
            MyClient.RegisterHandler(NetMessageType.Client_FreeTrinityState, Boss.FreeTrinity.NetworkFreeTrinityState);

            OnConnect?.Invoke();
            OnClientStarted?.Invoke(_myClient);

            RegisterClientHandlersInternal();

            if(!NetworkServer.active)
                UIManager.Current.EnableMultiplayerLobby();

            UIManager.Current.IsJoiningGame = false;
            GameLobbyManager.Current.LobbyUI.ConnectingMessage.StopAnimation(false);
            BossInfoUI.Current.Reset();

            //if(!NetworkServer.active) {
            //    CurrentParty = ScriptableObject.CreateInstance<GameParty>();
            //    CurrentParty.AddPlayer(connection, null, false, (ulong)SteamUser.GetSteamID(), SteamFriends.GetPersonaName());

            //    for(int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(SteamLobbyId); i++) {
            //        var memberSteamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobbyId, i);

            //        CurrentParty.AddPlayer(null, null, false, (ulong)memberSteamId, SteamFriends.GetFriendPersonaName(memberSteamId));
            //    }
            //}

            ClientScene.Ready(connection);
            ClientScene.AddPlayer(connection, 0);
        }

        void OnClientDisconnect(NetworkConnection connection) {
            Debug.Log("OnClientDisconnect");
            //Destroy(CurrentParty);

            GameManager.Current.EndFromDisconnect();
            PauseManager.Current.IsPaused = false;

            foreach(var data in ShipImporter.Current.syncedShipData) {
                Destroy(data.Value.currentMaterial);
                Destroy(data.Value.defaultMaterial);
                Destroy(data.Value.originalMeshObject);
                Destroy(data.Value.rootMeshObject);
                Destroy(data.Value.playerShellObject);
            }

            ShipImporter.Current.syncedShipData.Clear();

            foreach(var pool in ShipImporter.Current.syncedShipPoolManager.ObjectContainers)
                pool.Value.ClearAll();

            ShipImporter.Current.syncedShipPoolManager.ObjectContainers.Clear();

            foreach(var prefab in ShipImporter.Current.syncedShipPoolManager.PoolGenPrefabs)
                Destroy(prefab.Prefab);

            ShipImporter.Current.syncedShipPoolManager.PoolGenPrefabs.Clear();
            SteamAvatarManager.Current.ClearAvatars();

            NetworkClient.ShutdownAll();
        }

        void OnDisconnect(NetworkMessage netMsg) {
            OnClientDisconnect(netMsg.conn);
        }

        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
            JoinSteamLobby(callback.m_steamIDLobby);
        }

        //#if UNITY_EDITOR
        //        void OnGUI() {
        //            if(!NetworkServer.active && !SteamLobbyId.IsValid())
        //                if(GUI.Button(new Rect((Screen.width * .5f) - 100, 50, 200, 40), "Join Developer")) {
        //                    JoinDeveloper();
        //                }
        //        }
        //#endif

        void OnLobbyChatMessage(LobbyChatMsg_t callback) {
            var bufferSize = 256;
            byte[] data = new byte[bufferSize];

            var byteCount = SteamMatchmaking.GetLobbyChatEntry((CSteamID)callback.m_ulSteamIDLobby, (int)callback.m_iChatID, out CSteamID steamId, data, bufferSize, out EChatEntryType chatEntryType);

            switch(callback.m_eChatEntryType) {
                case (byte)EChatEntryType.k_EChatEntryTypeChatMsg:
                    using(var stream = new MemoryStream(data))
                    using(var reader = new BinaryReader(stream)) {
                        var messageType = reader.ReadByte();

                        HandleQuadrablazeChatMessage(messageType, reader);
                    }

                    break;
            }
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback) {
            var userId = new CSteamID(pCallback.m_ulSteamIDUserChanged);

            Debug.LogError("OnLobbyChatUpdate - " + SteamFriends.GetFriendPersonaName(userId));

            //if(pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) {
            //    Debug.LogError("k_EChatMemberStateChangeEntered");

            //    if(!NetworkServer.active && userId != SteamUser.GetSteamID())
            //        CurrentParty.AddPlayer(null, null, false, (ulong)userId, SteamFriends.GetFriendPersonaName(userId));
            //}

            if(pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft && pCallback.m_ulSteamIDLobby == SteamLobbyId.m_SteamID) {
                Debug.LogError("k_EChatMemberStateChangeLeft");

                if(NetworkServer.active)
                    RemoveConnection(userId);
                else if(userId == _hostId)
                    Stop();

                SteamNetworking.CloseP2PSessionWithUser(userId);
            }
        }

        void OnLobbyCreated(LobbyCreated_t pCallback) {
            Debug.LogErrorFormat("OnLobbyCreated Owner SteamID: {0}", SteamUser.GetSteamID().ToString());
        }

        void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback) {
            //Debug.Log("OnLobbyDataUpdate");
            //hasRequestedLobbyData = false;

            //var status = SteamMatchmaking.GetLobbyData(SteamLobbyId, "lobbyStatus");

            //if(!IsMemberInSteamLobby(SteamUser.GetSteamID())) {
            //    Debug.LogError("Lobby status (" + status + ")");

            //    if(status == "" || status == "inRound") {
            //        switch(status) {
            //            case "": Debug.LogError("Lobby no longer exists"); break;
            //            case "inRound": Debug.LogError("Lobby in round "); break;
            //        }

            //        if(connectingRoutine != null) {
            //            StopCoroutine(connectingRoutine);
            //            connectingRoutine = null;
            //        }

            //        Stop();
            //    }
            //}
        }

        void OnLobbyEntered(LobbyEnter_t pCallback) {
            var enterResponse = (EChatRoomEnterResponse)Enum.Parse(typeof(EChatRoomEnterResponse), pCallback.m_EChatRoomEnterResponse.ToString());

            Debug.Log("OnLobbyEntered - Response (" + enterResponse.ToString() + ")");

            switch(enterResponse) {
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess: {
                    SteamLobbyId = new CSteamID(pCallback.m_ulSteamIDLobby);

                    var hostUserId = SteamMatchmaking.GetLobbyOwner(SteamLobbyId);
                    var me = SteamUser.GetSteamID();

                    _hostId = hostUserId;

                    if(hostUserId.m_SteamID == me.m_SteamID) {
                        if(SteamMatchmaking.SetLobbyData(SteamLobbyId, "game", GAME_ID)) {
                            Debug.Log($"SteamMatchmaking.SetLobbyData({SteamLobbyId.ToString()}, {"game"}, {GAME_ID})");
                            StartServer();

                            SteamMatchmaking.SetLobbyData(SteamLobbyId, LobbyData_Title, SteamFriends.GetPersonaName());
                            SteamMatchmaking.SetLobbyData(SteamLobbyId, LobbyData_Status, "lobby");
                            SteamMatchmaking.SetLobbyData(SteamLobbyId, LobbyData_Version, GameManager.Current.GameVersion);
                            SteamMatchmaking.SetLobbyData(SteamLobbyId, LobbyData_GameMode, GameManager.Current.CurrentGameModeID.ToString());
                        }
                    }
                    else {
                        foreach(var poolManager in PoolManager.GetPools())
                            poolManager.RegisterSpawnHandlers();

                        if(IsLobbyDataValid(SteamLobbyId)) {
                            var gameVersion = SteamMatchmaking.GetLobbyData(SteamLobbyId, LobbyData_Version);

                            if(gameVersion != GameManager.Current.GameVersion) {
                                GameLobbyUI.Close();
                                UIManager.Current.GoToMenu("Main");
                                UIManager.Current.DoPopup("Multiplayer Error", string.Format("Can't connect - Game versions are different\nYour version({0}) Their Version ({1})", GameManager.Current.GameVersion, gameVersion), null,
                                    (LapinerTools.uMyGUI.uMyGUI_PopupManager.BTN_OK, null));

                                return;
                            }

                            var gameMode = SteamMatchmaking.GetLobbyData(SteamLobbyId, LobbyData_GameMode);

                            GameManager.Current.CurrentGameModeID = Convert.ToInt32(gameMode);

                            var status = SteamMatchmaking.GetLobbyData(SteamLobbyId, LobbyData_Status);

                            //// joined friend's lobby.
                            //if(status != "inRound")
                            connectingRoutine = StartCoroutine(RequestP2PConnectionWithHost());
                        }

                        //var gameVersion = SteamMatchmaking.GetLobbyData(SteamLobbyId, "gameVersion");

                        //if(gameVersion != GameManager.Current.GameVersion) {
                        //    GameLobbyUI.Close();
                        //    //UIManager.Current.EnableMainMenu();
                        //    UIManager.Current.mainMenuGroup.interactable = false;
                        //    UIManager.Current.mainMenuGroup.alpha = .5f;

                        //    var popup = UIManager.Current.DoPopup("Multiplayer Error", string.Format("Can't connect - Game versions are different\nYour version({0}) Their Version ({1})", GameManager.Current.GameVersion, gameVersion), null,
                        //    () => {
                        //        UIManager.Current.mainMenuGroup.interactable = true;
                        //        UIManager.Current.mainMenuGroup.alpha = 1;
                        //        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(UIManager.Current.playButton.gameObject);
                        //    });

                        //    return;
                        //}

                        //var status = SteamMatchmaking.GetLobbyData(SteamLobbyId, "lobbyStatus");

                        //// joined friend's lobby.
                        //if(status != "inRound")
                        //    connectingRoutine = StartCoroutine(RequestP2PConnectionWithHost());
                    }
                }
                break;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseFull:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseError:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseClanDisabled:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember:
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded:
                    GameLobbyUI.Close();
                    UIManager.Current.SetMenuScene(UIManager.MenuScene.MainMenu);
                    UIManager.Current.GoToMenu("Main");
                    //UIManager.Current.DoPopup("Connection Error", "Error:" + enterResponse.ToString(), null, () => {
                    //    UIManager.Current.mainMenuGroup.interactable = true;
                    //    UIManager.Current.mainMenuGroup.alpha = 1;
                    //    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(UIManager.Current.playButton.gameObject);
                    //});
                    return;
            }
        }

        void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure) {
            Debug.LogErrorFormat("OnLobbyMatchList Lobby Count: {0}", pCallback.m_nLobbiesMatching);

            if(IsFindingQuickMatch) {
                bool foundMatch = false;

                if(pCallback.m_nLobbiesMatching > 0) {
                    for(int i = 0; i < pCallback.m_nLobbiesMatching; i++) {
                        var lobbySteamId = SteamMatchmaking.GetLobbyByIndex(i);

                        if(SteamMatchmaking.GetNumLobbyMembers(lobbySteamId) < SteamNetworkManager.MAX_USERS)
                            if(IsLobbyDataValid(lobbySteamId)) {
                                JoinSteamLobby(lobbySteamId);
                                foundMatch = true;
                                break;
                            }
                    }
                }

                if(!foundMatch) {
                    NewGameOptions options = GameManager.Current.Options;
                    options.GameNetworkConnectionType = GameNetworkType.Host;
                    GameManager.Current.Options = options;

                    GameManager.Current.NewGame();
                }

                IsFindingQuickMatch = false;
                GameLobbyManager.Current.LobbyUI.ConnectingMessage.StopAnimation();
            }
            else {
                if(UIManager.Current.lobbyListUI.gameObject.activeInHierarchy)
                    UIManager.Current.lobbyListUI.GenerateList(pCallback);
            }
        }

        void OnP2PSessionConnectFail(P2PSessionConnectFail_t pCallback) {
            Debug.Log("OnP2PSessionConnectFail " + pCallback.m_eP2PSessionError);
        }

        void OnP2PSessionRequested(P2PSessionRequest_t pCallback) {
            Debug.LogError("P2P session request received");

            var member = pCallback.m_steamIDRemote;

            if(NetworkServer.active && IsMemberInSteamLobby(member)) {
                // Accept the connection if this user is in the lobby
                Debug.LogError("P2P connection accepted");
                SteamNetworking.AcceptP2PSessionWithUser(member);

                CreateP2PConnectionWithPeer(member);
            }
        }

        void OnServerAddPlayer(NetworkMessage netMsg) {
            Debug.Log("OnServerAddPlayer");

            var connection = netMsg.conn;

            var player = GetPlayer(connection);
            var playerInfo = Instantiate(playerPrefab);

            player.playerInfo = playerInfo.GetComponent<GamePlayerInfo>();
            player.playerInfo.SteamId = player.steamId;
            player.playerInfo.Username = player.username;

            NetworkServer.AddPlayerForConnection(player.serverToClientConnection, playerInfo, 0);

            GameLobbyManager.Current.LobbyUI.AddPlayer(player, connection);
            StartCoroutine(DelayedShipSend(connection, player));
        }

        void OnShipFullyReceived(NetworkDownloadHandler.Data data, MemoryStream stream, BinaryReader reader) {

            var gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(reader.ReadUInt32()));
            var gamePlayerInfo = gameObject.GetComponent<GamePlayerInfo>();
            Debug.Log("OnShipFullyReceived player netId:" + gamePlayerInfo.netId);

            var shipSettingsLength = reader.ReadInt32();
            var shipSettingsBytes = reader.ReadBytes(shipSettingsLength);
            ShipImportSettings shipImportSettings;

            using(var subStream = new MemoryStream(shipSettingsBytes))
            using(var subReader = new BinaryReader(subStream))
                shipImportSettings = new ShipImportSettings(shipSettingsBytes);

            var isBuiltinShip = reader.ReadBoolean();

            gamePlayerInfo.IsBuiltinShip = isBuiltinShip;
            gamePlayerInfo.IsShipDownloaded = true;
            gamePlayerInfo.ColorPreset = new ShipPreset("Temp Preset", false, shipImportSettings);
            gamePlayerInfo.ShipSettings = shipImportSettings;
            gamePlayerInfo.ShipReady = true;

            if(isBuiltinShip) {
                var assetHashName = reader.ReadString();

                gamePlayerInfo.ShipID = ShipImporter.Current.localShipData.FirstOrDefault(pair => pair.Value.assetHashName == assetHashName).Key;
                gamePlayerInfo.ShipAssetHashName = assetHashName;
            }
            else {
                var shipLength = reader.ReadInt32();
                var shipBytes = reader.ReadBytes(shipLength);

                gamePlayerInfo.DownloadedShip = ShipFile.LoadFromBytes(shipBytes);
                gamePlayerInfo.ShipID = (int)gamePlayerInfo.netId.Value;

                ShipImporter.Current.ImportGamePlayer(gamePlayerInfo, shipImportSettings);

                gamePlayerInfo.ShipAssetHashName = ShipImporter.Current.syncedShipData[gamePlayerInfo.ShipID].assetHashName;
            }

            if(NetworkServer.active) {
                gamePlayerInfo.SetPlayerHasShip((ulong)SteamUser.GetSteamID());

                if(SteamConnections.Count > 2) {
                    var sendToConnections = SteamConnections.Where(connection => connection.hostId > -1 && connection.connectionId != gamePlayerInfo.connectionToClient.connectionId);
                    //var sendToConnections = NetworkServer.connections.Where(connection => connection.hostId > -1 && connection.connectionId != gamePlayerInfo.connectionToClient.connectionId);

                    Debug.LogError("Sending id:" + gamePlayerInfo.connectionToClient.connectionId + " ship to these player connections");

                    foreach(var connection in sendToConnections)
                        Debug.LogError("    id:" + connection.connectionId);

                    //stream.Close();
                    var streamData = stream.ToArray();

                    //var guid = NetworkDownloadHandler.SendData(stream.GetBuffer(), d => { ServerShipRedirectFinished(d, sendToConnections); }, sendToConnections);
                    NetworkDownloadHandler.SendData(streamData, d => { ServerShipRedirectFinished(d, sendToConnections); }, sendToConnections);

                }
            }
        }

        void OnShipPartiallyReceived(NetworkDownloadHandler.Data data, MemoryStream stream, BinaryReader reader) {
            var gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(reader.ReadUInt32()));
            var gamePlayerInfo = gameObject.GetComponent<GamePlayerInfo>();
            var shipSettingsLength = reader.ReadInt32(); //shipSettingsLength

            reader.ReadBytes(shipSettingsLength); //shipSettingsBytes

            var isBuiltinShip = reader.ReadBoolean();

            if(!isBuiltinShip) {
                int percentage = (int)(((float)data.currentIndex / data.data.Length) * 100);
                gamePlayerInfo.LobbyPlayerInfoComponent.SetShipSync(percentage);
            }

        }

        void OnSpawnFinished(NetworkMessage netMsg) {
            Debug.Log("OnSpawnFinished");
        }

        void OnSpawnPlayer(NetworkMessage message) {
            var stringMessage = message.ReadMessage<StringMessage>();
            Debug.Log("Spawned player request");

            if(stringMessage != null) {
                ulong steamId;

                if(ulong.TryParse(stringMessage.value, out steamId)) {
                    var conn = GetConnection(new CSteamID(steamId));

                    if(conn != null) {
                        // spawn peer
                        if(SpawnPlayer(conn)) {
                            Debug.Log("Spawned player");
                            return;
                        }
                    }
                }
            }
        }

        void OnSteamConnect(NetworkMessage msg) {
            // Set to ready and spawn player
            Debug.Log("Connected to UNET server.");
            MyClient.UnregisterHandler(MsgType.Connect);

            RegisterNetworkPrefabs();

            var connection = MyClient.connection;

            Debug.LogFormat("SteamConnect({0}, {1})", MyClient.connection.GetHashCode(), msg.conn.GetHashCode());

            if(connection != null)
                OnClientConnect(connection);
        }

        public static void RegisterClientHandler(short messageType, NetworkMessageDelegate handler) {
            //if(OnClientRegisterHandler == null) OnClientRegisterHandler = new Dictionary<short, NetworkMessageDelegate>();

            //OnClientRegisterHandler[messageType] = handler;

            HashSet<NetworkMessageDelegate> messageDelegates;

            if(!OnClientRegisterEventHandler.TryGetValue(messageType, out messageDelegates)) {
                messageDelegates = new HashSet<NetworkMessageDelegate>();
                OnClientRegisterEventHandler.Add(messageType, messageDelegates);
            }

            messageDelegates.Add(handler);
        }

        public static void UnregisterClientHandler(short messageType) {
            //OnClientRegisterHandler.Remove(messageType);

            OnClientRegisterEventHandler.Remove(messageType);
        }
        public static void UnregisterClientHandler(short messageType, NetworkMessageDelegate handler) {
            if(OnClientRegisterEventHandler.TryGetValue(messageType, out HashSet<NetworkMessageDelegate> eventHandlers)) {
                eventHandlers.Remove(handler);

                if(eventHandlers.Count == 0)
                    OnClientRegisterEventHandler.Remove(messageType);
            }
        }

        public static void RegisterServerHandler(short messageType, NetworkMessageDelegate handler) {
            //if(OnServerRegisterHandler == null) OnServerRegisterHandler = new Dictionary<short, NetworkMessageDelegate>();

            //OnServerRegisterHandler[messageType] = handler;

            HashSet<NetworkMessageDelegate> messageDelegates;

            if(!OnServerRegisterEventHandler.TryGetValue(messageType, out messageDelegates)) {
                messageDelegates = new HashSet<NetworkMessageDelegate>();
                OnServerRegisterEventHandler.Add(messageType, messageDelegates);
            }

            messageDelegates.Add(handler);
        }

        public static void UnregisterServerHandler(short messageType) {
            //OnServerRegisterHandler.Remove(messageType);

            OnServerRegisterEventHandler.Remove(messageType);
        }
        public static void UnregisterServerHandler(short messageType, NetworkMessageDelegate handler) {
            if(OnServerRegisterEventHandler.TryGetValue(messageType, out HashSet<NetworkMessageDelegate> eventHandlers)) {
                eventHandlers.Remove(handler);

                if(eventHandlers.Count == 0)
                    OnServerRegisterEventHandler.Remove(messageType);
            }
        }

        static void ClientEventHandler(short id, NetworkMessage networkMessage) {
            if(!OnClientRegisterEventHandler.ContainsKey(id)) return;

            var eventHandler = OnClientRegisterEventHandler[id];

            foreach(var handler in eventHandler)
                handler(networkMessage);
        }

        static void ServerEventHandler(short id, NetworkMessage networkMessage) {
            if(!OnServerRegisterEventHandler.ContainsKey(id)) return;

            var eventHandler = OnServerRegisterEventHandler[id];

            foreach(var handler in eventHandler)
                handler(networkMessage);
        }

        void RegisterClientHandlersInternal() {
            //if(OnClientRegisterHandler != null)
            //    foreach(var register in OnClientRegisterHandler)
            //        MyClient.RegisterHandler(register.Key, register.Value);

            if(OnClientRegisterEventHandler != null)
                foreach(var register in OnClientRegisterEventHandler)
                    MyClient.RegisterHandler(register.Key, message => { ClientEventHandler(register.Key, message); });
        }

        void RegisterServerHandlersInternal() {
            //if(OnServerRegisterHandler != null)
            //    foreach(var register in OnServerRegisterHandler)
            //        NetworkServer.RegisterHandler(register.Key, register.Value);

            if(OnServerRegisterEventHandler != null)
                foreach(var register in OnServerRegisterEventHandler)
                    NetworkServer.RegisterHandler(register.Key, message => { ServerEventHandler(register.Key, message); });
        }

        void RegisterNetworkPrefabs() {
            ClientScene.RegisterPrefab(playerPrefab);
            ClientScene.RegisterPrefab(testPrefab);

            foreach(var item in networkedPrefabs)
                ClientScene.RegisterPrefab(item);
        }

        public void RequestMatchList() {
            var call = SteamMatchmaking.RequestLobbyList();

            _lobbyMatchList.Set(call, OnLobbyMatchList);
        }

        IEnumerator RequestP2PConnectionWithHost() {
            var hostUserId = SteamMatchmaking.GetLobbyOwner(SteamLobbyId);

            //send packet to request connection to host via Steam's NAT punch or relay servers
            Debug.Log("Sending packet to request P2P connection");
            SteamNetworking.SendP2PPacket(hostUserId, null, 0, EP2PSend.k_EP2PSendReliable);

            Debug.Log("Waiting for P2P acceptance message");
            uint packetSize;
            //float requestStartTime = Time.unscaledTime;
            //float timeout = 5;

            while(!SteamNetworking.IsP2PPacketAvailable(out packetSize)) {
                bool disconnect = false;

                //if(!hasRequestedLobbyData)
                //    if(!SteamMatchmaking.RequestLobbyData(SteamLobbyId)) {
                //        Debug.LogError("RequestLobbyData Lobby no longer exists");
                //        disconnect = true;
                //    }
                ////else {
                //var status = SteamMatchmaking.GetLobbyData(SteamLobbyId, "lobbyStatus");

                //if(status == "" || status == "inRound") {
                //    switch(status) {
                //        case "": Debug.LogError("Lobby no longer exists"); break;
                //        case "inRound": Debug.LogError("Lobby in round "); break;
                //    }

                //    disconnect = true;
                //}
                //}

                //if(!hasRequestedLobbyData)
                //    if(!SteamMatchmaking.RequestLobbyData(SteamLobbyId)) {
                //        Debug.LogError("RequestLobbyData Lobby no longer exists");
                //        disconnect = true;
                //    }

                //Debug.Log("Players = " + SteamMatchmaking.GetNumLobbyMembers(SteamLobbyId));

                //if(SteamMatchmaking.GetNumLobbyMembers(SteamLobbyId) == 0) {
                //    Debug.LogError("GetNumLobbyMembers Lobby no longer exists");
                //    disconnect = true;
                //}

                //if(Time.unscaledTime > requestStartTime + timeout) {
                //    Debug.LogError("Connection timed out");
                //    disconnect = true;
                //}

                if(disconnect) {
                    SteamNetworking.CloseP2PSessionWithUser(hostUserId);
                    Stop();
                    connectingRoutine = null;

                    yield break;
                }

                yield return null;
            }

            byte[] data = new byte[packetSize];

            CSteamID senderId;

            if(SteamNetworking.ReadP2PPacket(data, packetSize, out packetSize, out senderId))
                if(senderId.m_SteamID == hostUserId.m_SteamID) {
                    Debug.Log("P2P connection established");

                    // packet was from host, assume it's notifying client that AcceptP2PSessionWithUser was called
                    P2PSessionState_t sessionState;

                    if(SteamNetworking.GetP2PSessionState(hostUserId, out sessionState)) {
                        // connect to the unet server
                        ConnectToUnetServerForSteam(hostUserId);

                        yield break;
                    }
                }

            Debug.LogError("Connection failed");
        }

        void ReceiveErrorMessage(NetworkMessage netMsg) {
            var message = netMsg.ReadMessage<StringMessage>();

            Debug.LogError("Multiplayer Error: " + message.value);

            UIManager.Current.DoPopup("Multiplayer Error", message.value, null,
                (LapinerTools.uMyGUI.uMyGUI_PopupManager.BTN_OK, null));
        }

        public static void RegisterDownloadHandler(short messageType, Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader> handler) {
            downloadHandlers[messageType] = handler;
        }

        public static void RegisterPartialDownloadHandler(short messageType, Action<NetworkDownloadHandler.Data, MemoryStream, BinaryReader> handler) {
            downloadPartialHandlers[messageType] = handler;
        }

        public static void UnregisterDownloadHandler(short messageType) {
            downloadHandlers.Remove(messageType);
        }

        public void RemoveConnection(CSteamID userId) {
            var connection = GetConnection(userId);
            var steamConnection = connection as SteamNetworkConnection;

            if(connection != null) {
                connection.InvokeHandlerNoData(MsgType.Disconnect);

                steamConnection?.CloseP2PSession();

                DestroyPlayer(connection);
                CurrentParty.RemovePlayer(connection);
                SteamConnections.Remove(steamConnection);

                connection.hostId = -1;
                connection.Disconnect();
                connection.Dispose();
                connection = null;
            }
        }

        public static void SendMessage(short messageType, NetworkConnection connection, MessageBase message, int channelId = 0) {
            connection.SendByChannel(messageType, message, channelId);
        }
        public static void SendMessageToAll(short messageType, MessageBase message, int channelId = 0, bool includeHost = true) {
            for(int i = includeHost ? 0 : 1; i < Current.SteamConnections.Count; i++)
                SendMessage(messageType, Current.SteamConnections[i], message, channelId);
        }
        public static void SendMessageToServer(short messageType, MessageBase message, int channelId = 0) {
            SendMessage(messageType, Current.SteamConnections[0], message, channelId);
        }

        public static void SendWriter(NetworkWriter writer, int channelId) {
            Current?.MyClient?.connection.SendWriter(writer, channelId);
        }

        public static void SendWriterToAll(NetworkWriter writer) {
            SendWriterToAll(writer, 0, true);
        }
        public static void SendWriterToAll(NetworkWriter writer, bool includeHost = true) {
            SendWriterToAll(writer, 0, includeHost);
        }
        public static void SendWriterToAll(NetworkWriter writer, int channelId, bool includeHost = true) {
            for(int i = includeHost ? 0 : 1; i < Current.SteamConnections.Count; i++) {
                var connection = Current.SteamConnections[i];

                connection.SendWriter(writer, channelId);
            }
        }
        public static void SendWriterToServer(NetworkWriter writer, int channelId) {
            Current.SteamConnections[0].SendWriter(writer, channelId);
        }

        #region Game Network Message
        public static string GenerateNetworkMessageKey(NetworkMessageDelegate method) {
            return GenerateNetworkMessageKey(method.GetMethodInfo());
        }
        public static string GenerateNetworkMessageKey(MethodInfo methodInfo) {
            return $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
        }


        static void ReceiveNetworkEntityMessage(NetworkMessage netMsg) {
            var entityMessage = netMsg.ReadMessage<EntityMessageBase>();
            string messageKey;
            uint entityId;

            if(entityMessage != null) {
                messageKey = entityMessage.messageKey;
                entityId = entityMessage.id;

                netMsg.reader.SeekZero();
                netMsg.reader.ReadBytes(4);
            }
            else {
                netMsg.reader.SeekZero();
                netMsg.reader.ReadBytes(4);
                messageKey = netMsg.reader.ReadString();
                entityId = netMsg.reader.ReadUInt32();
            }

            if(EntityNetworkListeners.ContainsKey(messageKey)) {
                var actorEntity = GameManager.Current.GetActorEntity(entityId);

                if(actorEntity != null) {
                    var methodInfo = EntityNetworkListeners[messageKey];
                    var method = (NetworkMessageDelegate)methodInfo.CreateDelegate(typeof(NetworkMessageDelegate), actorEntity);

                    method.Invoke(netMsg);
                }
            }
        }

        static void ReceiveNetworkGameMessage(NetworkMessage netMsg) {
            string messageKey = netMsg.reader.ReadString();

            if(GameNetworkListeners.TryGetValue(messageKey, out var info)) {
                //string debugMessage = "NetworkGameMessage\n";

                if(info.methodInfo.IsStatic) {
                    //debugMessage += $"Static Method: {messageKey}";
                    info.staticMethod?.Invoke(netMsg);
                }
                else {
                    //debugMessage += $"{info.listeners.Count} Listeners: {messageKey}";
                    foreach(var item in info.listeners) {
                        item.Value(netMsg);
                        netMsg.reader.SeekZero();
                        netMsg.reader.ReadString();
                    }
                }

                //Debug.Log(debugMessage);
            }
        }

        public static void RegisterEntityNetworkMethods() {
            EntityNetworkListeners = new Dictionary<string, MethodInfo>();

            foreach((MethodInfo methodInfo, EntityNetworkListenerAttribute attribute) listener in AttributeUtility.GetAttributeInstances<EntityNetworkListenerAttribute>()) {
                var messageKey = GenerateNetworkMessageKey(listener.methodInfo);

                if(!EntityNetworkListeners.ContainsKey(messageKey)) {
                    if(listener.methodInfo.IsStatic) continue;

                    EntityNetworkListeners.Add(messageKey, listener.methodInfo);
                }
            }
        }

        public static void RegisterGameNetworkMethods() {
            GameNetworkListeners = new Dictionary<string, (MethodInfo, NetworkMessageDelegate, Dictionary<object, NetworkMessageDelegate>)>();
            TypeLookup = new List<Type>();
            MethodLookup = new Dictionary<ushort, List<MethodInfo>>();

            foreach((MethodInfo methodInfo, GameNetworkListenerAttribute attribute) listener in AttributeUtility.GetAttributeInstances<GameNetworkListenerAttribute>()) {
                var type = listener.methodInfo.DeclaringType;
                var typeIndex = TypeLookup.IndexOf(type);

                if(typeIndex == -1) {
                    TypeLookup.Add(type);
                    typeIndex = TypeLookup.IndexOf(type);

                    var methodList = new List<MethodInfo>() { listener.methodInfo };
                    MethodLookup.Add((ushort)typeIndex, methodList);
                }
                else {
                    MethodLookup[(ushort)typeIndex].Add(listener.methodInfo);
                }

                var messageKey = GenerateNetworkMessageKey(listener.methodInfo);
                if(GameNetworkListeners.ContainsKey(messageKey)) continue;

                NetworkMessageDelegate staticMethod = null;

                if(listener.methodInfo.IsStatic)
                    staticMethod = (NetworkMessageDelegate)listener.methodInfo.CreateDelegate(typeof(NetworkMessageDelegate));

                GameNetworkListeners.Add(messageKey, (listener.methodInfo, staticMethod, new Dictionary<object, NetworkMessageDelegate>()));

                //Debug.Log($"<b>Game Network Listener Method</b>" +
                //    $"\n{listener.method.DeclaringType.Namespace}.<color=#62c9b0><b>{listener.method.DeclaringType.Name}</b></color>.<color=#dbdca8>{listener.method.Name}</color>");
            }

            foreach(var x in TypeLookup)
                Debug.Log($"Type({x.Name})");
        }

        public static void RegisterGameNetworkListener(NetworkMessageDelegate method, object target) {
            var messageKey = GenerateNetworkMessageKey(method);

            if(GameNetworkListeners.TryGetValue(messageKey, out var info)) {
                if(info.methodInfo.IsStatic) {
                    Debug.LogError("Cannot regsiter game network listener to a static method");
                    return;
                }

                info.listeners.Add(target, method);
            }
        }

        /// <summary>Send network message about an ActorEntity instance. ActorEntity.Id cannot be 0.</summary>
        public static void SendEntityNetworkMessage(ActorEntity entity, NetworkMessageDelegate networkReceiveMethod, Action<NetworkWriter> writeMethod = null, int channelId = 0, bool includeHost = true) {
            var writer = GenerateEntityNetworkMessageWriter(entity, networkReceiveMethod, writeMethod);

            SendWriterToAll(writer, channelId, includeHost);
        }
        public static void SendEntityNetworkMessageToServer(ActorEntity entity, NetworkMessageDelegate networkReceiveMethod, Action<NetworkWriter> writeMethod = null, int channelId = 0) {
            var writer = GenerateEntityNetworkMessageWriter(entity, networkReceiveMethod, writeMethod);

            SendWriterToServer(writer, channelId);
        }
        static NetworkWriter GenerateEntityNetworkMessageWriter(ActorEntity entity, NetworkMessageDelegate networkReceiveMethod, Action<NetworkWriter> writeMethod = null) {
            var methodInfo = networkReceiveMethod.GetMethodInfo();

            if(entity.Id == 0) {
                Debug.LogError($"Cannot send entity network message '{methodInfo.Name}' if Id is 0");
                return null;
            }

            var messageKey = GenerateNetworkMessageKey(methodInfo);
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Network_EntityNetMessage);
            writer.Write(messageKey);
            writer.Write(entity.Id);
            writer.FinishMessage();

            return writer;
        }

        public static void SendEntityNetworkMessage(NetworkMessageDelegate networkReceiveMethod, EntityMessageBase message, int channelId = 0, bool includeHost = true) {
            if(ValidateEntityNetworkMessage(networkReceiveMethod, message))
                SendMessageToAll(NetMessageType.Network_EntityNetMessage, message, channelId, includeHost);
        }

        public static void SendEntityNetworkMessageToServer(NetworkMessageDelegate networkReceiveMethod, EntityMessageBase message, int channelId = 0, bool includeHost = true) {
            if(ValidateEntityNetworkMessage(networkReceiveMethod, message))
                SendMessageToServer(NetMessageType.Network_EntityNetMessage, message, channelId);
        }
        static bool ValidateEntityNetworkMessage(NetworkMessageDelegate networkReceiveMethod, EntityMessageBase message) {
            if(message.id == 0) {
                var methodInfo = networkReceiveMethod.GetMethodInfo();

                Debug.LogError($"Cannot send entity network message '{methodInfo.Name}' if Id is 0");

                return false;
            }

            return true;
        }

        public static void SendGameNetworkMessage(NetworkMessageDelegate listenToMethod, Action<NetworkWriter> writeMethod = null, int channelId = 0, bool includeHost = true) {
            if(listenToMethod == null) return;

            var methodInfo = listenToMethod.GetMethodInfo();

            if(methodInfo.GetCustomAttribute<GameNetworkListenerAttribute>() is var attribute) {
                string messageKey = GenerateNetworkMessageKey(methodInfo);

                //Debug.Log($"SendGameNetworkMessage for {messageKey}");

                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Network_GameNetMessage);
                writer.Write(messageKey);
                writeMethod(writer);
                writer.FinishMessage();

                SendWriterToAll(writer, channelId, includeHost);
            }
        }
        //public static void SendGameNetworkMessage(short gameNetMessageType, Action<NetworkWriter> writeMethod, int channelId, bool includeHost = true) {
        //    var writer = new NetworkWriter();

        //    writer.StartMessage(NetMessageType.Network_GameNetMessage);
        //    writer.Write(gameNetMessageType);
        //    writeMethod(writer);
        //    writer.FinishMessage();

        //    SendWriterToAll(writer, channelId, includeHost);
        //}
        #endregion

        void ServerShipRedirectFinished(NetworkDownloadHandler.Data data, IEnumerable<NetworkConnection> connections) {
            Debug.LogError("ServerShipSendRedirectFinished");

            GameObject gameObject;
            GamePlayerInfo gamePlayerInfo;

            using(var stream = new MemoryStream(data.data))
            using(var reader = new BinaryReader(stream)) {
                reader.ReadInt16();

                gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(reader.ReadUInt32()));
                gamePlayerInfo = gameObject.GetComponent<GamePlayerInfo>();
            }

            foreach(var connection in connections)
                if(connection != null)
                    gamePlayerInfo.SetPlayerHasShip((ulong)GetSteamIDForConnection(connection));
        }

        bool SpawnPlayer(NetworkConnection conn) {
            var player = Instantiate(playerPrefab);

            return NetworkServer.AddPlayerForConnection(conn, player, 0);
        }

        public void Stop() {
            bool wasServer = NetworkServer.active;

            if(SteamLobbyId.IsValid())
                SteamMatchmaking.LeaveLobby(SteamLobbyId);

            if(!wasServer)
                ClientScene.DestroyAllClientObjects();

            if(MyClient != null) {
                Debug.Log("Stop Client");
                MyClient.Disconnect();
                MyClient = null;
            }

            if(wasServer)
                ClientScene.DestroyAllClientObjects();

            if(SteamConnections != null) {
                for(int i = 0; i < SteamConnections.Count; i++) {
                    NetworkServer.SetClientNotReady(SteamConnections[i]);

                    var steamConnection = SteamConnections[i] as SteamNetworkConnection;

                    if(steamConnection != null)
                        SteamNetworking.CloseP2PSessionWithUser(steamConnection.steamId);
                }

                SteamConnections.Clear();
            }

            if(wasServer)
                NetworkServer.Shutdown();

            _steamLobbyId.Clear();
            Destroy(CurrentParty);

            GameLobbyUI.Close();

            if(wasServer)
                NetworkServer.ClearLocalObjects();

            UIManager.Current.SetMenuScene(UIManager.MenuScene.MainMenu);
            UIManager.Current.GoToMenu("Main");

            foreach(var info in GameNetworkListeners.Values)
                info.listeners.Clear();
        }

        public void StartServer() {
            Debug.Log("StartServer");

            foreach(var poolManager in PoolManager.GetPools())
                poolManager.RegisterSpawnHandlers();

            UNETExtensions.ResetIdCounter();
            var hostTopology = GetHostTopology();

            //NetworkServer.RegisterHandler(MsgType.SpawnFinished, OnSpawnFinished);

            NetworkServer.Configure(hostTopology);
            NetworkServer.dontListen = true;
            NetworkServer.Listen(0);

            //NetworkServer.RegisterHandler(NetMessageType.Network_Spawn, OnSpawnPlayer);
            NetworkServer.RegisterHandler(MsgType.AddPlayer, OnServerAddPlayer);
            NetworkServer.RegisterHandler(MsgType.Disconnect, ServerDisconnected);

            RegisterServerHandlersInternal();
            MyClient = ClientScene.ConnectLocalServer();

            MyClient.RegisterHandler(MsgType.Connect, ServerConnected);
            MyClient.Configure(hostTopology);
            MyClient.Connect("localhost", 0);
            MyClient.connection.ForceInitialize(hostTopology);

            var serverToClientConnection = NetworkServer.connections[0];
            AddConnection(serverToClientConnection);

            RegisterNetworkPrefabs();

            CurrentParty = ScriptableObject.CreateInstance<GameParty>();

            NetworkDownloadHandler.RegisterServerHandlers();
            CurrentParty.AddPlayer(MyClient.connection, serverToClientConnection, true, SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName());
        }

        void ServerConnected(NetworkMessage netMsg) {
            OnClientConnect(netMsg.conn);
        }

        void ServerDisconnected(NetworkMessage netMsg) {
            CurrentParty.RemovePlayer(netMsg.conn);

            foreach(var player in Players)
                player.playerInfo?.SetPlayerHasShip((ulong)GetSteamIDForConnection(netMsg.conn), true);
        }

        void Update() {
            if(!SteamManager.Initialized) return;

            if(UIManager.Current.IsJoiningGame) {
                if(Time.time > lastJoinPing + _joinPingFrequency) {
                    lastJoinPing = Time.time;

                    var lobbyData = SteamMatchmaking.GetLobbyData(SteamLobbyId, LobbyData_Status);

                    if(lobbyData == "inRound") {
                        Stop();
                        UIManager.Current.IsJoiningGame = false;
                        GameLobbyManager.Current.LobbyUI.ConnectingMessage.StopAnimation(false);
                        //UIManager.Current.EnableMainMenu();
                    }
                }
            }

            if(!NetworkClient.active) return;

            uint packetSize;
            int channels = GetHostTopology().DefaultConfig.ChannelCount;

            for(int channel = 0; channel < channels; channel++)
                while(SteamNetworking.IsP2PPacketAvailable(out packetSize, channel)) {
                    byte[] data = new byte[packetSize];

                    CSteamID senderId;
                    if(SteamNetworking.ReadP2PPacket(data, packetSize, out packetSize, out senderId, channel)) {
                        NetworkConnection connection;

                        if(NetworkServer.active) {
                            connection = GetConnection(senderId);

                            if(connection == null) {
                                P2PSessionState_t sessionState;

                                if(SteamNetworking.GetP2PSessionState(senderId, out sessionState) && Convert.ToBoolean(sessionState.m_bConnectionActive)) {
                                    Debug.Log("Update GetP2PSessionState");
                                    SteamNetworking.CloseP2PSessionWithUser(senderId);

                                    SteamNetworking.SendP2PPacket(senderId, null, 0, EP2PSend.k_EP2PSendReliable);

                                    connection = new SteamNetworkConnection(senderId);
                                    connection.ForceInitialize(GetHostTopology());
                                    NetworkServer.AddExternalConnection(connection);
                                    AddConnection(connection);
                                    CurrentParty.AddPlayer(MyClient.connection, connection, false, (ulong)senderId, SteamFriends.GetFriendPersonaName(senderId));
                                }
                            }
                        }
                        else {
                            connection = MyClient.connection;
                        }

                        if(connection != null)
                            connection.TransportReceive(data, Convert.ToInt32(packetSize), channel);
                    }
                }
        }

        public static void NetworkDebugLog(string message) {
            NetworkDebugLog(message, 0);
        }

        public static void NetworkDebugLogError(string message) {
            NetworkDebugLog(message, 1);
        }

        public static void NetworkDebugLogWarning(string message) {
            NetworkDebugLog(message, 2);
        }

        public static void NetworkDebugLog(string message, byte type) {
            if(NetworkServer.active) {
                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_DebugMessage);
                writer.Write(type);
                writer.Write(message);
                writer.FinishMessage();

                SendWriterToAll(writer, false);
            }
        }

        static void NetworkDebugMessage(NetworkMessage message) {
            var type = message.reader.ReadByte();
            var debugMessage = message.reader.ReadString();

            switch(type) {
                default: Debug.Log(debugMessage); break;
                case 1: Debug.LogError(debugMessage); break;
                case 2: Debug.LogWarning(debugMessage); break;
            }
        }
    }
}