using UnityEngine;

public class ResizableForceField : MonoBehaviour {

    [SerializeField]
    Transform _forceField;

    [SerializeField]
    Transform _forceFieldDisplay;

    [SerializeField]
    Transform _hook1;

    [SerializeField]
    Transform _hook2;

    #region Properties
    public Transform ForceField {
        get { return _forceField; }
        set { _forceField = value; }
    }

    public Transform Hook1 {
        get { return _hook1; }
        set { _hook1 = value; }
    }

    public Transform Hook2 {
        get { return _hook2; }
        set { _hook2 = value; }
    }

    public Vector3 Hook1Position {
        get { return Hook1.position; }
        set { Hook1.position = value; }
    }

    public Vector3 Hook2Position {
        get { return Hook2.position; }
        set { Hook2.position = value; }
    }

    public Transform ForceFieldDisplay {
        get { return _forceFieldDisplay; }
        set { _forceFieldDisplay = value; }
    }
    #endregion

    void LateUpdate() {
        UpdateSize();
    }

    public void UpdateSize() {
        if(!Hook1 || !Hook2 || !ForceField) return;

        Vector3 direction = Hook2Position - Hook1Position;

        if(Mathf.Approximately(direction.sqrMagnitude, 0)) return;

        Vector3 eulerAngles = Quaternion.LookRotation(direction).eulerAngles;
        //eulerAngles.x += 90;
        eulerAngles.y += 90;
        Hook2.eulerAngles = Hook1.eulerAngles = eulerAngles;

        Vector3 scale = ForceField.localScale;
        scale.x = Vector3.Distance(Hook1Position, Hook2Position);
        ForceField.localScale = scale;

        if(ForceFieldDisplay) {
            ForceFieldDisplay.eulerAngles = eulerAngles;
            ForceFieldDisplay.Rotate(90, 0, 0, Space.Self);
            ForceFieldDisplay.position = (Hook1Position + Hook2Position) / 2;
            ForceFieldDisplay.localScale = new Vector3(scale.x, ForceFieldDisplay.localScale.y, ForceFieldDisplay.localScale.z);
        }
    }

    [ContextMenu("Enable Force Field")]
    void Enable() {
        EnableForceField(true);
    }

    [ContextMenu("Disable Force Field")]
    void Disable() {
        EnableForceField(false);
    }

    public void EnableForceField(bool enable) {
        if(ForceField) {
            if(enable) {
                UpdateSize();

                ForceField.transform.rotation = Hook1.rotation;
                ForceField.transform.position = (Hook1Position + Hook2.position) / 2;
            }

            ForceField.gameObject.SetActive(enable);
        }

        if(ForceFieldDisplay)
            ForceFieldDisplay.gameObject.SetActive(enable);
    }
}