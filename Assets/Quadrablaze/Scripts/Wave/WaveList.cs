using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Waves/Wave List")]
    public class WaveList : ScriptableObject {
        [SerializeField]
        WaveOnLevel[] _waves;

        public WaveOnLevel this[int index] {
            get { return _waves[index]; }
        }

        #region Properties
        public int Count {
            get { return _waves.Length; }
        }
        #endregion

        public WaveOnLevel GetWaveOnLevel(int level) {
            WaveOnLevel wave = new WaveOnLevel(level, "");

            for(int i = 0; i < _waves.Length; i++)
                if(level >= _waves[i].Level)
                    wave = _waves[i];

            return wave;
        }
    }

    [System.Serializable]
    public struct WaveOnLevel {
        [SerializeField]
        string _waveName;

        [SerializeField]
        int _level;

        #region Properties
        public int Level {
            get { return _level; }
            set { _level = value; }
        }

        public string WaveName {
            get { return _waveName; }
            set { _waveName = value; }
        }
        #endregion

        public WaveOnLevel(int level, string waveName) {
            _level = level;
            _waveName = waveName;
        }
    }
}