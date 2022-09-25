using System;
using System.Collections.Generic;
using Quadrablaze.Boss;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class TimedBossSpawnController {

        public static int BossesSpawned { get; set; }

        public static event Action<float> OnTimerChanged;

        public static event Action OnBossSpawned;

        public static event Action OnBossDefeated;


        public BossInfoDatabase BossDatabase { get; private set; }

        public PoolManager BossPool { get; private set; }

        public GameObject CurrentSpawnedBoss { get; set; }

        public float CurrentSpawnTime { get; set; }

        public bool IsBossSpawned { get; private set; }

        public float SpawnTimeDelay { get; private set; }

        public bool TimerActive { get; set; }

        bool isHost;
        float lastNetworkSpawnTime;
        NetworkMessageDelegate networkSetTimerMethod = null;

        public TimedBossSpawnController(BossInfoDatabase bossDatabase, PoolManager bossPool, float spawnTimeDelay) {
            BossDatabase = bossDatabase;
            BossPool = bossPool;

            GameManager.Current.BossDatabase = bossDatabase;
            GameManager.Current.BossPool = bossPool;

            SpawnTimeDelay = spawnTimeDelay;

            GeneratePool();
            BossesSpawned = 0;
            isHost = NetworkServer.active;
            networkSetTimerMethod = NetworkSetTimer;
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_SetBossTimer, networkSetTimerMethod);
            QuadrablazeSteamNetworking.RegisterGameNetworkListener(CreateEntity, this);
            TimerActive = true;

            if(isHost)
                Start();
        }

        [GameNetworkListener]
        void CreateEntity(NetworkMessage message) {
            var index = message.reader.ReadInt32();
            var entityId = message.reader.ReadUInt32();
            var gameObject = message.reader.ReadGameObject();
            var entity = BossDatabase.Entities[index].CreateInstance() as Entities.BossEntity;

            entity.Id = entityId;
            entity.SetGameObject(gameObject);
            entity.InitializeSkillLayout();

            EnemyProxy.SpawnedBoss = CurrentSpawnedBoss.GetComponent<BossController>();
            entity.SetStage(1);
            CameraSoundController.Current.PlaySound("Boss Spawn", true);

            BossesSpawned++;

            if(OnBossSpawned != null) OnBossSpawned.Invoke();
            EnemyProxy.Proxy.RaiseEvent(EntityActions.Spawned, entity.ToArgs());
        }

        public void Finish() {
            QuadrablazeSteamNetworking.UnregisterClientHandler(NetMessageType.Client_SetBossTimer, networkSetTimerMethod);
        }

        void GeneratePool() {
            var prefabs = new List<PoolPrefab>();

            for(int i = 0; i < BossDatabase.Entities.Count; i++) {
                var entity = BossDatabase.Entities[i];

                var prefabInfo = new PoolPrefab() {
                    Active = true,
                    ID = i,
                    PoolSize = 1,
                    Prefab = entity.OriginalGameObject,
                    IsNetworked = true
                };

                prefabs.Add(prefabInfo);
            }

            BossPool.PoolGenPrefabs = prefabs;
            BossPool.InitializePools();
        }

        void NetworkSetTimer(NetworkMessage networkMessage) {
            float serverTimer = networkMessage.reader.ReadSingle();

            if(Mathf.Abs(serverTimer - CurrentSpawnTime) > 1)
                SetCurrentSpawnTime(serverTimer);
        }

        public void ResetTimer(bool start = true) {
            SetCurrentSpawnTime(SpawnTimeDelay);
            TimerActive = start;

            SendTimeOverNetwork();
        }

        void SendEntityCreationMessage(int index, GameObject gameObject) {
            var entityId = GameManager.Current.GetUniqueEntityId();

            QuadrablazeSteamNetworking.SendGameNetworkMessage(CreateEntity, writer => {
                writer.Write(index);
                writer.Write(entityId);
                writer.Write(gameObject);
            });
        }

        void SendTimeOverNetwork() {
            if(QuadrablazeSteamNetworking.Current.SteamConnections.Count == 1) return;

            lastNetworkSpawnTime = CurrentSpawnTime;

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_SetBossTimer);
            writer.Write(lastNetworkSpawnTime);
            writer.FinishMessage();

            for(int i = 1; i < QuadrablazeSteamNetworking.Current.SteamConnections.Count; i++)
                QuadrablazeSteamNetworking.Current.SteamConnections[i]?.SendWriter(writer, Channels.DefaultUnreliable);
        }

        void SetCurrentSpawnTime(float value) {
            CurrentSpawnTime = value;

            if(OnTimerChanged != null) OnTimerChanged.Invoke(value);
        }

        public void SpawnBoss() {
            if(IsBossSpawned) return;

            IsBossSpawned = true;

            var database = GameManager.Current.BossDatabase;
            var index = UnityEngine.Random.Range(0, database.Entities.Count);
            var prefab = BossPool.PoolGenPrefabs[index];
            var bossPrefabController = prefab.Prefab.GetComponent<BossController>();
            var position = bossPrefabController.GetStartPosition();
            var rotation = bossPrefabController.GetStartRotation();

            CurrentSpawnedBoss = GameManager.Current.CreateEntity(CreateEntityType.Boss, index, position, rotation);
        }

        public void Start() {
            ResetTimer();
        }

        public void Update() {
            if(!TimerActive) return;

            if(CurrentSpawnTime > 0) {
                SetCurrentSpawnTime(Mathf.Max(CurrentSpawnTime - Time.deltaTime, 0));

                if(isHost)
                    if(Mathf.Abs(lastNetworkSpawnTime - CurrentSpawnTime) >= .5f)
                        SendTimeOverNetwork();
            }

            if(isHost)
                if(!IsBossSpawned) {
                    if(CurrentSpawnTime == 0)
                        SpawnBoss();
                }
                else {
                    if(CurrentSpawnedBoss != null && !CurrentSpawnedBoss.activeInHierarchy)
                        CurrentSpawnedBoss = null;

                    if(CurrentSpawnedBoss == null) {
                        IsBossSpawned = false;
                        ResetTimer();
                    }
                }
        }


        public static void InvokeBossDefeated() {
            OnBossDefeated?.Invoke();
        }

        public static void InvokeBossSpawned() {
            OnBossSpawned?.Invoke();
        }

        public static void InvokeTimerChanged(float value) {
            OnTimerChanged?.Invoke(value);
        }
    }

    public interface ITimedBossSpawnController {
        TimedBossSpawnController CurrentBossController { get; }
    }
}