using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIAudioBar : MonoBehaviour {

    [SerializeField]
    AudioMixer _mixer;

    [SerializeField]
    string _audioMixerVariable = "";

    [SerializeField]
    string _playerPrefsKey = "";

    [SerializeField]
    Slider _audioSlider;

    [SerializeField]
    TextMeshProUGUI _audioAmountText;

    [SerializeField]
    AnimationCurve _sliderCurve;

    [SerializeField]
    bool _showDecibels;

    bool eventsConnected;

    #region Properties
    public TextMeshProUGUI AudioAmountText {
        get { return _audioAmountText; }
        set { _audioAmountText = value; }
    }

    public string AudioMixerVariable {
        get { return _audioMixerVariable; }
        set { _audioMixerVariable = value; }
    }

    public Slider AudioSlider {
        get { return _audioSlider; }
        set { _audioSlider = value; }
    }

    public AudioMixer Mixer {
        get { return _mixer; }
        set { _mixer = value; }
    }

    public string PlayerPrefsKey {
        get { return _playerPrefsKey; }
        set { _playerPrefsKey = value; }
    }

    public bool ShowDecibels {
        get { return _showDecibels; }
        set { _showDecibels = value; }
    }
    #endregion

    void Awake() {
        ConnectEvents();
    }

    void ConnectEvents() {
        if(!eventsConnected) {
            AudioSlider.onValueChanged.AddListener(s => UpdateAudioAmountText());
            AudioSlider.onValueChanged.AddListener(UpdateAudioMixer);

            UpdateAudioAmountText();

            eventsConnected = true;
        }
    }

    public void LoadPrefs() {
        ConnectEvents();
        AudioSlider.value = PlayerPrefs.GetFloat(PlayerPrefsKey, .5f);
    }

    public void SavePrefs() {
        //float volume;

        //if(Mixer.GetFloat(AudioMixerVariable, out volume))
            PlayerPrefs.SetFloat(PlayerPrefsKey, AudioSlider.value);
    }

    public void SetValue(float value) {
        AudioSlider.value = value;
    }

    public void UpdateAudioMixer(float value) {
        //Mixer.SetFloat(AudioMixerVariable, value);
        Mixer.SetFloat(AudioMixerVariable, GetVolume());
    }

    public void UpdateAudioAmountText() {
        AudioAmountText.text = ShowDecibels ?
        GetVolume().ToString("0") + "db" :
        (AudioSlider.normalizedValue * 100).ToString("0") + "%";
    }

    //float GetVolume(float volume) {
    //    return _sliderCurve.Evaluate(Mathf.InverseLerp(-80, 0, volume)) * -80;
    //}
    float GetVolume() {
        return _sliderCurve.Evaluate(1 - AudioSlider.normalizedValue) * -80;
    }
}