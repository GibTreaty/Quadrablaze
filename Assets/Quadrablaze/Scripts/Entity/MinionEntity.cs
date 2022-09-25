using System;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze.Entities {
    public class MinionEntity : EnemyEntity {

        const uint dropXpAmount = 2;
        const uint trickleAmount = 5;
        const int trickleChance = 25;
        const double xpModifier = 0.1;

        public uint CurrentXp { get; set; }

        public MinionEntity(GameObject gameObject, string name, uint id, UpgradeSet upgradeSet, float size, uint xp) : base(gameObject, name, id, upgradeSet, size) {
            CurrentXp = xp * (uint)Math.Max(Math.Ceiling(xpModifier * GameManager.Current.Level), 1);
        }

        public void DropAllXP() {
            if(CurrentXp > 0) {
                var dropAmount = (uint)Math.Ceiling(CurrentXp / (double)dropXpAmount);

                for(uint i = 0; i < dropAmount; i++)
                    DropXP(dropAmount);
            }
        }

        public void DropXP(uint amount) {
            if(CurrentXp > 0) {
                if(amount > CurrentXp)
                    amount = CurrentXp;

                if(amount > 0) {
                    CurrentXp -= amount;
                    XPParticleManager.Current.SpawnXP(amount, CurrentTransform.position);
                }
            }
        }

        public void DropXPTrickle() {
            if(UnityEngine.Random.Range(0, trickleChance + 1) == trickleChance)
                DropXP(trickleAmount);
        }

        protected override void GameObjectWasCleared(GameObject previousGameObject) {

        }

        protected override void GameObjectWasSet(GameObject gameObject) {
            if(gameObject != null) {
                foreach(var hitSpot in gameObject.GetComponentsInChildren<EntityHitSpot>())
                    EnemyProxy.Targets.Add(hitSpot.transform);

                //EnemyProxy.Targets.Add(gameObject.transform);
                EnemyProxy.Enemies.Add(this);
                Listener.RaiseEvent(EntityActions.Spawned, this.ToArgs());
            }
        }

        protected override void OnDeathEvent() {
            DropAllXP();

            if(CurrentGameObject != null)
                //EnemyProxy.Targets.Remove(CurrentTransform);
                foreach(var hitSpot in CurrentGameObject.GetComponentsInChildren<EntityHitSpot>())
                    EnemyProxy.Targets.Remove(hitSpot.transform);

            //SetGameObject(null);
        }

        public override void UnloadEntity() {
            EnemyProxy.Enemies.Remove(this);

            base.UnloadEntity();
        }
    }
}