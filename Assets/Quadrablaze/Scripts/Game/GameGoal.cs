using UnityEngine;
using UnityEngine.Events;

namespace Quadrablaze {
    public class GameGoal : MonoBehaviour {

        public static GameGoal Current { get; private set; }

        [SerializeField]
        int _bossesDefeated;

        [SerializeField]
        int _bossesToDefeat;

        public UnityEvent OnReachedGoal;

        #region Properties
        public int BossesDefeated {
            get { return _bossesDefeated; }
            set {
                if(BossesDefeated == value) return;

                _bossesDefeated = value;

                if(ReachedGoal()) {
                    GameDebug.Log("GameGoal - Reached Goal", "Goal");
                    OnReachedGoal.InvokeEvent();
                }
            }
        }

        public int BossesToDefeat {
            get { return _bossesToDefeat; }
            set { _bossesToDefeat = value; }
        }
        #endregion

        void OnEnable() {
            Current = this;
        }

        void Start() {
            BossSpawner.Current.OnBossDefeated.AddListener(DefeatedBoss);
        }

        public void DefeatedBoss() {
            BossesDefeated++;
        }

        public bool ReachedGoal() {
            return BossesToDefeat > 0 && BossesDefeated >= BossesToDefeat;
        }

        public void Reset() {
            BossesDefeated = 0;
        }
    }
}