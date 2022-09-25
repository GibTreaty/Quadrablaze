using System;
using Quadrablaze.Entities;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

#pragma warning disable CS0618

// TODO: Player Input - Player input continues moving in the direction they were going in the previous round
namespace Quadrablaze {
    public class PlayerInput : ActorInputBase {

        public bool initialized = false;

        //public Vector3Event onWorldCursorPositionChanged;
        public Vector3Event onMovementInputChanged;
        public IntEvent onActionInput;
        public UnityEvent onShootInput;
        public event Action onShootStartInput;
        public event Action onShootStopInput;
        public UnityEvent onUpgradeMenuInput;
        public UnityEvent onOverviewInput;
        public Vector3Event onShootDirectionInput;
        public IntEvent onUseAbilityWheelInput;

        public float movementInputSendFrequency = .1f;

        float lastSendTime;

        Collider cursorPlane;

        Vector3 _movementInput;
        float _shootAngle;
        Vector3 _shootDirectionInput;
        bool _shootInput;
        bool shootingFlag;

        bool _aimingWithGamepad;
        bool _aimingWithKeyboard;
        bool _aimingWithMouse;
        ControllerType _aimDevice = ControllerType.Mouse;

        float _deadzone;

        //DashController dashController;

        #region Properties
        BaseMovementController BaseMovementControllerComponent { get; set; }

        public Vector3 MovementInput {
            get { return _movementInput; }

            private set {
                _movementInput = value;

                onMovementInputChanged.InvokeEvent(MovementInput);
            }
        }

        Player RewiredPlayer { get; set; }

        public float ShootAngle {
            get { return _shootAngle; }
            set { _shootAngle = value; }
        }

        public Vector3 ShootDirectionFromAngle {
            get { return new Vector3(Mathf.Sin(_shootAngle), 0, Mathf.Cos(_shootAngle)); }
        }

        public Vector3 ShootDirectionInput {
            get { return _shootDirectionInput; }
            set {
                bool authorityFlag = hasAuthority;

                if(authorityFlag)
                    if(value.sqrMagnitude <= .01f) return;

                _shootDirectionInput = value;

                if(authorityFlag)
                    onShootDirectionInput.InvokeEvent(ShootDirectionInput);
            }
        }

        public bool ShootInput {
            get { return _shootInput; }
            private set { _shootInput = value; }
        }
        #endregion

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            base.ActorEntityObjectInitialize(entity);

            BaseMovementControllerComponent = entity.BaseMovementControllerComponent;

            var cursorPlaneObject = GameObject.Find("Cursor Plane");

            if(cursorPlaneObject)
                cursorPlane = cursorPlaneObject.GetComponent<Collider>();

            if(ReInput.isReady)
                SetRewiredPlayer(0);

            initialized = true;

            ResetInput();
            //dashController = actor.GetSkillController<DashController>();
        }

        //void OnDisable() {
        //    ResetInput();
        //}

        void Update() {
            if(!hasAuthority) return;
            if(!ReInput.isReady) return;
            if(RewiredPlayer == null) return;
            if(!UIManager.IsInitialized) return;

            bool skip = true;

            if(UIManager.Current.CurrentMenuItem != null) {
                switch(UIManager.Current.CurrentMenuItem.Name) {
                    case "Ability Wheel":
                    case "Upgrade":
                        skip = false;
                        break;
                }
            }
            else {
                skip = false;
            }

            if(skip) return;

            UpdateMenuInput();

            if(UIManager.Current.UIOpenLastFrame) return;

            UpdateMovementInput();

            if(UIManager.Current.AbilityWheelOpen) return;

            UpdateShootDirectionInput();
            UpdateActionInput();

            if(RoundManager.RoundInProgress)
                UpdateOveriewInput();

            if(ShootInput) onShootInput.InvokeEvent();

            if(ShootInput != shootingFlag)
                if(ShootInput) {
                    onShootStartInput?.Invoke();
                    shootingFlag = true;
                }
                else {
                    onShootStopInput?.Invoke();
                    shootingFlag = false;
                }
        }

        public override float GetNetworkSendInterval() {
            return movementInputSendFrequency;
        }

        void LateUpdate() {
            if(!ReInput.isReady) return;
            if(RewiredPlayer == null) return;

            UpdateShootInput();
        }

        public void ResetInput() {
            MovementInput = Vector3.zero;
            ShootInput = false;
        }

        public void SetRewiredPlayer(int playerID) {
            RewiredPlayer = ReInput.players.GetPlayer(playerID);
            _deadzone = ReInput.mapping.GetInputBehavior(playerID, "Axis").buttonDeadZone;
        }

