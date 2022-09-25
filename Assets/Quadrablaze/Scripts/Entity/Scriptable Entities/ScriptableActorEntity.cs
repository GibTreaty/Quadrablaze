using System.Collections.Generic;
using Quadrablaze.Skills;
using StatSystem;
using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    public abstract class ScriptableActorEntity : ScriptableEntity {

        [SerializeField]
        protected ScriptableUpgradeSet _originalUpgradeSet;

        [SerializeField]
        protected float _size;

        [SerializeField]
        protected float _movementInterruptLength;

        [SerializeField]
        protected List<HealthInfo> _healthSlots;

        public List<HealthInfo> HealthSlots => _healthSlots;
        public float MovementInterruptLength => _movementInterruptLength;
        public float Size => _size;

        public CappedStat[] GetHealthStatsArray() {
            if(_healthSlots.Count == 0) return null;

            var stats = new CappedStat[_healthSlots.Count];

            for(int i = 0; i < stats.Length; i++)
                stats[i] = new CappedStat(i, _healthSlots[i].baseHealth, _healthSlots[i].baseHealth);

            return stats;
        }

        [System.Serializable]
        public struct HealthInfo {
            public int baseHealth;
            public float levelModifier;
        }
    }
}