using UnityEngine;

[CreateAssetMenu(fileName = "Animation Curve Asset")]
public class AnimationCurveAsset : ScriptableObject {

    [SerializeField]
    AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);

    #region Properties
    public AnimationCurve Curve {
        get { return _curve; }
        set { _curve = value; }
    }
    #endregion
    
    /// <summary>Input time and output curve value.</summary>
    public float Evaluate(float time) {
        return _curve.Evaluate(time);
    }
}