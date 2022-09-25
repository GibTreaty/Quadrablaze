using Quadrablaze;
using UnityEngine;
using UnityEngine.Networking;

//[ExecuteInEditMode]
public class SkyboxEffects : MonoBehaviour {

    public static SkyboxEffects Current { get; private set; }

    public string colorProperty = "_Tint";
    public float colorChangeSpeed = 1;

    public Color editorColor = Color.white;
    public Color color = Color.white;

    public bool randomizeColorOnAwake = true;
    public float sendHueFrequency = 2f;
    public bool updateHue = true;

    public float currentHue;

    Material skyboxMaterial;
    MaterialPropertyBlock skyboxPropertyBlock;

    Vector3 hsv;
    float lastSendTime;

    void Awake() {
        Current = this;

        skyboxMaterial = Instantiate(RenderSettings.skybox);
        RenderSettings.skybox = skyboxMaterial;

        if(Application.isPlaying) {
            if(randomizeColorOnAwake)
                currentHue = Random.value;
        }
        else {
            RenderSettings.skybox.SetColor(colorProperty, editorColor);
        }

        color = RenderSettings.skybox.GetColor(colorProperty);
        Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);
    }

    void Update() {
        if(Application.isPlaying && updateHue) {
            currentHue += colorChangeSpeed * Time.unscaledDeltaTime;
            currentHue %= 1;

            color = Color.HSVToRGB(currentHue, hsv.y, hsv.z);
        }

        RenderSettings.skybox.SetColor(colorProperty, Application.isPlaying ? color : editorColor);

        if(NetworkServer.active && QuadrablazeSteamNetworking.Current.SteamConnections != null && QuadrablazeSteamNetworking.Current.SteamConnections.Count > 1)
            if(Time.time - lastSendTime > sendHueFrequency) {
                lastSendTime = Time.time;

                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_SkyboxHue);
                writer.Write(currentHue);
                writer.FinishMessage();

                foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
                    if(connection != null)
                        if(connection.hostId > -1)
                            if(connection.isReady)
                                connection.SendWriter(writer, 0);

                //foreach(var player in QuadrablazeNetworkManager.Current.Players)
                //    if(!player.isHost)
                //        player.connection.SendWriter(writer, Channels.DefaultUnreliable);
            }
    }

    public static void SetCurrentHue(NetworkMessage netMessage) {
        if(!Current) return;

        Current.currentHue = netMessage.reader.ReadSingle();
    }
}