using UnityEngine;

namespace StatSystem {
    public class CappedStat : Stat {

        public int MaxValue { get; set; }
        public float NormalizedValue {
            get { return (float)Value / (float)MaxValue; }
        }

        public CappedStat(int id, int value, int maxValue) : base(id, value) {
            MaxValue = maxValue;
        }

        public void ClampValue() {
            Value = Mathf.Clamp(Value, 0, MaxValue);
        }

        public virtual StatEvent ModifyMaxValue(int value, object affectedObject, object sourceObject, bool continuousEffect) {
            int previousValue = MaxValue;

            MaxValue = value;

            int valueDifference = Value - previousValue;

            return new StatEvent(this, continuousEffect, affectedObject, sourceObject, new CappedChangeMaxValue(valueDifference));
        }

        public override StatEvent ModifyValue(int value, object affectedObject, object sourceObject, bool continuousEffect) {
            if(value < Value && Invincible) return null;

            int previousValue = Value;

            Value = value;
            ClampValue();

            int valueDifference = Value - previousValue;

            return new StatEvent(this, continuousEffect, affectedObject, sourceObject, new StatChangeValueMessage(valueDifference));
        }
    }

    public class CappedChangeMaxValue : StatMessageBase {
        public int AmountChanged { get; }

        public CappedChangeMaxValue(int amountChanged) {
            AmountChanged = amountChanged;
        }
    }
}