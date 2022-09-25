using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class WaveController {

        public static event System.Action OnStartWave;

        bool isHost;

        #region Properties
        public EventTimer BetweenWaveTimer { get; set; }
        public WaveBudget CurrentBudget { get; set; }
        public EnemyInfoDatabase EnemyDatabase { get; set; }
        public List<Entities.ScriptableMinionEntity> EntitySpawnList { get; set; }
        public AnimationCurveAsset MinimumCostCurve { get; set; }
        public AnimationCurveAsset MaximumCostCurve { get; set; }
        public PoolManager PoolManagerComponent { get; set; }
        public bool SpawningActive { get; set; }
        public AnimationCurveAsset TierCurve { get; set; }
        #endregion

        public WaveController(PoolManager enemyPool, EnemyInfoDatabase enemyDatabase, EventTimer betweenWaveTimer, AnimationCurveAsset minimumCostCurve, AnimationCurveAsset maximumCostCurve, AnimationCurveAsset tierCurve) {
            EntitySpawnList = null;
            PoolManagerComponent = enemyPool;
            EnemyDatabase = enemyDatabase;
            BetweenWaveTimer = betweenWaveTimer;
            MinimumCostCurve = minimumCostCurve;
            MaximumCostCurve = maximumCostCurve;
            TierCurve = tierCurve;
            CurrentBudget = new WaveBudget(0, 0, 0, 0);
            GeneratePool(enemyPool);

            isHost = NetworkServer.active;
        }

        public void CalculateWalletValues() {
            int level = GameManager.Current.Level;
            Debug.Log("GameManager Level " + level);

            CurrentBudget.Points = 10 + ((level - 1) * 2);
            CurrentBudget.MinimumSpendLimit = (int)MinimumCostCurve.Evaluate(level);
            CurrentBudget.MaximumSpendLimit = (int)MaximumCostCurve.Evaluate(level);
            CurrentBudget.Tier = (int)TierCurve.Evaluate(level);

            GameDebug.Log($"New Budget {CurrentBudget.Points}, {CurrentBudget.MinimumSpendLimit}-{CurrentBudget.MaximumSpendLimit}, {CurrentBudget.Tier}");
        }

        public void Finish() {
            foreach(var pool in PoolManagerComponent.ObjectContainers)
                pool.Value.ClearAll();

            PoolManagerComponent.ObjectContainers.Clear();
            PoolManagerComponent.PoolGenPrefabs.Clear();
        }

        void GeneratePool(PoolManager enemyPool) {
            var enemyPrefabs = new List<PoolPrefab>();

            for(int i = 0; i < EnemyDatabase.Entities.Count; i++) {
                var entity = EnemyDatabase.Entities[i];

                var prefabInfo = new PoolPrefab() {
                    Active = true,
                    ID = i,
                    PoolSize = entity.PoolSize,
                    Prefab = entity.OriginalGameObject,
                    IsNetworked = true
                };

                enemyPrefabs.Add(prefabInfo);
            }

            enemyPool.PoolGenPrefabs = enemyPrefabs;
            enemyPool.InitializePools();
        }

        public void StartWave() {
            if(!EnemyDatabase) return;

            GameDebug.Log("StartWave");
            EntitySpawnList = EnemyDatabase.SpendAllPointsOnEntities(CurrentBudget);
            OnStartWave?.Invoke();

            SpawningActive = true;
            BetweenWaveTimer.Start(true);
        }

        public void StartWaveManager() {
            SpawningActive = true;
            BetweenWaveTimer.Start(true);
        }

        public void Update() {
            if(!isHost) return;

            BetweenWaveTimer.Update();
        }
    }
}