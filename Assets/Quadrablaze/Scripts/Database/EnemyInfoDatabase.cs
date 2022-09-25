using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Quadrablaze/Database/Enemy Info Database")]
    public class EnemyInfoDatabase : ScriptableObject {

        [SerializeField]
        List<ScriptableMinionEntity> _entities = new List<ScriptableMinionEntity>();

        #region Properties
        public List<ScriptableMinionEntity> Entities => _entities;
        #endregion
    }

    public static class EnemyCostUtility {
        public static bool CanBuy(this ScriptableMinionEntity minionEntity, IWaveBudget budget) {
            var points = minionEntity.Points;

            bool canBuy = !minionEntity.Spawnable &&
                 points > 0 &&
                 points <= budget.Points &&
                 points >= budget.MinimumSpendLimit &&
                 points <= budget.MaximumSpendLimit &&
                 (string.IsNullOrEmpty(minionEntity.Group) ? true : minionEntity.Tier == budget.Tier);

            //if(canBuy) {
            //    Debug.Log($"Bought Tier:{minionEntity.Tier} - Budget Tier:{budget.Tier}" +
            //        $"\nMinion: {minionEntity.name}");
            //}

            if(canBuy)
                if(minionEntity.RequireAbilities.Length > 0) {
                    bool hasAbilities = false;

                    foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                        if(player.playerInfo != null)
                            if(player.playerInfo.AttachedEntity != null)
                                if(player.playerInfo.AttachedEntity.HasAllSkills(minionEntity.RequireAbilities)) {
                                    hasAbilities = true;
                                    break;
                                }

                    canBuy &= hasAbilities;
                }

            return canBuy;
        }

        public static Dictionary<string, HashSet<ScriptableMinionEntity>> GenerateGroupedEntities(this EnemyInfoDatabase database) {
            var groupedEnemyEntities = new Dictionary<string, HashSet<ScriptableMinionEntity>>();

            foreach(var enemyEntity in database.Entities)
                if(!string.IsNullOrEmpty(enemyEntity.Group))
                    GetGroupedEntities(groupedEnemyEntities, enemyEntity.Group).Add(enemyEntity);

            return groupedEnemyEntities;
        }

        public static HashSet<ScriptableMinionEntity> GetGroupedEntities(Dictionary<string, HashSet<ScriptableMinionEntity>> groupedEntities, string groupName) {
            HashSet<ScriptableMinionEntity> list;

            if(!groupedEntities.TryGetValue(groupName, out list)) {
                list = new HashSet<ScriptableMinionEntity>();
                groupedEntities.Add(groupName, list);
            }

            return list;
        }

        public static ScriptableMinionEntity GetRandomEntity(this EnemyInfoDatabase database, IWaveBudgetSpendable budget) {
            var list = database.GetAllEntitiesWithinBudget(budget);

            return list.Count > 0 ? list[Random.Range(0, list.Count)] : null;
        }

        public static List<ScriptableMinionEntity> GetAllEntitiesWithinBudget(this EnemyInfoDatabase database, IWaveBudgetSpendable budget) {
            return database.Entities.Where(s => s.CanBuy(budget)).ToList();
        }

        public static List<ScriptableMinionEntity> SpendAllPointsOnEntities(this EnemyInfoDatabase database, IWaveBudgetSpendable budget) {
            var bought = new List<ScriptableMinionEntity>();
            var entities = new List<ScriptableMinionEntity>();
            bool spent;

            do {
                spent = false;
                entities.Clear();

                foreach(var entity in database.Entities)
                    if(entity.CanBuy(budget)) {
                        if(entity.MaximumSpawnCount > 0) {
                            var entityTypeCount = bought.Count(s => entity.Equals(s));

                            if(entityTypeCount >= entity.MaximumSpawnCount)
                                continue;
                        }

                        entities.Add(entity);
                    }

                if(entities.Count > 0) {
                    var entity = entities[Random.Range(0, entities.Count)];

                    if(entity.SpawnCountTarget > 0) {
                        var loopCount = 1;

                        spent = true;

                        if(entity.MaximumSpawnCount > 0) {
                            var entityTypeCount = bought.Count(s => entity.Equals(s));

                            loopCount = Mathf.Max(0, entity.MaximumSpawnCount - entityTypeCount);
                        }

                        for(int i = 0; i < loopCount; i++) {
                            bought.Add(entity);
                            budget.Spend(entity.Points);
                        }
                    }
                }
            }
            while(spent);

            return bought;

            while(database.Entities.Exists(s => s.CanBuy(budget))) {
                var entity = database.GetRandomEntity(budget);
                bool added = false;

                if(entity.SpawnCountTarget > 0) {
                    for(int i = 0; i < Mathf.Max(1, entity.SpawnCountTarget); i++) {
                        //if(entity.MaximumSpawnCount > 0) {
                        //    int inQueue = 0;

                        //    foreach(var item in bought)
                        //        if(item == entity)
                        //            inQueue++;
                        //    Debug.Log($"InQueue1: {inQueue}");

                        //    foreach(var gameEntity in GameManager.Current.Entities)
                        //        if(gameEntity is MinionEntity minionEntity)
                        //            if(entity == minionEntity.OriginalScriptableObject)
                        //                inQueue++;

                        //    Debug.Log($"InQueue2: {inQueue}");

                        //    if(inQueue >= entity.MaximumSpawnCount)
                        //        break;
                        //}
                        if(entity.MaximumSpawnCount > 0 && bought.Count(s => entity.Equals(s)) >= entity.MaximumSpawnCount)
                            break;

                        bought.Add(entity);
                        added = true;
                    }

                    if(added)
                        budget.Spend(entity.Points);
                }

                // TODO: Might need to replace this to avoid infinite loop
                //if(!added) break;
            }

            return bought;
        }
    }
}