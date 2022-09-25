using System;
using UnityEngine;

namespace Quadrablaze.Skills {
    [Serializable]
    public struct SetSkillAmount {
        [SerializeField]
        float _amount;

        public FloatEvent onUpgrade;

        public float Amount {
            get { return _amount; }
            set { _amount = value; }
        }

        public SetSkillAmount(float amount) {
            _amount = amount;
            onUpgrade = new FloatEvent();
        }

        public void InvokeUpgrade() {
            if(onUpgrade != null) onUpgrade.Invoke(Amount);
        }
    }
}