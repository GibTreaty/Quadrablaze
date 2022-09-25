using System;
using System.Collections.Generic;
using DG;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Quadrablaze {
    public class GlobalInput : MonoBehaviour {

        public static GlobalInput Current { get; private set; }
        public static Controller LastUsedController { get; private set; }

        public static ControllerTypeEvent OnChangedController = new ControllerTypeEvent();
        public static ControllerTypeEvent OnChangedPCorConsoleController = new ControllerTypeEvent();

        /// <summary>
        /// Only fires if you have changed from using a Keyboard+Mouse to a Gamepad or the other way around. Will not fire if changing from using a Keyboard to Mouse or Mouse to Keyboard.
        /// </summary>

        #region Properties
        public bool OpenedWithHotkey {
            get { return _openedWithHotkey; }
            private set { _openedWithHotkey = value; }
        }

        Player RewiredPlayer { get; set; }

        public bool UsingGamepad {
            get { return LastUsedController.type == ControllerType.Joystick || LastUsedController.type == ControllerType.Custom; }
        }

        public bool UsingKeyboardAndMouse {
            get { return LastUsedController.type == ControllerType.Keyboard || LastUsedController.type == ControllerType.Mouse; }
        }
        #endregion

        List<DisableCanvasGroup> disableCanvases = new List<DisableCanvasGroup>();
        bool _pauseState = false;
        bool _openedWithHotkey = false;

        void OnEnable() {
            Current = this;
        }

        void Awake() {
            RewiredPlayer = ReInput.players.GetPlayer(0);
        }

        void Update() {
            UpdateMenuInput();
        }

        void UpdateMenuInput() {
            if(!UIManager.Current) return;

            if(RewiredPlayer.GetButtonDown("Feedback"))
                if(!UIManager.Current.IsFeedbackPanelOpen)
                    if(!UIManager.Current.optionsUIManager.uiControlList.controlMapper.isOpen)
                        OpenFeedback();

            //if(RewiredPlayer.GetButtonDown("Pause Menu"))
            //    if(UIManager.Current.IsFeedbackPanelOpen) {
            //        UIManager.Current.SkipInputFrame = true;
            //        UIManager.Current.CloseMenu("Feedback");
            //        //UIManager.Current.EnableTrelloUIManager(false, !OpenedWithHotkey);
            //    }

            //return;

            //var checkController = RewiredPlayer.controllers.GetLastActiveController();

            //if(checkController != null) {
            //    var previousDevice = LastUsedController;
            //    LastUsedController = checkController;

            //    if(previousDevice != LastUsedController) {
            //        Debug.Log("Changed controller " + LastUsedController.name);

            //        OnChangedController.InvokeEvent(LastUsedController);

            //        switch(LastUsedController.type) {
            //            case ControllerType.Keyboard:
            //            case ControllerType.Mouse:
            //                if(previousDevice == null || previousDevice.type != ControllerType.Keyboard || previousDevice.type != ControllerType.Mouse)
            //                    OnChangedPCorConsoleController.InvokeEvent(LastUsedController);

            //                break;

            //            case ControllerType.Joystick:
            //                if(previousDevice == null || previousDevice.type != ControllerType.Joystick)
            //                    OnChangedPCorConsoleController.InvokeEvent(LastUsedController);

            //                break;
            //        }
            //    }
            //}
        }

        public void ForceUpdateActiveController() {
            var checkController = RewiredPlayer.controllers.GetLastActiveController();

            if(checkController != null) {
                LastUsedController = checkController;

                OnChangedController.InvokeEvent(LastUsedController);
                OnChangedPCorConsoleController.InvokeEvent(LastUsedController);
            }
        }

        void OpenFeedback() {
            OpenedWithHotkey = true;
            _pauseState = PauseManager.Current.IsPaused;
            PauseManager.Current.IsPaused = true;

            disableCanvases.Clear();

            var gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach(var gameObject in gameObjects) {
                //if(canvasGroup == !DG.UIManagerTextMeshPro.Instance?.canvasGroup) continue;
                //disableCanvases.AddRange(gameObject.GetComponentsInChildren<CanvasGroup>(true));

                foreach(var canvasGroup in gameObject.GetComponentsInChildren<CanvasGroup>(true)) {
                    //if(canvasGroup == UIManagerTextMeshPro.Instance.canvasGroup) continue;
                    disableCanvases.Add(new DisableCanvasGroup(canvasGroup));
                }
            }

            UIManager.Current.onFeedbackPanelState.AddListener(CheckFeedbackPanelState);

            UIManager.Current.OpenMenu("Feedback");
            //UIManager.Current.EnableTrelloUIManager(true, false);

            foreach(var canvasGroup in UIManagerTextMeshPro.Instance.GetComponentsInChildren<CanvasGroup>(true)) {
                var disableCanvasGroup = disableCanvases.Find(s => s.canvasGroup == canvasGroup);

                if(disableCanvasGroup != null) {
                    disableCanvasGroup.Revert();
                    disableCanvases.Remove(disableCanvasGroup);
                }
            }
            //foreach(var canvasGroup in FindObjectsOfType<CanvasGroup>()) {
            //    if(canvasGroup == !DG.UIManagerTextMeshPro.Instance?.canvasGroup) continue;

            //    disableCanvases.Add(new DisableCanvasGroup(canvasGroup));
            //}
        }

        void CloseFeedback() {
            foreach(var canvasGroup in disableCanvases)
                canvasGroup.Revert();

            disableCanvases.Clear();

            UIManager.Current.onFeedbackPanelState.RemoveListener(CheckFeedbackPanelState);
            PauseManager.Current.IsPaused = _pauseState;
            OpenedWithHotkey = false;
        }

        void CheckFeedbackPanelState(bool state) {
            if(!state) CloseFeedback();
        }

        [System.Serializable]
        public class ControllerTypeEvent : UnityEvent<Controller> { }

        class DisableCanvasGroup {
            public CanvasGroup canvasGroup;

            bool interactableStateBefore;

            public DisableCanvasGroup(CanvasGroup canvasGroup) {
                this.canvasGroup = canvasGroup;
                interactableStateBefore = canvasGroup.interactable;

                Disable();
            }

            public void Disable() {
                canvasGroup.interactable = false;
            }

            public void Revert() {
                canvasGroup.interactable = interactableStateBefore;
            }
        }
    }
}