using System;
using UnityEngine;

public class AnimateMineMaterial : MonoBehaviour {

    [ColorUsage(false, true)]
    [SerializeField]
    Color fromColor = Color.white;

    [ColorUsage(false, true)]
    [SerializeField]
    Color toColor = Color.white;

    [SerializeField]
    float _speed = 1;

    [SerializeField]
    float _time;

    [SerializeField]
    string _propertyName;

    MaterialPropertyBlock colorProperty;

    public static event Action<MaterialPropertyBlock> OnColorUpdated;

    void Awake() {
        colorProperty = new MaterialPropertyBlock();
    }

    void Update() {
        //if(RoundManager.Current && RoundManager.Current.RoundInProgress) {
        _time = (_time + Time.deltaTime * _speed) % 2;
        var pingPongTime = Mathf.PingPong(_time, 1);

        colorProperty.SetColor(_propertyName, Color.Lerp(fromColor, toColor, pingPongTime));

        OnColorUpdated?.Invoke(colorProperty);
        //}
    }
}