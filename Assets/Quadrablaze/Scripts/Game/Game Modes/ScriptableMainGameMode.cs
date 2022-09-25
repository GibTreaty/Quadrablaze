using UnityEngine;
using Quadrablaze.GameModes;
using System;
using YounGenTech.PoolGen;

namespace Quadrablaze.GameModes {
    [CreateAssetMenu(menuName = "Quadrablaze/Modes/Main")]
    [Serializable]
    public class ScriptableMainGameMode : ScriptableGameMode {

        [Header("Boss Settings")]
        [SerializeField]
        BossInfoDatabase _bossDatabase;

#if UNITY_EDITOR
        [SerializeField]
        BossInfoDatabase _editorBossDatabase;
#endif

        [SerializeField]
        string _bossPoolName = "";

        [SerializeField]
        float _bossSpawnTimeDelay = 60;

        [Header("Regular Enemy Settings")]
        [SerializeField]
        EnemyInfoDatabase _enemyDatabase;

#if UNITY_EDITOR
        [SerializeField]
        EnemyInfoDatabase _editorEnemyDatabase;
#endif

        [SerializeField]
        string _enemyPoolName = "";

        [SerializeField]
        int _maxEnemies = 20;

        [SerializeField]
        int _enemySpawnTimeDelay = 1;

        [SerializeField]
        float _betweenWaveTimer = 1;

        [Header("Enemy Budget Settings")]
        [SerializeField]
        AnimationCurveAsset _minimumCostCurve;

        [SerializeField]
        AnimationCurveAsset _maximumCostCurve;

        [SerializeField]
        AnimationCurveAsset _tierCurve;

        public override GameMode InstantiateMode(Action onWinListener) {
#if UNITY_EDITOR
            BossInfoDatabase bossDatabase = _editorBossDatabase;
            EnemyInfoDatabase enemyDatabase = _editorEnemyDatabase;
#else
            BossInfoDatabase bossDatabase = _bossDatabase;
            EnemyInfoDatabase enemyDatabase = _enemyDatabase;
#endif

            return new MainGameMode(
                onWinListener,
                bossDatabase,
                PoolManager.GetPool(_bossPoolName),
                _bossSpawnTimeDelay,

                enemyDatabase,
                PoolManager.GetPool(_enemyPoolName),
                _maxEnemies,
                new EventTimer(_enemySpawnTimeDelay) { AutoReset = true },

                new EventTimer(_betweenWaveTimer) { AutoReset = true, AutoDisable = true },
                _minimumCostCurve,
                _maximumCostCurve,
                _tierCurve
            );
        }
    }
}