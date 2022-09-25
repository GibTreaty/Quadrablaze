using UnityEngine;
using UnityEngine.Networking;

public class NetworkAudio : NetworkBehaviour {

    #region Properties
    AudioSource AudioSourceComponent { get; set; }
    #endregion

    void Awake() {
        AudioSourceComponent = GetComponent<AudioSource>();

        if(!AudioSourceComponent) enabled = false;
    }

    [Server]
    public void Play() {
        Rpc_Play();
    }

    void Internal_Play() {
        AudioSourceComponent?.Play();
    }

    [ClientRpc]
    void Rpc_Play() {
        Internal_Play();
    }
}