using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DG {
    using UnityEngine.Networking;
    using Util;

    public class UIManager : Singleton<UIManager> {
        public bool pauseGameOnActive = true;
        [Header("UI objects")]
        public Canvas canvas;
        public InputField inputTitle;
        public InputField inputDescription;
        public Dropdown reportOptionsDropDown;
        public Painter painter;
        public List<RawImage> screenshotSlots;

        bool paused;

        // used to keep track of what texture is being manipulated by the drawings tool
        private int _screenshotIndex;

        void Start() {
            if(UsageExample.Instance != null)
                reportOptionsDropDown.AddOptions(UsageExample.Instance.reportTypes);

            painter._OnDisable += UpdateUITexture;
        }

        public void ReportIssue() {
            if(UsageExample.Instance != null) {
                var usedSlots = screenshotSlots.FindAll((RawImage ri) => { return ri.texture != null; });
                List<Texture2D> screenshots = new List<Texture2D>();

                foreach(RawImage ri in usedSlots) {
                    screenshots.Add((Texture2D)ri.texture);
                }

                if(UsageExample.Instance.SendReport(inputTitle.text, inputDescription.text, reportOptionsDropDown.captionText.text, screenshots) != null) {
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

        public void PauseGame() {
            if(!pauseGameOnActive) return;
            if(!paused) {
                paused = true;

                Time.timeScale = 0;
                //Stop other scripts that dont use time scale
            }
            else {
                paused = false;
                Time.timeScale = 1;
                //Re-activate other scripts that dont use time scale
            }
        }

        public void RemoveScreenshot(int screenshotIndex) {
            screenshotSlots[screenshotIndex].texture = null;
            screenshotSlots[screenshotIndex].color = new Color(0.36f, 0.15f, 0.6f, 1);
        }
    }
}