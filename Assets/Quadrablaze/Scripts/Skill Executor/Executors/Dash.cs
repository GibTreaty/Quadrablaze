using System;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze.SkillExecutors {
    public class Dash : SkillExecutor, ICooldownTimer, ISkillUpdate, IMovementInterrupter {

        public event Action OnStartDash;

        RaycastHit teleportHit;

        Action onStartDashMethod;
        Action onEndDashMethod;

        float currentGraphicsTime;

        public EventTimer CooldownTimer { get; }
        public EventTimer DashTimer { get; }
        public int DashesInbetween { get; set; }
        public Vector3 Direction { get; set; }
        public bool InterruptMovement => IsDashing;
        public bool IsDashing => DashTimer > 0;
        public override bool IsReady => (CooldownTimer.CurrentTime == 0 || DashesInbetween < MaxDashes) && Direction.sqrMagnitude > 0;
        public int MaxDashes { get; set; }
        public new ScriptableDashSkillExecutor OriginalSkillExecutor { get; }
        public float Speed { get; set; }

        public Dash(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableDashSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            CooldownTimer = new EventTimer(OriginalSkillExecutor.CooldownDuration) { AutoDisable = true };
            DashTimer = new EventTimer(OriginalSkillExecutor.DashDuration) { AutoDisable = true };

            CooldownTimer.OnElapsed = new UnityEvent();
            CooldownTimer.OnElapsed.AddListener(EndCooldown);

            MaxDashes = OriginalSkillExecutor.MaxDashes;
            Speed = OriginalSkillExecutor.Speed;
        }

        public override void ApplyToGameObject(GameObject gameObject) {
            if(CurrentActorEntity is PlayerEntity) {
                onStartDashMethod = OnStart;
                onEndDashMethod = OnEnd;
            }

            currentGraphicsTime = -1;
            UpdateGraphicalRadius();
        }

        void EndCooldown() {
            CurrentActorEntity.BaseMovementControllerComponent.AccelerationMultiplier -= OriginalSkillExecutor.AccelerationMultiplierChange;
            onEndDashMethod?.Invoke();

            DashesInbetween = 0;
        }

        public void EndDash() {
            DashTimer.Reset();
        }

        public Vector3 GetDirectionFromType() {
            switch(OriginalSkillExecutor.DashDirection) {
                default: return Direction;
                case ScriptableDashSkillExecutor.DashDirectionType.TransformForward: return CurrentActorEntity.CurrentTransform.forward;
                case ScriptableDashSkillExecutor.DashDirectionType.Velocity: return CurrentActorEntity.RigidbodyComponent.velocity;
            }
        }

        public void ImmediateDash(Vector3 direction, float speed, float teleportDistance, int level) {
            if(direction.sqrMagnitude > 0) {
                if(DashesInbetween <= 0)
                    CurrentActorEntity.BaseMovementControllerComponent.AccelerationMultiplier += OriginalSkillExecutor.AccelerationMultiplierChange;

                DashesInbetween++;
                DashTimer.Reset();
                //CooldownTimer.Reset();
                CooldownTimer.Start(true);
                CurrentActorEntity.StartMovementInterrupt(DashTimer.Length);

                switch(level) {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        CurrentActorEntity.RigidbodyComponent.velocity = direction * speed;
                        //actor.RigidbodyComponent.velocity = Vector3.zero;
                        //actor.BaseMovementControllerComponent.PushDirection(direction * speed);
                        //actor.BaseMovementControllerComponent.MoveTo(actor.transform.position + direction * speed);
                        break;
                    case 5:
                    case 6:
                        Teleport(direction, teleportDistance);
                        break;
                }

                onStartDashMethod?.Invoke();
            }
        }

        public void InvokeDash() {
            InvokeDash(Direction, Speed, OriginalSkillExecutor.TeleportDistance, CurrentLayoutElement.CurrentLevel);
        }
        public void InvokeDash(Vector3 direction) {
            InvokeDash(direction, Speed, OriginalSkillExecutor.TeleportDistance, CurrentLayoutElement.CurrentLevel);
        }
        public void InvokeDash(Vector3 direction, float speed, float teleportDistance) {
            InvokeDash(direction, speed, teleportDistance, CurrentLayoutElement.CurrentLevel);
        }
        public void InvokeDash(Vector3 direction, float speed, float teleportDistance, int level) {
            if(CooldownTimer.CurrentTime == 0 || DashesInbetween < MaxDashes)
                ImmediateDash(direction, speed, teleportDistance, level);
        }

        public override void InvokeSkillAuthority() {
            InvokeDash();
        }

        public override void InvokeSkillNonAuthority() {
            DashesInbetween++;
            DashTimer.Reset();
            CooldownTimer.Reset();
            onStartDashMethod?.Invoke();
        }

        public override void InvokeSkillServer() {
            InvokeDash();
        }

        public override void LevelChanged(int level, int previousLevel) {
            if(CurrentLayoutElement.OriginalLayoutElement.SetSkillAmounts.Length > 0) {
                switch(level) {
                    default:
                        Speed = CurrentLayoutElement.OriginalLayoutElement.SetSkillAmounts[0].Amount; break;
                    case 2:
                        Speed = CurrentLayoutElement.OriginalLayoutElement.SetSkillAmounts[1].Amount; break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        Speed = CurrentLayoutElement.OriginalLayoutElement.SetSkillAmounts[2].Amount; break;
                }
            }

            switch(level) {
                default: MaxDashes = 1; break;
                case 4:
                case 6:
                    MaxDashes = 2; break;
            }
        }

        void OnEnd() {
            CurrentActorEntity.BaseMovementControllerComponent.EnableMovementOvertime = true;
        }

        void OnStart() {
            CurrentActorEntity.PlaySound("Dash", true);

            if(NetworkServer.active)
                CurrentActorEntity.GiveInvincibilityMinimum(0.2f);

            CurrentActorEntity.BaseMovementControllerComponent.EnableMovementOvertime = false;
        }

        //public void ResetSkill() {
        //    DashTimer.CurrentTime = 0;
        //    CooldownTimer.CurrentTime = 0;
        //    DashesInbetween = 0;
        //}

        //void SetDashVelocity() {
        //    SetDashVelocity(Direction, Speed);
        //}
        void SetDashVelocity(Vector3 direction, float speed) {
            if(direction.sqrMagnitude > 0)
                CurrentActorEntity.BaseMovementControllerComponent.SetVelocity(direction * speed);
        }

        public void SkillUpdate() {
            CooldownTimer.Update();
            DashTimer.Update();
            UpdateGraphicalRadius();
        }

        void Teleport(Vector3 direction, float teleportDistance) {
            if(direction.sqrMagnitude > 0) {
                direction *= teleportDistance;
                Vector3 teleportPosition = CurrentActorEntity.RigidbodyComponent.transform.position + direction;

                if(Physics.Raycast(CurrentActorEntity.CurrentTransform.position, direction, out teleportHit, direction.magnitude, OriginalSkillExecutor.TeleportMask, QueryTriggerInteraction.Ignore))
                    teleportPosition = teleportHit.point - (direction.normalized * CurrentActorEntity.Size);

                CurrentActorEntity.RigidbodyComponent.transform.position = teleportPosition;
                CurrentActorEntity.BaseMovementControllerComponent.MoveTo(teleportPosition);
            }
        }

        void UpdateGraphicalRadius() {
            if(!string.IsNullOrEmpty(OriginalSkillExecutor.GraphicsRadiusTransformPath))
                if(CurrentActorEntity.CurrentGameObject != null)
                    if(currentGraphicsTime != CooldownTimer.NormalizedTime)
                        if(CurrentActorEntity.CurrentTransform.Find(OriginalSkillExecutor.GraphicsRadiusTransformPath) is Transform radiusTransform) {
                            var time = DashesInbetween < MaxDashes ? 1 : 1 - CooldownTimer.NormalizedTime;
                            var radius = OriginalSkillExecutor.GraphicsRadius * time;

                            currentGraphicsTime = CooldownTimer.NormalizedTime;
                            radiusTransform.localScale = Vector3.one * radius * 2;
                        }
        }
    }
}