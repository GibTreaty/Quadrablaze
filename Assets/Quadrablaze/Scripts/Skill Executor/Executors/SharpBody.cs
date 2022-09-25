using System.Collections.Generic;
using Quadrablaze.Entities;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    public class SharpBody : SkillExecutor, IEntityCollisionTrigger {

        Dictionary<uint, float> lastHitTime = new Dictionary<uint, float>();
        EntityCollisionTrigger collisionTrigger = null;

        public float Damage { get; set; }

        public new ScriptableSharpBodySkillExecutor OriginalSkillExecutor { get; }

        public SharpBody(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableSharpBodySkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
        }

        public override void ApplyToGameObject(GameObject gameObject) {
            collisionTrigger = gameObject.AddComponent<EntityCollisionTrigger>();
            collisionTrigger.EntityId = CurrentActorEntity.Id;
        }

        public override void ClearGameObject(GameObject gameObject) {
            Object.Destroy(collisionTrigger);
        }

        public float GetCalculatedDamage(float damage) {
            var outputDamage = OriginalSkillExecutor.Damage * OriginalSkillExecutor.DamageMultiplier;

            return outputDamage + (Mathf.Max(CurrentLayoutElement.CurrentLevel - 1, 0) * outputDamage * OriginalSkillExecutor.DamageLevelMultiplier);
        }

        public void ApplyForce(Collider collider) {
            var rigidbody = collider.attachedRigidbody;

            if(rigidbody) {
                var baseMovementController = rigidbody.GetComponent<BaseMovementController>();

                if(baseMovementController != null) {
                    var direction = (baseMovementController.transform.position - CurrentActorEntity.CurrentTransform.position).normalized;

                    baseMovementController.MoveToOvertime(1, baseMovementController.transform.position + direction * OriginalSkillExecutor.NonRigidbodyForce, 1, 100);
                }
                else {
                    rigidbody.AddExplosionForce(OriginalSkillExecutor.Force, CurrentActorEntity.CurrentTransform.position, 10);
                }
            }
        }

        void CleanupHitTimes() {
            var keyList = new List<uint>(lastHitTime.Keys);

            foreach(var item in keyList)
                if(Time.time > (lastHitTime[item] + OriginalSkillExecutor.HitDelay))
                    lastHitTime.Remove(item);
        }

        public void DoDamageOnCollision(Collision collision) {
            var hitGameObject = collision.collider.gameObject;

            if(hitGameObject.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot) {
                Debug.Log($"{CurrentActorEntity.Name} EntityCollisionTrigger");
                if(GameManager.Current.GetActorEntity(collision.transform.root.gameObject, out ActorEntity entity)) {
                    if((OriginalSkillExecutor.HitMask & (1 << hitGameObject.layer)) == 0) return;
                    
                    Debug.Log($"{entity.Name} DoDamageOnCollision");

                    bool wasHit = lastHitTime.TryGetValue(entity.Id, out float time);
                    bool canHit = !wasHit || Time.time > time + OriginalSkillExecutor.HitDelay;

                    if(!canHit) return;
                    else lastHitTime[entity.Id] = Time.time;

                    CleanupHitTimes();

                    float damagePercent = 0;

                    if(OriginalSkillExecutor.DamageLevelCurve != null)
                        damagePercent = OriginalSkillExecutor.DamageLevelCurve.Evaluate(CurrentLayoutElement.CurrentLevel);

                    if(CurrentLayoutElement.CurrentLevel > 1)
                        ApplyForce(collision.collider);

                    if(damagePercent > 0) {
                        if(GameManager.Current.GetActorEntity(collision.gameObject, out ActorEntity actorEntity))
                            if(actorEntity is Entities.BossEntity)
                                if(hitGameObject.layer != LayerMask.NameToLayer("Boss Hit Spot"))
                                    return;

                        hitGameObject.DoHealthChange(-damagePercent, hitSpot.HealthId, this, false);
                     }
                }
            }
        }

        public override void LevelChanged(int level, int previousLevel) {
            Damage = GetCalculatedDamage(CurrentLayoutElement.CurrentSkillAmount);
        }

        public void OnEntityCollision(Collision collision) {
            DoDamageOnCollision(collision);
        }
    }
}