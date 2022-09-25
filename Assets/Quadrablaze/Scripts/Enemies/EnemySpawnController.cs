using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;
using YounGenTech.Entities;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class EnemySpawnController {

        //public EnemyInfoDatabase EnemyDatabase { get; set; }
        //public PoolManager EnemyPool { get; private set; }
        public bool IsSpawningEnemies { get; private set; }
        public int MaxEnemies { get; set; }
        public int QueueCount => queuedEnemyIDs.Count;
        public int MinionCount { get; set; }
        public EventTimer SpawnTimer { get; private set; }
        public Transform[] SpawnPoints => EnemySpawnShipManager.Current.SpawnPoints;

        int _spawnPointIterator = 0;
        Queue<int> queuedEnemyIDs = new Queue<int>();
        List<int> randomEnemyLocations = new List<int>();
        bool delayedEnd;

        //int[] entityIds = null;
        int[] spawnPointIndices = null;

        bool isHost;
        System.Action startWaveMethod = null;

        public EnemySpawnController(PoolManager enemyPool, EnemyInfoDatabase enemyDatabase, int maxEnemies, EventTimer spawnTimer, WaveController waveController) {
            GameManager.Current.MinionPool = enemyPool;
            GameManager.Current.MinionDatabase = enemyDatabase;
            MaxEnemies = maxEnemies;
            SpawnTimer = spawnTimer;

            SpawnTimer.OnElapsed = new UnityEngine.Events.UnityEvent();
            SpawnTimer.OnElapsed.AddListener(SpawnEnemyFromQueue);

            isHost = NetworkServer.active;

            spawnPointIndices = Enumerable.Range(0, SpawnPoints.Length - 1).ToArray();

            startWaveMethod = () => OnStartWave(waveController);
            WaveController.OnStartWave += startWaveMethod;

            Start(waveController);

            QuadrablazeSteamNetworking.RegisterGameNetworkListener(CreateMinionEntity, this);
            EnemyProxy.Proxy.Subscribe(EntityActions.Despawned, EnemyProxy_OnDespawned);
        }

        public void ClearQueue() {
            queuedEnemyIDs.Clear();
            randomEnemyLocations.Clear();
        }

        [GameNetworkListener]
        void CreateMinionEntity(NetworkMessage message) {
            var entityIndex = message.reader.ReadInt32();
            var entityId = message.reader.ReadUInt32();
            var gameObject = message.reader.ReadGameObject();
            var entityDatabaseEntry = GameManager.Current.MinionDatabase.Entities[entityIndex];
            var entity = entityDatabaseEntry.CreateInstance() as MinionEntity;

            if(!entityDatabaseEntry.Spawnable)
                MinionCount++;

            entity.Id = entityId;

            if(entity.HealthSlots != null)
                if(entity.HealthSlots.Length > 0)
                    HealthManager.UpdateHealth(entity);

            entity.SetGameObject(gameObject);
            entity.InitializeSkillLayout();
        }

        public void Despawn() {
            GameManager.Current.MinionPool.DespawnAll();
            SpawnTimer.Reset(true);
            MinionCount = 0;
            ClearQueue();
        }

        void EnemyDestroyed(Entities.EnemyEntity entity) {
            if(entity is MinionEntity minionEntity)
                if(!(minionEntity.OriginalScriptableObject as ScriptableMinionEntity).Spawnable)
                    MinionCount--;
        }

        void EnemyProxy_OnDespawned( System.EventArgs args) {
            EnemyDestroyed(((EntityArgs)args).GetEntity<Entities.EnemyEntity>());
        }

        public void Finish() {
            WaveController.OnStartWave -= startWaveMethod;
            EnemyProxy.Proxy.Unsubscribe(EntityActions.Despawned, EnemyProxy_OnDespawned);
        }

        public Vector3 GetNextSpawnPoint() {
            if(SpawnPoints.Length == 0) return Vector3.zero;

            int index;

            if(randomEnemyLocations.Count > 0) {
                var randomIndex = Random.Range(0, randomEnemyLocations.Count);

                index = randomEnemyLocations[randomIndex];
                randomEnemyLocations.RemoveAt(randomIndex);
            }
            else {
                index = _spawnPointIterator;
                _spawnPointIterator++;
                _spawnPointIterator %= SpawnPoints.Length;
            }

            return GetSpawnpoint(index);
        }

        int GetRandomIndex() {
            return Random.Range(0, SpawnPoints.Length);
        }

        public Vector3 GetRandomSpawnPoint() {
            if(SpawnPoints.Length == 0) return Vector3.zero;

            int index = Random.Range(0, SpawnPoints.Length);

            return GetSpawnpoint(index);
        }

        public Vector3 GetSpawnpoint(int index) {
            return SpawnPoints[index].position;
        }

        void OnStartWave(WaveController waveController) {
            //if(entityIds == null) {
            //    entityIds = new int[waveController.EntitySpawnList.Count];

            //    for(int i = 0; i < entityIds.Length; i++)
            //        entityIds[i] = i;
            //}

            var entityIds = new int[waveController.EntitySpawnList.Count];

            for(int i = 0; i < entityIds.Length; i++) {
                var entity = waveController.EntitySpawnList[i];

                entityIds[i] = GameManager.Current.MinionDatabase.Entities.IndexOf(entity);
            }

            QueueSpawn(entityIds);
        }

        public void QueueSpawn(params int[] poolPrefabIDs) {
            string ids = "";
            for(int i = 0; i < poolPrefabIDs.Length; i++) {
                ids += poolPrefabIDs[i];

                if(i < poolPrefabIDs.Length - 1)
                    ids += "\n";
            }

            var randomIndices = new List<int>();

            for(int i = 0; i < poolPrefabIDs.Length; i++) {
                if(randomIndices.Count == 0)
                    randomIndices.AddRange(spawnPointIndices);

                queuedEnemyIDs.Enqueue(poolPrefabIDs[i]);

                var randomIndex = Random.Range(0, randomIndices.Count);

                try {
                    randomEnemyLocations.Add(randomIndices[randomIndex]);
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
            SpawnTimer.Reset();
        }

        public GameObject SpawnEnemy(int poolPrefabID) {
            var position = GetNextSpawnPoint();
            var rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            return GameManager.Current.CreateEntity(CreateEntityType.Minion, poolPrefabID, position, rotation);
            //var gameObject = GameManager.Current.MinionPool.Spawn(GameManager.Current.MinionPool.IndexFromPrefabID(poolPrefabID), position, rotation).gameObject;

            //SetEnemyPosition(gameObject);

            //GameCoroutine.BeginCoroutine(SendCreateMinionEntityMessage(poolPrefabID, gameObject));

            //return gameObject;
        }
        public GameObject SpawnEnemy(int poolPrefabID, Vector3 position) {
            var gameObject = GameManager.Current.MinionPool.Spawn(GameManager.Current.MinionPool.IndexFromPrefabID(poolPrefabID), position, Quaternion.Euler(0, Random.Range(0f, 360f), 0)).gameObject;

            GameCoroutine.BeginCoroutine(SendCreateMinionEntityMessage(poolPrefabID, gameObject));

            return gameObject;
        }

        IEnumerator SendCreateMinionEntityMessage(int index, GameObject gameObject) {
            //while(gameObject != null && networkIdentity.netId.Value == 0)
            yield return null;
            var entityId = GameManager.Current.GetUniqueEntityId();

            QuadrablazeSteamNetworking.SendGameNetworkMessage(CreateMinionEntity, writer => {
                writer.Write(index); //TODO:Write the index of the ScriptableMinionEntity from the database
                writer.Write(entityId);
                writer.Write(gameObject);
            });
        }

        void SpawnEnemyFromQueue() {
            if(MinionCount < MaxEnemies)
                SpawnEnemy(queuedEnemyIDs.Dequeue());
        }

        public void Start(WaveController waveController) {
            ResetTimer();
            IsSpawningEnemies = true;

            waveController.BetweenWaveTimer.Active = true;
        }

        public void Update(WaveController waveController) {
            if(!isHost) return;

            SpawnTimer.Active = IsSpawningEnemies && QueueCount > 0;
            SpawnTimer.Update();

            if(MinionCount == 0 && QueueCount == 0 && !waveController.BetweenWaveTimer.Active) {
                waveController.CalculateWalletValues();
                waveController.StartWave();
            }
        }
    }

    public interface IEnemySpawnController {
        EnemySpawnController CurrentEnemyController { get; }
    }
}