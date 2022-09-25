using System;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class UIGameOptions : UIOptionsTab {

        const string gameOptionPrefix = "Game-";
        const string shootOnAimPref = gameOptionPrefix + "ShootOnAim";
        const string cameraShakePref = gameOptionPrefix + "CameraShake";
        const string cameraShakeStrengthPref = gameOptionPrefix + "CameraShakeStrength";

        public Toggle shootOnAimToggle;
        public Toggle cameraShakeToggle;
        public Slider cameraShakeStrengthSlider;
        public TMPro.TMP_Text cameraShakeStrengthText;

        public override void Initialize() {
            base.Initialize();

            shootOnAimToggle.onValueChanged.AddListener(UpdateShootOnAim);
            cameraShakeToggle.onValueChanged.AddListener(UpdateCameraShake);
            cameraShakeStrengthSlider.onValueChanged.AddListener(UpdateCameraShakeStrength);

            UpdateShootOnAim(shootOnAimToggle.isOn);
            UpdateCameraShake(cameraShakeToggle.isOn);
            UpdateCameraShakeStrength(cameraShakeStrengthSlider.value);
        }

        public override void LoadPrefs() {
            shootOnAimToggle.isOn = PlayerPrefs.GetInt(shootOnAimPref, 1) > 0;
            cameraShakeToggle.isOn = PlayerPrefs.GetInt(cameraShakePref, 1) > 0;
            cameraShakeStrengthSlider.value = PlayerPrefs.GetFloat(cameraShakeStrengthPref, 1);
        }

        public override void SavePrefs() {
            PlayerPrefs.SetInt(shootOnAimPref, shootOnAimToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(cameraShakePref, cameraShakeToggle.isOn ? 1 : 0);
            PlayerPrefs.SetFloat(cameraShakeStrengthPref, cameraShakeStrengthSlider.value);
        }

        public override void SetToDefault() {
            shootOnAimToggle.isOn = true;
            cameraShakeToggle.isOn = true;
            cameraShakeStrengthSlider.value = 1;
        }

        void UpdateCameraShake(bool enable) {
            ShakeyCamera.Current.enableShaking = enable;

            ShakeyCamera.Current.Reset();
            ShakeyCamera.Current.ResetPosition();
        }

        void UpdateCameraShakeStrength(float strength) {
            ShakeyCamera.Current.strengthMultiplier = strength;
            cameraShakeStrengthText.text = strength.ToString("f2");
        }

        void UpdateShootOnAim(bool enable) {
            PlayerSpawnManager.Current.shootOnAim = enable;
        }
    }
}