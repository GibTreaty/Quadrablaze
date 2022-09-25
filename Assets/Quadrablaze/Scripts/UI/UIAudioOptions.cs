using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Quadrablaze {
    public class UIAudioOptions : UIOptionsTab {

        [Header("Sliders")]
        public UIAudioBar masterBar;
        public UIAudioBar gameBar;
        public UIAudioBar musicBar;

        public override void LoadPrefs() {
            masterBar.LoadPrefs();
            gameBar.LoadPrefs();
            musicBar.LoadPrefs();
        }

        public override void SavePrefs() {
            masterBar.SavePrefs();
            gameBar.SavePrefs();
            musicBar.SavePrefs();

            PlayerPrefs.Save();
        }

        public override void SetToDefault() {
            masterBar.SetValue(0);
            gameBar.SetValue(0);
            musicBar.SetValue(0);
        }
    }
}