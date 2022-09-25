using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using Rewired;
using StatSystem;
using Steamworks;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class InGameUIManager : MonoBehaviour {

        public static InGameUIManager Current { get; private set; }

        [Header("Upgrade Menu UI")]
        public GameObject upgradeMenu;
        public Tabs upgradeTabs;

        [Header("Skills")]
        public RectTransform skillContentTransform;
        public RectTransform activeSkillContentTransform;
        public RectTransform passiveSkillContentTransform;
        float activeContentHeight = 0;
        float passiveContentHeight = 0;

        [UnityEngine.Serialization.FormerlySerializedAs("skillContainer")]
        public GameObject activeSkillContainer;
        public GameObject passiveSkillContainer;
        public GameObject skillButton;
        public ToggleGroup skillContainerToggleGroup;
        public UpgradeSkillPanel skillPanel;
        public Button skillLevelUp;

        [Header("Weapon Upgrades")]
        public GameObject upgradeContainer;
        public GameObject[] upgradeContainers;
        public Button weaponLevelUp;
        public TextMeshProUGUI weaponUpgradeNotificationText;

        [Header("Player")]
        public GameObject playerInfoContainer;
        public Slider xpBar;
        public TextMeshProUGUI currentXPText;
        public TextMeshProUGUI nextXPText;
        public Slider healthBar;
        public Slider shieldBar;
        float shieldLerp;
        float lastShieldSlider;
        public TextMeshProUGUI currentHealthText;
        public TextMeshProUGUI maxHealthText;
        public SkillPointsUIGlow skillPointsGlow;

        [UnityEngine.Serialization.FormerlySerializedAs("playerLevel")]
        public TextMeshProUGUI gameLevelText;
        public TextMeshProUGUI playerSkillPoints;

        [Header("Other Players")]
        public OtherPlayerUI otherPlayerUIPrefab;
        public GameObject otherPlayerInfoContainer;

        [Header("Help")]
        public CanvasGroup shootHelp;
        public CanvasGroup movementHelp;

        public HashSet<UISkillButton> uiSkillButtons;

        Player RewiredPlayer { get; set; }

        //TODO: [InGameUIManager] Fix this?
        public void CreateOtherPlayerUI() {
            //public void CreateOtherPlayerUI(PlayerActor actor) {
            var otherPlayerUIObject = Instantiate(otherPlayerUIPrefab.gameObject, otherPlayerInfoContainer.transform);
            var otherPlayerUI = otherPlayerUIObject.GetComponent<OtherPlayerUI>();

            //otherPlayerUI.PlayerSteamId = (CSteamID)actor.PlayerInfo.SteamId;

            var avatar = SteamAvatarManager.Current.GetAvatars(otherPlayerUI.PlayerSteamId, SteamAvatarManager.SteamAvatarSize.Medium);

            otherPlayerUI.SetAvatar(avatar.MediumAvatar.avatarSprite);
            otherPlayerUI.SetName(SteamFriends.GetFriendPersonaName(otherPlayerUI.PlayerSteamId));
        }

        IEnumerator FadeShowHelp(float delay) {
            shootHelp.alpha = 1;
            movementHelp.alpha = 1;

            yield return new WaitForSeconds(delay);

            float time = 1;

            while(time > 0) {
                time = Mathf.Max(time - Time.deltaTime, 0);
                shootHelp.alpha = time;
                movementHelp.alpha = time;

                yield return new WaitForEndOfFrame();
            }
        }

        public void GenerateSkillButtons(GamePlayerInfo playerInfo) {
            uiSkillButtons.Clear();
            GenerateMainSkills(playerInfo);
            GenerateWeaponSkills(playerInfo);
        }

        void GenerateMainSkills(GamePlayerInfo playerInfo) {
            var skillElements = playerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.Elements.OrderBy(s => s.OriginalLayoutElement.name);

            foreach(var element in skillElements)
                if(!(element.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor)) {
                    var buttonGameObject = Instantiate(skillButton);
                    var uiSkillButton = buttonGameObject.GetComponent<UISkillButton>();
                    var toggle = uiSkillButton.GetComponent<Toggle>();

                    uiSkillButtons.Add(uiSkillButton);

                    buttonGameObject.SetActive(true);

                    if(element.Passive)
                        buttonGameObject.transform.SetParent(passiveSkillContainer.transform, false);
                    else
                        buttonGameObject.transform.SetParent(activeSkillContainer.transform, false);

                    uiSkillButton.GetComponent<UpgradeSkillDescription>().panel = skillPanel;
                    toggle.group = skillContainerToggleGroup;
                    uiSkillButton.skillLayout = playerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout;
                    uiSkillButton.skillLayoutElement = element;
                    uiSkillButton.playerInfo = playerInfo;

                    uiSkillButton.RefreshText();
                }

            var activeSkillButtons = activeSkillContainer.transform.GetComponentsInChildren<UISkillButton>();
            var passiveSkillButtons = passiveSkillContainer.transform.GetComponentsInChildren<UISkillButton>();

            SetupNavigation(activeSkillButtons, passiveSkillButtons);
            SetupNavigation(passiveSkillButtons, activeSkillButtons);

            void SetupButtonNavigation(UISkillButton thisButton, UISkillButton previousButton, UISkillButton nextButton, UISkillButton oppositeHorizontalButton) {
                var leftOppositeSelectable = oppositeHorizontalButton.selectSkillButton;
                var rightOppositeSelectable = oppositeHorizontalButton.selectSkillButton;
                //var leftOppositeSelectable = oppositeHorizontalButton.upgradeSkillButton;
                //var rightOppositeSelectable = oppositeHorizontalButton.selectSkillButton;

                thisButton.selectSkillButton.navigation = new Navigation() {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = previousButton.selectSkillButton,
                    selectOnDown = nextButton.selectSkillButton,
                    selectOnLeft = oppositeHorizontalButton.selectSkillButton,
                    selectOnRight = oppositeHorizontalButton.selectSkillButton
                };

                //thisButton.toggleSkill.navigation = new Navigation() {
                //    mode = Navigation.Mode.Explicit,
                //    selectOnUp = previousButton.toggleSkill,
                //    selectOnDown = nextButton.toggleSkill,
                //    selectOnLeft = thisButton.upgradeSkillButton,
                //    selectOnRight = thisButton.selectSkillButton
                //};

                //thisButton.selectSkillButton.navigation = new Navigation() {
                //    mode = Navigation.Mode.Explicit,
                //    selectOnUp = previousButton.selectSkillButton,
                //    selectOnDown = nextButton.selectSkillButton,
                //    selectOnLeft = leftOppositeSelectable,
                //    selectOnRight = thisButton.upgradeSkillButton
                //};

                //thisButton.upgradeSkillButton.navigation = new Navigation() {
                //    mode = Navigation.Mode.Explicit,
                //    selectOnUp = previousButton.upgradeSkillButton,
                //    selectOnDown = nextButton.upgradeSkillButton,
                //    selectOnLeft = thisButton.selectSkillButton,
                //    selectOnRight = rightOppositeSelectable
                //};
            }

            void SetupNavigation(UISkillButton[] thisArray, UISkillButton[] oppositeArray) {
                var length = thisArray.Length;

                for(int i = 0; i < length; i++) {
                    var previousPositiveVerticalButtonIndex = (i - 1) % length;
                    var nextPositiveVerticalButtonIndex = (i + 1) % length;

                    if(previousPositiveVerticalButtonIndex < 0)
                        previousPositiveVerticalButtonIndex += length;

                    var thisButton = thisArray[i];
                    var previousButton = thisArray[previousPositiveVerticalButtonIndex];
                    var nextButton = thisArray[nextPositiveVerticalButtonIndex];
                    var nearestOppositeButton = oppositeArray[Mathf.Min(i, oppositeArray.Length - 1)];

                    SetupButtonNavigation(thisButton, previousButton, nextButton, nearestOppositeButton);
                }
            }
        }

        void GenerateWeaponSkills(GamePlayerInfo playerInfo) {
            var skillElements = playerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.Elements.Where(s => s.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor).ToArray();
            var shipInfo = ShipImporter.Current.GetShipData(playerInfo);
            var shipInfoObject = shipInfo.rootMeshObject.GetComponent<ShipInfoObject>();

            int weaponCount = shipInfoObject.WeaponPivots.Count(s => s != null);

            for(int i = 0; i < 8; i++) {
                var button = upgradeContainers[i].GetComponent<UISkillButton>();

                if(i < weaponCount) {
                    uiSkillButtons.Add(button);

                    button.skillLayout = playerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout;
                    button.skillLayoutElement = skillElements[i];
                    button.playerInfo = playerInfo;

                    button.gameObject.SetActive(true);
                    button.RefreshText();
                }
                else {
                    button.gameObject.SetActive(false);
                }
            }
        }

        public UISkillButton GetSkillButton(SkillLayoutElement element) {
            return uiSkillButtons.FirstOrDefault(button => button.skillLayoutElement == element);
        }
        public UISkillButton GetSkillButton(string id) {
            return uiSkillButtons.FirstOrDefault(button => button.skillLayoutElement.OriginalLayoutElement.name == id);
        }

        public void Initialize() {
            Current = this;
            RewiredPlayer = ReInput.players.GetPlayer(0);

            uiSkillButtons = new HashSet<UISkillButton>();
            skillButton.SetActive(false);

            PlayerProxy.Proxy.Subscribe(PlayerActions.ChangedLevel, PlayerProxy_ChangedLevel);
            PlayerProxy.Proxy.Subscribe(PlayerActions.ChangedSkillPoints, PlayerProxy_ChangedSkillPoints);
            PlayerProxy.Proxy.Subscribe(HealthActions.ChangedStat, PlayerProxy_ChangedStat);
            PlayerProxy.Proxy.Subscribe(HealthActions.ChangedStatMax, PlayerProxy_ChangedStat);
            PlayerProxy.Proxy.Subscribe(PlayerActions.ChangedXP, PlayerProxy_ChangedXP);
            PlayerProxy.Proxy.Subscribe(PlayerActions.ChangedNextXP, PlayerProxy_ChangedXP);
            PlayerProxy.Proxy.Subscribe(PlayerActions.Upgraded, PlayerProxy_Upgraded);

            SetupCallbacks();
        }

        void PlayerProxy_ChangedLevel(EventArgs args) {
            RefreshGameLevelText();
        }

        void PlayerProxy_ChangedSkillPoints(EventArgs args) {
            var entity = ((EntityArgs)args).GetEntity<PlayerEntity>();

            if(CheckPlayerAvailability(entity)) {
                RefreshSkillPointText(entity);
                skillPointsGlow.EnableGlow(entity.PlayerInfo.HasSkillPoints(1));
            }
        }

        void PlayerProxy_ChangedStat(EventArgs args) {
            var statEvent = ((StatEventArgs)args).Stat;

            if(statEvent.AffectedObject is Shield shield)
                CheckPlayerAvailability((PlayerEntity)shield.CurrentActorEntity, RefreshShieldInfo);
            else if(statEvent.AffectedObject is PlayerEntity playerEntity)
                RefreshHealthInfo(playerEntity);
        }

        void PlayerProxy_ChangedXP(EventArgs args) {
            RefreshXPInfo();
        }

        void PlayerProxy_Upgraded(EventArgs args) {
            var entity = ((EntityArgs)args).GetEntity<PlayerEntity>();

            if(CheckPlayerAvailability(entity)) {
                var shieldExecutor = entity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Shield>();

                if(shieldExecutor != null)
                    RefreshShieldInfo(entity);
            }
        }

        public void RefreshHealthInfo(PlayerEntity playerEntity) {
            if(playerEntity != null) {
                var health = playerEntity.HealthSlots[0];

                healthBar.normalizedValue = health.NormalizedValue;
                currentHealthText.text = health.Value.ToString();
                maxHealthText.text = health.MaxValue.ToString();
            }
            else {
                healthBar.normalizedValue = 0;
                currentHealthText.text = "0";
                maxHealthText.text = "0";
            }
        }

        public void RefreshGameLevelText() {
            gameLevelText.text = GameManager.Current.Level.ToString();
        }

        public void RefreshShieldInfo(PlayerEntity entity) {
            //TODO: Disable shield UI when shield is not purchased
            if(entity != null && entity.CurrentUpgradeSet != null && entity.CurrentUpgradeSet.CurrentSkillLayout != null) {
                var shieldExecutor = entity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Shield>();

                if(shieldExecutor != null) {
                    var stat = shieldExecutor.Health;

                    shieldLerp = stat.NormalizedValue;
                    lastShieldSlider = shieldBar.normalizedValue;

                    return;
                }
            }

            shieldLerp = 0;
            shieldBar.normalizedValue = 0;
        }

        public void RefreshSkillPointText() {
            PlayerEntity entity = null;

            if(QuadrablazeSteamNetworking.Current.MyPlayerInfo != null)
                entity = QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity;

            RefreshSkillPointText(entity);
        }
        public void RefreshSkillPointText(PlayerEntity entity) {
            playerSkillPoints.text = entity != null && entity.CurrentUpgradeSet != null ? entity.CurrentUpgradeSet.SkillPoints.ToString() : "0";
        }

        public void RefreshXPInfo() {
            xpBar.normalizedValue = Mathf.InverseLerp(0, GameManager.Current.NextXP, GameManager.Current.XP);

#if GAME_DEMO
                currentXPText.text = player.Level >= PlayerLevel.MaxDemoLevel ? "-" : GameManager.Current.ToString();
                nextXPText.text = player.Level >= PlayerLevel.MaxDemoLevel ? "-" : GameManager.Current.ToString();
#else
            currentXPText.text = GameManager.Current.XP.ToString();
            nextXPText.text = GameManager.Current.NextXP.ToString();
#endif
        }

        public void RemoveSkillButtons() {
            foreach(Transform child in activeSkillContainer.transform)
                Destroy(child.gameObject);

            foreach(Transform child in passiveSkillContainer.transform)
                Destroy(child.gameObject);

            foreach(var upgradeObject in upgradeContainers) {
                var button = upgradeObject.GetComponent<UISkillButton>();

                button.skillLayout = null;
                button.skillLayoutElement = null;
                button.playerInfo = null;
                button.Reset();
            }

            uiSkillButtons.Clear();
        }

        void SetupCallbacks() {
            activeSkillContentTransform.GetComponent<OnRectTransformChangeTrigger>().OnDimensionsChanged.AddListener(UpdateSkillUISize);
            passiveSkillContentTransform.GetComponent<OnRectTransformChangeTrigger>().OnDimensionsChanged.AddListener(UpdateSkillUISize);
        }

        public void ShowHelp() {
            StopCoroutine("FadeShowHelp");
            StartCoroutine("FadeShowHelp", 2);
        }
        public void ShowHelp(GameObject gameObject) {
            ShowHelp();
        }

        void Update() {
            if(shieldBar.normalizedValue != shieldLerp) {
                var healthDelta = shieldLerp - lastShieldSlider;
                var timeDelta = Time.deltaTime;

                if(Mathf.Abs(healthDelta) < .5f)
                    if(shieldBar.normalizedValue < shieldLerp)
                        timeDelta /= Shield.ShieldHealthMultiplier;

                shieldBar.normalizedValue = Mathf.MoveTowards(shieldBar.normalizedValue, shieldLerp, timeDelta);
            }
        }

        public void UpdateMenuInput() {
            if(RewiredPlayer != null) {
                if(RewiredPlayer.GetNegativeButtonDown("Menu Tab Navigation"))
                    upgradeTabs.SelectPreviousTab(false);
                else if(RewiredPlayer.GetButtonDown("Menu Tab Navigation"))
                    upgradeTabs.SelectNextTab(false);

                //if(upgradeTabs.GetCurrentlyActiveButton().name == "Weapon Upgrades") {

                //}
            }
        }

        void UpdateSkillUISize() {
            activeContentHeight = activeSkillContentTransform.rect.height;
            passiveContentHeight = passiveSkillContentTransform.rect.height;

            var contentSize = skillContentTransform.sizeDelta;
            var toSize = Mathf.Max(activeContentHeight, passiveContentHeight);

            contentSize.y = toSize;

            skillContentTransform.sizeDelta = contentSize;
        }

        static bool CheckPlayerAvailability(PlayerEntity entity) {
            return entity != null && entity.CurrentGameObject != null && entity.Owner;
        }
        static void CheckPlayerAvailability(PlayerEntity entity, Action method) {
            if(CheckPlayerAvailability(entity)) method();
        }
        static void CheckPlayerAvailability(PlayerEntity entity, Action<PlayerEntity> method) {
            if(CheckPlayerAvailability(entity)) method(entity);
        }

        struct ButtonNotification {
            public string name;
            public ControllerType controllerType;
            public ActionElementMap actionElementMap;

            public ButtonNotification(string name, ControllerType controllerType, ActionElementMap actionElementMap) {
                this.name = name;
                this.controllerType = controllerType;
                this.actionElementMap = actionElementMap;
            }
        }
    }
}