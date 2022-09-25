using System.Collections.Generic;
using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    public class PlayerEntity : ActorEntity {

        public static event System.Action<PlayerEntity> OnPlayerEntityCreated;
        public static event System.Action<PlayerEntity> OnPlayerEntityDestroyed;

        //public const string BuiltInShipPoolName = "Built-In Ships";
        //public const string SyncedShipPoolName = "Synced Ships";

        //public NetworkConnection Connection { get; set; }

        //public bool IsBuiltInShip { get; set; }

        public Nametag NametagComponent { get; set; }
        public bool Owner { get; set; }
        public PlayerInput PlayerInputComponent { get; set; }
        public Material PlayerMaterial { get; set; }
        public GamePlayerInfo PlayerInfo { get; set; }

        /// <summary>Amount of times this player has spawned</summary>
        public int SpawnedCount { get; set; }

        System.Action networkShootStartMethod;
        System.Action networkShootStopMethod;

        public PlayerEntity() : base(null, "Player", default, null, 0) { }
        public PlayerEntity(GameObject gameObject, string name, uint id, UpgradeSet upgradeSet, float size, GamePlayerInfo playerInfo) : base(gameObject, name, id, upgradeSet, size) {
            PlayerInfo = playerInfo;

            networkShootStartMethod = () => NetworkShoot(true);
            networkShootStopMethod = () => NetworkShoot(false);

            OnPlayerEntityCreated?.Invoke(this);

            if(Owner)
                GameManager.Proxy.Subscribe(GameManagerActions.LevelChanged, UpdateHealth);
        }

        public bool HasAllSkills(params string[] skillNames) {
            if(skillNames.Length == 0) return true;

            if(CurrentUpgradeSet != null)
                return CurrentUpgradeSet.CurrentSkillLayout.HasAllSkills(skillNames);

            return false;
        }

        public override void DestroyEntity() {
            base.DestroyEntity();
            GameManager.Proxy.Unsubscribe(GameManagerActions.LevelChanged, UpdateHealth);
            OnPlayerEntityDestroyed?.Invoke(this);
        }

        protected override void GameObjectWasCleared(GameObject previousGameObject) {
            RemoveListeners();
        }

        protected override void GameObjectWasSet(GameObject gameObject) {
            NametagComponent = gameObject?.GetComponentInChildren<Nametag>(false);
            PlayerInputComponent = gameObject?.GetComponent<PlayerInput>();

            if(PlayerInputComponent != null) {
                PlayerInputComponent.onActionInput.AddListener(OnActionInput);
                PlayerInputComponent.onMovementInputChanged.AddListener(OnMovementInput);
                PlayerInputComponent.onUseAbilityWheelInput.AddListener(OnUseAbilityWheel);
                PlayerInputComponent.onOverviewInput.AddListener(ToggleOverview.ToggleOverviewStatus);
                PlayerInputComponent.onShootDirectionInput.AddListener(ShootDirectionChanged);
                PlayerInputComponent.onShootStartInput += networkShootStartMethod;
                PlayerInputComponent.onShootStopInput += networkShootStopMethod;
            }

            if(gameObject != null) {
                if(Owner) {
                    GameManager.Current.ShooterCameraComponent.Target = gameObject.transform;
                    GameManager.Current.UpdatePlayerUI();

                    Object.FindObjectOfType<ShooterCamera>().Target = gameObject.transform;

                    if(NetworkServer.active)
                        GlobalTargetController.GetController("Global Player Target").Target = gameObject.transform;

                    WeaponUpgradeShipRenderController.Current.CreateShipRenderObject(ShipImporter.Current.localShipData[QuadrablazeSteamNetworking.Current.MyPlayerInfo.SelectedShipID].rootMeshObject);
                    PlayerSpawnManager.Current.RefreshPlayerHUD();
                }

                if(NametagComponent != null) {
                    NametagComponent.SetNametagText(SteamFriends.GetFriendPersonaName((CSteamID)PlayerInfo.SteamId));
                    NametagComponent.SetNametagActive(!Owner);
                }

                SpawnedCount++;

                if(SpawnedCount > 1) { // Respawned

                }
                else { // First spawn

                }

                PlayerSpawnManager.Current.OnPlayerSpawned.InvokeEvent(gameObject);

                SetMaterialColors();
                SetupAbilityUI();
            }
        }

        void OnActionInput(int actionIndex) {
            var skillId = UIManager.Current.abilityWheel.GetSkillId(actionIndex);

            if(!string.IsNullOrEmpty(skillId)) {
                var element = CurrentUpgradeSet.CurrentSkillLayout.GetSkill(skillId);

                TryUseAbility(element);
            }
        }

        protected override void OnDeathEvent() {
            //SetupHealthProxy(true);
            SetGameObject(null);
        }

        void OnMovementInput(Vector3 input) {
            //Debug.Log($"OnMovementInput {Id}");
            BaseMovementControllerComponent.Move(input);

            if(CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Dash>() is Dash executor) {
                switch(BaseMovementControllerComponent.MovementStyle) {
                    case BaseMovementController.MovementType.Directional: executor.Direction = input.normalized; break;
                    case BaseMovementController.MovementType.Arcade: executor.Direction = (CurrentTransform.rotation * input).normalized; break;
                }
            }
        }

        protected override void OnPermadeathEvent() {
            PlayerProxy.Players.Remove(this);
            PlayerInfo?.SetDead();
        }

        void OnUseAbilityWheel(int activeAbilityIndex) {
            var element = CurrentUpgradeSet.CurrentSkillLayout.GetActiveSkill(activeAbilityIndex);

            TryUseAbility(element);
        }

        void RemoveListeners() {
            Debug.Log("Remove listeners: " + (PlayerInputComponent != null));
            if(PlayerInputComponent == null) return;
            //if(CurrentGameObject == null) return;

            PlayerInputComponent.onActionInput.RemoveListener(OnActionInput);
            PlayerInputComponent.onMovementInputChanged.RemoveListener(OnMovementInput);
            PlayerInputComponent.onUseAbilityWheelInput.RemoveListener(OnUseAbilityWheel);
            PlayerInputComponent.onOverviewInput.RemoveListener(ToggleOverview.ToggleOverviewStatus);
            PlayerInputComponent.onShootDirectionInput.RemoveListener(ShootDirectionChanged);
            PlayerInputComponent.onShootStartInput -= networkShootStartMethod;
            PlayerInputComponent.onShootStopInput -= networkShootStopMethod;
        }

        void SetupAbilityUI() {
            CurrentUpgradeSet.CurrentSkillLayout.ReloadSkillExecutors(this);

            if(Owner) {
                UIAbilityBar.Current.RemoveAllIcons();

                string[] currentAbilityWheelSetup = new string[AbilityWheel.Current.SkillIds.Length];
                System.Array.Copy(AbilityWheel.Current.SkillIds, currentAbilityWheelSetup, AbilityWheel.Current.SkillIds.Length);

                AbilityWheel.Current.ResetSkills();

                var buttons = new List<UISkillButton>(InGameUIManager.Current.uiSkillButtons);

                foreach(var skill in CurrentUpgradeSet.CurrentSkillLayout.OrderedSkills) {
                    UISkillButton selectedButton = null;

                    foreach(var button in buttons) {
                        if(button.skillLayoutElement.OriginalLayoutElement.name == skill) {
                            button.Acquired = true;
                            selectedButton = button;
                            button.UpdateAbilityIcon(false);

                            break;
                        }
                    }

                    if(selectedButton != null)
                        buttons.Remove(selectedButton);
                }

                foreach(var button in buttons)
                    button.UpdateAbilityIcon(false);

                UpdateAbilityWheelIcons();

                for(int i = 0; i < currentAbilityWheelSetup.Length; i++)
                    if(!string.IsNullOrEmpty(currentAbilityWheelSetup[i]))
                        AbilityWheel.Current.SetInputToAbility(i, currentAbilityWheelSetup[i]);
            }
        }

        public void SetMaterialColors() {
            PlayerMaterial = new Material(ShipImporter.defaultMaterial);

            PlayerInfo.ColorPreset.SetMaterialValues(PlayerMaterial);

            //TODO: Make sure this only changes the ship model renderer materials
            foreach(var renderer in CurrentTransform.Find("Ship Mesh").GetComponentsInChildren<MeshRenderer>(true))
                renderer.sharedMaterial = PlayerMaterial;
        }

        void ShootDirectionChanged(Vector3 direction) {
            RotateTransforms.ForEach(s => s.PointInDirection(direction));
        }

        public override void UnloadEntity() {
            RemoveListeners();

            base.UnloadEntity();
        }

        public void UpdateAbilityWheelIcons() {
            var activeSkillCount = CurrentUpgradeSet.CurrentSkillLayout.ActiveSkills.Count;

            UIManager.Current.abilityPieWheel.RemoveAllIcons();
            UIManager.Current.abilityPieWheel.SetSlices(activeSkillCount);

            for(int i = 0; i < activeSkillCount; i++) {
                var element = CurrentUpgradeSet.CurrentSkillLayout.GetActiveSkill(i);

                UIManager.Current.abilityPieWheel.AddIcon(element.OriginalLayoutElement.SkillIcon);
            }
        }

        void UpdateHealth(System.EventArgs _) {
            HealthManager.UpdateHealth(this);
        }
    }
}