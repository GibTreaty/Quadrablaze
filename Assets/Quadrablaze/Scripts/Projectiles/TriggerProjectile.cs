using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class TriggerProjectile : ProjectileBase {
        [SerializeField]
        string[] _hitsTags;

        [SerializeField]
        LayerMask _hitMask = -1;

        [SerializeField]
        int _maxPenetrations = 1;

        [SerializeField]
        float _hitForce = 0;

        [SerializeField]
        float _hitRigidbodyForce = 0;

        [SerializeField]
        string _impactPoolName = "";

        [SerializeField]
        string _impactPoolPrefabName = "";

        int penetrations;

        #region Properties
        public LayerMask HitMask {
            get { return _hitMask; }
            set { _hitMask = value; }
        }

        public string[] HitsTags {
            get { return _hitsTags; }
            set { _hitsTags = value; }
        }

        Rigidbody RigidbodyComponent { get; set; }
        #endregion

        void CreateImpactEffect(Vector3 position, Quaternion rotation) {
            if(!string.IsNullOrEmpty(_impactPoolName) && !string.IsNullOrEmpty(_impactPoolPrefabName)) {
                var pool = PoolManager.GetPool(_impactPoolName);

                if(pool)
                    pool.Spawn(pool.IndexFromPrefabName(_impactPoolPrefabName), position, rotation);
            }
        }

        void Hit(Collider collider) {
            if(collider.isTrigger) return;

            penetrations++;

            if(_hitForce > 0) {
                if(collider.attachedRigidbody) {
                    var movement = collider.attachedRigidbody.GetComponent<BaseMovementController>();

                    if(movement) {
                        if(_hitForce > 0)
                            movement.MoveToOvertime(1, movement.transform.position + transform.forward * _hitForce, 1, 100);
                    }
                    else {
                        if(_hitRigidbodyForce > 0)
                            collider.attachedRigidbody.AddForceAtPosition(transform.forward * _hitRigidbodyForce, transform.position);
                    }
                }
            }

            DoDamage(collider);
            FireHitEvent(collider);

            if(_maxPenetrations > -1)
                if(penetrations >= _maxPenetrations)
                    LastHit(collider);
        }

        public override void Initialize() {
            RigidbodyComponent = GetComponent<Rigidbody>();
        }

        void LastHit(Collider collider) {
            DoDestroy();
        }

        void OnDisable() {
            ResetProjectile();
        }

        void OnTriggerEnter(Collider collider) {
            bool hit = false;

            if(_hitMask != 0 && (_hitMask & (1 << collider.gameObject.layer)) > 0)
                hit = true;

            if(!hit)
                if(_hitsTags.Length > 0)
                    foreach(var tag in _hitsTags)
                        if(!string.IsNullOrEmpty(tag))
                            if(collider.CompareTag(tag)) {
                                hit = true;
                                break;
                            }

            if(!hit)
                return;

            Hit(collider);
        }

        void ResetProjectile() {
            penetrations = 0;
            RigidbodyComponent.Sleep();
        }

#if UNITY_EDITOR
        [ContextMenu("Reset Position")]
        void ResetPosition() {
            penetrations = 0;

            RigidbodyComponent.Sleep();

            transform.position = Vector3.zero;

            GetComponent<SingleForce>().Push();
        }
#endif
    }
}