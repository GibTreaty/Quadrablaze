using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Quadrablaze {
    public class UIVideoOptions : UIOptionsTab {

        const string resolutionWidthPref = "Screenmanager Resolution Width";
        const string resolutionHeightPref = "Screenmanager Resolution Height";
        const string displayPref = "UnitySelectMonitor";
        const string displayModePref = "Screenmanager Display Mode";
        //const string refreshRatePref = "Screenmanager Refresh Rate";
        const string frameratePref = "Screenmanager Framerate";
        const string vsyncPref = "VSync";

        const string bloomImageEffectPref = "ImageEffect-Bloom";
        const string antiAliasingImageEffectPref = "ImageEffect-AntiAliasing";
        const string tonemappingImageEffectPref = "ImageEffect-Tonemapping";
        const string ambientOcclusionImageEffectPref = "ImageEffect-AmbientOcclusion";

        public Dropdown resolutionDropdown;
        public Dropdown displayDropdown;
        public Dropdown displayModeDropdown;
        //public Dropdown refreshRateDropdown;
        public Slider framerateSlider;
        public TMPro.TMP_Text framerateText;
        //public Toggle fullscreenToggle;
        public Toggle vsyncToggle;

        public Toggle bloomImageEffect;
        public Toggle antiAliasingImageEffect;
        public Toggle tonemappingImageEffect;
        public Toggle ambientOcclusionImageEffect;

        public UnityEngine.PostProcessing.PostProcessingProfile postProcessingStack;

        bool initialized;

        List<ResolutionData> resolutionData = null;
        List<FullScreenMode> displayModes = null;
        List<int> refreshRates = null;

        #region Properties
        public int DisplayId {
            get { return displayDropdown.value; }
        }

        public FullScreenMode DisplayMode {
            get { return displayModes[displayModeDropdown.value]; }
        }

        public int Framerate {
            get {
                if(framerateSlider.value == 0) {
                    return -1;
                }
                else {
                    return (int)framerateSlider.value;
                }
            }
        }

        public int Height {
            get { return ScreenResolution.height; }
        }

        public int Width {
            get { return ScreenResolution.width; }
        }

        //public int RefreshRate {
        //    get { return refreshRates[refreshRateDropdown.value]; }
        //}

        public ResolutionData ScreenResolution {
            get { return resolutionData[resolutionDropdown.value]; }
        }
        #endregion

        protected override void OnEnable() {
            base.OnEnable();

            if(initialized)
                UpdateDropdowns();
        }

        public override void Initialize() {
            base.Initialize();

            //Debug.LogError("All Resolutions");

            //string resolutionLog = "All Resolutions\n";

            //for(int i = 0; i < Screen.resolutions.Length; i++) {
            //    resolutionLog += string.Format("{0}{1}", Screen.resolutions[i].ToString(), i < Screen.resolutions.Length - 1 ? "\n" : "");
            //}

            //Debug.LogError(resolutionLog);

            Display.onDisplaysUpdated += UpdateDisplayDropdown;

            refreshRates = new List<int>();
            resolutionData = new List<ResolutionData>();
            UpdateDropdowns();
            InitializeFullscreenModeDropdown();
            initialized = true;

            framerateSlider.onValueChanged.AddListener(UpdateFramerateSlider);

            vsyncToggle.onValueChanged.AddListener(s => QualitySettings.vSyncCount = s ? 1 : 0);
            bloomImageEffect.onValueChanged.AddListener(s => Camera.main.GetComponent<AmplifyBloom.AmplifyBloomEffect>().enabled = s);
            antiAliasingImageEffect.onValueChanged.AddListener(s => postProcessingStack.antialiasing.enabled = s);
            tonemappingImageEffect.onValueChanged.AddListener(s => postProcessingStack.colorGrading.enabled = s);
            ambientOcclusionImageEffect.onValueChanged.AddListener(s => postProcessingStack.ambientOcclusion.enabled = s);

            //resolutionDropdown.onValueChanged.AddListener(value => UpdateRefreshRateDropdowns());
        }

        void InitializeFullscreenModeDropdown() {
            var modes = (FullScreenMode[])System.Enum.GetValues(typeof(FullScreenMode));

            displayModes = new List<FullScreenMode>(modes);

            displayModeDropdown.AddOptions(new List<string>{
                "Exclusive Fullscreen",
                "Fullscreen Window",
                "Maximized Window",
                "Windowed"
            });
        }

        int GetDisplayModeIndex(int displayMode) {
            FullScreenMode mode;

            return (int)(System.Enum.TryParse(displayMode.ToString(), out mode) ? mode : FullScreenMode.ExclusiveFullScreen);
        }

        int GetRefreshRateIndex(int refreshRate) {
            var nearestRefreshRate = refreshRates[refreshRates.Count - 1];
            var distance = Mathf.Abs(nearestRefreshRate - refreshRate);

            for(int i = 0; i < refreshRates.Count - 1; i++) {
                var checkDistance = Mathf.Abs(refreshRates[i] - refreshRate);

                if(checkDistance < distance) {
                    nearestRefreshRate = refreshRates[i];
                    distance = checkDistance;
                }
            }

            return refreshRates.IndexOf(nearestRefreshRate);
        }

        int GetResolutionIndex(int width, int height) {
#if UNITY_EDITOR
            return 0;
#else
            return Mathf.Max(resolutionData.IndexOf(new ResolutionData(width, height)), 0);
#endif

            //var compare = new ResolutionData(width, height);

            //for(int i = 0; i < resolutionData.Count; i++)
            //    if(resolutionData[i].Equals(compare))
            //        return i;

            //return 0;
        }

        public override void LoadPrefs() {

            //if(displayMode > Display.displays.Length - 1)
            //    displayMode = Display.displays.Length - 1;

            int displayNumber = PlayerPrefs.GetInt(displayPref, 0);

            var defaultResolution = new Resolution() { width = Display.displays[displayNumber].renderingWidth, height = Display.displays[displayNumber].renderingHeight, refreshRate = Screen.currentResolution.refreshRate };
            //var defaultResolution = Screen.resolutions[0]; // TODO Use Display not Screen

            int width = PlayerPrefs.GetInt(resolutionWidthPref, defaultResolution.width);
            int height = PlayerPrefs.GetInt(resolutionHeightPref, defaultResolution.height);
            int displayMode = PlayerPrefs.GetInt(displayModePref, 0);
            //int refreshRate = PlayerPrefs.GetInt(refreshRatePref, defaultResolution.refreshRate);
            int framerate = PlayerPrefs.GetInt(frameratePref, -1);
            bool vsync = PlayerPrefs.GetInt(vsyncPref, 1) > 0;

            bool bloom = PlayerPrefs.GetInt(bloomImageEffectPref, 1) > 0;
            bool antiAliasing = PlayerPrefs.GetInt(antiAliasingImageEffectPref, 1) > 0;
            bool tonemapping = PlayerPrefs.GetInt(tonemappingImageEffectPref, 1) > 0;
            bool ambientOcclusion = PlayerPrefs.GetInt(ambientOcclusionImageEffectPref, 1) > 0;

            Canvas.ForceUpdateCanvases();

            displayDropdown.value = displayNumber;
            displayModeDropdown.value = GetDisplayModeIndex(displayMode);
            resolutionDropdown.value = GetResolutionIndex(width, height);
            //refreshRateDropdown.value = GetRefreshRateIndex(refreshRate);
            framerateSlider.value = framerate;
            vsyncToggle.isOn = vsync;

            bloomImageEffect.isOn = bloom;
            antiAliasingImageEffect.isOn = antiAliasing;
            tonemappingImageEffect.isOn = tonemapping;
            ambientOcclusionImageEffect.isOn = ambientOcclusion;

            Application.targetFrameRate = Framerate;

            UpdateFramerateSlider(Framerate);
            UpdateFullscreenCursorLock();
        }

        public override void SavePrefs() {
            PlayerPrefs.SetInt(resolutionWidthPref, Width);
            PlayerPrefs.SetInt(resolutionHeightPref, Height);
            PlayerPrefs.SetInt(displayModePref, (int)DisplayMode);
            //PlayerPrefs.SetInt(refreshRatePref, RefreshRate);
            PlayerPrefs.SetInt(frameratePref, Framerate);
            PlayerPrefs.SetInt(vsyncPref, QualitySettings.vSyncCount > 0 ? 1 : 0);

            PlayerPrefs.SetInt(bloomImageEffectPref, bloomImageEffect.isOn ? 1 : 0);
            PlayerPrefs.SetInt(antiAliasingImageEffectPref, antiAliasingImageEffect.isOn ? 1 : 0);
            PlayerPrefs.SetInt(tonemappingImageEffectPref, tonemappingImageEffect.isOn ? 1 : 0);
            PlayerPrefs.SetInt(ambientOcclusionImageEffectPref, ambientOcclusionImageEffect.isOn ? 1 : 0);

            Debug.LogError("displayDropdown.value " + displayDropdown.value);

            if(displayDropdown.value < Display.displays.Length)
                PlayerPrefs.SetInt(displayPref, displayDropdown.value);

            PlayerPrefs.Save();

            //var currentDisplay = Display.displays[Mathf.Max(displayDropdown.value - 1, 0)];

            //Debug.LogError("Set resolution to " + new Resolution() { width = Width, height = Height, refreshRate = RefreshRate });
            
            Screen.SetResolution(Width, Height, DisplayMode);
            //Screen.SetResolution(Width, Height, DisplayMode, RefreshRate);
            //currentDisplay.Activate(Width, Height, RefreshRate);

            UpdateResolutionDropdown();
            //UpdateRefreshRateDropdowns();

            Application.targetFrameRate = Framerate;
            UpdateFullscreenCursorLock();

            //Debug.LogError("All Resolutions");

            //string resolutionLog = "All Resolutions\n";

            //for(int i = 0; i < Screen.resolutions.Length; i++) {
            //    resolutionLog += string.Format("{0}{1}", Screen.resolutions[i].ToString(), i < Screen.resolutions.Length - 1 ? "\n" : "");
            //}

            //Debug.LogError(resolutionLog);

            if(isActiveAndEnabled)
                StartCoroutine(UpdateCanvases());
        }

        public override void SetToDefault() {
            var width = Display.displays[0].renderingWidth;
            var height = Display.displays[0].renderingHeight;

            Screen.SetResolution(width, height, FullScreenMode.ExclusiveFullScreen);

            resolutionDropdown.value = GetResolutionIndex(width, height);


            //RefreshDropdownValue();
        }

        IEnumerator UpdateCanvases() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            Camera.main.ResetAspect();
        }

        void UpdateDisplayDropdown() {
            var displays = new List<string>();

            if(Display.displays.Length <= 1)
                displays.Add("Only One Monitor");
            else
                for(int i = 0; i < Display.displays.Length; i++)
                    displays.Add((i + 1).ToString());

            displayDropdown.ClearOptions();
            displayDropdown.AddOptions(displays);
            displayDropdown.interactable = displays.Count > 1;
        }

        void UpdateDropdowns() {
            UpdateResolutionDropdown();
            UpdateDisplayDropdown();
            //UpdateRefreshRateDropdowns();
        }

        void UpdateFramerateSlider(float framerate) {
            framerateText.text = framerate <= 0 ? "Unlimited" : framerate + "fps";
        }

        void UpdateFullscreenCursorLock() {
            switch(DisplayMode) {
                case FullScreenMode.ExclusiveFullScreen:
                case FullScreenMode.FullScreenWindow:
                    Cursor.lockState = CursorLockMode.Confined;
                    break;
                default:
                    Cursor.lockState = CursorLockMode.None;
                    break;
            }
        }

        //void UpdateRefreshRateDropdowns() {
        //    refreshRates.Clear();

        //    var refreshRateStrings = new List<string>();

        //    foreach(var resolution in Screen.resolutions) {
        //        var data = new ResolutionData(resolution.width, resolution.height);

        //        if(Screen.currentResolution.width == data.width && Screen.currentResolution.height == data.height) {
        //            var refreshRate = resolution.refreshRate;

        //            refreshRate++;

        //            if(!refreshRates.Contains(refreshRate)) {
        //                refreshRates.Add(refreshRate);
        //                refreshRateStrings.Add(refreshRate.ToString());
        //            }
        //        }
        //    }

        //    refreshRateDropdown.ClearOptions();
        //    refreshRateDropdown.AddOptions(refreshRateStrings);
        //}

        void UpdateResolutionDropdown() {
            resolutionData.Clear();

            var resolutions = new List<string>();

            foreach(var resolution in Screen.resolutions) {
                var data = new ResolutionData(resolution.width, resolution.height);

                if(!resolutionData.Contains(data)) {
                    resolutionData.Add(data);
                    resolutions.Add(data.ToString());
                }
            }



            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutions);

            if(isActiveAndEnabled)
                StartCoroutine(UpdateResolutionValue());
        }

        IEnumerator UpdateResolutionValue() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            var currentDisplay = Display.displays[Mathf.Max(displayDropdown.value - 1, 0)];

            //Debug.LogError("Screen Resolution: " + Screen.currentResolution.ToString());
            //Debug.LogError("Display Resolution: " + new Resolution() { width = Display.main.renderingWidth, height = Display.main.renderingHeight });

            resolutionDropdown.value = GetResolutionIndex(currentDisplay.renderingWidth, currentDisplay.renderingHeight);
            //refreshRateDropdown.value = GetRefreshRateIndex(Screen.currentResolution.refreshRate);

        }

        public struct ResolutionData {
            public int width, height;

            public ResolutionData(int width, int height) {
                this.width = width;
                this.height = height;
            }

            public override bool Equals(object obj) {
                if(!(obj is ResolutionData))
                    return false;

                var other = (ResolutionData)obj;

                return width == other.width && height == other.height;
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }

            public override string ToString() {
                return width + "x" + height;
            }
        }
    }
}