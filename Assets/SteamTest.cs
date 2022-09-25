using Steamworks;
using UnityEngine;

public class SteamTest : MonoBehaviour {

    void Start() {
        Debug.developerConsoleVisible = true;
        Debug.Log("IsSteamRunning:" + SteamAPI.IsSteamRunning());
        Debug.LogError("Packsize:" + Packsize.Test());
        Debug.LogError("DllCheck:" + DllCheck.Test());
        //Debug.Log("Init:" + SteamAPI.Init());
        Debug.LogError("Init:" + SteamAPI.Init());
        SteamAPI.Shutdown();
    }
}