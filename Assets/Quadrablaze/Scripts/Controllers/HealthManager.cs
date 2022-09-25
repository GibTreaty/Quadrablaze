using System.Collections.Generic;
using Quadrablaze.Entities;
using StatSystem;
using UnityEngine;

namespace Quadrablaze {
    public static class HealthManager {

        public static float CalculateModifierValue(ActorEntity actorEntity) {
            float value = 1;
            int level = GameManager.Current.Level - 1;

            switch(actorEntity) {
                case PlayerEntity entity: { value = Mathf.Floor((level + 1) / 5f) * (1 / 5f); break; }
                case MinionEntity entity: { value = 0; break; }
                case Entities.BossEntity entity: { value = ((level + Mathf.Max(0, TimedBossSpawnController.BossesSpawned - 1)) * .65f); break; }
            }

            value += 1;

            return value;
        }
        /*public static float CalculateModifierValue(ActorTypes actorType) {
            float value = 1;
            int level = GameManager.Current.Level - 1;

            switch(actorType) {
                case ActorTypes.Player: value = Mathf.Floor((level + 1) / 5f) * (1 / 5f); break;
                //case ActorTypes.Player: value = level * (1f / (1f + level)); break;
                //case ActorTypes.Player: value = globalLevel * (1f / 6f); break;
                case ActorTypes.MediumEnemy: value = Mathf.Floor(level * .5f); break;
                case ActorTypes.SmallEnemy: value = 0; break;
                //case ActorTypes.SmallEnemy: value = (level * .5f); break;
                case ActorTypes.Boss: value = ((level + Mathf.Max(0, TimedBossSpawnController.BossesSpawned - 1)) * .65f); break;
                    //case ActorTypes.Boss: value = ((level + Mathf.Max(0, BossSpawner.Current.BossesSpawnedThisGame - 1)) * .65f); break;
            }

            value += 1;

            return value;
        }*/

        public static void UpdateHealth() {
            foreach(var id in GameManager.Current.PlayerEntityIds)
                if(GameManager.Current.GetActorEntity(id) is PlayerEntity playerEntity)
                    UpdateHealth(playerEntity);

            //foreach(var entity in GameManager.Current.Entities)
            //    if(entity is PlayerEntity playerEntity)
            //            UpdateHealth(playerEntity);
        }
        public static void UpdateHealth(ActorEntity entity) {
            foreach(var health in entity.HealthSlots)
                UpdateHealth(entity, health);
        }
        public static void UpdateHealth(ActorEntity entity, Stat stat) {
            var modifier = CalculateModifierValue(entity);
            var baseHealth = entity.OriginalScriptableObject.HealthSlots[stat.Id].baseHealth;
            int newHealth = (int)(baseHealth * modifier);

            //if(entity is PlayerEntity)
            stat.Value = newHealth;

            if(stat is CappedStat cappedStat)
                cappedStat.MaxValue = newHealth;
        }
    }
}