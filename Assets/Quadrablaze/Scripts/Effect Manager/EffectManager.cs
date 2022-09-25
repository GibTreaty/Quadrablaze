using UnityEngine;

namespace Quadrablaze {
    public class EffectManager : MonoBehaviour {

        public static EffectManager Current { get; private set; }

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_sounds")]
        EffectDatabase _effects;

        public EffectDatabase Effects {
            get { return _effects; }
            set { _effects = value; }
        }

        void OnEnable() {
            Current = this;
        }
    }
}