using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

// TODO: Make the graphics blaze radius ring be instantiated into the world and positioned manually by the Blaze ability
namespace Quadrablaze.SkillExecutors {
    public class Blaze : SkillExecutor, ICooldownTimer, ISkillUpdate, ISkillFixedUpdate {

        public EventTimer BlazeTimer { get; }
        public EventTimer CooldownTimer { get; }
        public override bool IsReady => !BlazeTimer.Active && !CooldownTimer.Active;
        public new ScriptableBlazeSkillExecutor OriginalSkillExecutor { get; }

        BlazeAfterburner[] afterBurnerParticles;
        BlazeRing blazeRing;
        float currentRadius;
        float currentGraphicsTime;
        int hits;
        Collider[] hitColliders = new Collider[15];
        ParticleSystem[] particleSystems;
        bool updateRadius;
        float lastHitTime;

        public Blaze(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableBlazeSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            BlazeTimer = new EventTimer(OriginalSkillExecutor.BlazeDuration) { AutoDisable = true };
            CooldownTimer = new EventTimer(OriginalSkillExecutor.BlazeRechargeDuration) { AutoDisable = true };

            BlazeTimer.OnElapsed.AddListener(Recharge);

            currentRadius = GetRadius();
            currentGraphicsTime = .01f;
        }

        public override void ApplyToGameObject(GameObject gameObject) {
            if(OriginalSkillExecutor.BlazeAfterburnerPrefab) {
                var jetPivots = new List<Transform>();
                GameHelper.GetAllChildrenRecurvisely("Jet Pivot", CurrentActorEntity.CurrentTransform.root, jetPivots);
                GameHelper.GetAllChildrenRecurvisely("Jet_Pivot", CurrentActorEntity.CurrentTransform.root, jetPivots);

                particleSystems = new ParticleSystem[jetPivots.Count];
                afterBurnerParticles = new BlazeAfterburner[jetPivots.Count];

                for(int i = 0; i < jetPivots.Count; i++) {
                    var transform = jetPivots[i];
                    var effectGameObject = Object.Instantiate(OriginalSkillExecutor.BlazeAfterburnerPrefab.gameObject, transform, false);
                    var blazeAfterburner = effectGameObject.GetComponent<BlazeAfterburner>();

                    particleSystems[i] = blazeAfterburner.afterburnerParticles;
                    afterBurnerParticles[i] = blazeAfterburner;
                    blazeAfterburner.SetStartSpeed(OriginalSkillExecutor.AfterburnerSpeed);
                }
            }

            if(OriginalSkillExecutor.BlazeRingPrefab) {
                var instantiatedBlazeRing = Object.Instantiate(OriginalSkillExecutor.BlazeRingPrefab.gameObject, CurrentActorEntity.CurrentTransform.root, false);

                blazeRing = instantiatedBlazeRing.GetComponent<BlazeRing>();
                blazeRing.SetRadius(OriginalSkillExecutor.Radius);
            }
        }

        public void EnableParticles(bool enable) {
            foreach(var particleSystem in particleSystems)
                if(enable) particleSystem.Play();
                else particleSystem.Stop();

            if(blazeRing)
                if(enable) blazeRing.ringParticles.Play();
                else blazeRing.ringParticles.Stop();
        }

        void GetColliders(Vector3 position) {
            hits = Physics.OverlapSphereNonAlloc(position, currentRadius, hitColliders, OriginalSkillExecutor.HitMask, QueryTriggerInteraction.Ignore);
        }

        float GetRadius() {
            return OriginalSkillExecutor.Radius + (OriginalSkillExecutor.Radius * OriginalSkillExecutor.RadiusLevelMultiplier * Mathf.Max(CurrentLayoutElement.CurrentLevel - 1, 0));
        }

        public override void Invoke() {
            if(BlazeTimer.Active || CooldownTimer.Active) return;

            CooldownTimer.Reset();
            BlazeTimer.Start(true);
            EnableParticles(true);
            UpdateCurrentRadius();

            if(blazeRing)
                blazeRing.SetRadius(currentRadius);

            GiveSkillFeedback(false);
        }

        public override void LevelChanged(int level, int previousLevel) {
            updateRadius = true;
        }

