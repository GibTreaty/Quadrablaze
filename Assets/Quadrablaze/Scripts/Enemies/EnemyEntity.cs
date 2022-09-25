using Quadrablaze.Skills;
using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Quadrablaze/Entities/Enemy Entity")]
    public class EnemyEntity : ScriptableObject {

        [SerializeField]
        ScriptableUpgradeSet _defaultUpgradeSet;

        [SerializeField]
        int _points;

        [SerializeField]
        GameObject _prefab;

        [SerializeField]
        string _id;

        [SerializeField]
        int _tier;

        [SerializeField]
        string _group;

        [SerializeField]
        int _spawnCountTarget;

        [SerializeField]
        int _maximumSpawnCount;

        [SerializeField]
        string[] _requireAbilities;

        [Header("Object Pool Settings")]
        [SerializeField]
        int _prefabId;

        [SerializeField]
        int _poolSize;

        #region Properties
        public string Group {
            get { return _group; }
            set { _group = value; }
        }

        public string Id {
            get { return _id; }
            set { _id = value; }
        }

        public int MaximumSpawnCount {
            get { return _maximumSpawnCount; }
            set { _maximumSpawnCount = value; }
        }


        public int Points {
            get { return _points; }
            set { _points = value; }
        }

        public int PoolSize {
            get { return _poolSize; }
            set { _poolSize = value; }
        }

        public GameObject Prefab {
            get { return _prefab; }
            set { _prefab = value; }
        }

        public int PrefabId {
            get { return _prefabId; }
            set { _prefabId = value; }
        }

        public string[] RequireAbilities {
            get { return _requireAbilities; }
            set { _requireAbilities = value; }
        }
        public int SpawnCountTarget {
            get { return _spawnCountTarget; }
            set { _spawnCountTarget = value; }
        }

        public int Tier {
            get { return _tier; }
            set { _tier = value; }
        }
        #endregion

        public bool CanBuy(IWaveBudget budget) {
            bool canBuy = _points > 0 &&
                   _points <= budget.Points &&
                   _points >= budget.MinimumSpendLimit &&
                   _points <= budget.MaximumSpendLimit &&
                   (string.IsNullOrEmpty(_group) ? true : _tier == budget.Tier);

            if(canBuy)
                if(_requireAbilities.Length > 0) {
                    bool hasAbilities = false;

                    foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                        if(player.playerInfo != null)
                            if(player.playerInfo.AttachedEntity != null)
                                if(player.playerInfo.AttachedEntity.HasAllSkills(_requireAbilities)) {
                                    hasAbilities = true;
                                    break;
                                }

                    canBuy &= hasAbilities;
                }

            return canBuy;
        }
    }
}