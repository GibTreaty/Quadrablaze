using Rewired;
using System.Collections;
using System.Collections.Generic;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Steamworks;
using UnityEngine.Events;
using Quadrablaze.Entities;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class GamePlayerInfo : NetworkBehaviour {

        public static HashSet<GamePlayerInfo> PlayerInfos = new HashSet<GamePlayerInfo>();

        //[SyncVar(hook = "Sync_SteamID")]
        //[SerializeField]
        //ulong _steamID;

        [SerializeField]
        ScriptablePlayerEntity _originalPlayerEntity;

        [Header("Ship Info")]
        [SerializeField]
        SharePlayerShip _shareShip;

        [SerializeField]
        int _shipID = -1;

        [SerializeField]
        string _shipAssetHashName = "";

        [SerializeField]
        GameObject _playerShipPrefab;

        [SerializeField]
        bool _isShipDownloaded = false;

        [SerializeField]
        bool _isBuiltinShip = false;

        [SerializeField]
        bool _shipReady = false;

        [SerializeField]
        HashSet<ulong> _shipUploadedToPlayers = new HashSet<ulong>();

        [SerializeField]
        ShipPreset _colorPreset;

        [SerializeField]
        ShipImportSettings _shipSettings;

        [SerializeField]
        int _selectedShipID;

        [SyncVar(hook = "Sync_LobbyPlayerInfoID")]
        internal NetworkInstanceId _lobbyPlayerInfoID;

        internal LobbyPlayerInfo _lobbyPlayerInfo;

        [SyncVar(hook = "Sync_SteamId")]
        ulong _steamId;

        [SyncVar(hook = "Sync_Username")]
        string _username = "";

        bool _isSpawned;
        float _spawnTimer;
        ShipFile _downloadedShip = null;

        [SyncVar(hook = "Sync_IsDead")]
        bool _isDead = false;

        UnityAction<SkillLayout, SkillLayoutElement> onLevelChangedAction;

        #region Properties
        public PlayerEntity AttachedEntity { get; set; }
        public uint ActorEntityId { get; set; }

        public ShipPreset ColorPreset {
            get { return _colorPreset; }
            set { _colorPreset = value; }
        }

        public ShipFile DownloadedShip {
            get { return _downloadedShip; }
            set { _downloadedShip = value; }
        }

        public bool IsBuiltinShip {
            get { return _isBuiltinShip; }
            set { _isBuiltinShip = value; }
        }

        public bool IsDead => _isDead;

        public bool IsShipDownloaded {
            get { return _isShipDownloaded; }
            set { _isShipDownloaded = value; }
        }

        public LobbyPlayerInfo LobbyPlayerInfoComponent {
            get { return _lobbyPlayerInfo; }
            set { _lobbyPlayerInfo = value; }
        }

        public GameObject PlayerShipPrefab {
            get { return _playerShipPrefab; }
            set { _playerShipPrefab = value; }
        }

        Player RewiredPlayer { get; set; }

        public int SelectedShipID {
            get { return _selectedShipID; }
            set { _selectedShipID = value; }
        }

        public SharePlayerShip ShareShip {
            get { return _shareShip; }
        }

        public string ShipAssetHashName {
            get { return _shipAssetHashName; }
            set { _shipAssetHashName = value; }
        }

        public int ShipID {
            get { return _shipID; }
            set { _shipID = value; }
        }

        public bool ShipReady {
            get { return _shipReady; }
            set { _shipReady = value; }
        }

        public HashSet<ulong> ShipUploadedToPlayers {
            get { return _shipUploadedToPlayers; }
            set { _shipUploadedToPlayers = value; }
        }

        public ShipImportSettings ShipSettings {
            get { return _shipSettings; }
            set { _shipSettings = value; }
        }

        public ulong SteamId {
            get { return _steamId; }
            set { _steamId = value; }
        }

        public string Username {
            get { return _username; }
            set { _username = value; }
        }
        #endregion

        public override void OnStartAuthority() {
            onLevelChangedAction = (layout, element) => {
                AttachedEntity.Listener.RaiseEvent(PlayerActions.Upgraded, AttachedEntity.ToArgs());
            };

            //QuadrablazeNetworkManager.Current.MyPlayerInfo = this;
            //QuadrablazeNetworkManager.Current.MyNetworkIdentity = GetComponent<NetworkIdentity>();

            PlayerSpawnManager.Current.CurrentPlayerInfo = this;
            QuadrablazeSteamNetworking.Current.MyPlayerInfo = this;
            QuadrablazeSteamNetworking.Current.MyNetworkIdentity = GetComponent<NetworkIdentity>();

            ShipID = PlayerSpawnManager.Current.nextPlayerPrefabId;
            SelectedShipID = PlayerSpawnManager.Current.nextPlayerPrefabId;

            var shipInfo = ShipImporter.Current.localShipData[SelectedShipID];
            Debug.Log("GamePlayerInfo OnStartAuthority");
            IsBuiltinShip = shipInfo.isBuiltinShip;

            var settings = ShipImporter.Current.localSettings[SelectedShipID];

            settings.primaryColor = ShipSelectionUIManager.Current.CurrentPreset.PrimaryColor;
            settings.secondaryColor = ShipSelectionUIManager.Current.CurrentPreset.SecondaryColor;
            settings.accessoryPrimaryColor = ShipSelectionUIManager.Current.CurrentPreset.AccessoryPrimaryColor;
            settings.accessorySecondaryColor = ShipSelectionUIManager.Current.CurrentPreset.AccessorySecondaryColor;
            settings.glowColor = ShipSelectionUIManager.Current.CurrentPreset.GlowColor;

            ShipAssetHashName = IsBuiltinShip ? shipInfo.assetHashName : ShipImporter.GenerateAssetHashName((int)netId.Value, shipInfo.playerShellObject, false, false, false, true);
            ShipSettings = settings;

            if(!IsBuiltinShip) {
                ShipID = (int)netId.Value;
                Debug.Log("GamePlayerInfo netId = " + netId.Value);
                ShipImporter.Current.ImportGamePlayer(this, settings);
            }

            ColorPreset = new ShipPreset("Temp Preset", false, settings);

            ShipReady = true;

            if(NetworkServer.active)
                IsShipDownloaded = true;
            else
                ShareShip.DelayedSend();

            if(ReInput.isReady)
                SetRewiredPlayer(0);
        }

        public override void OnStartClient() {
            if(!NetworkServer.active) {
                Debug.Log("GamePlayerInfo OnStartClient");
                Sync_SteamId(_steamId);
                Sync_Username(_username);
                Sync_LobbyPlayerInfoID(_lobbyPlayerInfoID);
            }
        }

        public override void OnStartServer() {
            Debug.Log("OnStartServer SteamID: " + SteamId);

            SetPlayerHasShip(SteamId);
        }

        void Update() {
            if(NetworkClient.active)
                if(hasAuthority)
                    if(_isDead) {
                        if(PlayerProxy.Players.Count > 0)
                            if(GameManager.Current.ShooterCameraComponent.Target == null) {
                                var player = PlayerProxy.Players.FirstOrDefault();

                                if(player != null) {
                                    SpectatorHUD.SetSpectatedPlayer(player);

                                    if(!SpectatorHUD.Current.IsOpen)
                                        SpectatorHUD.Current.Open();

                                    InGameUIManager.Current.playerInfoContainer.SetActive(false);
                                }
                            }

                        if(RewiredPlayer.GetButtonDown(RewiredActions.Default_Overview))
                            ToggleOverview.ToggleOverviewStatus();

                        if(RewiredPlayer.GetButtonDown(RewiredActions.Menu_Menu_Tab_Navigation))
                            SpectatorHUD.ChangeSpectatedPlayer(1);

                        if(RewiredPlayer.GetNegativeButtonDown(RewiredActions.Menu_Menu_Tab_Navigation))
                            SpectatorHUD.ChangeSpectatedPlayer(-1);
                    }
        }

        public bool DoAllPlayersHaveShip() {
            foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                if(!ShipUploadedToPlayers.Contains(player.steamId))
                    return false;

            return true;
        }

        public bool HasSkillPoints(int value) {
            return AttachedEntity?.CurrentUpgradeSet.SkillPoints >= value;
        }

        void OnDisable() {
            PlayerInfos.Remove(this);

            //if(hasAuthority)

            //    SkillLayoutProxy.OnLevelChanged.RemoveListener(onLevelChangedAction);
        }

        void OnEnable() {
            if(PlayerInfos == null) PlayerInfos = new HashSet<GamePlayerInfo>();

            PlayerInfos.Add(this);
        }

        public void RemoveSkillPoints(int amount) {
            AttachedEntity.CurrentUpgradeSet.SkillPoints -= amount;
            AttachedEntity.Listener.RaiseEvent(PlayerActions.ChangedSkillPoints, AttachedEntity.ToArgs());
        }

        public void ResetPlayerForNewGame() {
            AttachedEntity = null; //TODO: [GamePlayerInfo] Unload PlayerEntity
            _isSpawned = false;
            _isDead = false;
        }

        public void SetDead() {
            _isDead = true;
            AttachedEntity = null;
        }

        public void SetPlayerHasShip(ulong steamId, bool remove = false) {
            //Debug.Log("SetPlayerHasShip " + remove);

            if(remove)
                ShipUploadedToPlayers.Remove(steamId);
            else
                ShipUploadedToPlayers.Add(steamId);

            if(_lobbyPlayerInfo != null)
                _lobbyPlayerInfo.SetReady(DoAllPlayersHaveShip());
        }

        public void SetRewiredPlayer(int playerID) {
            RewiredPlayer = ReInput.players.GetPlayer(playerID);
        }

        public void SetSkillPoints(int value) {
            AttachedEntity.CurrentUpgradeSet.SkillPoints = value;

            if(AttachedEntity != null)
                AttachedEntity.Listener.RaiseEvent(PlayerActions.ChangedSkillPoints, AttachedEntity.ToArgs());

            NetworkUpgradeManager.Current.Server_SetPlayerSkillPoints(gameObject, value);
        }

        [ContextMenu("Spawn Player", true)]
        bool SpawnPlayerValidate() {
            return NetworkServer.active;
        }

        [ContextMenu("Spawn Player")]
        [Server]
        public void SpawnPlayer() {
            if(AttachedEntity != null) return;

            var gameObject = PlayerSpawnManager.Current.SpawnPlayerGameObject(connectionToClient, this);
            var entityId = GameManager.Current.GetUniqueEntityId();
            var networkIdentity = gameObject.GetComponent<NetworkIdentity>();
            GameDebug.Log($"Spawn Player EntityId:{entityId}");
            Rpc_CreatePlayerEntity(netId, networkIdentity.netId, entityId);

            _isSpawned = true;
            _isDead = false;
        }

        [ClientRpc]
        void Rpc_CreatePlayerEntity(NetworkInstanceId playerInfoNetId, NetworkInstanceId gameObjectNetId, uint entityId) {
            var localGamePlayerInfoObject = NetworkServer.FindLocalObject(playerInfoNetId);
            var localPlayerGameObject = NetworkServer.FindLocalObject(gameObjectNetId);

            AttachedEntity = _originalPlayerEntity.CreateInstance() as PlayerEntity;
            AttachedEntity.PlayerInfo = this;
            AttachedEntity.Id = entityId;
            AttachedEntity.CurrentUpgradeSet.Id = SteamId.ToString();
            AttachedEntity.Owner = hasAuthority;

            HealthManager.UpdateHealth(AttachedEntity);

            if(hasAuthority) {
                //PlayerSpawnManager.Current.CurrentPlayerEntity = AttachedEntity;
                PlayerSpawnManager.Current.CurrentPlayerEntityId = entityId;
            }

            AttachedEntity.SetGameObject(localPlayerGameObject);

            PlayerProxy.Players.Add(AttachedEntity);
            AttachedEntity.Listener.RaiseEvent(EntityActions.Spawned, AttachedEntity.ToArgs());

            if(hasAuthority) {
                UIManager.Current.inGameUIManager.RemoveSkillButtons();
                UIManager.Current.inGameUIManager.GenerateSkillButtons(this);
            }

            AttachedEntity.InitializeSkillLayout();

            if(hasAuthority) {
                foreach(var button in UIManager.Current.inGameUIManager.uiSkillButtons)
                    button.RefreshAll();

                //SkillLayoutProxy.OnLevelChanged.AddListener(onLevelChangedAction);

                var levelUpEffect = localPlayerGameObject.GetComponentInChildren<LevelUpEffect>();

                foreach(var element in AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    element.Listener.Subscribe(EntityActions.SkillLevelChanged, _ => LevelUp());

                void LevelUp() {
                    levelUpEffect?.LeveledUp();
                    AttachedEntity.PlaySound("Level Up");
                }
            }
        }

        [Server]
        public void StartSpawnTimer() {
            StartCoroutine(SpawnRoutine(1.5f));
        }

        IEnumerator SpawnRoutine(float delay) {
            if(_isSpawned) yield break;

            Target_PlaySoundEffect(connectionToClient);

            yield return new WaitForSeconds(delay);

            SpawnPlayer();
        }

        [TargetRpc]
        void Target_PlaySoundEffect(NetworkConnection connection) {
            CameraSoundController.Current.PlaySound("Spawn");
            var effectIndex = PlayerSpawnManager.Current.playerEffectsPool.IndexFromPrefabName("Player Spawn Particles");
            PlayerSpawnManager.Current.playerEffectsPool.Spawn(effectIndex, new Vector3(0, 0, 0), Quaternion.Euler(-90, 0, 0));
        }

        void Sync_IsDead(bool isDead) {
            _isDead = isDead;

            if(_isDead && hasAuthority)
                GameManager.Current.ShooterCameraComponent.Target = null;
        }

        void Sync_LobbyPlayerInfoID(NetworkInstanceId netID) {
            _lobbyPlayerInfoID = netID;

            if(!_lobbyPlayerInfoID.IsEmpty()) {
                _lobbyPlayerInfo = ClientScene.FindLocalObject(_lobbyPlayerInfoID)?.GetComponent<LobbyPlayerInfo>();
                _lobbyPlayerInfo?.SetReady(NetworkServer.active ? DoAllPlayersHaveShip() : _lobbyPlayerInfo.Ready);
                //_lobbyPlayerInfo.SetAvatar();
            }
        }

        void Sync_SteamId(ulong steamId) {
            _steamId = steamId;
        }

        void Sync_Username(string username) {
            _username = username;
        }
    }
}