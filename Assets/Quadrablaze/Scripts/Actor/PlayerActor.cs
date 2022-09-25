using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Effects;
using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class PlayerActor : Actor {

        [SerializeField]
        int _shipId;

        [SerializeField]
        Material _playerMaterial;

        [SerializeField]
        Transform _meshTransform;

        [SerializeField]
        Canvas _nametagCanvas;

        [SerializeField]
        TMP_Text _nametagText;

        List<RotateTransform> rotateTransforms;
        bool _healthEventsInitialized = false;

        UnityAction<int> onLeveledChangedMethod;
        UnityAction<int> onLeveledUpMethod;

        #region Properties
        public override ActorTypes ActorType {
            get { return ActorTypes.Player; }
        }

        public Transform MeshTransform {
            get { return _meshTransform; }
            set { _meshTransform = value; }
        }

        public Nametag NametagComponent { get; private set; }

        public UnityEvent OnServerShoot { get; private set; }

        public GamePlayerInfo PlayerInfo { get; set; }

        public PlayerInput PlayerInputComponent { get; private set; }

        public Material PlayerMaterial {
            get { return _playerMaterial; }
            set { _playerMaterial = value; }
        }
        #endregion

        public override void OnStartServer() {
            base.OnStartServer();

            PlayerSpawnManager.Current.OnPlayerSpawned.InvokeEvent(gameObject);

            if(!_healthEventsInitialized) {
                HealthGroupComponent.MainHealthLayer.HealthComponent.OnChangedHealth.AddListener(healthValue =>
                SendHealthMessage(NetMessageType.Client_PlayerSetHealth, healthValue));

                HealthGroupComponent.MainHealthLayer.HealthComponent.OnChangedMaxHealth.AddListener(maxHealthValue =>
                SendHealthMessage(NetMessageType.Client_PlayerSetMaxHealth, maxHealthValue));

                _healthEventsInitialized = true;

                void SendHealthMessage(short messageId, float value) {
                    if(gameObject.activeInHierarchy && NetworkServer.active)
                        NetworkSendHealthValueToAll(messageId, value);
                }
            }
        }

        [TargetRpc]
        public void TargetRpc_StartPlayerRespawned(NetworkConnection connection) {
            CameraSoundController.Current.PlaySound("Respawn");

            StartCoroutine(StartRoutine(true));
        }

        void AddPlayerComponents() {
            var collisionTrigger = gameObject.AddComponent<CollisionTrigger>();

            collisionTrigger.CollisionMask = LayerMask.GetMask("Enemy", "Mine");
            collisionTrigger.SkillTimer.Active = true;

            if(!gameObject.GetComponent<PoolUser>())
                gameObject.AddComponent<PoolUser>();

            //TODO: These components need to be redone in ActorEntity
            gameObject.AddComponent<InvincibilityTime>();
            gameObject.AddComponent<CameraSound>();
            gameObject.AddComponent<PausableObject>();
        }

        public override void Initialize() {
            if(OnServerShoot == null) OnServerShoot = new UnityEvent();

            base.Initialize();
            _isInitialized = false;

            NametagComponent = GetComponentInChildren<Nametag>(true);

            OnAssignedSkill.AddListener(SetupNewSkill);

            _isInitialized = true;
        }

        void OnActionInput(int actionIndex) {
            //return;
            //var skillId = UIManager.Current.abilityWheel.GetSkillId(actionIndex);

            //if(!string.IsNullOrEmpty(skillId)) {
            //    var element = CurrentUpgradeSet.CurrentSkillLayout.GetSkill(skillId);

            //    NetworkSkillControllerComponent.TryUseAbility(CurrentUpgradeSet, element);
            //}
        }

        void OnMovementInput(Vector3 input, BaseMovementController baseMovementController) {
            baseMovementController.Move(input);

            //switch(baseMovementController.MovementStyle) {
            //    case BaseMovementController.MovementType.Directional:
            //        if(hasAuthority)
            //            Cmd_OnMoveDirectionChanged(input);

            //        break;
            //    case BaseMovementController.MovementType.Arcade:
            //        if(hasAuthority)
            //            Cmd_OnMoveDirectionChanged(transform.rotation * input);

            //        break;
            //}
        }

        void OnMovementInputDash(Dash dash, Vector3 input, BaseMovementController baseMovementController) {
            switch(baseMovementController.MovementStyle) {
                case BaseMovementController.MovementType.Directional:
                    dash.Direction = input;

                    break;
                case BaseMovementController.MovementType.Arcade:
                    dash.Direction = transform.rotation * input;

                    break;
            }
        }

        void OnUseAbilityWheel(int activeAbilityIndex) {
            //var element = CurrentUpgradeSet.CurrentSkillLayout.GetActiveSkill(activeAbilityIndex);
            //NetworkSkillControllerComponent.TryUseAbility(CurrentUpgradeSet, element);
        }

        public override void PoolInstantiate(PoolUser user) {
            AddPlayerComponents();
            if(!user) user = gameObject.GetComponent<PoolUser>();
            base.PoolInstantiate(user);
            SetupPlayer();
            //SetupUI();
        }

        void SetupPlayer() {
            //TODO: These components need to be redone in ActorEntity
            var cameraSound = GetComponent<CameraSound>();
            var health = GetComponent<Health>();
            var invincibilityTime = GetComponent<InvincibilityTime>();
            var levelUpEffect = GetComponentInChildren<LevelUpEffect>();

            onLeveledChangedMethod = level => {
                HealthManager.UpdateHealth();

                if(NetworkServer.active) health.Reset();
            };

            onLeveledUpMethod = level => {
                if(gameObject.activeInHierarchy) {
                    levelUpEffect.LeveledUp();
                    cameraSound.PlaySound("Level Up");
                }
            };

            health.OnDeath.AddListener(s => {
                if(NetworkServer.active) Kill();

                UserComponent.SpawnHere("Explosions");
            });

            OnPermadeath.AddListener(() => { if(hasAuthority) ToggleOverview.SetOverviewStatus(false); }); //TODO: Move this

            SetupPlayerInputEvents();
        }

        public void SetMaterialColors() {
            //PlayerMaterial = ShipImporter.Current.ApplyDefaultMaterial(MeshTransform.gameObject, PlayerInfo.ShipSettings);
            PlayerMaterial = new Material(ShipImporter.defaultMaterial);

            PlayerInfo.ColorPreset.SetMaterialValues(PlayerMaterial);

            foreach(var renderer in MeshTransform.GetComponentsInChildren<MeshRenderer>(true))
                renderer.sharedMaterial = PlayerMaterial;
        }

        void SetupPlayerInputEvents() {
            PlayerInputComponent = GetComponent<PlayerInput>();
            PlayerInputComponent.onOverviewInput.AddListener(ToggleOverview.ToggleOverviewStatus);

            //PlayerInputComponent.onShootInput.AddListener(NetworkShoot);
            PlayerInputComponent.onShootStartInput += () => NetworkShoot(true);
            PlayerInputComponent.onShootStopInput += () => NetworkShoot(false);

            rotateTransforms = new List<RotateTransform>(GetComponentsInChildren<RotateTransform>());
            PlayerInputComponent.onShootDirectionInput.AddListener(ShootDirectionChanged);

            PlayerInputComponent.onMovementInputChanged.AddListener(s => OnMovementInput(s, BaseMovementControllerComponent));

            PlayerInputComponent.onUseAbilityWheelInput.AddListener(OnUseAbilityWheel);
            PlayerInputComponent.onActionInput.AddListener(OnActionInput);
        }

        void SetupNewSkill(SkillLayoutElement skillLayoutElement) {
            //if(skillLayoutElement.IsWeapon) return;

            //if(CurrentUpgradeSet.CurrentSkillLayout.SkillLookupTable.TryGetValue(skillLayoutElement, out SkillExecutor skillExecutor))
            //    if(skillExecutor is DashSkillExecutor)
            //        PlayerInputComponent.onMovementInputChanged.AddListener(s => OnMovementInputDash(skillExecutor as DashSkillExecutor, s, BaseMovementControllerComponent));
        }

        IEnumerator StartServerRoutine() {
            yield return new WaitForEndOfFrame();

            if(!hasAuthority)
                NetworkSendHealthValue(NetMessageType.Client_PlayerSetMaxHealth, HealthGroupComponent.MainHealthLayer.HealthComponent.MaxValue);
        }

        IEnumerator StartRoutine(bool hasRespawned) {
            while(!IsInitialized)
                yield return null;

            if(!hasRespawned) {
                InGameUIManager.Current.ShowHelp();

                //UIManager.Current.inGameUIManager.GenerateSkillButtons(PlayerInfo);

                UIManager.Current.abilityPieWheel.RemoveAllIcons();
                UIManager.Current.abilityPieWheel.SetSlices(0);
                UIManager.Current.abilityPieWheel.EnableWheel(false);
            }

            FindObjectOfType<ShooterCamera>().Target = transform;

            if(NetworkServer.active)
                GlobalTargetController.GetController("Global Player Target").Target = transform;

            WeaponUpgradeShipRenderController.Current.CreateShipRenderObject(ShipImporter.Current.localShipData[QuadrablazeSteamNetworking.Current.MyPlayerInfo.SelectedShipID].rootMeshObject);
            PlayerSpawnManager.Current.RefreshPlayerHUD();
            //PlayerSpawnManager.Current.OnPlayerSpawned.InvokeEvent(gameObject);

            HasRespawned = false;
        }

        void NetworkShoot() {
            //var message = new ShootMessage(PlayerInputComponent.ShootDirectionInput);

            //Cmd_OnNetworkShoot(message);
        }
        void NetworkShoot(bool flag) {
            //Cmd_OnNetworkShootFlag(new ShootEnableMessage(flag, PlayerInputComponent.ShootDirectionInput));
        }

        void ShootDirectionChanged(Vector3 direction) {
            //Cmd_OnShootDirectionChanged(direction);
            rotateTransforms.ForEach(s => s.PointInDirection(direction));
        }

        [Command]
        void Cmd_OnNetworkShootFlag(ShootEnableMessage message) {
            rotateTransforms.ForEach(s => s.PointInDirection(message.shootDirection));

            SetShootingStatus(message.shootFlag);
        }

        [Command]
        public void Cmd_OnShootDirectionChanged(Vector3 direction) {
            //var turretExecutor = CurrentUpgradeSet?.CurrentSkillLayout?.GetFirstElementWithExecutor<TurretSkillExecutor>();

            //if(turretExecutor != null)
            //    turretExecutor.DeployPushDirection = direction;

            //Rpc_OnShootDirectionChanged(direction);

            //foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
            //    if(connectionToClient != connection)
            //        Target_OnShootDirectionChanged(connection, direction);
        }

        [TargetRpc]
        void Target_OnShootDirectionChanged(NetworkConnection connection, Vector3 direction) {
            rotateTransforms.ForEach(s => s.PointInDirection(direction));
            PlayerInputComponent.ShootDirectionInput = direction;
        }

        [ClientRpc]
        void Rpc_OnShootDirectionChanged(Vector3 direction) {
            if(!hasAuthority)
                rotateTransforms.ForEach(s => s.PointInDirection(direction));
        }

        [Command]
        void Cmd_OnMoveDirectionChanged(Vector3 direction) {
            //var dashController = GetSkillController<DashController>();

            //dashController.Direction = direction; // TODO Change to new Dash skill executor

            //CurrentSkillLayout.SkillElements

            //Debug.LogError("Cmd_OnMoveDirectionChanged");

            //Rpc_OnMoveDirectionChanged(direction);
            //foreach(var player in QuadrablazeNetworkManager.Current.Players) {
            //    if(player.playerInfo && player.playerInfo.ActorObject) {
            //        //Debug.LogError("id:" + player.connection.connectionId + " ActorObject:" + player.playerInfo.ActorObject.netId + " netId:" + netId);

            //        if(player.playerInfo.ActorObject.netId != netId) {
            //            //Debug.LogError("attempting:" + player.connection.connectionId + " Target_OnMoveDirectionChanged");
            //            //Debug.LogError("to:" + player.connection.connectionId + " Target_OnMoveDirectionChanged");
            //            Cmd_OnMoveDirectionChanged(player.connection, direction);
            //        }
            //    }
            //}
        }

        [ClientRpc]
        void Rpc_OnMoveDirectionChanged(Vector3 direction) {
            if(!hasAuthority)
                //Debug.LogError("Target_OnMoveDirectionChanged");
                PlayerInputComponent.onMovementInputChanged.InvokeEvent(direction);
        }

        void NetworkSendHealthValue(short messageID, float value) {
            NetworkSendHealthValue(this, messageID, value);
        }
        public static void NetworkSendHealthValue(PlayerActor actor, short messageID, float value) {
            var writer = new NetworkWriter();

            writer.StartMessage(messageID);
            writer.Write(actor.gameObject);
            writer.Write(value);
            writer.FinishMessage();

            actor.PlayerInfo?.connectionToClient?.SendWriter(writer, 0);
        }

        void NetworkSendHealthValueToAll(short messageID, float value) {
            NetworkSendHealthValueToAll(gameObject, messageID, value);
        }
        public static void NetworkSendHealthValueToAll(GameObject gameObject, short messageID, float value) {
            var writer = new NetworkWriter();

            writer.StartMessage(messageID);
            writer.Write(gameObject);
            writer.Write(value);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer, Channels.DefaultReliable, false);
        }

        //TODO: Setup health syncing with Entity system
        public static void Client_SetHealth(NetworkMessage netMessage) {
            //var playerObject = netMessage.reader.ReadGameObject();

            //if(playerObject) {
            //    var value = netMessage.reader.ReadSingle();

            //    if(playerObject) {
            //        var playerActor = playerObject.GetComponent<PlayerActor>();

            //        HealthManager.UpdateHealth(null);

            //        playerActor.HealthGroupComponent.MainHealthLayer.HealthComponent.Value = value;
            //    }
            //}
        }

        public static void Client_SetMaxHealth(NetworkMessage netMessage) {
            //var playerObject = netMessage.reader.ReadGameObject();

            //if(playerObject) {
            //    var value = netMessage.reader.ReadSingle();

            //    var playerActor = playerObject.GetComponent<PlayerActor>();

            //    playerActor.HealthGroupComponent.MainHealthLayer.HealthComponent.MaxValue = value;
            //    HealthManager.UpdateHealth(null);
            //}
        }

        public static void Client_SetShieldHealth(NetworkMessage netMessage) {
            var playerObject = netMessage.reader.ReadGameObject();

            if(playerObject) {
                //var value = netMessage.reader.ReadSingle();
                //var playerActor = playerObject.GetComponent<PlayerActor>();
                //var shieldExecutor = playerActor.CurrentUpgradeSet.CurrentSkillLayout.GetFirstElementWithExecutor<ShieldSkillExecutor>();

                //if(shieldExecutor != null) {
                //    var healthComponent = shieldExecutor.ShieldHealth;

                //    HealthManager.Current.UpdateHealth(playerActor);

                //    healthComponent.Value = value;

                //    if(healthComponent.Value < healthComponent.MaxValue)
                //        shieldExecutor.RegenerationTimer.Start(true);

                //    shieldExecutor.UpdateShieldActive();
                //}
            }
        }

        public static void Client_SetShieldMaxHealth(NetworkMessage netMessage) {
            var playerObject = netMessage.reader.ReadGameObject();

            if(playerObject) {
                //var value = netMessage.reader.ReadSingle();

                //var playerActor = playerObject.GetComponent<PlayerActor>();
                //var shieldExecutor = playerActor.CurrentUpgradeSet.CurrentSkillLayout.GetFirstElementWithExecutor<ShieldSkillExecutor>();

                //if(shieldExecutor != null)
                //    shieldExecutor.ShieldHealth.MaxValue = value;
            }
        }

        //[RegisterNetworkHandlers]
        //public static void RegisterHandlers() {
        //    QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_PlayerSetHealth, Client_SetHealth);
        //    QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_PlayerSetShieldHealth, Client_SetShieldHealth);
        //    QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_PlayerSetMaxHealth, Client_SetMaxHealth);
        //    QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_PlayerSetShieldMaxHealth, Client_SetShieldMaxHealth);
        //}
    }
}