using System;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze.GameModes {
    public class MainGameMode : GameMode, IEnemySpawnController, ITimedBossSpawnController {

        Action onBossSpawnedMethod;
        Action onBossDefeatedMethod;

        #region Properties
        public TimedBossSpawnController CurrentBossController { get; }

        public EnemySpawnController CurrentEnemyController { get; }

        public WaveController CurrentWaveController { get; }
        #endregion

        public MainGameMode(
                Action onWinListener,
                BossInfoDatabase bossDatabase,
                PoolManager bossPool,
                float bossSpawnTimeDelay,

                EnemyInfoDatabase enemyDatabase,
                PoolManager enemyPool,
                int maxEnemies,
                EventTimer spawnTimer,

                EventTimer betweenWaveTimer,
                AnimationCurveAsset minimumCostCurve,
                AnimationCurveAsset maximumCostCurve,
                AnimationCurveAsset tierCurve

            ) : base(onWinListener) {
            CurrentBossController = new TimedBossSpawnController(bossDatabase, bossPool, bossSpawnTimeDelay);
            CurrentWaveController = new WaveController(enemyPool, enemyDatabase, betweenWaveTimer, minimumCostCurve, maximumCostCurve, tierCurve);
            CurrentEnemyController = new EnemySpawnController(enemyPool, enemyDatabase, maxEnemies, spawnTimer, CurrentWaveController);

            onBossSpawnedMethod = () => CurrentWaveController.SpawningActive = false;
            onBossDefeatedMethod = () => CurrentWaveController.SpawningActive = true; ;

            TimedBossSpawnController.OnBossSpawned += onBossSpawnedMethod;
            TimedBossSpawnController.OnBossDefeated += onBossDefeatedMethod;

            if(IsHost)
                CurrentWaveController.StartWaveManager();
        }

        public override void EndGame() {
            TimedBossSpawnController.OnBossSpawned -= onBossSpawnedMethod;
            TimedBossSpawnController.OnBossDefeated -= onBossDefeatedMethod;

            CurrentBossController.Finish();
            CurrentWaveController.Finish();
            CurrentEnemyController.Finish();
        }

        public override void UpdateMode() {
            CurrentBossController.Update();

            // TODO: Boss - Not being destroyed properly. Happened on the second boss death.
            if(CurrentBossController.CurrentSpawnedBoss == null) {
                CurrentWaveController.Update();
                CurrentEnemyController.Update(CurrentWaveController);
            }
        }
    }
}