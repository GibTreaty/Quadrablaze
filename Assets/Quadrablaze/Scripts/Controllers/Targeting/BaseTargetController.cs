using UnityEngine;

namespace Quadrablaze {
    public abstract class BaseTargetController : MonoBehaviour {

        [SerializeField]
        protected LayerMask _targetMask = -1;

        [SerializeField]
        protected Transform _target;

        public TransformEvent onTargetChanged;

        #region Properties
        public virtual Transform Target {
            get { return _target; }
            set {
                if(Target == value) return;

                _target = value;

                if(onTargetChanged != null) onTargetChanged.Invoke(Target);
            }
        }

        public LayerMask TargetMask {
            get { return _targetMask; }
            private set { _targetMask = value; }
        }
        #endregion

        protected virtual void OnDisable() {
            Reset();
        }

        public virtual void Reset() {
            Target = null;
        }
    }
}