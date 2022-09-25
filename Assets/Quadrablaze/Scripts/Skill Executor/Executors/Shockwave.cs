using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities.Weapon;

// TODO: Make this do more damage, starting at 1 damage
// TODO: Shockwave radius sprite
namespace Quadrablaze.SkillExecutors {
    public class Shockwave : SkillExecutor, ICooldownTimer, ISkillUpdate {

        public event System.Action OnInvoke;

        public EventTimer CooldownTimer { get; }
        public override bool IsReady => CooldownTimer.HasElapsed;
        public new ScriptableShockwaveSkillExecutor OriginalSkillExecutor { get; }

        int _hits;
        Collider[] _hitColliders = new Collider[15];
        float currentGraphicsTime;
        public Shockwave(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableShockwaveSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            CooldownTimer = new EventTimer(OriginalSkillExecutor.CooldownDuration) { AutoDisable = true };

            currentGraphicsTime = .01f;
        }

        void DoDamage(Collider hit) {
            //float damage = OriginalSkillExecutor.DamageLevelMultiplierCurve
            float damage = 1;

            if(CurrentActorEntity is PlayerEntity)
                if(!EnemyProxy.Targets.Contains(hit.transform))
                    return;

            if(hit.gameObject.layer == LayerMask.NameToLayer("Boss Hit Spot")) {
                //if(OriginalSkillExecutor.BossDamageLevelMultiplierCurve != null)
                //    damage = OriginalSkillExecutor.BossDamageLevelMultiplierCurve.Evaluate(CurrentLayoutElement.CurrentLevel);
            }
            else {
                //if(OriginalSkillExecutor.DamageLevelMultiplierCurve != null)
                //    damage = OriginalSkillExecutor.DamageLevelMultiplierCurve.Evaluate(CurrentLayoutElement.CurrentLevel);
                damage = 1 + Mathf.FloorToInt(GameManager.Current.Level / 6f);
            }

            hit.gameObject.DoHealthChange((int)-damage, this, false); //TODO: Do something about percentage damage
            //hit.Damage(new HealthEvent(CurrentActorEntity.CurrentGameObject, damage, HealthEvent.EventType.Percent));
        }

        void DoForce(Rigidbody hit, float force, float rigidbodyForce) {
            if(hit)
                if(CurrentActorEntity.CurrentTransform != null)
                    if(hit.GetComponent<BaseMovementController>() is BaseMovementController baseMovementController) {
                        var direction = (hit.position - CurrentActorEntity.CurrentTransform.position).normalized;

                        baseMovementController.SetPushDirection(direction * force);
                        //baseMovementController.MoveToOvertime(1, hit.position + direction * force, 1, 100);
                    }
                    else if(GameManager.Current.GetProjectileEntity(hit.gameObject, out ProjectileEntity projectileEntity)) {
                        var direction = (projectileEntity.Position - CurrentActorEntity.CurrentTransform.position).normalized;
                        var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                        projectileEntity.Angle = angle;
                    }
                    else {
                        hit.AddExplosionForce(rigidbodyForce, CurrentActorEntity.CurrentTransform.position, OriginalSkillExecutor.Radius);
                    }
        }

        void DoPush() {
            GetColliders();

            // TODO: Shockwave - Effect projectile entities
            HashSet<Transform> hitRoots = new HashSet<Transform>();

            for(int i = 0; i < _hits; i++) {
                Collider collider = _hitColliders[i];

                if(hitRoots.Contains(collider.transform.root)) continue;
                else hitRoots.Add(collider.transform.root);

                Rigidbody rigidbody = collider.attachedRigidbody;
                GameObject rootObject = rigidbody ? rigidbody.gameObject : collider.gameObject;

                if(!NetworkServer.active) {
                    var networkIdentity = rootObject.GetComponent<NetworkIdentity>();

                    if(!networkIdentity || !networkIdentity.hasAuthority)
                        continue;
                }

                if(rigidbody == CurrentActorEntity.RigidbodyComponent) continue;
                if(collider.isTrigger)
                    if(!(collider.GetComponent<TriggerProjectile>() ||
                        collider.GetComponent<TriggerBehaviour>()))
                        continue;

                switch(CurrentLayoutElement.CurrentLevel) {
                    case 1:
                        DoForce(rigidbody, OriginalSkillExecutor.BaseForce, OriginalSkillExecutor.BaseRigidbodyForce);
                        break;
                    case 2:
                        DoForce(rigidbody, OriginalSkillExecutor.BaseForce * 2, OriginalSkillExecutor.BaseRigidbodyForce * 2);
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        DoForce(rigidbody, OriginalSkillExecutor.BaseForce * 2, OriginalSkillExecutor.BaseRigidbodyForce * 2);
                        DoDamage(collider);
                        break;
                }
            }
        }

        void Execute() {
            if(!IsReady) return;

            DoPush();
            CooldownTimer.Start(true);
            CameraSoundController.Current.PlaySound("Shockwave", true);
        }

        void GetColliders() {
            if(CurrentActorEntity.CurrentTransform)
                _hits = Physics.OverlapSphereNonAlloc(CurrentActorEntity.CurrentTransform.position, OriginalSkillExecutor.Radius, _hitColliders, OriginalSkillExecutor.HitMask, QueryTriggerInteraction.Collide);
        }

        //public override void InvokeSkillAuthority(Actor actor) {
        //    InvokeSkillNonAuthority(actor);
        //}

        //public override void InvokeSkillNonAuthority(Actor actor) {
        //    DoPush(actor); // TODO Is this needed?
        //    CooldownTimer.Start(true);
        //    OnInvoke?.Invoke();
        //}

        public override void InvokeSkillServer() {
            Execute();
            GiveSkillFeedback(false);
            OnInvoke?.Invoke();
        }

        public override void OnSkillFeedback(byte[] parameters) {
            CooldownTimer.Start(true);
            OnInvoke?.Invoke();
        }

        public void SkillUpdate() {
            CooldownTimer.Update();
            UpdateGraphicalRadius();
        }

        void UpdateGraphicalRadius() {
            if(!string.IsNullOrEmpty(OriginalSkillExecutor.GraphicsRadiusTransformPath))
                if(CurrentActorEntity.CurrentGameObject != null)
                    if(currentGraphicsTime != CooldownTimer.NormalizedTime)
                        if(CurrentActorEntity.CurrentTransform.Find(OriginalSkillExecutor.GraphicsRadiusTransformPath) is Transform radiusTransform) {
                            var time = 1 - CooldownTimer.NormalizedTime;
                            var radius = OriginalSkillExecutor.Radius * time;

                            currentGraphicsTime = CooldownTimer.NormalizedTime;
                            radiusTransform.localScale = Vector3.one * radius * 2;
                        }
        }
    }
}