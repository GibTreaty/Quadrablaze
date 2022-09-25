using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;

namespace Quadrablaze {
    public class TargetController : BaseTargetController {

        [SerializeField]
        float _searchRadius = 5;

        [SerializeField]
        float _searchFrequency = 1;

        [SerializeField]
        float _targetLostTime = 2;

        float _searchTimer;
        float _targetLostTimer;

        bool start;

        #region Properties
        public float SearchFrequency {
            get { return _searchFrequency; }
            private set { _searchFrequency = value; }
        }

        public float SearchRadius {
            get { return _searchRadius; }
            private set { _searchRadius = value; }
        }

        public float SearchTimer {
            get { return _searchTimer; }
            private set { _searchTimer = value; }
        }

        public override Transform Target {
            set {
                if(Target == value) return;

                _target = value;

                if(Target) {
                    var collider = _target.GetComponent<Collider>();

                    if(collider)
                        TargetBody = collider.attachedRigidbody ? collider.attachedRigidbody.transform : collider.transform;
                    else
                        TargetBody = _target;
                    //TargetBody = _target.GetComponent<Rigidbody>();
                }
                else
                    TargetBody = null;

                if(onTargetChanged != null) 
                    onTargetChanged.Invoke(Target);
            }
        }

        public virtual Transform TargetBody { get; protected set; }

        public float TargetLostTime {
            get { return _targetLostTime; }
            private set { _targetLostTime = value; }
        }

        public float TargetLostTimer {
            get { return _targetLostTimer; }
            private set { _targetLostTimer = value; }
        }
        #endregion

        IEnumerator Start() {
            yield return null;
            start = true;
        }

        void Update() {
            if(!start) return;

            if(NetworkServer.active)
                UpdateTarget();
        }

        public virtual void UpdateTarget() {
            if(!Target) {
                if(SearchTimer > 0)
                    SearchTimer = Mathf.Max(SearchTimer - Time.deltaTime, 0);

                if(SearchTimer == 0) {
                    Target = FindTarget();

                    if(!Target) DelaySearchTimer();
                }
            }

            if(Target)
                if(!Target.gameObject.activeInHierarchy) {
                    Target = null;
                }
                else if(Vector3.Distance(Target.position, transform.position) > SearchRadius) {
                    if(TargetLostTimer > 0)
                        TargetLostTimer = Mathf.Max(TargetLostTimer - Time.deltaTime, 0);

                    if(TargetLostTimer == 0)
                        Target = null;
                }
                else {
                    DelayTargetLostTimer();
                }
        }

        public void DelaySearchTimer() {
            SearchTimer = SearchFrequency;
        }

        public void DelayTargetLostTimer() {
            TargetLostTimer = TargetLostTime;
        }

        public virtual Transform FindTarget() {
            var colliders = GetColliders();

            if(colliders.Length == 0) return null;

            if(colliders.Length > 1)
                colliders = colliders.OrderBy(s => Mathf.Abs((s.transform.position - transform.position).sqrMagnitude)).ToArray();

            return colliders[0].transform;
        }

        protected Collider[] GetColliders() {
            return Physics.OverlapSphere(transform.position, SearchRadius, TargetMask);
        }

        protected virtual void OnDrawGizmos() {
            Gizmos.color = new Color(1, .5f, 0, .5f);
            Gizmos.DrawWireSphere(transform.position, SearchRadius);
        }

        protected virtual void OnDrawGizmosSelected() {
            if(Target) {
                Gizmos.color = new Color(1, .25f, 0, .5f);
                Gizmos.DrawLine(transform.position, Target.position);
            }
        }

        public override void Reset() {
            base.Reset();

            ResetTargetLostTimer();
            ResetSearchTimer();
        }

        public void ResetSearchTimer() {
            SearchTimer = 0;
        }

        public void ResetTargetLostTimer() {
            TargetLostTimer = 0;
        }
    }
}