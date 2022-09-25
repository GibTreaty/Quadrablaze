using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class DasherController : MonoBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill, ISpecialAbility, ITelegraphHandler {

        [SerializeField]
        float _nearDashSpeed = 10;

        [SerializeField]
        float _dodgeSpeed;

        [SerializeField]
        Transform _dashGlow;

        [SerializeField]
        int _dodgeLevel = 2;

        [SerializeField]
        int _moveLevel = 2;

        [SerializeField]
        GameObject _telegraphObject;

        [SerializeField]
        float _damageTimeAfterMove = 1;

        [SerializeField]
        DoDamageWhen _damageType = DoDamageWhen.Near;

        float _baseDashSpeed;
        float _baseTargetOffsetDistance;
        Vector3 moveDirection;

        float lastMoveTime;

        bool initialized;

        public bool isNear;
        public float distance;

        #region Properties
        CollisionDamage CollisionDamageComponent { get; set; }

        public DoDamageWhen DamageType {
            get { return _damageType; }
            set { _damageType = value; }
        }

        Dash DashExecutor { get; set; }

        public int DodgeLevel {
            get { return _dodgeLevel; }
            set { _dodgeLevel = value; }
        }

        public float DodgeSpeed {
            get { return _dodgeSpeed; }
            set { _dodgeSpeed = value; }
        }

        EnemyInput EnemyInputComponent { get; set; }

        public int MoveLevel {
            get { return _moveLevel; }
            set { _moveLevel = value; }
        }

        public float NearDashSpeed {
            get { return _nearDashSpeed; }
            set { _nearDashSpeed = value; }
        }

        NetworkAudio NetworkAudioComponent { get; set; }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            EnemyInputComponent = GetComponent<EnemyInput>();
            NetworkAudioComponent = GetComponent<NetworkAudio>();

            _baseTargetOffsetDistance = EnemyInputComponent.TargetOffsetDistance;

            CollisionDamageComponent = GetComponent<CollisionDamage>();
            CollisionDamageComponent.OnCollision.AddListener(() => {
                EnemyInputComponent.DelayRegularMovementTimer();
                EnemyInputComponent.DelayRandomMovementTimer();
            });

            SetTelegraphState(false);

            initialized = true;
        }

        void Update() {
            if(!initialized) return;

            if(NetworkServer.active)
                if(DashExecutor != null) {
                    if(CollisionDamageComponent.EnableDamage)
                        if(Time.time > lastMoveTime + _damageTimeAfterMove)
                            CollisionDamageComponent.EnableDamage = false;

                    if(EnemyInputComponent.Target) {
                        Vector3 direction = (EnemyInputComponent.Target.position - transform.position);
                        bool nearTarget = direction.sqrMagnitude <= _baseTargetOffsetDistance * _baseTargetOffsetDistance;

                        distance = direction.magnitude;
                        isNear = nearTarget;

                        DashExecutor.CurrentLayoutElement.CurrentLevel = nearTarget ? MoveLevel : DodgeLevel;
                        DashExecutor.Speed = nearTarget ? NearDashSpeed : _baseDashSpeed;
                        EnemyInputComponent.TargetOffsetDistance = nearTarget ? 0 : _baseTargetOffsetDistance;
                    }
                    else {
                        DashExecutor.CurrentLayoutElement.CurrentLevel = MoveLevel; //TODO: Null error
                        DashExecutor.Speed = _baseDashSpeed;
                        EnemyInputComponent.TargetOffsetDistance = _baseTargetOffsetDistance;
                    }
                }

            _dashGlow.localScale = Vector3.one * Mathf.Lerp(1, 2.35f, 1 - (EnemyInputComponent.RegularMovementTimer / EnemyInputComponent.RegularMovementDelay));
        }

        public void DodgeCollider(Collider collider) {
            if(EnemyInputComponent.TelegraphTimer == 0)
                if(collider.attachedRigidbody) {
                    Transform dodgeTarget = collider.attachedRigidbody.transform;
                    Vector3 direction = collider.attachedRigidbody.velocity.normalized;
                    float dot = Vector3.Dot((dodgeTarget.position - transform.position).normalized, Quaternion.Euler(0, 90, 0) * direction);
                    float dodgeSignedDirection = -Mathf.Sign(dot);
                    Vector3 dodgeDirection = Quaternion.Euler(0, 90 * dodgeSignedDirection, 0) * direction;

                    Debug.DrawRay(transform.position, dodgeDirection * 2, Color.red, 5);

                    DashExecutor.InvokeDash(dodgeDirection, DodgeSpeed, DodgeSpeed, DodgeLevel);
                }
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Dash dashExeuctor) {
                DashExecutor = dashExeuctor;

                element.CurrentLevel = MoveLevel;
                _baseDashSpeed = DashExecutor.Speed;

                DashExecutor.OnStartDash += OnStartDash;

                EnemyInputComponent.onTelegraphStart.AddListener(StartTelegraph);
                EnemyInputComponent.onTelegraphEnd.AddListener(EndTelegraph);
                EnemyInputComponent.onMovementInputChanged.AddListener(SetDashStuff);
                //EnemyInputComponent.onRandomMovementInput.AddListener(SetDashStuff);

                void SetDashStuff(Vector3 vector) {
                    moveDirection = vector;
                    EnemyInputComponent.DelayTelegraphTimer();
                }

                void StartTelegraph() {
                    if(_telegraphObject != null) {
                        SetTelegraphState(true);
                        TelegraphStateHandler.SendTelegraphState(gameObject, true);
                    }
                }

                void EndTelegraph() {
                    if(_telegraphObject != null) {
                        SetTelegraphState(false);
                        TelegraphStateHandler.SendTelegraphState(gameObject, false);
                    }

                    //float teleportDistance = DodgeSpeed;

                    if(EnemyInputComponent.Target != null) {
                        moveDirection = EnemyInputComponent.Target.position - transform.position;

                        //if(DodgeLevel >= 5 && DashExecutor.LayoutElement.CurrentLevel == DodgeLevel)
                        //    teleportDistance = Mathf.Max(moveDirection.magnitude - 2, 0);

                        moveDirection = moveDirection.normalized;

                        bool doDamageOnMove = true;

                        switch(_damageType) {
                            case DoDamageWhen.Near:
                                if(!isNear) doDamageOnMove = false;
                                break;

                            case DoDamageWhen.Far:
                                if(isNear) doDamageOnMove = false;
                                break;
                        }

                        if(doDamageOnMove) {
                            lastMoveTime = Time.time;
                            CollisionDamageComponent.EnableDamage = true;
                        }
                    }

                    DashExecutor.Direction = moveDirection;
                    //DashExecutor.TeleportDistance = teleportDistance;

                    DashExecutor.InvokeDash();
                }
            }

        }

        void OnDisable() {
            if(DashExecutor != null) {
                DashExecutor.OnStartDash -= OnStartDash;
                DashExecutor = null;
            }

            initialized = false;
        }

        void OnStartDash() {
            CollisionDamageComponent.ResetDamageDelayTimer();
            NetworkAudioComponent.Play();
            this.SendEnemyUsedSpecialAbility();
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            _telegraphObject.SetActive(enable);
        }

        public void UseSpecialAbility() {
            EnemyInputComponent.DelayRegularMovementTimer();
        }

        public enum DoDamageWhen {
            Any, Near, Far
        }
    }
}