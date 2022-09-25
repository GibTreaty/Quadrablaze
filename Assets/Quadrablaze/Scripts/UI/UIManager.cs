using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rewired;
using System;
using System.Collections;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using Rewired.UI.ControlMapper;
using DG;
using Quadrablaze.MenuSystem;
using Quadrablaze.Skills;
using System.Collections.Generic;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class UIManager : MonoBehaviour {

        public static UIManager Current { get; private set; }
        public static bool IsInitialized { get; private set; }

        public GameObject uiRoot;

        [Header("Startup")]
        public TMPro.TMP_Text inDevMessage;
        public LoadingText loadingText;

        [Header("Popup")]
        public uMyGUI_Popup textPopup;
        public ScriptableMenu popupMenu;

        [Header("Main Menu")]
        public CanvasGroup menuBackground;
        public GameObject title;
        public GameObject startMenuTitle;
        public GameObject mainMenuButtons;
        public CanvasGroup mainMenuGroup;
        public Button resumeButton;
        public Button playButton;
        public Button selectShipButton;
        public Button optionsButton;
        public Button mainMenuButton;
        public Button workshopButton;
        public Button quitButton;

        [Header("Options")]
        public OptionsUIManager optionsUIManager;
        public ControlMapper controlMapper;
        public CanvasGroup controlMapperInputCanvasGroup;

        [Header("Multiplayer")]
        public GameLobbyManager gameLobbyManager;
        public LobbyListUI lobbyListUI;

        [Header("Player UI")]
        public InGameUIManager inGameUIManager;

        public PieWheel abilityPieWheel;
        public AbilityWheel abilityWheel;

        [Header("Game")]
        public GameObject otherUIObject;
        public Text gameOver;
        public Text goalCompleted;

        [Header("Ship Selection")]
        public ShipSelectionUIManager shipSelectionUIManager;

        [Header("Feedback")]
        public UIManagerTextMeshPro trelloUIManager;
        public Button feedbackButton;
        public BooleanEvent onFeedbackPanelState;

        [Header("Menu System")]
        [SerializeField]
        ScriptableMenuScene _mainMenuScene;

        [SerializeField]
        ScriptableMenuScene _inGameMenuScene;

        ScriptableMenuScene _currentMenuScene = null;

        [SerializeField]
        List<MenuItem> _menuItems;
        List<MenuItem> _openMenus = null;
        List<ScriptableMenu> _openMenuLogic = null;
        MenuItem _currentMenuItem = null;

        SteamWorkshopPopupBrowse steamWorkshopBrowsePopup;

        IEnumerator lerpBackground = null;

        public Action popupHideAction;

        bool _isPollingForInput;
        bool _controlMapperPopupOpen;
        bool _initialized;

        #region Properties
        public bool AbilityWheelOpen => abilityPieWheel.WheelActive;

        public MenuItem CurrentMenuItem => _openMenus.Count > 0 ? _openMenus[_openMenus.Count - 1] : null;

        public bool IsFeedbackPanelOpen => UIManagerTextMeshPro.Instance != null ? UIManagerTextMeshPro.Instance.gameObject.activeInHierarchy : false;

        public bool IsJoiningGame { get; set; }

        public bool IsPopupShowing { get; set; }

        public bool MultiplayerLobbyOpen => gameLobbyManager.LobbyUI.gameObject.activeSelf;

        public bool MainMenuOpen => mainMenuButtons.activeSelf;

        public bool OptionsMenuOpen => optionsUIManager.gameObject.activeInHierarchy;

        public MenuItem ParentMenuItem => _openMenus.Count > 1 ? _openMenus[_openMenus.Count - 2] : null;

        public MenuItem RootMenuItem => _openMenus.Count > 0 ? _openMenus[0] : null;

        public bool SkipInputFrame { get; set; }

        public int SkipInputFrames { get; set; }

        Player RewiredPlayer { get; set; }

        public bool UpgradeMenuOpen => inGameUIManager.upgradeMenu.activeSelf;

        public bool UIOpenLastFrame { get; private set; }
        #endregion

        void OnEnable() {
            Current = this;
        }

        public void Initialize() {
            Current = this;
            enabled = true;

            EnablePlayerUI();
            RewiredPlayer = ReInput.players.GetPlayer(0);
            SetupStartMenuButtons();

            if(onFeedbackPanelState == null) onFeedbackPanelState = new BooleanEvent();

            inGameUIManager.Initialize();
            optionsUIManager.Initialize();
            otherUIObject.SetActive(true);
            abilityPieWheel.Initialize();
            abilityWheel.Initialize();

            //EnableMainMenu(true);
            EnableGame(false);

            {
                SetMenuScene(MenuScene.MainMenu);

                DeactivateTitleDelayed();

                //EnableMainMenu(false);
                ShowGameOver(false);
                ShowGoalCompleted(false);

                _openMenus = new List<MenuItem>();
                _openMenuLogic = new List<ScriptableMenu>();

                foreach(var item in _menuItems) {
                    ScriptableMenu menuLogic;

                    if(item.MenuLogic != null)
                        menuLogic = item.MenuLogic;
                    else
                        menuLogic = ScriptableObject.CreateInstance<ScriptableMenu>();

                    menuLogic.Close(item, this);
                }

                OpenMenu("Main");
            }

            controlMapper.onInputPollingStarted += () => _isPollingForInput = true;
            controlMapper.onInputPollingEnded += () => _isPollingForInput = false;

            controlMapper.onPopupWindowOpened += () => {
                _controlMapperPopupOpen = true;
                controlMapperInputCanvasGroup.interactable = false;
            };
            controlMapper.onPopupWindowClosed += () => {
                _controlMapperPopupOpen = false;
                controlMapperInputCanvasGroup.interactable = true;
            };

            optionsUIManager.uiControlList.controlMapperModifyButton.onClick.AddListener(OnModifyButton);
            optionsUIManager.uiControlList.controlMapperExitButton.onClick.AddListener(() => EnableKeybindings(false));

            SetupPopup();

            StartCoroutine(PauseGameRoutine());

            _initialized = true;
            IsInitialized = true;
        }

        IEnumerator DeactivateTitleCoroutine(TitleDeactivateOptions options) {
            if(!title.activeSelf) yield break;

            if(options.delay > 0)
                yield return new WaitForSeconds(options.delay);

            var canvasGroup = title.GetComponent<CanvasGroup>();
            var canvasGroup2 = startMenuTitle.GetComponent<CanvasGroup>();

            {
                float startTime = options.fadeTitleOutTime;
                float time = startTime;

                while(time >= 0) {
                    time -= Time.deltaTime;
                    canvasGroup.alpha = time / options.fadeTitleOutTime;

                    if(options.fadeTitleOutTime > 0)
                        yield return new WaitForEndOfFrame();
                }
            }

            title.SetActive(false);

            canvasGroup2.alpha = 0;
            startMenuTitle.SetActive(true);

            {
                float startTime = options.fadeStartMenuTitleInTime;
                float time = startTime;

                while(time >= 0) {
                    time -= Time.deltaTime;
                    canvasGroup2.alpha = 1 - (time / options.fadeStartMenuTitleInTime);

                    if(options.fadeStartMenuTitleInTime > 0)
                        yield return new WaitForEndOfFrame();
                }
            }
        }
        public void DeactivateTitle(TitleDeactivateOptions options) {
            StopCoroutine("DeactivateTitleCoroutine");
            StartCoroutine("DeactivateTitleCoroutine", options);
        }

        public void DeactivateTitleInstant() {
            DeactivateTitle(new TitleDeactivateOptions { delay = 0, fadeTitleOutTime = 0, fadeStartMenuTitleInTime = 0 });
        }
        public void DeactivateTitle() {
            DeactivateTitle(new TitleDeactivateOptions { delay = 0, fadeTitleOutTime = 0, fadeStartMenuTitleInTime = 1 });
        }
        public void DeactivateTitleDelayed() {
            DeactivateTitle(new TitleDeactivateOptions { delay = 5, fadeTitleOutTime = 1, fadeStartMenuTitleInTime = 1 });
        }

        IEnumerator DoNextFrameRoutine(Action action) {
            yield return new WaitForEndOfFrame();
            action();
        }

        //public uMyGUI_PopupButtons DoPopup(string title, string message) {
        //    return ((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText(title, message).ShowButton(uMyGUI_PopupManager.BTN_OK);
        //}
        public uMyGUI_PopupButtons DoPopup(string title, string message, Action onHide, params (string buttonType, Action callback)[] buttonCallbacks) {
            // TODO: Make this work with the new menu system
            var popup = ((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText(title, message);

            popupHideAction = onHide;

            foreach((string buttonType, Action callback) in buttonCallbacks)
                popup.ShowButton(buttonType, callback);

            var firstSelectable = popup.GetComponentInChildren<Selectable>(false);

            if(firstSelectable != null)
                EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);

            return popup;
        }

        public void EnableGame(bool enable = true) {
            resumeButton.gameObject.SetActive(enable);
            playButton.gameObject.SetActive(!enable);
            selectShipButton.gameObject.SetActive(!enable);
            mainMenuButton.gameObject.SetActive(enable);
            workshopButton.gameObject.SetActive(!enable);

            EnablePlayerUI(enable);
        }

        public void EnableKeybindings(bool enable = true) {
            optionsUIManager.uiControlList.controlMapperModifyButton.gameObject.SetActive(!enable);
            optionsUIManager.uiControlList.optionsCanvasGroup.interactable = !enable;
            optionsUIManager.uiControlList.optionsCanvasGroup.alpha = enable ? 0.001f : 1;
            optionsUIManager.uiControlList.controlMapperInputGroup.gameObject.SetActive(enable);
            optionsUIManager.uiControlList.controlMapperInputGroup.interactable = enable;
            optionsUIManager.uiControlList.controlMapperInputGroup.alpha = enable ? 1 : 0;

            if(enable)
                EventSystem.current.SetSelectedGameObject(optionsUIManager.uiControlList.controlMapperRemapFirstSelected);
            else
                EventSystem.current.SetSelectedGameObject(optionsUIManager.uiControlList.controlMapperNormalFirstSelected);
        }

        public void EnableMultiplayerLobby(bool enable = true) {
            if(enable) {
                GoToMenu("Lobby");
                gameLobbyManager.LobbyUI.OnOpen(GameManager.Current.Options);
            }
            else {
                GoToParentMenu();
            }
        }

        public void EnablePlayerUI(bool enable = true) {
            inGameUIManager.playerInfoContainer.SetActive(true);
            inGameUIManager.gameObject.SetActive(enable);
        }

        public void EnableWorkshopMenu(bool enable = true) {
            if(enable) {
                DeactivateTitle();

                var oldPopup = steamWorkshopBrowsePopup;

                steamWorkshopBrowsePopup = (SteamWorkshopPopupBrowse)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_browse");

                if(steamWorkshopBrowsePopup != oldPopup) {
                    var closeTransform = steamWorkshopBrowsePopup.transform.Find("BG/Title/Close");

                    if(closeTransform) {
                        var closeButton = closeTransform.GetComponent<Button>();

                        closeButton.onClick.AddListener(() => EnableWorkshopMenu(false));
                    }
                }
            }
            else if(steamWorkshopBrowsePopup != null)
                steamWorkshopBrowsePopup.Hide();

            mainMenuGroup.interactable = !enable;
            mainMenuGroup.alpha = !enable ? 1 : 0;
        }

        public MenuItem FindMenu(string name) {
            if(!string.IsNullOrEmpty(name))
                foreach(var item in _menuItems)
                    if(item.Name == name)
                        return item;

            return null;
        }

        //void GenerateAllSkills() {
        //    foreach(SkillBase skill in PlayerSpawnManager.Current.CurrentPlayer.GetComponentsInChildren<SkillBase>()) {
        //        GameObject buttonGameObject = Instantiate(skillButton);
        //        UISkillButton button = buttonGameObject.GetComponent<UISkillButton>();
        //        Transform parent;

        //        buttonGameObject.SetActive(true);

        //        switch(skill.transform.parent.name) {
        //            default: parent = skillContainer.transform; break;
        //            case "Weapon Upgrades": parent = upgradeContainer.transform; break;
        //        }

        //        buttonGameObject.transform.SetParent(parent, false);

        //        button.skill = skill;
        //        button.player = PlayerSpawnManager.Current.CurrentPlayer.GetComponent<PlayerLevel>();

        //        button.RefreshText();
        //    }
        //}

        void LateUpdate() {
            UIOpenLastFrame = false;
        }

        IEnumerator LerpBackgroundAlpha(float from, float to, float duration) {
            float startTime = Time.realtimeSinceStartup;
            float endTime = startTime + duration;

            while(true) {
                float currentTime = Time.realtimeSinceStartup;
                float lerp = Mathf.InverseLerp(startTime, endTime, currentTime);
                float alpha = Mathf.Lerp(from, to, lerp);

                menuBackground.alpha = alpha;

                yield return new WaitForEndOfFrame();

                if(currentTime >= endTime) break;
            }
        }
        public void LerpBackgroundAlpha(bool enable) {
            if(lerpBackground != null)
                StopCoroutine(lerpBackground);

            float lerpFrom = menuBackground.alpha;
            float lerpTo = enable ? (RoundManager.RoundInProgress ? 0 : 1) : 0;

            lerpBackground = LerpBackgroundAlpha(lerpFrom, lerpTo, .5f);

            StartCoroutine(lerpBackground);
        }

        public void ClearOpenMenuLists() {
            _openMenus.Clear();
            _openMenuLogic.Clear();
        }

        public void CloseMenu() {
            if(_openMenus.Count > 0)
                CloseMenu(_openMenus[_openMenus.Count - 1]);
        }
        public void CloseMenu(string name) {
            CloseMenu(FindMenu(name));
        }
        public void CloseMenu(MenuItem item) {
            if(item == null) return;

            //Debug.Log($"CloseMenu {item.Name}");

            _openMenus.Remove(item);
            _openMenuLogic.Remove(item.MenuLogic);

            CloseMenuItem_Internal(item);
        }

        void CloseMenuItem_Internal(MenuItem item) {
            item.MenuLogic.Close(item, this);
        }

        public void CloseMenus() {
            for(int i = _openMenus.Count - 1; i >= 0; i--)
                CloseMenu(_openMenus[i]);
        }

        public List<MenuItem> GetMenuHierarchy(MenuItem item) {
            List<MenuItem> output = new List<MenuItem>() { item };
            //MenuItem parent = null;

            //do {
            //    parent = FindMenu(item.ParentMenuName);

            //    if(parent != null)
            //        output.Insert(0, parent);
            //}
            //while(parent != null);

            return output;
        }

        public void GoToMenu(string name, bool regeneratePath = true) {
            GoToMenu(FindMenu(name), regeneratePath);
        }
        public void GoToMenu(ScriptableMenu menu, bool regeneratePath = true) {
            foreach(var item in _menuItems)
                if(item.MenuLogic == menu) {
                    GoToMenu(item, regeneratePath);
                    return;
                }
        }
        public void GoToMenu(MenuItem item, bool regeneratePath = true) {
            if(item == null) {
                foreach(var menu in _openMenus)
                    if(!menu.MenuLogic.Options.closableOutOfRound || RoundManager.RoundInProgress)
                        CloseMenuItem_Internal(menu);

                ClearOpenMenuLists();

                return;
            }
            else {
                List<MenuItem> menuItems = new List<MenuItem>();

                if(regeneratePath) {
                    var path = _currentMenuScene.GetPath(item.MenuLogic);


                    foreach(var pathItem in path)
                        menuItems.Add(_menuItems.Find(menu => pathItem == menu.MenuLogic));

                    _openMenuLogic.Clear();
                    _openMenuLogic.AddRange(path);
                }

                if(CurrentMenuItem != null)
                    CloseMenuItem_Internal(CurrentMenuItem);

                if(regeneratePath) {
                    _openMenus.Clear();
                    _openMenus.AddRange(menuItems);

                    OpenMenuItem_Internal(item);
                }
                else
                    OpenMenu(item);
            }
        }

        public void GoToParentMenu() {
            if(_openMenus.Count > 0) {
                if(_openMenus.Count == 1) {
                    if(CurrentMenuItem.MenuLogic.Options.closableRoot ||
                      (CurrentMenuItem.MenuLogic.Options.closableDuringRound && _currentMenuScene == _inGameMenuScene)
                    )
                        CloseMenu(CurrentMenuItem);
                }
                else {
                    var parentMenuItem = ParentMenuItem;

                    CloseMenu(CurrentMenuItem);
                    OpenMenuItem_Internal(parentMenuItem);
                }
            }
        }

        void OpenMenuItem_Internal(MenuItem item) {
            UIOpenLastFrame = true;
            item.MenuLogic.Open(item, this);
        }

        public void OpenMenu(string name) {
            OpenMenu(FindMenu(name));
        }
        public void OpenMenu(MenuItem item) {
            if(item == null) return;

            Debug.Log($"OpenMenu {item.Name}");

            if(!_openMenus.Contains(item)) {
                _openMenus.Add(item);
                _openMenuLogic.Add(item.MenuLogic);
            }

            OpenMenuItem_Internal(item);
        }

        public static bool MouseOverUI() {
            return EventSystem.current.IsPointerOverGameObject();
        }

        void OnMainMenuButton() {
            DoPopup("Really?", "Are you sure you want to go back to the Main Menu?", null,
                (uMyGUI_PopupManager.BTN_YES, QuadrablazeSteamNetworking.Current.Stop),
                (uMyGUI_PopupManager.BTN_NO, null)
            );
        }

        void OnModifyButton() {
            EnableKeybindings();
        }

        IEnumerator PauseGameRoutine() {
            yield return new WaitForEndOfFrame();

            while(true) {
                var currentPauseStatus = PauseManager.Current.IsPaused;

                if(currentPauseStatus) {
                    if(!RoundManager.RoundInProgress || _openMenus.Count == 0)
                        PauseManager.Current.IsPaused = false;
                }
                else {
                    if(RoundManager.RoundInProgress && _openMenus.Count > 0)
                        if(CurrentMenuItem.MenuLogic.Options.pauseWhileOpen)
                            PauseManager.Current.IsPaused = true;
                }

                yield return null;
            }
        }

        public static void SelectUIElement(GameObject gameObject) {
            GameDebug.Log(EventSystem.current.currentSelectedGameObject + " - " + gameObject);
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public static void SelectedUIElementDelayed(GameObject gameObject) {
            Current.StartCoroutine("SelectUIElementRoutine", gameObject);
        }

        IEnumerator SelectUIElementRoutine(GameObject gameObject) {
            yield return null;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void SetMenuScene(MenuScene scene) {
            switch(scene) {
                case MenuScene.MainMenu: _currentMenuScene = _mainMenuScene; break;
                case MenuScene.InGame: _currentMenuScene = _inGameMenuScene; break;
            }

            //Debug.Log($"Menu Change: {scene}");
        }

        void SetupPopup() {
            textPopup.OnShow += () => {
                IsPopupShowing = true;
                UIManager.Current.mainMenuGroup.interactable = false;
                UIManager.Current.mainMenuGroup.alpha = .5f;
                GoToMenu(popupMenu, false);
            };

            textPopup.OnHide += () => {
                IsPopupShowing = false;

                if(popupHideAction != null) {
                    popupHideAction();
                    popupHideAction = null;
                }
                else {
                    UIManager.Current.mainMenuGroup.interactable = true;
                    UIManager.Current.mainMenuGroup.alpha = 1;

                    if(RoundManager.RoundInProgress) {
                        EventSystem.current.SetSelectedGameObject(UIManager.Current.resumeButton.gameObject);
                        UIManager.Current.resumeButton.gameObject.GetComponent<Selectable>().Select();
                    }
                    else {
                        EventSystem.current.SetSelectedGameObject(UIManager.Current.playButton.gameObject);
                        UIManager.Current.playButton.gameObject.GetComponent<Selectable>().Select();
                    }
                }

                GoToParentMenu();
            };
        }

        void SetupStartMenuButtons() {
            playButton.onClick.AddListener(() => GoToMenu("Play"));
            selectShipButton.onClick.AddListener(() => GoToMenu("Ship Selection"));
            optionsButton.onClick.AddListener(() => GoToMenu("Options"));
            resumeButton.onClick.AddListener(() => GoToParentMenu());

            mainMenuButton.onClick.AddListener(() => OnMainMenuButton());

            var feedbackMenu = UIManagerTextMeshPro.Instance;
            feedbackMenu.closeButton.onClick.AddListener(() => GoToParentMenu());
            feedbackButton.onClick.AddListener(() => GoToMenu("Feedback"));

            workshopButton.onClick.AddListener(() => GoToMenu("Workshop"));

            quitButton.onClick.AddListener(QuitApplication.QuitApp);
        }

        public void ShowGameOver(bool enable) {
            GameDebug.Log("Game Over", "Player, Death");

            if(enable) gameOver.CrossFadeAlpha(1, 2, false);
            else gameOver.CrossFadeAlpha(0, 0, false);
        }

        public void ShowGoalCompleted(bool enable) {
            GameDebug.Log("Goal Completed", "Player, Goal");

            if(enable) goalCompleted.CrossFadeAlpha(1, 2, false);
            else goalCompleted.CrossFadeAlpha(0, 0, false);
        }

        public void ToggleUpgradeMenu() {
            //EnableUpgradeMenu(!UpgradeMenuOpen);

            if(CurrentMenuItem != null && CurrentMenuItem.Name == "Upgrade") {
                CloseMenus();
            }
            else {
                GoToMenu("Upgrade");

                foreach(var set in inGameUIManager.GetComponentsInChildren<SetTransformOrder>(true))
                    set.UpdateOrder();

                if(InGameUIManager.Current.skillPanel.CurrentSkill == null)
                    InGameUIManager.Current.skillPanel.CurrentSkill = InGameUIManager.Current.skillPanel.GetComponentInChildren<UpgradeSkillDescription>();
            }
        }

        void Update() {
            if(!_initialized) return;

            if(SkipInputFrame) {
                SkipInputFrame = false;
                return;
            }

            if(SkipInputFrames > 0) {
                SkipInputFrames--;
                return;
            }

            if(IsPopupShowing) return;
            if(_isPollingForInput) return;
            if(_controlMapperPopupOpen) {
                SkipInputFrames = 2;
                return;
            }

            if(!UIOpenLastFrame && CurrentMenuItem != null)
                UIOpenLastFrame = true;

            bool playerExists = RewiredPlayer != null;

            bool pauseButton = playerExists && RewiredPlayer.GetButtonDown("Pause Menu");
            bool cancelButton = playerExists && RewiredPlayer.GetButtonDown("Cancel");
            bool tabLeftButton = RewiredPlayer.GetNegativeButtonDown("Menu Tab Navigation");
            bool tabRightButton = RewiredPlayer.GetButtonDown("Menu Tab Navigation");

            bool continueMenuFlag = true;
            //return;
            var currentSelected = EventSystem.current.currentSelectedGameObject;

            if(currentSelected != null) {
                if(
                    currentSelected.GetComponent<InputField>() != null ||
                    currentSelected.GetComponent<TMPro.TMP_InputField>() != null
                    ) {
                    continueMenuFlag = false;
                }
            }

            if(continueMenuFlag)
                if(_openMenus.Count > 0 && CurrentMenuItem != null) {
                    if(cancelButton) {
                        if(!CurrentMenuItem.MenuLogic.Options.disableCancelButton)
                            GoToParentMenu();
                    }
                    else {
                        if(CurrentMenuItem.MenuLogic is TabbedMenu) {
                            var tabbedMenu = CurrentMenuItem.MenuLogic as TabbedMenu;

                            if(tabbedMenu.CanChangeTabs(CurrentMenuItem, this)) {
                                if(tabLeftButton) {
                                    tabbedMenu.ChangeTab(TabbedMenu.TabDirection.Left, CurrentMenuItem, this);
                                }
                                else if(tabRightButton) {
                                    tabbedMenu.ChangeTab(TabbedMenu.TabDirection.Right, CurrentMenuItem, this);
                                }
                            }
                        }
                    }
                }
                else {
                    if(RoundManager.RoundInProgress && pauseButton)
                        GoToMenu("Main");
                }
        }


        public static void GetMenuPathString(List<ScriptableMenu> path) {
            string[] pathNames = new string[path.Count];

            for(int i = 0; i < path.Count; i++)
                pathNames[i] = path[i].MenuName;

            string finalPath = System.IO.Path.Combine(pathNames);
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

        public struct TitleDeactivateOptions {
            public float delay;
            public float fadeTitleOutTime;
            public float fadeStartMenuTitleInTime;
        }

        public enum MenuScene {
            MainMenu,
            InGame
        }
    }
}