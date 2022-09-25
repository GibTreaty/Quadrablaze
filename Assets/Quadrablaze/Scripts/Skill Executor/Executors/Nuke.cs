using System.Collections;
using System.Collections.Generic;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors {
    public class Nuke : SkillExecutor, IExecutorActorDisable, ISkillUpdate {

        float nextExplosion;
        float explosionRange;

        IEnumerator NukeEffectMethod = null;
        bool effectIsPlaying = false;

        public new ScriptableNukeSkillExecutor OriginalSkillExecutor { get; }

        public Nuke(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableNukeSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
        }

        void DoNukeEffect() {
            NukeEffectMethod = NukeEffectRoutine(NetworkServer.active);

            GameCoroutine.BeginCoroutine(NukeEffectMethod);
        }

        void GenerateNextExplosionTime() {
            float repeatDelay = OriginalSkillExecutor.RepeatDelayCurve.Evaluate(CurrentLayoutElement.CurrentLevel);
            float repeatVariation = OriginalSkillExecutor.RepeatVariationCurve.Evaluate(CurrentLayoutElement.CurrentLevel);

            repeatVariation = Random.Range(-repeatVariation, repeatVariation);

            nextExplosion = Time.time + repeatDelay + repeatVariation;
        }

        public override void Invoke() {
            DoNukeEffect();
        }

        void KillEnemyActors() {
            var entities = new List<Entities.EnemyEntity>();

            if(OriginalSkillExecutor.RangeCurve != null) {
                foreach(var enemy in EnemyProxy.Enemies) {
                    if(enemy is Entities.BossEntity) continue;

                    float distance = Vector3.Distance(CurrentActorEntity.CurrentTransform.position, enemy.CurrentTransform.position);

                    if(distance - enemy.Size * .5f <= explosionRange)
                        entities.Add(enemy);
                }
            }
            else {
                foreach(var enemy in EnemyProxy.Enemies) {
                    if(enemy is Entities.BossEntity) continue;

                    entities.Add(enemy);
                }
            }

            int count = 0;

            foreach(var entity in entities) {
                if(entity.CurrentUpgradeSet != null)
                    entity.CurrentUpgradeSet.Lives = 0;

                var user = entity.CurrentGameObject.GetComponent<PoolUser>();

                user.SpawnHere("Explosions");

                if(entity is MinionEntity minionEntity)
                    minionEntity.DropAllXP();

                entity.Kill();
                count++;
            }
        }

        IEnumerator NukeEffectRoutine(bool isServer) {
            effectIsPlaying = true;

            yield return new WaitForSeconds(0.1f);

            if(OriginalSkillExecutor.RangeCurve != null)
                explosionRange = OriginalSkillExecutor.RangeCurve.Evaluate(CurrentLayoutElement.CurrentLevel);

            if(explosionRange > 0)
                EffectManager.Current.Effects.Play("Nuke", CurrentActorEntity.CurrentTransform, null, explosionRange.ToString());
            else
                EffectManager.Current.Effects.Play("Nuke", CurrentActorEntity.CurrentTransform, null, explosionRange.ToString());

            yield return new WaitForSeconds(2);

            if(isServer) {
                KillEnemyActors();
                GenerateNextExplosionTime();
            }

            effectIsPlaying = false;
        }

        public void OnActorDisabled() {
            RemoveRoutine();
        }

        void OnSoundPlay(object callbackOutput) {
            var gameObject = callbackOutput as GameObject;

            if(gameObject != null)
                gameObject.transform.SetParent(CurrentActorEntity.CurrentTransform, false);
        }

        public void RemoveRoutine() {
            if(NukeEffectMethod != null)
                GameCoroutine.EndCoroutine(NukeEffectMethod);

            NukeEffectMethod = null;
        }

        public void SkillUpdate() {
            if(NetworkServer.active)
                if(!effectIsPlaying && Time.time > nextExplosion)
                    Invoke();
        }
    }
}