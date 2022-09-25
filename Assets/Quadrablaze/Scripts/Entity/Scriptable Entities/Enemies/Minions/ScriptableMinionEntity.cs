using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    [CreateAssetMenu(menuName = "Quadrablaze/Entities/Minion")]
    public class ScriptableMinionEntity : ScriptableActorEntity {

        /// <summary>Can this entity be spawned outside of the EnemySpawnController by another entity</summary>
        [SerializeField, Tooltip("Can this entity be spawned outside of the EnemySpawnController by another entity")]
        bool _spawnable;

        [SerializeField]
        uint _xp = 1;

        [SerializeField]
        int _points;

        [SerializeField]
        int _tier;

        [SerializeField]
        string _group;

        [SerializeField]
        string[] _requireAbilities;

        [Header("Object Pool Settings")]
        [SerializeField]
        int _spawnCountTarget;

        [SerializeField]
        int _maximumSpawnCount;

        [SerializeField]
        int _poolSize;

        public string Group => _group;
        public int MaximumSpawnCount => _maximumSpawnCount;
        public int Points => _points;
        public int PoolSize => _poolSize;
        public string[] RequireAbilities => _requireAbilities;
        public bool Spawnable => _spawnable;
        public int SpawnCountTarget => _spawnCountTarget;
        public int Tier => _tier;

        public override Entity CreateInstance() {
            var entity = new MinionEntity(null, name, 0, _originalUpgradeSet?.CreateInstance(), _size, _xp) {
                OriginalScriptableObject = this,
                MovementInterruptTimer = new Timer(_movementInterruptLength, Timer.TimerDirection.Down),
                HealthSlots = GetHealthStatsArray()
            };

            if(_originalUpgradeSet != null)
                entity.CurrentUpgradeSet.CurrentSkillLayout.CurrentEntity = entity;

            return entity;
        }
    }
}