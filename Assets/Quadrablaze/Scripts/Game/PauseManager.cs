using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class PauseManager : MonoBehaviour {

        public static PauseManager Current { get; private set; }

        bool _isPaused;

        [SerializeField]
        float _fixedDeltaTime;

        public BooleanEvent onPauseChange;

        public bool IsPaused {
            get { return _isPaused; }
            set {
                if(value == IsPaused) return;

                _isPaused = value;

                if(GameManager.Current.Options.GameNetworkConnectionType == GameNetworkType.SinglePlayer) {
                    StopCoroutine("WindUpUnpause");

                    if(IsPaused) {
                        Time.timeScale = 0;
                        Time.fixedDeltaTime = _fixedDeltaTime;
                    }
                    else
                        StartCoroutine("WindUpUnpause", 1);
                }

                if(onPauseChange != null) onPauseChange.Invoke(IsPaused);
            }
        }

        public void Initialize() {
            Current = this;
            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        public void Subscribe(UnityAction<bool> action, bool updateOnSubscribe = true) {
            onPauseChange.AddListener(action);

            if(updateOnSubscribe) action(IsPaused);
        }

        IEnumerator WindUpUnpause(float speed) {
            float time = Time.timeScale;

            while(time < 1) {
                time = Mathf.Min(time + Time.unscaledDeltaTime * speed, 1);

                Time.timeScale = time;
                Time.fixedDeltaTime = Mathf.Lerp(_fixedDeltaTime * .1f, _fixedDeltaTime, time);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}