        void UpdateActionInput() {
            if(!IsImmobilized)
                if(!UIManager.Current.AbilityWheelOpen)
                    for(int i = 0; i < 4; i++)
                        if(RewiredPlayer.GetButtonDown("Action " + (i + 1)))
                            onActionInput.InvokeEvent(i);
        }

        void UpdateMenuInput() {
            bool menuOpen = UIManager.Current.CurrentMenuItem != null;
            bool abilityWheelOpen = menuOpen && UIManager.Current.CurrentMenuItem.Name == "Ability Wheel";
            bool upgradeMenuOpen = menuOpen && UIManager.Current.CurrentMenuItem.Name == "Upgrade";

            if(!abilityWheelOpen) {
                if(RewiredPlayer.GetButtonDown("Ability Wheel"))
                    if(!upgradeMenuOpen) {
                        UIManager.Current.GoToMenu("Ability Wheel");
                    }
            }
            else {
                if(RewiredPlayer.GetButtonUp("Ability Wheel")) {
                    var captureSelectedAbility = UIManager.Current.abilityPieWheel.GetCurrentSlice();

                    if(captureSelectedAbility > 0)
                        if(UIManager.Current.abilityPieWheel.GetDeadzonedSelection().sqrMagnitude > 0) {

                            onUseAbilityWheelInput.InvokeEvent(captureSelectedAbility - 1);
                        }

                    UIManager.Current.CloseMenus();
                }
                else {
                    UIManager.Current.abilityWheel.PollForBind();
                }
            }

            if(abilityWheelOpen) return;

            if(RewiredPlayer.GetButtonDown(RewiredActions.Default_Upgrade)) {
                UIManager.Current.ToggleUpgradeMenu();

                onUpgradeMenuInput.InvokeEvent();

                return;
            }
        }

        void UpdateMovementInput() {
            Vector3 input;

            switch(BaseMovementControllerComponent.MovementStyle) {
                default:
                    input = Vector3.ClampMagnitude(new Vector3(RewiredPlayer.GetAxis(RewiredActions.Default_Horizontal), 0, RewiredPlayer.GetAxis(RewiredActions.Default_Vertical)), 1);
                    break;

                case BaseMovementController.MovementType.Arcade:
                    input = new Vector3(RewiredPlayer.GetAxis(RewiredActions.Default_Horizontal), 0, RewiredPlayer.GetAxis(RewiredActions.Default_Vertical));
                    break;
            }

            var entity = GameManager.Current.GetActorEntity(EntityId);

            if(entity.MovementInterruptTimer.HasElapsed) {
                if(input.sqrMagnitude > 0) {
                    MovementInput = IsImmobilized ?
                        Vector3.zero :
                        input;
                }
                else {
                    if(MovementInput != Vector3.zero)
                        MovementInput = Vector3.zero;
                }
            }

            if(ClientScene.readyConnection != null)
                if(Time.time - lastSendTime > movementInputSendFrequency) {
                    lastSendTime = Time.time;

                    var writer = new NetworkWriter();

                    writer.StartMessage(NetMessageType.Server_PlayerInputMovement);
                    writer.Write(netId);
                    writer.Write(MovementInput);
                    writer.Write(ShootAngle);
                    writer.FinishMessage();

                    ClientScene.readyConnection.SendWriter(writer, Channels.DefaultUnreliable);
                }
        }

        void UpdateOveriewInput() {
            if(RewiredPlayer.GetButtonDown(RewiredActions.Default_Overview))
                onOverviewInput.InvokeEvent();
        }

        void UpdateShootInput() {
            if(IsImmobilized) {
                ShootInput = false;
                return;
            }

            bool tempShootInput = RewiredPlayer.GetButton(RewiredActions.Default_Aim_Horizontal) ||
                    RewiredPlayer.GetNegativeButton(RewiredActions.Default_Aim_Horizontal) ||
                    RewiredPlayer.GetButton(RewiredActions.Default_Aim_Vertical) ||
                    RewiredPlayer.GetNegativeButton(RewiredActions.Default_Aim_Vertical);

            //if(_aimingWithMouse && EventSystem.current.IsPointerOverGameObject())
            //    ShootInput = false;
            //else

            ShootInput = PlayerSpawnManager.Current.shootOnAim && tempShootInput ?
                tempShootInput :
                RewiredPlayer.GetButton(RewiredActions.Default_Shoot);
        }

