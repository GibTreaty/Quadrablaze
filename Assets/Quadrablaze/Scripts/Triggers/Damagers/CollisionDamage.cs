using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class CollisionDamage : MonoBehaviour, IDamageMultiplier {

        [SerializeField]
        LayerMask _collisionMask = -1;

        [SerializeField]
        float _damage = 1;

        [SerializeField]
        float _damageMultiplier = 1;

        [SerializeField]
        float _damageDelayTime;

        [SerializeField]
        bool _enableDamage = true;

        float _damageDelayTimer;
        bool didDamage;

        public UnityEvent OnCollision;

        #region Properties
        public LayerMask CollisionMask {
            get { return _collisionMask; }
            set { _collisionMask = value; }
        }

        public float Damage {
            get { return _damage * DamageMultiplier; }
            set { _damage = value; }
        }

        public float DamageDelayTime {
            get { return _damageDelayTime; }
            set { _damageDelayTime = value; }
        }

        public float DamageDelayTimer {
            get { return _damageDelayTimer; }
            set { _damageDelayTimer = value; }
        }

        public float DamageMultiplier {
            get { return _damageMultiplier; }
            set { _damageMultiplier = value; }
        }

        public bool EnableDamage {
            get { return _enableDamage; }
            set { _enableDamage = value; }
        }
        #endregion

        void Update() {
            if(!NetworkServer.active) return;

            if(didDamage) {
                DelayDamageDelayTimer();

                didDamage = false;
            }

            if(DamageDelayTimer > 0)
                DamageDelayTimer = Mathf.Max(DamageDelayTimer - Time.deltaTime, 0);
        }

        void OnCollisionEnter(Collision hit) {
            OnHit(hit);
        }

        void OnCollisionStay(Collision hit) {
            OnHit(hit);
        }

        void OnHit(Collision hit) {
            if(_enableDamage)
                if(NetworkServer.active)
                    if(!didDamage)
                        if((CollisionMask & (1 << hit.collider.gameObject.layer)) > 0)
                            if(DamageDelayTimer == 0) {
                                if(hit.collider.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot) {
                                    if(GameManager.Current.GetActorEntity(hit.collider.gameObject, out ActorEntity actorEntity)) {
                                        didDamage = true;

                                        actorEntity.DoHealthChange(-(int)Damage, hitSpot.HealthId, this, false);

                                        if(OnCollision != null) OnCollision.Invoke();
                                    }
                                }
                            }
        }

        public void ResetDamageDelayTimer() {
            DamageDelayTimer = 0;
        }

        public void DelayDamageDelayTimer() {
            DamageDelayTimer = DamageDelayTime;
        }
    }
}