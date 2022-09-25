using Quadrablaze.Skills;
using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    public class EnemyEntity : ActorEntity {
        public EnemyEntity(GameObject gameObject, string name, uint id, UpgradeSet upgradeSet, float size) : base(gameObject, name, id, upgradeSet, size) {
            //TODO: What dayfook
            //OnDeath.AddListener(() => EnemyProxy.OnDeath.Invoke(this));

            //OnDespawn.AddListener(() => EnemyProxy.Enemies.Remove(this));
            //OnDespawn.AddListener(() => EnemyProxy.OnDespawned.Invoke(this));

            //OnRespawn.AddListener(() => EnemyProxy.OnRespawned.Invoke(this));

            //OnSpawn.AddListener(() => EnemyProxy.Enemies.Add(this));
            //OnSpawn.AddListener(() => EnemyProxy.OnSpawned.Invoke(this));

            //OnPermadeath.AddListener(() => EnemyProxy.Enemies.Remove(this));
            //OnPermadeath.AddListener(() => EnemyProxy.OnPermadeath.Invoke(this));

            //if(ActorType != ActorTypes.Boss) {
            //    OnSpawn.AddListener(() => EnemyProxy.Targets.Add(transform));
            //    OnDespawn.AddListener(() => EnemyProxy.Targets.Remove(transform));
            //    OnPermadeath.AddListener(() => EnemyProxy.Targets.Remove(transform));
            //}
        }
    }
}