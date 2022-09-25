using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Quadrablaze/Database/Boss Info Database")]
    public class BossInfoDatabase : ScriptableObject, IEnumerable<Entities.ScriptableBossEntity> {

        [SerializeField]
        List<Entities.ScriptableBossEntity> _entities = new List<Entities.ScriptableBossEntity>();

        [SerializeField]
        List<BossEntity> _bossEntities = new List<BossEntity>();

        #region Properties
        public List<Entities.ScriptableBossEntity> Entities => _entities;
        #endregion

        public Entities.ScriptableBossEntity this[int index] {
            get { return _entities[index]; }
        }

        public IEnumerator<Entities.ScriptableBossEntity> GetEnumerator() {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _entities.GetEnumerator();
        }
    }
}