        void UpdateShootDirectionInput() {
            _aimingWithGamepad =
                RewiredPlayer.IsCurrentInputSource(RewiredActions.Default_Aim_Horizontal, ControllerType.Joystick) ||
                RewiredPlayer.IsCurrentInputSource(RewiredActions.Default_Aim_Vertical, ControllerType.Joystick);

            _aimingWithKeyboard =
                RewiredPlayer.IsCurrentInputSource(RewiredActions.Default_Aim_Horizontal, ControllerType.Keyboard) ||
                RewiredPlayer.IsCurrentInputSource(RewiredActions.Default_Aim_Vertical, ControllerType.Keyboard);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector2 mousePosition = Input.mousePosition;
            bool onScreen = new Rect(0, 0, Screen.width, Screen.height).Contains(mousePosition);

            if(onScreen) {
                var mouseSpeed = new Vector2(Input.GetAxis("Mouse Horizontal"), Input.GetAxis("Mouse Vertical"));

                if(mouseSpeed.sqrMagnitude <= Mathf.Epsilon)
                    onScreen = false;
            }

            _aimingWithMouse = onScreen && new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).sqrMagnitude > .001f;

            // TODO Can't aim with mouse when Shoot On Aim is enabled

            if(_aimDevice == ControllerType.Mouse && !_aimingWithMouse)
                if(_aimingWithGamepad) _aimDevice = ControllerType.Joystick;
                else if(_aimingWithKeyboard) _aimDevice = ControllerType.Keyboard;

            if(_aimingWithMouse)
                if((_aimDevice == ControllerType.Joystick && !_aimingWithGamepad) || (_aimDevice == ControllerType.Keyboard && !_aimingWithKeyboard))
                    _aimDevice = ControllerType.Mouse;

            switch(_aimDevice) {
                case ControllerType.Mouse:
                    if(onScreen)
                        if(cursorPlane.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
                            Vector3 direction = new Vector3(hit.point.x, 0, hit.point.z) - transform.position;

                            if(direction.sqrMagnitude > float.Epsilon) {
                                ShootDirectionInput = direction;
                                ShootAngle = Mathf.Atan2(direction.x, direction.z);
                            }
                        }

                    break;

                case ControllerType.Joystick:
                    Vector3 shootJoystickDirectionAxis = new Vector3(RewiredPlayer.GetAxis(RewiredActions.Default_Aim_Horizontal), 0, RewiredPlayer.GetAxis(RewiredActions.Default_Aim_Vertical));

                    //float shootDirectionSqrMagnitude = shootDirectionAxis.sqrMagnitude;

                    //if(shootDirectionSqrMagnitude > .001f && shootDirectionSqrMagnitude >= _deadzone)
                    if(shootJoystickDirectionAxis.magnitude > _deadzone) {
                        ShootDirectionInput = shootJoystickDirectionAxis;
                        ShootAngle = Mathf.Atan2(shootJoystickDirectionAxis.x, shootJoystickDirectionAxis.z);
                    }

                    break;

                case ControllerType.Keyboard:
                    Vector3 shootKeyboardDirectionAxis = new Vector3(RewiredPlayer.GetAxis(RewiredActions.Default_Aim_Horizontal), 0, RewiredPlayer.GetAxis(RewiredActions.Default_Aim_Vertical));

                    if(shootKeyboardDirectionAxis.sqrMagnitude > 0) {
                        ShootDirectionInput = shootKeyboardDirectionAxis;
                        ShootAngle = Mathf.Atan2(shootKeyboardDirectionAxis.x, shootKeyboardDirectionAxis.z);
                    }

                    break;
            }
        }

        public static void Client_PlayerInputMovement(NetworkMessage netMsg) {
            var gameObject = netMsg.reader.ReadGameObject();
            var movementInput = netMsg.reader.ReadVector3();
            var shootAngle = netMsg.reader.ReadSingle();
            var playerInput = gameObject?.GetComponent<PlayerInput>();

            if(playerInput) {
                playerInput.MovementInput = movementInput;
                playerInput.ShootAngle = shootAngle;
                playerInput.ShootDirectionInput = playerInput.ShootDirectionFromAngle;

                var entity = GameManager.Current.GetActorEntity(playerInput.EntityId);

                entity.RotateTransforms.ForEach(s => s.PointInDirection(playerInput.ShootDirectionInput));
            }
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_PlayerInputMovement, Client_PlayerInputMovement);
            QuadrablazeSteamNetworking.RegisterServerHandler(NetMessageType.Server_PlayerInputMovement, Server_PlayerInputMovement);
        }

        public static void Server_PlayerInputMovement(NetworkMessage netMsg) {
            var netId = netMsg.reader.ReadNetworkId();
            var movementInput = netMsg.reader.ReadVector3();
            var shootAngle = netMsg.reader.ReadSingle();

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_PlayerInputMovement);
            writer.Write(netId);
            writer.Write(movementInput);
            writer.Write(shootAngle);
            writer.FinishMessage();

            foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                if(player.serverToClientConnection.connectionId != netMsg.conn.connectionId)
                    player.serverToClientConnection.SendWriter(writer, Channels.DefaultUnreliable);
        }
    }
}