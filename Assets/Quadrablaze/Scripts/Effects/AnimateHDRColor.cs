using UnityEngine;

public class AnimateHDRColor : MonoBehaviour {

    [ColorUsage(false, true)]
    public Color fromColor = Color.white;

    [ColorUsage(false, true)]
    public Color toColor = Color.white;

    public float speed = 1;
    public float time;
    public float pingPongTime;

    public string propertyName;

    public Material material;


    void Awake() {
        //colorProperty.SetColor

        if(!material.HasProperty(propertyName)) {
            Debug.Log("Color property not found : " + propertyName);
            enabled = false;
        }
    }

    void Update() {
        time = (time + Time.deltaTime * speed) % 2;
        pingPongTime = Mathf.PingPong(time, 1);

        material.SetColor(propertyName, Color.Lerp(fromColor, toColor, pingPongTime));
    }

    [ContextMenu("From Color")]
    void FromColor() {
        material.SetColor(propertyName, fromColor);
    }

    [ContextMenu("To Color")]
    void ToColor() {
        material.SetColor(propertyName, toColor);
    }
}