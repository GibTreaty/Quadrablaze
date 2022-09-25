using UnityEngine;
using UnityEngine.UI;

public class SkillPointsUIGlow : MonoBehaviour {

    [SerializeField]
    Image backgroundGlow;

    [SerializeField]
    Outline iconGlow;

    [SerializeField]
    float _oscillationSpeed = .5f;

    [SerializeField]
    float _oscillationTime;

    #region Properties
    public float OutputAmount {
        get { return Mathf.PingPong(OscillationTime, 1); }
    }

    public float OscillationSpeed {
        get { return _oscillationSpeed; }
        set { _oscillationSpeed = value; }
    }

    public float OscillationTime {
        get { return _oscillationTime; }
        set { _oscillationTime = value; }
    }
    #endregion

    public void EnableGlow(bool enable) {
        backgroundGlow.gameObject.SetActive(enable);
        iconGlow.enabled = enable;
    }

    void Update() {
        if(!backgroundGlow.gameObject.activeInHierarchy && OscillationTime > 0)
            OscillationTime = 0;

        if(backgroundGlow.gameObject.activeInHierarchy) {
            OscillationTime = (OscillationTime + Time.deltaTime * OscillationSpeed) % 2;

            Color backgroundColor = backgroundGlow.color;
            Color iconColor = iconGlow.effectColor;

            backgroundColor.a = OutputAmount;
            iconColor.a = OutputAmount;

            backgroundGlow.color = backgroundColor;
            iconGlow.effectColor = iconColor;
        }
    }
}