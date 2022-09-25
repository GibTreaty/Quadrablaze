using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Boss;
using Quadrablaze.Entities;
using Quadrablaze.GameModes;
using Quadrablaze.Skills;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using YounGenTech.Entities;
using YounGenTech.Entities.Weapon;
using YounGenTech.PoolGen;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class GameManager : MonoBehaviour {

        public static readonly ProxyListener<ProxyAction> Proxy = new ProxyListener<ProxyAction>();

        public static GameManager Current { get; private set; }

        public bool ignoreCollisions = true;

        [SerializeField]
        QuadrablazeSteamNetworking _networkManager;

        [SerializeField]
        OverviewCamera _overviewCameraComponent;

        [SerializeField]
        ShooterCamera _shooterCameraComponent;

        [SerializeField]
        GameAudioPlayer _gameAudioPlayer;

        [SerializeField]
        int _difficultyLevel;

        [SerializeField]
        NewGameOptions _options = new NewGameOptions(GameNetworkType.SinglePlayer);

        [Header("Game Modes"), SerializeField]
        GameModeDatabase _gameModes;

        [Header("Arena"), SerializeField]
        float _arenaRadius = 50;

        [SerializeField]
        GameObject _arenaForceField;

        [SerializeField]
        Material _arenaGridMaterial;

        [SerializeField]
        int _selectedShip = -1;

        [SerializeField]
        XPParticleManager _xpParticles;

        [SerializeField]
        TMPro.TMP_Text _versionText;

        Coroutine _gameOverCoroutine;

        [SerializeField]
        int _level;

        uint _xp;
        uint _nextXp;

        uint lastEntityId = 0;

        #region Properties
        public HashSet<uint> ActorEntityIds { get; protected set; }
        public float ArenaRadius {
            get { return _arenaRadius; }
            set { _arenaRadius = value; }
        }
        public BossInfoDatabase BossDatabase { get; set; }
        public PoolManager BossPool { get; set; }
        public GameMode CurrentGameMode { get; private set; }
        public int CurrentGameModeID { get; set; }
        public List<Entity> Entities => Entity.Entities;
        public string GameVersion {
            get { return Application.version; }
        }
        public int Level {
            get { return _level; }
            set { _level = value; }
        }
        public NewGameOptions Options {
            get { return _options; }
            set { _options = value; }
        }
        public OverviewCamera OverviewCameraComponent {
            get { return _overviewCameraComponent; }
        }
        public EnemyInfoDatabase MinionDatabase { get; set; }
        public PoolManager MinionPool { get; set; }
        public uint NextXP {
            get { return _nextXp; }
            set { _nextXp = value; }
        }
        public HashSet<uint> PlayerEntityIds { get; protected set; }
        public int SelectedShip {
            get { return _selectedShip; }
            set { _selectedShip = value; }
        }
        public ShooterCamera ShooterCameraComponent {
            get { return _shooterCameraComponent; }
        }
        public uint XP {
            get { return _xp; }
            set { _xp = value; }
        }
        #endregion

        public void AddXP(uint value) {
            SetXP(XP + value);
        }

        public void ClearEntities() {
            foreach(var entity in new List<Entity>(Entities)) {
                entity.DestroyEntity();

                switch(entity) {
                    case ActorEntity actorEntity: actorEntity.UnloadEntity(); break;
                        //case ProjectileEntity projectileEntity: projectileEntity.DestroyEntity(); break;
                }
            }

            Entities.Clear();
        }

        void Client_EndToLobby(NetworkMessage netMsg) {
            UIManager.Current.ShowGameOver(true);

            StartCoroutine(GameOverCoroutine(EndToLobby));
        }

        void CheckGameEnd() {
            if(PlayerProxy.Players.Count == 0) {
                UIManager.Current.ShowGameOver(true);
                GameDebug.Log("Game Ended - Returning to lobby");

                if(Options.GameNetworkConnectionType == GameNetworkType.SinglePlayer) {
                    UIManager.Current.ShowGameOver(true);

                    StartCoroutine(GameOverCoroutine(QuadrablazeSteamNetworking.Current.Stop));
                }
                else {
                    NetworkServer.SendToAll(NetMessageType.Client_EndToLobby, new EmptyMessage());
                }
            }
        }

        // TODO: GameManager - Use CreateEntity to spawn the player
        [GameNetworkListener]
        void Internal_CreateEntity(NetworkMessage message) {
            var entityTypeByte = message.reader.ReadByte();
            var entityIndex = message.reader.ReadInt32();
            var entityId = message.reader.ReadUInt32();
            var gameObject = message.reader.ReadGameObject();

            ActorEntity entity = null;

            if(Enum.TryParse(entityTypeByte.ToString(), out CreateEntityType entityType))
                switch(entityType) {
                    case CreateEntityType.Player:
                        //entity = MinionDatabase.Entities[entityIndex].CreateInstance() as MinionEntity;
                        break;
                    case CreateEntityType.Minion:
                        var databaseEntry = MinionDatabase.Entities[entityIndex];

                        entity = databaseEntry.CreateInstance() as MinionEntity;

                        if(!databaseEntry.Spawnable)
                            if(CurrentGameMode is IEnemySpawnController spawnController)
                                spawnController.CurrentEnemyController.MinionCount++;

                        break;
                    case CreateEntityType.Boss:
                        entity = BossDatabase.Entities[entityIndex].CreateInstance() as Entities.BossEntity;
                        break;
                }

            entity.Id = entityId;

            if(entity.HealthSlots != null)
                if(entity.HealthSlots.Length > 0)
                    HealthManager.UpdateHealth(entity);

            entity.SetGameObject(gameObject);
            entity.InitializeSkillLayout();

            if(entity is Entities.BossEntity bossEntity) {
                if(CurrentGameMode is ITimedBossSpawnController bossController) {
                    TimedBossSpawnController.BossesSpawned++;
                    TimedBossSpawnController.InvokeBossSpawned();
                }

                EnemyProxy.SpawnedBoss = gameObject.GetComponent<BossController>();
                bossEntity.SetStage(1);
                CameraSoundController.Current.PlaySound("Boss Spawn", true);
                EnemyProxy.Proxy.RaiseEvent(EntityActions.Spawned, bossEntity.ToArgs());
            }
        }

        public GameObject CreateEntity(CreateEntityType entityType, int databaseIndex, Vector3 position, Quaternion rotation) {
            var entityId = GetUniqueEntityId();
            GameObject gameObject = null;

            switch(entityType) {
                //case CreateEntityType.Player: pool = null; break;
                case CreateEntityType.Minion:
                    gameObject = MinionPool.Spawn(MinionPool.IndexFromPrefabID(databaseIndex), position, rotation).gameObject;
                    break;
                case CreateEntityType.Boss:
                    gameObject = BossPool.Spawn(BossPool.IndexFromPrefabID(databaseIndex), position, rotation).gameObject;
                    break;
            }

            StartCoroutine(CreateEntityRoutine(entityType, databaseIndex, entityId, gameObject));

            return gameObject;
        }

        IEnumerator CreateEntityRoutine(CreateEntityType entityType, int index, uint entityId, GameObject gameObject) {
            yield return null;

            QuadrablazeSteamNetworking.SendGameNetworkMessage(Internal_CreateEntity, writer => {
                writer.Write((byte)entityType);
                writer.Write(index);
                writer.Write(entityId);
                writer.Write(gameObject);
            });
        }

        public void EndFromDisconnect() {
            EndToMenu();
            EndGame();
        }

        public void EndGame() {
            if(_gameOverCoroutine != null)
                StopCoroutine(_gameOverCoroutine);

            ResetGameFunctions();

            RoundManager.EndRound();
            UIManager.Current.SetMenuScene(UIManager.MenuScene.MainMenu);
            UIManager.Current.GoToMenu("Main");
            //UIManager.Current.EnableMainMenu(true);
        }

        public void EndInDefeat() {
            UIManager.Current.ShowGameOver(true);
            EndToMenu();

            _gameOverCoroutine = StartCoroutine(GameOverCoroutine(EndGame));
        }

        public void EndInVictory() {
            UIManager.Current.ShowGoalCompleted(true);
            EndToMenu();
        }

        public void EndToLobby() {
            UIManager.Current.SetMenuScene(UIManager.MenuScene.MainMenu);
            UIManager.Current.inGameUIManager.RemoveSkillButtons();
            Level = 0;

            RoundManager.EndRound();

            if(NetworkServer.active)
                foreach(var poolManager in PoolManager.GetPools())
                    poolManager.DespawnAll();

            ResetGameFunctions();

            UIManager.Current.EnablePlayerUI(false);
            UIManager.Current.EnableMultiplayerLobby(true);
            UIAbilityBar.Current.RemoveAllIcons();
            UIManager.Current.abilityWheel.ResetSkills();

            if(NetworkServer.active) {
                foreach(var player in QuadrablazeSteamNetworking.Current.Players) {
                    // TODO: Possibly do this on the Client too?
                    player.playerInfo.ResetPlayerForNewGame();
                    GameLobbyManager.Current.LobbyUI.AddPlayer(player, player.serverToClientConnection);
                }
            }
        }

        public void EndToMenu() {
            UIManager.Current.EnableGame(false);
            UIManager.Current.inGameUIManager.RemoveSkillButtons();
            Level = 0;

            RoundManager.EndRound();
            ResetGameFunctions();
            UIManager.Current.playButton.Select();
        }

        public void Initialize() {
            //Proxy.OnListenEvent += Proxy_OnEvent;
            Proxy.Subscribe(GameManagerActions.CreateEntity, Proxy_CreateEntity);
            QuadrablazeSteamNetworking.RegisterGameNetworkListener(Internal_CreateEntity, this);

            _arenaForceField.SetActive(true);
            _arenaForceField.transform.localScale = Vector3.one * ArenaRadius * 2;
            _arenaGridMaterial.SetTextureScale("_MainTex", Vector2.one * ArenaRadius);

            //GameGoal.Current.OnReachedGoal.AddListener(EndInVictory);

            //BossSpawner.Current.OnBossSpawned.AddListener(() => AdvancedWaveManager.Current.SpawningActive = false);
            //BossSpawner.Current.OnBossDefeated.AddListener(() => AdvancedWaveManager.Current.SpawningActive = true);

            ActorEntity.Proxy.Subscribe(EntityActions.Created, ActorEntity_Created);
            ActorEntity.Proxy.Subscribe(EntityActions.Destroyed, ActorEntity_Destroyed);

            PlayerProxy.Proxy.Subscribe(EntityActions.Created, PlayerProxy_Created);
            PlayerProxy.Proxy.Subscribe(EntityActions.Destroyed, PlayerProxy_Destroyed);
            PlayerProxy.Proxy.Subscribe(EntityActions.Permadeath, PlayerProxy_Permadeath);

            _versionText.gameObject.SetActive(true);
            _versionText.text = GameVersion;

            //LoadPrefs(); // TODO Enable this if IP support is re-added

            ActorEntityIds = new HashSet<uint>();
            PlayerEntityIds = new HashSet<uint>();

            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_EndToLobby, Client_EndToLobby);
        }

        void ActorEntity_Destroyed(EventArgs args) {
            var entity = (EntityArgs)args;

            ActorEntityIds.Remove(entity.Id);
        }

        void ActorEntity_Created(EventArgs args) {
            var entity = (EntityArgs)args;

            ActorEntityIds.Add(entity.Id);
        }

        void InitializeGameMode() {
            CurrentGameMode = _gameModes[CurrentGameModeID].InstantiateMode(null);
        }

        public void InvincibilityAndDisableEnemies() {
            if(NetworkServer.active) {
                foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                    player?.playerInfo?.AttachedEntity?.GiveInvincibility(Mathf.Infinity);

                if(CurrentGameMode is MainGameMode) {
                    var gameMode = CurrentGameMode as MainGameMode;

                    gameMode.CurrentEnemyController.SpawnTimer.CurrentTime = Mathf.Infinity;
                    gameMode.CurrentEnemyController.SpawnTimer.Length = Mathf.Infinity;
                    gameMode.CurrentBossController.TimerActive = false;
                }

                //EnemySpawnManager.Current.SpawnTimer.CurrentTime = Mathf.Infinity;
                //EnemySpawnManager.Current.SpawnTimer.Length = Mathf.Infinity;
                //BossSpawner.Current.TimerActive = false;
            }
        }

        public void InvokeGame() {
            UIManager.Current.SetMenuScene(UIManager.MenuScene.InGame);
            UIManager.Current.CloseMenus();

            GameLobbyUI.Close();
            UIManager.Current.DeactivateTitleInstant();
            CameraSoundController.Current.StopSounds();

            UIAbilityBar.Current.RemoveAllIcons();

            UIManager.Current.inGameUIManager.RemoveSkillButtons();
            UIManager.Current.abilityWheel.ResetSkills();
            UIManager.Current.abilityWheel.UpdateActionGlyphs();
            UIManager.Current.abilityPieWheel.RemoveAllIcons();

            SetLevel(1);
            SetXP(0);
        }

        IEnumerator GameOverCoroutine(Action endAction) {
            yield return new WaitForSecondsRealtime(1);
            UIManager.Current.gameOver.CrossFadeAlpha(0, 1, false);
            yield return new WaitForSecondsRealtime(1);
            endAction?.Invoke();
        }

        public ActorEntity GetActorEntity(uint id) {
            foreach(var entity in Entities)
                if(entity is ActorEntity actorEntity)
                    if(entity.Id == id)
                        return actorEntity;

            return null;
        }

        public bool GetActorEntity(GameObject gameObject, out ActorEntity output) {
            var root = gameObject.transform.root;

            foreach(var entity in Entities)
                if(entity is ActorEntity actorEntity)
                    if(entity.CurrentTransform == root) {
                        output = actorEntity;
                        return true;
                    }

            output = null;
            return false;
        }

        /// <summary>Checks if GameObject is an ActorEntity without outputting the entity</summary>
        public bool GetActorEntity(GameObject gameObject) {
            var root = gameObject.transform.root;

            foreach(var entity in Entities)
                if(entity is ActorEntity actorEntity)
                    if(entity.CurrentTransform == root)
                        return true;

            return false;
        }

        public Entity GetEntity(uint id) {
            foreach(var entity in Entities)
                if(entity.Id == id)
                    return entity;

            return null;
        }

        public bool GetProjectileEntity(GameObject gameObject, out YounGenTech.Entities.Weapon.ProjectileEntity output) {
            var root = gameObject.transform.root;

            foreach(var entity in Entities)
                if(entity is YounGenTech.Entities.Weapon.ProjectileEntity projectileEntity)
                    if(entity.CurrentTransform == root) {
                        output = projectileEntity;
                        return true;
                    }

            output = null;
            return false;
        }

        public uint GetUniqueEntityId() {
            return ++lastEntityId;
        }

        public void LevelUp() {
            SetXP(NextXP);
        }

        public void NewGame() {
            NewGame(Options);
        }
        public void NewGame(NewGameOptions options) {
            StartPools();

            switch(options.GameNetworkConnectionType) {
                case GameNetworkType.SinglePlayer:
                    UIManager.Current.CloseMenus();
                    UIManager.Current.SetMenuScene(UIManager.MenuScene.InGame);

                    RoundManager.StartRound();

                    QuadrablazeSteamNetworking.Current.StartServer();
                    StartCoroutine(StartHostGameDelayed());
                    ////StartHostGame();
                    break;

                case GameNetworkType.Host:
                    //_networkManager.StartHost(); 
                    SteamMatchmaking.CreateLobby(QuadrablazeSteamNetworking.DefaultLobbyType, 4);

                    //UIManager.Current.EnableMainMenu(false);
                    UIManager.Current.EnableMultiplayerLobby();
                    break;
                case GameNetworkType.Join:
                case GameNetworkType.Invited:
                case GameNetworkType.Search:
                case GameNetworkType.QuickMatch:
                    UIManager.Current.CloseMenus();

                    UIManager.Current.IsJoiningGame = true;
                    GameLobbyManager.Current.LobbyUI.ConnectingMessage.StartAnimation();
                    break;
            }
        }

        void OnDrawGizmos() {
            if(!Application.isPlaying) {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0, 1));
                Gizmos.color = new Color(0, 1, 0, .5f);
                Gizmos.DrawWireSphere(Vector3.zero, ArenaRadius);
            }
        }

        void OnEnable() {
            Current = this;
        }

        void OnPlayerEntityCreated(PlayerEntity playerEntity) {
            PlayerEntityIds.Add(playerEntity.Id);
        }

        void OnPlayerEntityDestroyed(PlayerEntity playerEntity) {
            PlayerEntityIds.Remove(playerEntity.Id);

            if(playerEntity.Owner)
                PlayerSpawnManager.Current.CurrentPlayerEntityId = 0;
        }

        public void PlayerDied() { //TODO: Reconnect this?
            if(PlayerProxy.Players.Count == 0)
                EndInDefeat();
        }

        void PlayerProxy_Created(EventArgs args) {
            var entity = (EntityArgs)args;

            PlayerEntityIds.Add(entity.Id);
        }

        void PlayerProxy_Destroyed(EventArgs args) {
            var entity = (EntityArgs)args;

            PlayerEntityIds.Remove(entity.Id);

            if(entity.GetEntity<PlayerEntity>().Owner)
                PlayerSpawnManager.Current.CurrentPlayerEntityId = 0;
        }

        void Proxy_CreatedEntity(EventArgs args) {
            // TODO: [GameManager] Finish this
        }

        void PlayerProxy_Permadeath(EventArgs args) {
            CheckGameEnd();
        }

        public void PreInitialize() {
            Current = this;
        }

        void Proxy_CreateEntity(EventArgs args) {
            Debug.Log("Create Entity");
            var createEntity = (CreateEntityArgs)args;
            var gameObject = CreateEntity(createEntity.EntityType, createEntity.Index, createEntity.Position, createEntity.Rotation);

            createEntity.Callback?.Invoke(gameObject);
        }

        public void QuitMultiplayerLobby() {
            if(QuadrablazeSteamNetworking.Current.SteamLobbyId.IsValid())
                QuadrablazeSteamNetworking.Current.Stop();
        }

        public void ResetEntityIds() {
            lastEntityId = 0;
        }

        public void ResetGameFunctions() {
            ClearEntities();
            //Entities.Clear();
            ActorEntityIds.Clear();
            PlayerEntityIds.Clear();

            ResetEntityIds();

            GameDebug.Log("Reset Game Functions", "Reset");

            ShooterCamera shooterCamera = FindObjectOfType<ShooterCamera>();
            shooterCamera.Target = null;

            FindObjectOfType<ShakeyCamera>().Reset();

            var overviewCamera = FindObjectOfType<OverviewCamera>();

            if(!overviewCamera.Status)
                shooterCamera.InstantMove(Vector3.zero);
            else
                overviewCamera.Status = false;

            BossInfoUI.Current.Reset();
            TriformerForceFieldController.Current.EnableForceField(false);

            if(CurrentGameMode != null) {
                CurrentGameMode.EndGame();
                CurrentGameMode = null;
            }

            UIManager.Current.abilityPieWheel.RemoveAllIcons();
            UIManager.Current.abilityPieWheel.SetSlices(0);
            UIManager.Current.abilityPieWheel.EnableWheel(false);
        }

        public void ResetXP() {
            SetXP(0);
        }

        public void SetLevel(int value) {
            if(value == Level) return;

            var oldLevel = Level;
            var args = new EventArgs<int>(Level);

            _level = value;

            NextXP = GetXP(Level);
            Proxy.RaiseEvent(GameManagerActions.LevelChanged, args);

            if(Level > oldLevel) {
                Proxy.RaiseEvent(GameManagerActions.LeveledUp, args);

                HealthManager.UpdateHealth();
                //foreach(var entityId in ActorEntityIds)
                //    HealthManager.UpdateHealth(GetActorEntityById(entityId));
            }

            if(NetworkServer.active)
                NetworkUpgradeManager.Current.Server_SetLevel(Level);
        }

        public void SetXP(uint value) {
            if(value == XP) return;

            _xp = value;

            if(NetworkServer.active) {
                UpdateLevel();
                NetworkUpgradeManager.Current.Server_SetXP(XP);
            }
            
            Proxy.RaiseEvent(GameManagerActions.XPChanged, new EventArgs<uint>(XP));
        }

        public void SpawnBoss() {
            if(CurrentGameMode != null)
                if(CurrentGameMode is MainGameMode mainGameMode)
                    mainGameMode.CurrentBossController.SpawnBoss();
        }

        public void SpawnMine() {
            var randomCircle = UnityEngine.Random.insideUnitCircle;
            var randomPosition = new Vector3(randomCircle.x, 0, randomCircle.y) * (ArenaRadius - 1);

            PoolManager.Spawn("Mines", randomPosition, Quaternion.identity);
        }

        public void StartHostGame() {
            bool canStart = QuadrablazeSteamNetworking.Current.Players.All(player => player.playerInfo ? player.playerInfo.ShipReady : false);

            if(canStart) {
                Debug.Log("StartHostGame");

                InvokeGame();

                NetworkServer.SendToAll(NetMessageType.Client_StartJoinGame, new EmptyMessage());

                foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                    player.playerInfo.StartSpawnTimer();

                SteamMatchmaking.SetLobbyData(QuadrablazeSteamNetworking.Current.SteamLobbyId, "lobbyStatus", "inRound");
                SteamMatchmaking.SetLobbyJoinable(QuadrablazeSteamNetworking.Current.SteamLobbyId, false);
            }
        }

        IEnumerator StartHostGameDelayed() {
            yield return new WaitForEndOfFrame();

            while(QuadrablazeSteamNetworking.Current.MyPlayerInfo == null)
                yield return new WaitForEndOfFrame();

            StartHostGame();
        }

        public void StartJoinGame() {
            Debug.Log("StartJoinGame");

            InitializeGameMode();

            UIManager.Current.EnableGame(true);
            RoundManager.StartRound();
            //UIManager.Current.EnableMultiplayerLobby(false);
            UIManager.Current.CloseMenus();
            UIManager.Current.SetMenuScene(UIManager.MenuScene.InGame);
            //UIManager.Current.GoToParentMenu();
            //UIManager.Current.GoToMenu("Main");
            SpectatorHUD.Current.Close();
        }

        public void StartPools() {
            foreach(var pool in PoolManager.GetPools()) {
                pool.InitializePools();
                pool.RefillPools();
            }
        }

        /// <returns>Amount taken</returns>
        public uint TakeXP(uint value) {
            uint xpBefore = XP;

            if(XP > 0)
                XP = (uint)Math.Max((long)XP - value, 0);

            return xpBefore < value ? xpBefore : value;
        }

        void Update() {
            if(CurrentGameMode != null)
                CurrentGameMode.UpdateMode();

            if(ColliderProjectile.ignoreCollisions != ignoreCollisions)
                ColliderProjectile.ignoreCollisions = ignoreCollisions;

            if(Entities != null)
                foreach(var entity in new List<Entity>(Entities))
                    if(entity is IEntityUpdate item)
                        item.EntityUpdate();
        }

        public bool UpdateLevel() {
            uint rollOverXP = _xp; ;
            bool leveledUp = false;
            int skillPointPayout = 0;
            int level = Level;
            uint nextXp = NextXP;

            while(rollOverXP >= nextXp) {
                if(rollOverXP >= nextXp) rollOverXP -= nextXp;

                leveledUp = true;
                level++;

                skillPointPayout += 1;

                nextXp = GetXP(level);
            }

            SetLevel(level);

            if(skillPointPayout > 0)
                foreach(var player in QuadrablazeSteamNetworking.Current.Players) {
                    int currentSkillPoints = player.playerInfo.AttachedEntity.CurrentUpgradeSet.SkillPoints;

                    player.playerInfo.SetSkillPoints(currentSkillPoints + skillPointPayout);
                }

            XP = rollOverXP;

            return leveledUp;
        }

        public void UpdatePlayerUI() {
            foreach(var upgradeObject in UIManager.Current.inGameUIManager.upgradeContainers) {
                var button = upgradeObject.GetComponent<UISkillButton>();

                button.RefreshAll();
            }
        }

        void UpdatePlayerUI(int level) {
            UpdatePlayerUI();
        }

        public static uint GetXP(int level) {
            //return Mathf.FloorToInt(level + (300 * (Mathf.Pow(2, level / 7f)))) / 4;
            return (uint)level * 5;
        }
    }

    public static class GameManagerActions {
        public static readonly ProxyAction CreateEntity = new ProxyAction();
        public static readonly ProxyAction LevelChanged = new ProxyAction();
        public static readonly ProxyAction LeveledUp = new ProxyAction();
        public static readonly ProxyAction XPChanged = new ProxyAction();
    }

    [Serializable]
    public struct NewGameOptions {

        [SerializeField]
        GameNetworkType _gameNetworkConnectionType;

        #region Properties
        public GameNetworkType GameNetworkConnectionType {
            get { return _gameNetworkConnectionType; }
            set { _gameNetworkConnectionType = value; }
        }
        #endregion

        public NewGameOptions(GameNetworkType gameNetworkConnectionType) {
            _gameNetworkConnectionType = gameNetworkConnectionType;
        }
    }

    public enum GameNetworkType {
        SinglePlayer = 0,
        Host = 1,
        Join = 2,
        Invited = 3,
        Search = 4,
        QuickMatch = 5
    }

    public enum CreateEntityType {
        Player = 0,
        Minion = 1,
        Boss = 2
    }
}