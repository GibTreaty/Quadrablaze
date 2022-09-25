using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze {
    public class EnemySpawnShipManager : MonoBehaviour {

        public static EnemySpawnShipManager Current { get; private set; }

        [SerializeField]
        GameObject _originalSpawnShip;

        [SerializeField]
        int _spawnShipCount = 16;

        public Transform[] SpawnPoints { get; private set; }

        void OnEnable() {
            Current = this;
        }

        void Start() {
            float angle = 360f / _spawnShipCount;
            float currentAngle = 0;

            List<Transform> spawnShips = new List<Transform>();

            for(int i = 0; i < _spawnShipCount; i++) {
                var gameObject = Instantiate(_originalSpawnShip);
                Quaternion rotation = Quaternion.AngleAxis(currentAngle + Random.Range(-2.5f, 2.5f), Vector3.up);
                Vector3 direction = rotation * Vector3.forward;

                gameObject.SetActive(true);
                gameObject.transform.position = direction * (GameManager.Current.ArenaRadius + Random.Range(2.8f, 6));
                gameObject.transform.rotation = rotation * Quaternion.Euler(0, Random.Range(0, 2) == 0 ? -90 : 90, 0);
                gameObject.transform.SetParent(transform, true);

                spawnShips.Add(gameObject.transform);

                currentAngle += angle;
            }

            SpawnPoints = spawnShips.ToArray();
        }
    }
}