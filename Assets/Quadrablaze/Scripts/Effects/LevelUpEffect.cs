using System.Collections;
using UnityEngine;

public class LevelUpEffect : MonoBehaviour {

    [SerializeField]
    float _speed = 1;

    [SerializeField]
    AnimationCurve _effectCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 0));

    Material _levelUpMaterial;
    float _time;

    public float Time {
        get { return _time; }
        set {
            float newValue = Mathf.Clamp01(value);

            if(_time == newValue) return;

            _time = newValue;

            UpdateMaterial();
        }
    }

    void OnEnable() {
        Time = 0;
        UpdateMaterial();
    }

    void OnDisable() {
        Time = 0;
        UpdateMaterial();
    }

    void Awake() {
        _levelUpMaterial = GetComponent<MeshRenderer>().material;
    }

    [ContextMenu("Leveled Up")]
    public void LeveledUp() {
        if(!Application.isPlaying) return;

        StopCoroutine(DoEffect());
        StartCoroutine(DoEffect());
    }

    IEnumerator DoEffect() {
        Time = 0;

        while(Time < 1) {
            Time = Mathf.Min(Time + UnityEngine.Time.deltaTime * _speed, 1);

            yield return new WaitForEndOfFrame();
        }
    }

    void UpdateMaterial() {
        _levelUpMaterial.SetFloat("_Height", _effectCurve.Evaluate(Time));
    }
}