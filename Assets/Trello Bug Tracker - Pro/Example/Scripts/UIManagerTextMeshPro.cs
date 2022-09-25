using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DG {
    using System;
    using Quadrablaze;
    using TMPro;
    using TrelloAPI;
    using UIWidgets;
    using UnityEngine.EventSystems;
    using Util;

    public class UIManagerTextMeshPro : Singleton<UIManagerTextMeshPro> {
        public bool pauseGameOnActive = true;
        [Header("UI objects")]
        public Canvas canvas;
        public CanvasGroup canvasGroup;
        public TMP_InputField inputTitle;
        public TMP_InputField inputDescription;
        public Painter painter;
        public Button feedbackButton;
        public TMP_Text feedbackText;
        public ListView feedbackTypeListView;

        public Button sendFeedbackButton;
        public Button saveScreenshotButton;
        public Button closeButton;

        public GameObject defaultPanel;
        public GameObject feedbackTypePanel;

        public List<RawImage> screenshotSlots;


        // used to keep track of what texture is being manipulated by the drawings tool
        private int _screenshotIndex;

        public void Initialize() {
            Instance = this;

            if(UsageExample.Instance != null)
                foreach(var option in UsageExample.Instance.reportTypes)
                    feedbackTypeListView.Add(option.text);

            sendFeedbackButton.onClick.AddListener(() => { ReportIssue(); });

            saveScreenshotButton.onClick.AddListener(() => TakeScreenshotButton());

            feedbackButton.onClick.AddListener(() => EnableFeedbackListView());
            feedbackTypeListView.OnSelect.AddListener((s, x) => OnFeedbackTypeSelect());
            feedbackText.text = feedbackTypeListView.DataSource[0];

            painter._OnDisable += UpdateUITexture;

            EnableFeedbackListView(false);
        }

        public void Open() {
            inputTitle.text = "";
            inputDescription.text = "";
            RemoveScreenshot(0);
            RemoveScreenshot(1);
            RemoveScreenshot(2);
            TakeScreenshotButton();

            EnableFeedbackListView(false);
        }

        //public void Close() {
        //    RemoveScreenshot(0);
        //    RemoveScreenshot(1);
        //    RemoveScreenshot(2);
        //}

        public void EnableFeedbackListView(bool enable = true) {
            //EventSystem.current.SetSelectedGameObject(null);

            feedbackTypePanel.SetActive(enable);
            defaultPanel.SetActive(!enable);

            //feedbackTypeListView.Select(feedbackTypeListView.SelectedIndex);

            //if(enable) {
            //foreach(var selectable in feedbackTypeListView.GetComponentsInChildren<Selectable>()) {
            //    selectable.Select();
            //}
            //foreach(var uiBehaviour in feedbackTypeListView.GetComponentsInChildren<UIBehaviour>()) {
            //    ExecuteEvents.Execute<IDeselectHandler>(uiBehaviour.gameObject, null, (x,y) => { x.OnDeselect(y); });
            //    //EventSystem.current.
            //    //uiBehaviour.();
            //    //Quadrablaze.UIManager.SelectedUIElementDelayed(uiBehaviour.gameObject);

            //}
            //}
            //if(enable) {
            //    Debug.Log("Select? " + feedbackTypePanel.GetComponentInChildren<Selectable>()?.gameObject, feedbackTypePanel.GetComponentInChildren<Selectable>()?.gameObject);
            //    Quadrablaze.UIManager.SelectedUIElementDelayed(feedbackTypePanel.GetComponentInChildren<Selectable>()?.gameObject);
            //}
            //else
            //    Quadrablaze.UIManager.SelectedUIElementDelayed(defaultPanel.GetComponentInChildren<Selectable>()?.gameObject);
        }

        void OnFeedbackTypeSelect() {
            EnableFeedbackListView(false);

            feedbackText.text = feedbackTypeListView.DataSource[feedbackTypeListView.SelectedIndex];
        }

        public void ReportIssue() {
            if(UsageExample.Instance != null) {
                var usedSlots = screenshotSlots.FindAll((RawImage ri) => { return ri.texture != null; });
                List<Texture2D> screenshots = new List<Texture2D>();

                foreach(RawImage ri in usedSlots)
                    screenshots.Add((Texture2D)ri.texture);

                if(UsageExample.Instance.SendReport(inputTitle.text, inputDescription.text, feedbackText.text, screenshots) != null) {
                    // After reporting We clear the input fields so they are ready to be used again
                    inputTitle.text = "";
                    inputDescription.text = "";

                    RemoveScreenshot(0);
                    RemoveScreenshot(1);
                    RemoveScreenshot(2);
                    TakeScreenshotButton();
                }
            }
        }

        public void TakeScreenshotButton() {
            StartCoroutine(TakeScreenshotRoutine());
        }

        public IEnumerator TakeScreenshotRoutine() {
            // check if any screenshot slot has its raycast target deactivated, which means it has no screenshot attached
            // if all screenshots are full, a new screenshot is not taken and the control is returned
            int index = screenshotSlots.FindIndex((RawImage ri) => { return ri.texture == null; });
            if(index < 0) yield break;

            canvas.enabled = false;

            yield return new WaitForEndOfFrame();
            screenshotSlots[index].texture = (ScreenshotTool.TakeScreenshot());
            screenshotSlots[index].color = Color.white;

            canvas.enabled = true;
        }

        public void OpenPainter(int screenshotIndex) {
            _screenshotIndex = screenshotIndex;
            if(screenshotSlots[screenshotIndex].texture != null) {
                painter.enabled = true;
                painter.baseTex = (Texture2D)screenshotSlots[screenshotIndex].texture;
            }
        }

        public void UpdateUITexture() {
            screenshotSlots[_screenshotIndex].texture = painter.baseTex;
        }

        public void RemoveScreenshot(int screenshotIndex) {
            screenshotSlots[screenshotIndex].texture = null;
            //screenshotSlots[screenshotIndex].color = new Color(0.36f, 0.15f, 0.6f, 1);
            screenshotSlots[screenshotIndex].color = Color.clear;
        }
    }
}