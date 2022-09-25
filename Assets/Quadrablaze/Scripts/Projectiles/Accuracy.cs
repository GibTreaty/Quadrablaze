using UnityEngine;
using UnityEngine.Events;

namespace YounGenTech.YounGenShooter {
    public class Accuracy : MonoBehaviour {

        [SerializeField]
        Vector3 _minAccuracy;

        [SerializeField]
        Vector3 _maxAccuracy;

        [SerializeField]
        float _recoil = .1f;

        [SerializeField]
        float _accuracyRegenRate = 1;

        [SerializeField]
        float _accuracyRegenTime;

        [SerializeField]
        float _regenDelay;

        [SerializeField]
        float _regenDelayTime;

        public Vector3Event OnAccuracyChanged;

        public UnityEvent OnAccurate;
        public UnityEvent OnCompletelyInaccurate;

        //bool _shotLastFrame;

        #region Properties
        public float AccuracyRegenRate {
            get { return _accuracyRegenRate; }
            set { _accuracyRegenRate = value; }
        }

        public float AccuracyRegenTime {
            get { return _accuracyRegenTime; }
            set {
                if(AccuracyRegenTime == value) return;

                _accuracyRegenTime = Mathf.Clamp01(value);

                if(OnAccuracyChanged != null) OnAccuracyChanged.Invoke(CurrentAccuracy);

                if(AccuracyRegenTime == 0) {
                    if(OnAccurate != null) OnAccurate.Invoke();
                }
                else if(AccuracyRegenTime == 1) {
                    if(OnCompletelyInaccurate != null) OnCompletelyInaccurate.Invoke();
                }
            }
        }

        public Vector3 CurrentAccuracy {
            get { return Vector3.Lerp(MinAccuracy, MaxAccuracy, AccuracyRegenTime); }
        }

        public Vector3 MinAccuracy {
            get { return _minAccuracy; }
            set { _minAccuracy = value; }
        }

        public float Recoil {
            get { return _recoil; }
            set { _recoil = value; }
        }

        public Vector3 MaxAccuracy {
            get { return _maxAccuracy; }
            set { _maxAccuracy = value; }
        }

        public float RegenDelay {
            get { return _regenDelay; }
            set { _regenDelay = value; }
        }

        public float RegenDelayTime {
            get { return _regenDelayTime; }
            set { _regenDelayTime = value; }
        }
        #endregion

        void OnDisable() {
            AccuracyRegenTime = 0;
        }

        void Update() {
            if(RegenDelayTime == 0)
                if(AccuracyRegenTime > 0) AccuracyRegenTime -= Time.deltaTime * AccuracyRegenRate;

            if(RegenDelayTime > 0)
                RegenDelayTime = Mathf.Max(RegenDelayTime - Time.deltaTime, 0);
        }

        public void AddToInaccuracy() {
            AccuracyRegenTime += Recoil;

            RegenDelayTime = RegenDelay;
        }
    }
}