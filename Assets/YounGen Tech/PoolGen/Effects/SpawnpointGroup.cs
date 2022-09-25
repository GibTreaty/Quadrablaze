using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.PoolGen.Effects {
    [AddComponentMenu("YounGen Tech/PoolGen/Effects/Spawnpoint Group")]
    /// <summary>A group of spawnpoints meant for use with <see cref="PoolManager"/>'s <see cref="PoolManager.OnSpawn"/> event</summary>
    public class SpawnpointGroup : MonoBehaviour {

        [SerializeField]
        List<Transform> _spawnpoints = new List<Transform>();

        [SerializeField]
        bool _randomRotation = true;

        #region Properties
        /// <summary>Should a random rotation be applied when the moved to a spawnpoint</summary>
        public bool RandomRotation {
            get { return _randomRotation; }
            set { _randomRotation = value; }
        }

        /// <summary>List of spawnpoint transforms</summary>
        public List<Transform> Spawnpoints {
            get { return _spawnpoints; }
            set { _spawnpoints = value; }
        }
        #endregion

        /// <summary>Move <see cref="PoolUser"/> to a random spawnpoint</summary>
        public void MoveToSpawnpoint(PoolUser user) {
            MoveToSpawnpoint(user, Random.Range(0, Spawnpoints.Count));
        }
        /// <summary>Move <see cref="PoolUser"/> to a specific spawnpoint</summary>
        public void MoveToSpawnpoint(PoolUser user, int spawnpointIndex) {
            var spawnpoint = Spawnpoints[spawnpointIndex];

            user.transform.position = spawnpoint.position;
            user.transform.rotation = RandomRotation ? spawnpoint.rotation * Quaternion.Euler(0, Random.Range(0f, 360f), 0) : spawnpoint.rotation;
        }
    }
}