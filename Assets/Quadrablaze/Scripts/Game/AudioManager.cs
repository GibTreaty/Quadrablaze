using UnityEngine;

public class AudioManager : MonoBehaviour {

    public void PauseAllSounds(bool enable) {
        if(enable) PauseAllSounds();
        else UnPauseAllSounds();
    }

    public void PauseAllSounds() {
        foreach(AudioSource source in FindObjectsOfType<AudioSource>())
            source.Pause();
    }

    public void UnPauseAllSounds() {
        foreach(AudioSource source in FindObjectsOfType<AudioSource>())
            source.UnPause();
    }
}