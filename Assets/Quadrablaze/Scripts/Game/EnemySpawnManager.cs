// TODO Necroformer, reformer and possibly other enemies don't show up sometimes

using UnityEngine;
using System.Collections;
using YounGenTech.PoolGen;
using System.Collections.Generic;

namespace Quadrablaze {
    public class EnemySpawnManager : MonoBehaviour {

        public static EnemySpawnManager Current { get; private set; }

        public PoolManager enemyPool;

        [SerializeField]
        int _maxEnemies = 20;

        [SerializeField]
        bool _isSpawningEnemies;

        [SerializeField]
        EventTimer _spawnTimer = new EventTimer(1);

        int _spawnPointIterator = 0;
        Queue<int> queuedEnemyIDs = new Queue<int>();
        List<int> randomEnemyLocations = new List<int>();

        List<int> spawnPointIndices = new List<int>();
        Transform[] _spawnPoints;

        #region Properties
        public bool IsSpawningEnemies {
            get { return _isSpawningEnemies; }
            set {
                if(_isSpawningEnemies == value) return;

                _isSpawningEnemies = value;

                if(!IsSpawningEnemies) _spawnTimer.Reset(false);
            }
        }

        public int MaxEnemies {
            get { return _maxEnemies; }
            set { _maxEnemies = value; }
        }

        public int QueueCount {
            get { return queuedEnemyIDs.Count; }
        }

        public Transform[] SpawnPoints {
            get { return _spawnPoints; }
            set {
                _spawnPoints = value;
                
                for(int i = 0; i < _spawnPoints.Length; i++)
                    spawnPointIndices.Add(i);
            }
        }

        public EventTimer SpawnTimer {
            get { return _spawnTimer; }
        }
        #endregion

        void OnEnable() {
            Current = this;
        }

        void Awake() {
            _spawnTimer.OnElapsed.AddListener(SpawnEnemyFromQueue);
        }

        void Update() {
            if(!UnityEngine.Networking.NetworkServer.active) return;

            _spawnTimer.Active = IsSpawningEnemies && queuedEnemyIDs.Count > 0;
            _spawnTimer.Update();

            //if(IsSpawningEnemies) {
            //    if(EnemySpawnTimer > 0)
            //        EnemySpawnTimer = Mathf.Max(EnemySpawnTimer - Time.deltaTime, 0);

            //    if(EnemySpawnTimer == 0) SpawnEnemy();
            //}
        }

        public void ClearQueue() {
            queuedEnemyIDs.Clear();
            randomEnemyLocations.Clear();
        }

        public void Despawn() {
            enemyPool.DespawnAll();
            _spawnTimer.Reset(true);
            ClearQueue();
        }

        public Vector3 GetNextSpawnPoint() {
            if(_spawnPoints.Length == 0) return Vector3.zero;

            int index;

            if(randomEnemyLocations.Count > 0) {
                var randomIndex = Random.Range(0, randomEnemyLocations.Count);

                index = randomEnemyLocations[randomIndex];
                randomEnemyLocations.RemoveAt(randomIndex);
            }
            else {
                index = _spawnPointIterator;
                _spawnPointIterator++;
                _spawnPointIterator %= _spawnPoints.Length;
            }

            return GetSpawnpoint(index);
        }

        int GetRandomIndex() {
            return Random.Range(0, _spawnPoints.Length);
        }

        public Vector3 GetRandomSpawnPoint() {
            if(_spawnPoints.Length == 0) return Vector3.zero;

            int index = Random.Range(0, _spawnPoints.Length);

            return GetSpawnpoint(index);
        }

        public Vector3 GetSpawnpoint(int index) {
            return _spawnPoints[index].position;
        }

        public void MoveToRandomPosition(EnemyInput enemyInput) {
            Vector3 randomPosition = Random.insideUnitSphere * (GameManager.Current.ArenaRadius - 1);
            randomPosition.y = 0;

            enemyInput.MoveToPosition(randomPosition);
        }

        public void QueueSpawn(params int[] poolPrefabIDs) {
            var randomIndices = new List<int>();

            //for(int i = 0; i < spawnPoints.Length; i++)
            //    randomIndices.Add(i);

            for(int i = 0; i < poolPrefabIDs.Length; i++) {
                if(randomIndices.Count == 0)
                    randomIndices.AddRange(spawnPointIndices);

                queuedEnemyIDs.Enqueue(poolPrefabIDs[i]);

                var randomIndex = Random.Range(0, randomIndices.Count);
                try {
                    randomEnemyLocations.Add(randomIndices[randomIndex]); // TODO Fix index out of range
                }
                catch(System.Exception e) {
                    Debug.LogError($"RandomIndices({randomIndices.Count}) RanomIndex({randomIndex})\n" + e.Message);
                }

                randomIndices.RemoveAt(randomIndex);
            }
        }

        public void Reset() {
            ClearQueue();
            IsSpawningEnemies = false;
        }

        public void ResetTimer() {
            _spawnTimer.Reset();
        }

        public void SetEnemyPosition(GameObject gameObject) {
            //gameObject.transform.position = GetNextSpawnPoint();
            Debug.DrawRay(gameObject.transform.position, Vector3.up * 2, Color.magenta, 10, false);
            switch(gameObject.name) {
                default:
                    //gameObject.GetComponent<EnemyInput>().MoveToPosition(PlayerSpawnManager.Current.CurrentPlayer.transform.position);
                    break;
                case "Mine Layer":
                    MoveToRandomPosition(gameObject.GetComponent<EnemyInput>());
                    break;
            }
        }

        //public void SpawnEnemy() {
        //    if(enemyPool.SpawnedObjectsCount < MaxEnemies) {
        //        //if(PlayerSpawnManager.IsPlayerAlive) {
        //        var position = GetNextSpawnPoint();
        //        var gameObject = enemyPool.Spawn(position, Quaternion.Euler(0, Random.Range(0f, 360f), 0)).gameObject;
        //        //GameObject gameObject = enemyPool.Spawn(new Vector3(0, 1000, 0), Quaternion.Euler(0, Random.Range(0f, 360f), 0)).gameObject;

        //        SetEnemyPosition(gameObject);
        //    }
        //}
        public void SpawnEnemy(int poolPrefabID) {
            if(enemyPool.SpawnedObjectsCount < MaxEnemies) {
                var position = GetNextSpawnPoint();
                var gameObject = enemyPool.Spawn(enemyPool.IndexFromPrefabID(poolPrefabID), position, Quaternion.Euler(0, Random.Range(0f, 360f), 0)).gameObject;

                SetEnemyPosition(gameObject);
            }
        }

        void SpawnEnemyFromQueue() {
            SpawnEnemy(queuedEnemyIDs.Dequeue());
        }
    }
}