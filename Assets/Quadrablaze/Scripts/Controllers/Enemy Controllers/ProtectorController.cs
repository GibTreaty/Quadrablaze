using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.Entities;

//TODO: Protector doesn't use ActorEntity shizzel
//TODO: Make the target position go AROUND the target to protect rather than having the protector move straight into the target
namespace Quadrablaze {
    public class ProtectorController : MonoBehaviour, IActorEntityObjectInitialize {

        public const float DistanceFromPlayer = 16;
        public const float DistanceFromPlayerSqr = DistanceFromPlayer * DistanceFromPlayer;

        [SerializeField]
        Actor _targetToProtect;

        [SerializeField]
        float _targetPositionOffset;

        [SerializeField]
        EventTimer _findTargetTimer = new EventTimer(1);

        float detectionRadius;

        #region Properties
        EnemyInput ControllerInput { get; set; }

        BaseMovementController Movement { get; set; }

        public int ProtectEntityId { get; private set; }

        public float TargetPositionOffset {
            get { return _targetPositionOffset; }
            set { _targetPositionOffset = value; }
        }

        public Actor TargetToProtect {
            get { return _targetToProtect; }
            set { _targetToProtect = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            Movement = GetComponent<BaseMovementController>();
            ControllerInput = GetComponent<EnemyInput>();

            _findTargetTimer.OnElapsed.AddListener(UpdateFindTarget);

            detectionRadius = entity.Size;
        }

        void Update() {
            if(TargetToProtect && TargetToProtect.HealthGroupComponent.MainHealthLayer.HealthStatusAlive())
                UpdateProtecting();
            else
                _findTargetTimer.Update();
        }

        void UpdateProtecting() {
            //if(PlayerSpawnManager.Current.CurrentPlayerEntityId > 0) {
            //    var playerEntity = PlayerSpawnManager.Current.GetCurrentEntity();

            //    //var playerEntity = PlayerSpawnManager.IsPlayerAlive ? PlayerSpawnManager.Current.CurrentPlayerEntity : null;

            //    //if(playerEntity != null) return;

            //    Vector3 directionToPlayer = playerEntity.CurrentTransform.position - TargetToProtect.transform.position;
            //    Vector3 directionToPlayerNormalized = directionToPlayer.normalized;

            //    Vector3 goToPosition = TargetToProtect.transform.position + (directionToPlayerNormalized * (TargetToProtect.Size + detectionRadius + TargetPositionOffset));

            //    //Movement.Move(goToPosition - transform.position);
            //    ControllerInput.MoveToPosition(goToPosition);
            //}
        }

        void OnDrawGizmos() {
            if(TargetToProtect) {
                Gizmos.color = new Color(1, .5f, .5f);
                Gizmos.DrawLine(transform.position, TargetToProtect.transform.position);
            }
        }

        void UpdateFindTarget() {
            //float distance = Mathf.Infinity;
            //TODO: Fix this if this enemy is going to exist in the game
            //Actor targetActor = null;

            //foreach(var actor in FindObjectsOfType<Actor>()) {
            //    switch(actor.ActorType) {
            //        case ActorTypes.SmallEnemy:
            //        case ActorTypes.MediumEnemy:
            //            //if(!actor.name.Contains("Protector") && !actor.name.Contains("Dasher")) {
            //            if(!actor.name.Contains("Dasher")) {
            //                float tempDistance = Vector3.Distance(transform.position, actor.transform.position);

            //                if(tempDistance < distance) {
            //                    distance = tempDistance;
            //                    targetActor = actor;
            //                }
            //            }

            //            break;
            //    }

            //}

            //TargetToProtect = targetActor;
            _findTargetTimer.Reset(false);
        }
    }
}