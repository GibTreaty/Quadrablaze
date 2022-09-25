using UnityEngine;

namespace StatSystem {
    public class Stat {

        public int Id { get; set; }
        public bool Invincible { get; set; }
        public int Value { get; set; }

        public Stat(int id, int value) {
            Id = id;
            Value = value;
        }

        public virtual StatEvent ModifyValue(int value, object affectedObject, object sourceObject, bool repeatedEffect) {
            if(value < Value && Invincible) return null;

            int previousValue = Value;

            Value = value;

            int valueDifference = Value - previousValue;

            return new StatEvent(this, repeatedEffect, affectedObject, sourceObject, new StatChangeValueMessage(valueDifference));
        }
    }

    public class StatEvent {
        /// <summary>Stat that was modified</summary>
        public Stat AffectedStat { get; }
        public object AffectedObject { get; }
        public bool ContinuousEffect { get; }

        /// <summary>Object that caused the stat to be modified</summary>
        public object SourceObject { get; }

        StatMessageBase message;

        public StatMessageBase GetMessage() {
            return message;
        }
        public T GetMessage<T>() where T : StatMessageBase {
            return message as T;
        }

        public StatEvent(Stat affectedStat, bool continuousEffect, object affectedObject, object sourceObject, StatMessageBase message) {
            AffectedStat = affectedStat;
            AffectedObject = affectedObject;
            ContinuousEffect = continuousEffect;
            SourceObject = sourceObject;
            this.message = message;
        }
    }

    public class StatEventArgs : System.EventArgs {
        public StatEvent Stat { get; }

        public StatEventArgs(StatEvent stat) {
            Stat = stat;
        }
    }

    public class StatMessageBase { }

    public class StatChangeValueMessage : StatMessageBase {
        public int AmountChanged { get; }

        public StatChangeValueMessage(int amountChanged) {
            AmountChanged = amountChanged;
        }
    }
}