        public override void OnSkillFeedback(byte[] parameters) {
            CooldownTimer.Reset();
            BlazeTimer.Start(true);
            EnableParticles(true);
            UpdateCurrentRadius();

            if(blazeRing)
                blazeRing.SetRadius(OriginalSkillExecutor.Radius);
        }

        public void Recharge() {
            if(BlazeTimer.Active || CooldownTimer.Active) return;

            CooldownTimer.Reset();
            CooldownTimer.Start();
            EnableParticles(false);
        }

        public void ResetSkill() {
            BlazeTimer.CurrentTime = 0;
            CooldownTimer.CurrentTime = 0;
            EnableParticles(false);
        }

        public void SkillFixedUpdate() {
            //bool flag = ((AssignedActorEntity is PlayerEntity playerEntity) && playerEntity.Owner) || NetworkServer.active;

            bool flag = CurrentActorEntity is PlayerEntity playerEntity ?
                playerEntity.Owner :
                NetworkServer.active;

            if(flag)
                if(BlazeTimer.Active && CurrentActorEntity.BaseMovementControllerComponent.MovementStyle != BaseMovementController.MovementType.Arcade)
                    CurrentActorEntity.RigidbodyComponent.AddRelativeTorque(0, OriginalSkillExecutor.RotationSpeed, 0);
        }

        public void SkillUpdate() {
            BlazeTimer.Update();
            CooldownTimer.Update();

            if(updateRadius)
                if(!BlazeTimer.Active) {
                    UpdateCurrentRadius();
                    updateRadius = false;
                }

            if(NetworkServer.active)
                if(BlazeTimer.Active)
                    if(Time.time > lastHitTime + (1 / OriginalSkillExecutor.DamagePerSecond)) {
                        GetColliders(CurrentActorEntity.CurrentTransform.position);

                        for(int i = 0; i < hits; i++) {
                            //float damage = OriginalSkillExecutor.DamagePerSecond;
                            ////Debug.Log($"Hit for {damage} damage", hitColliders[i]);

                            //if(CurrentActorEntity is PlayerEntity playerEntity)
                            //    if(!EnemyProxy.Targets.Contains(hitColliders[i].transform))
                            //        continue;
                            //    else {
                            //        if(hitColliders[i].gameObject.layer == LayerMask.NameToLayer("Boss Hit Spot"))
                            //            damage = OriginalSkillExecutor.BossDamagePerSecond;
                            //    }

                            int damage = 1 + Mathf.FloorToInt(OriginalSkillExecutor.DamageLevelMultiplier * GameManager.Current.Level);

                            hitColliders[i].gameObject.DoHealthChange(-damage, CurrentActorEntity, true);
                            lastHitTime = Time.time;
                        }
                    }

            UpdateGraphicalRadius();
        }

        public override void Unload() {
            foreach(var afterBurner in afterBurnerParticles)
                Object.Destroy(afterBurner.gameObject);
        }

        void UpdateCurrentRadius() {
            currentRadius = GetRadius();
            UpdateAfterburnerParticleSpeed();
            //UpdateBlazeRadiusGraphics();
        }

        public void UpdateAfterburnerParticleSpeed() {
            foreach(var afterburner in afterBurnerParticles)
                afterburner.SetStartSpeed(OriginalSkillExecutor.AfterburnerSpeed + (OriginalSkillExecutor.AfterburnerSpeed * OriginalSkillExecutor.RadiusLevelMultiplier * Mathf.Max(CurrentLayoutElement.CurrentLevel - 1, 0)));
        }

        void UpdateGraphicalRadius() {
            if(!string.IsNullOrEmpty(OriginalSkillExecutor.GraphicsRadiusTransformPath))
                if(CurrentActorEntity.CurrentGameObject != null)
                    if(currentGraphicsTime != CooldownTimer.NormalizedTime)
                        if(CurrentActorEntity.CurrentTransform.Find(OriginalSkillExecutor.GraphicsRadiusTransformPath) is Transform radiusTransform) {
                            var time = 1 - CooldownTimer.NormalizedTime;
                            var radius = currentRadius * time;

                            currentGraphicsTime = CooldownTimer.NormalizedTime;
                            radiusTransform.localScale = Vector3.one * radius * 2;
                        }
        }
    }
}