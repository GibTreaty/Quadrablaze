using UnityEngine;

public class DebugMessage : MonoBehaviour {

    [SerializeField]
    string _message = "";

    [SerializeField]
    DebugType _type = DebugType.Default;

    public void Execute() {
        Debug.Log("Execute debug message");
        switch(_type) {
            default:
                Debug.Log(_message);
                break;

            case DebugType.Warning:
                Debug.LogWarning(_message);
                break;

            case DebugType.Error:
                Debug.LogError(_message);
                break;
        }
    }

    public enum DebugType {
        Default,
        Warning,
        Error
    }
}