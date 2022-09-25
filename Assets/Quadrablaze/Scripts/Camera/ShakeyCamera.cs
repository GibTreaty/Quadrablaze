using UnityEngine;
using System.Collections;

public class ShakeyCamera : MonoBehaviour {

    public static ShakeyCamera Current { get; private set; }

    public float recoverStrength = 1;
    public float strengthMultiplier = 1;
    public AnimationCurve shakeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    public Transform shakePivot;
    public bool enableShaking = true;

    Vector3 startPosition;
    Vector3 offset;
    float velocity;

    void OnEnable() {
        Current = this;
    }

    void Awake() {
        startPosition = shakePivot.localPosition;
    }

    void FixedUpdate() {
        if(!enableShaking) return;

        velocity -= velocity * Time.deltaTime * recoverStrength;

        if(velocity > 0)
            offset = Random.insideUnitSphere * shakeCurve.Evaluate(velocity) * strengthMultiplier * velocity;

        shakePivot.localPosition = startPosition + offset;
    }

    public void Reset() {
        velocity = 0;
    }

    public void ResetPosition() {
        shakePivot.localPosition = startPosition;
    }

    public void Shake(float power) {
        if(enableShaking)
            velocity += power;
    }

    public void ShakeCapped(float power) {
        if(enableShaking)
            velocity += Mathf.Max(power - velocity, 0);
    }

    [ContextMenu("Shake")]
    void ShakeTest() {
        float value = Random.value * 10;

        Shake(value);
        GameDebug.Log("Shaking Camera(" + value + ")");
    }
}