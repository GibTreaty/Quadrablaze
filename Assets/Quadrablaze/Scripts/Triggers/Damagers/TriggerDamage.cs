using UnityEngine;

namespace Quadrablaze {
    public class TriggerDamage : MonoBehaviour {

        [SerializeField]
        LayerMask _triggerMask = -1;

        [SerializeField]
        float _damage = 1;

        [SerializeField]
        float _damageDelayTime;
        
        float _damageDelayTimer;
        bool didDamage;

        #region Properties
        public LayerMask TriggerMask {
            get { return _triggerMask; }
            set { _triggerMask = value; }
        }

        public float Damage {
            get { return _damage; }
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
        #endregion

        void Update() {
            if(didDamage) {
                DelayDamageDelayTimer();

                didDamage = false;
            }

            if(DamageDelayTimer > 0)
                DamageDelayTimer = Mathf.Max(DamageDelayTimer - Time.deltaTime, 0);
        }

        void OnEnable() {
            DelayDamageDelayTimer();
        }

        void OnTriggerEnter(Collider hit) {
            OnHit(hit);
        }

        void OnTriggerStay(Collider hit) {
            OnHit(hit);
        }

        void OnHit(Collider hit) {
            if(!didDamage)
                if((TriggerMask & (1 << hit.gameObject.layer)) > 0)
                    if(DamageDelayTimer == 0) {
                        didDamage = true;

                        hit.gameObject.DoHealthChange(-(int)Damage, this, false);
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