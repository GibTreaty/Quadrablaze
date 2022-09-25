using Quadrablaze;
using UnityEngine;
using UnityEngine.Events;

public class AudioPauseListener : MonoBehaviour {

    UnityAction<bool> onPauseChange;
    AudioSource audioSourceComponent;
    bool wasPlayingBeforePause = false;

    void OnEnable() {
        if(GameManager.Current.Options.GameNetworkConnectionType != GameNetworkType.SinglePlayer) return;
        if(audioSourceComponent == null) audioSourceComponent = GetComponent<AudioSource>();
        if(audioSourceComponent == null) return;

        onPauseChange = OnPauseChange;

        OnPauseChange(PauseManager.Current.IsPaused);

        PauseManager.Current.onPauseChange.AddListener(onPauseChange);
    }

    void OnDisable() {
        if(PauseManager.Current != null && onPauseChange != null)
            PauseManager.Current.onPauseChange.RemoveListener(onPauseChange);

        onPauseChange = null;
    }

    void OnPauseChange(bool value) {
        if(value) {
            wasPlayingBeforePause = audioSourceComponent.isPlaying;
            audioSourceComponent.Pause();
        }
        else {
            if(wasPlayingBeforePause)
                audioSourceComponent.UnPause();
        }
    }
}