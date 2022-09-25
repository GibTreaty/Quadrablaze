using UnityEngine;

[CreateAssetMenu(fileName = "Sound Item", menuName = "Create Sound Item")]
public class SoundItemAsset : ScriptableObject {

    [SerializeField]
    string _name;

    [SerializeField]
    AudioClip _clip;

    #region Properties
    public string Name {
        get { return _name; }
    }

    public AudioClip Clip {
        get { return _clip; }
    }
    #endregion
}