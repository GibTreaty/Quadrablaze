using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class EnemyInput : ActorInputBase {

        [SerializeField, Tooltip("Finds a Global Pooled Target Controller on Awake")]
        string _globalTargetName;

        [SerializeField]
        GlobalTargetController _globalTarget;

        [SerializeField]
        Transform _target;

        [SerializeField]
        float _targetOffsetDistance;

        [SerializeField]
        float _distanceDamping = 1;

        [SerializeField]
        float _dampingRadius = .03f;

        [SerializeField]
        MoveType _moveStatus = MoveType.None;

        /// <summary>
        /// Changes the move status to Target if true. If false, it only changes to Target if it was set to None.
        /// </summary>
        [SerializeField]
        bool _changeMoveStatusIfTargetSelected = true;

        [SerializeField]
        float _telegraphLength = 1;

        [SerializeField]
        float _regularMovementDelay = 2;

        [SerializeField]
        float _randomMovementDelay = 2;

        [SerializeField]
        float _moveToPositionRange = .5f;

        [SerializeField]
        bool _pointAtTarget;

        [SerializeField]
        float _rubberbanding;

        public UnityEvent onReachedPosition;

        Vector3 _movementInput;
        float _telegraphTimer;
        float _regularMovementTimer;
        float _randomMovementTimer;
        bool _reachedPosition;

        bool initialized;

        public UnityEvent onTelegraphStart;
        public UnityEvent onTelegraphEnd;
        public Vector3Event onMovementInputChanged;
        public Vector3Event onRandomMovementInput;

        #region Properties
        BaseMovementController BaseMovementControllerComponent { get; set; }

        public bool ChangeMoveStatusIfTargetSelected {
            get { return _changeMoveStatusIfTargetSelected; }
            set { _changeMoveStatusIfTargetSelected = value; }
        }

        public GlobalTargetController GlobalTarget {
            get { return _globalTarget; }
            set {
                if(GlobalTarget == value) return;

                //if(!value && GlobalTarget) {
                //    GlobalTarget.onTargetChanged.RemoveListener(ChangeTarget);
                //}

                _globalTarget = value;

                if(GlobalTarget) {
                    ChangeTarget(GlobalTarget.Target);
                    GlobalTarget.onTargetChanged.AddListener(ChangeTarget);
                }
            }
        }

        public MoveType MoveStatus {
            get { return _moveStatus; }
            set {
                if(MoveStatus == value) return;

                _moveStatus = value;
                ResetRandomMovementTimer();
            }
        }

        public Vector3 MovementInput {
            get { return _movementInput; }

            private set {
                if(RegularMovementTimer > 0) return;
                if(RegularMovementDelay > 0) DelayRegularMovementTimer();

                _movementInput = value;

                if(onMovementInputChanged != null) onMovementInputChanged.Invoke(MovementInput);
            }
        }

        public float MoveToPositionRange {
            get { return _moveToPositionRange; }
            set { _moveToPositionRange = value; }
        }

        public float RandomMovementDelay {
            get { return _randomMovementDelay; }
            set { _randomMovementDelay = value; }
        }

        public float RandomMovementTimer {
            get { return _randomMovementTimer; }
            set { _randomMovementTimer = value; }
        }

        public float RegularMovementDelay {
            get { return _regularMovementDelay; }
            set { _regularMovementDelay = value; }
        }

        public float RegularMovementTimer {
            get { return _regularMovementTimer; }
            set { _regularMovementTimer = value; }
        }

        Rigidbody RigidbodyComponent { get; set; }

        public float Rubberbanding {
            get { return _rubberbanding; }
            set { _rubberbanding = value; }
        }

        public Transform Target {
            get { return GlobalTarget ? GlobalTarget.Target : _target; }
            set {
                if(Target == value) return;

                ChangeTarget(value);
            }
        }

        public Vector3 TargetMoveToPosition { get; set; }

        public float TargetOffsetDistance {
            get { return _targetOffsetDistance; }
            set { _targetOffsetDistance = value; }
        }

        EnemyTargetFinder TargetFinderComponent { get; set; }

        public float TelegraphLength {
            get { return _telegraphLength; }
            set { _telegraphLength = value; }
        }

        public float TelegraphTimer {
            get { return _telegraphTimer; }
            set { _telegraphTimer = value; }
        }
        #endregion

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            base.ActorEntityObjectInitialize(entity);

            BaseMovementControllerComponent = entity.BaseMovementControllerComponent;
            RigidbodyComponent = entity.RigidbodyComponent;
            TargetFinderComponent = GetComponent<EnemyTargetFinder>();

            if(TargetFinderComponent) {
                TargetFinderComponent.onTargetChanged.AddListener(ChangeTarget);
                //TargetFinderComponent.onTargetChanged.AddListener(target => { Target = target; Debug.Log($"Target == {Target != null}"); });
            }
            //if(!string.IsNullOrEmpty(_globalTargetName))
            //    GlobalTarget = GlobalTargetController.GetController(_globalTargetName);

            initialized = true;
        }

        //void Start() {
        //    if(!string.IsNullOrEmpty(_globalTargetName))
        //        GlobalTarget = GlobalTargetController.GetController(_globalTargetName);
        //}

        void Update() {
            if(initialized)
                SelectMoveType();
        }

        [ServerCallback]
        void FixedUpdate() {
            if(initialized)
                RubberbandBackToArena();
        }

        [ServerCallback]
        void LateUpdate() {
            if(initialized)

                if(IsImmobilized) return;

            if(_pointAtTarget && Target)
                BaseMovementControllerComponent.PointAt(Target.position);
        }

        void OnDrawGizmos() {
            if(MoveStatus == MoveType.MoveToPosition) {
                Gizmos.color = new Color(1, 1, 0, .6f);
                Gizmos.DrawLine(transform.position, TargetMoveToPosition);
            }

            //Gizmos.color = new Color(.2f, 1, .2f, .5f);
            //Gizmos.DrawSphere(transform.position, TargetOffsetDistance);
        }

        void ChangeTarget(Transform target) {
            _target = target;

            if(ChangeMoveStatusIfTargetSelected || MoveStatus == MoveType.None)
                MoveStatus = Target ? MoveType.Target : MoveType.None;
        }

        public void DelayRandomMovementTimer() {
            RandomMovementTimer = RandomMovementDelay;
        }

        public void DelayRegularMovementTimer() {
            RegularMovementTimer = RegularMovementDelay;
        }

        public void DelayTelegraphTimer() {
            if(TelegraphTimer != TelegraphLength)
                onTelegraphStart.InvokeEvent();

            TelegraphTimer = TelegraphLength;
        }

        public void MoveForwardStatus() {
            MovementInput = transform.forward;
        }

        public void MoveToPosition(Vector3 targetPosition) {
            TargetMoveToPosition = targetPosition;
            MoveStatus = MoveType.MoveToPosition;
            _reachedPosition = false;
        }

        void MoveToPositionStatus() {
            if(!_reachedPosition) {
                Vector3 direction = TargetMoveToPosition - transform.position;

                MovementInput = Vector3.ClampMagnitude(direction, 1);

                if(MoveToPositionRange == 0) return;

                if(direction.sqrMagnitude <= MoveToPositionRange * MoveToPositionRange) {
                    _reachedPosition = true;
                    MovementInput = Vector3.zero;

                    onReachedPosition.InvokeEvent();
                }
            }
        }

        public void RandomMove() {
            if(onRandomMovementInput != null) {
                var direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

                onRandomMovementInput.Invoke(direction);

                //onRandomMovementInput.Invoke(Vector3.Scale(Random.onUnitSphere, new Vector3(1, 0, 1)));
            }

            DelayRandomMovementTimer();
        }

        void RandomMoveStatus() {
            if(RandomMovementTimer > 0)
                RandomMovementTimer = Mathf.Max(RandomMovementTimer - Time.deltaTime, 0);

            if(RandomMovementTimer == 0) RandomMove();
        }

        public void ResetRandomMovementTimer() {
            RandomMovementTimer = 0;
        }

        public void ResetRegularMovementTimer() {
            RegularMovementTimer = 0;
        }

        public void ResetTelegraphTimer() {
            TelegraphTimer = 0;
        }

        void RubberbandBackToArena() {
            if(IsImmobilized) return;

            if(Rubberbanding > 0)
                if(RigidbodyComponent.position.magnitude > 60)
                    RigidbodyComponent.MovePosition(RigidbodyComponent.position - (Vector3.ClampMagnitude(RigidbodyComponent.position, 1) * Time.deltaTime * Rubberbanding));
        }

        void SelectMoveType() {
            if(IsImmobilized) return;

            if(TelegraphTimer > 0) {
                TelegraphTimer = Mathf.Max(TelegraphTimer - Time.deltaTime, 0);

                if(TelegraphTimer == 0)
                    onTelegraphEnd.InvokeEvent();
            }
            else {
                if(RegularMovementTimer > 0)
                    RegularMovementTimer = Mathf.Max(RegularMovementTimer - Time.deltaTime, 0);
            }

            if(NetworkServer.active)
                switch(MoveStatus) {
                    case MoveType.Random: RandomMoveStatus(); break;
                    case MoveType.Target: TargetMoveStatus(); break;
                    case MoveType.MoveToPosition: MoveToPositionStatus(); break;
                    case MoveType.MoveForward: MoveForwardStatus(); break;
                }
        }

        public void SetMoveForwardStatus() {
            MoveStatus = MoveType.MoveForward;
        }

        public void SetMoveToPositionStatus() {
            MoveStatus = MoveType.MoveToPosition;
        }

        public void SetRandomMoveStatus() {
            MoveStatus = MoveType.Random;
        }

        public void SetTargetMoveStatus() {
            MoveStatus = MoveType.Target;
        }

        void TargetMoveStatus() {
            if(Target) {
                Vector3 direction = Target.position - transform.position;

                if(TargetOffsetDistance > 0)
                    direction = (Target.position - (direction.sqrMagnitude > 0 ? direction.normalized : Vector3.up) * TargetOffsetDistance) - transform.position;

                MovementInput = Vector3.ClampMagnitude(direction, 1);
                //Debug.DrawRay(transform.position, MovementInput, Color.yellow);
            }
        }

        public enum MoveType {
            None = 0,
            Target = 1,
            Random = 2,
            MoveToPosition = 3,
            MoveForward = 4
        }
    }
}