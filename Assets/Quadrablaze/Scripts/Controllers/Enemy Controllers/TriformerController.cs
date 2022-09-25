using System;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    [RequireComponent(typeof(EnemyInput))]
    public class TriformerController : NetworkBehaviour, IActorEntityObjectInitialize {

        static TriformerController[] trianglePoints = new TriformerController[3];
        static TriformerForceFieldController _forceFieldController;
        static bool _allNearTrianglePoints;
        static bool _trianglePointsFull;
        static Transform _currentTarget = null;
        static int _spawnedTriformers = 0;

        public const float DistanceFromPlayer = 10;
        public const float DistanceFromPlayerSqr = DistanceFromPlayer * DistanceFromPlayer;

        public const float DistanceFromTrianglePoint = 2;
        public const float DistanceFromTrianglePointSqr = DistanceFromTrianglePoint * DistanceFromTrianglePoint;

        public UnityEvent OnForceFieldStart;
        public UnityEvent OnForceFieldEnd;
        public UnityEvent OnForceFieldEndPlayerDeath;

        GlobalPlayerTarget _playerTarget;
        UnityAction<PlayerEntity> onPlayerDeathMethod;

        bool initialize;

        #region Properties
        public static bool AllNearTrianglePoints {
            get { return _allNearTrianglePoints; }
            private set {
                if(AllNearTrianglePoints == value) return;

                _allNearTrianglePoints = value;

                ForceFieldController.EnableForceField(_allNearTrianglePoints);
            }
        }

        EnemyInput ControllerInput { get; set; }

        public static TriformerForceFieldController ForceFieldController {
            get {
                if(!_forceFieldController)
                    _forceFieldController = FindObjectOfType<TriformerForceFieldController>();

                return _forceFieldController;
            }
        }

        public bool IsInTriangle {
            get { return TriangleIndex > -1; }
            private set { if(!value) TriangleIndex = -1; }
        }

        int TriangleIndex {
            get; set;
        }

        public static bool TrianglePointsFull {
            get { return _trianglePointsFull; }
            private set {
                if(TrianglePointsFull == value) return;

                _trianglePointsFull = value;

                var controller1 = ForceFieldController.Hook1.GetComponent<TriformerController>();
                var controller2 = ForceFieldController.Hook2.GetComponent<TriformerController>();
                var controller3 = ForceFieldController.Hook3.GetComponent<TriformerController>();

                if(TrianglePointsFull) {
                    controller1.OnForceFieldStart.Invoke(); // TODO: Null error here
                    controller2.OnForceFieldStart.Invoke();
                    controller3.OnForceFieldStart.Invoke();
                }
                else {
                    if(PlayerSpawnManager.IsPlayerAlive) {
                        controller1.OnForceFieldEnd.Invoke();
                        controller2.OnForceFieldEnd.Invoke();
                        controller3.OnForceFieldEnd.Invoke();
                    }
                    else {
                        controller1.OnForceFieldEndPlayerDeath.Invoke();
                        controller2.OnForceFieldEndPlayerDeath.Invoke();
                        controller3.OnForceFieldEndPlayerDeath.Invoke();
                    }
                }
            }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            if(!initialize) {
                ControllerInput = GetComponent<EnemyInput>();
                onPlayerDeathMethod = OnPlayerDeath;
            }

            _playerTarget = GlobalTargetController.GetController("Triformer Player Target") as GlobalPlayerTarget;

            PlayerProxy.Proxy.Subscribe(EntityActions.Death, PlayerProxy_Death);

            TriangleIndex = -1;

            if(_spawnedTriformers == 0)
                ForceFieldController.EnableForceField(false);

            //_spawnedTriformers++;

            initialize = true;
        }

        void PlayerProxy_Death(EventArgs args) {
            onPlayerDeathMethod(((EntityArgs)args).GetEntity<PlayerEntity>());
        }

        public void AddToTriangle() {
            for(int i = 0; i < 3; i++)
                if(!trianglePoints[i]) {
                    AddToTriangle(i);
                    break;
                }
        }
        public void AddToTriangle(int index) {
            if(TriangleIndex == -1 && !trianglePoints[index]) {
                trianglePoints[index] = this;
                TriangleIndex = index;
                IsInTriangle = true;

                switch(index) {
                    case 0: ForceFieldController.Hook1 = transform; break;
                    case 1: ForceFieldController.Hook2 = transform; break;
                    case 2: ForceFieldController.Hook3 = transform; break;
                }

                UpdateTriangleFull();
            }
        }

        public static Vector3 GetCenterOfTriangle() {
            if(TrianglePointsFull) {
                Vector3 a = trianglePoints[0].transform.position;
                Vector3 b = trianglePoints[1].transform.position;
                Vector3 c = trianglePoints[2].transform.position;

                return (a + b + c) / 3;
            }

            return Vector3.zero;
        }

        public Vector3 GetNearestPointOnTriangle(Vector3 position) {
            if(TrianglePointsFull && _currentTarget) {
                Vector3 playerPosition = _currentTarget.position;

                Vector3 a = trianglePoints[0].transform.position;
                Vector3 b = trianglePoints[1].transform.position;
                Vector3 c = trianglePoints[2].transform.position;

                Vector3 center = (a + b + c) / 3;
                Vector3 localPosition = position - center;

                Vector3 directionA = (a - center).normalized;
                float bSide = Mathf.Sign(Vector3.Dot(Quaternion.Euler(0, 90, 0) * directionA, b - a));

                //switch(TriangleIndex) {
                //    case 0: return center + (directionA * DistanceFromPlayer);
                //    case 1: return center + (Quaternion.Euler(0, 120 * bSide, 0) * directionA * DistanceFromPlayer);
                //    case 2: return center + (Quaternion.Euler(0, 120 * -bSide, 0) * directionA * DistanceFromPlayer);
                //}

                switch(TriangleIndex) {
                    case 0: return playerPosition + (directionA * DistanceFromPlayer);
                    case 1: return playerPosition + (Quaternion.Euler(0, 120 * bSide, 0) * directionA * DistanceFromPlayer);
                    case 2: return playerPosition + (Quaternion.Euler(0, 120 * -bSide, 0) * directionA * DistanceFromPlayer);
                }
            }

            return position;
        }

        void MoveToTrianglePoint() {
            Vector3 toPoint = GetNearestPointOnTriangle(transform.position);

            Debug.DrawLine(transform.position, toPoint, Color.red, 0, false);
            ControllerInput.MoveToPosition(toPoint);
        }

        void OnDisable() {
            _spawnedTriformers--;

            if(_spawnedTriformers == 0)
                _currentTarget = null;

            RemoveAllFromTriangle();
            ForceFieldController?.EnableForceField(false);

            PlayerProxy.Proxy.Unsubscribe(EntityActions.Death, PlayerProxy_Death);
        }

        void OnEnable() {
            _spawnedTriformers++;
        }

        void OnPlayerDeath(PlayerEntity entity) {
            if(entity.CurrentGameObject == null || entity.CurrentTransform == _currentTarget)
                _currentTarget = null;

            if(PlayerProxy.Players.Count == 0)
                RemoveFromTriangle();
        }

        public void RemoveAllFromTriangle() {
            for(int i = 0; i < 3; i++)
                if(trianglePoints[i] != null)
                    trianglePoints[i].RemoveFromTriangle();

            if(ForceFieldController != null) {
                ForceFieldController.Hook1 = null;
                ForceFieldController.Hook2 = null;
                ForceFieldController.Hook3 = null;
            }
        }

        //static void RemoveFromTriangle(int index) {
        //    if(trianglePoints[index])
        //        trianglePoints[index].RemoveFromTriangle();
        //}

        public void RemoveFromTriangle() {
            if(IsInTriangle) {
                trianglePoints[TriangleIndex] = null;

                IsInTriangle = false;
                UpdateTriangleFull();
                AllNearTrianglePoints = false;
            }
        }

        [ServerCallback]
        void Update() {
            if(!initialize) return;

            if(!_currentTarget) {
                _playerTarget.RandomizeTarget();
                _currentTarget = _playerTarget.Target;
            }

            if(_currentTarget && ControllerInput.Target != _currentTarget)
                ControllerInput.Target = _currentTarget;

            if(_spawnedTriformers >= 3) {
                if(!TrianglePointsFull)
                    if(_currentTarget)
                        if((transform.position - _currentTarget.position).sqrMagnitude <= DistanceFromPlayerSqr)
                            AddToTriangle();

                if(IsInTriangle && TrianglePointsFull) {
                    if(!AllNearTrianglePoints)
                        UpdateAllNearTriangles();

                    MoveToTrianglePoint();
                }
            }
        }

        static void UpdateTriangleFull() {
            bool full = true;
            //Debug.Log("UpdateTriangleFull");

            //for(int i = 0; i < 3; i++) {
            //    Debug.LogFormat("i={0} {1}", i, trianglePoints[i] == null ? "null" : "exists");
            //}

            for(int i = 0; i < 3; i++)
                if(!trianglePoints[i]) {
                    full = false;
                    break;
                }

            TrianglePointsFull = full;
        }

        static void UpdateAllNearTriangles() {
            bool all = true;

            if(TrianglePointsFull) {
                for(int i = 0; i < 3; i++) {
                    Vector3 toPoint = trianglePoints[i].GetNearestPointOnTriangle(trianglePoints[i].transform.position);

                    if((trianglePoints[i].transform.position - toPoint).sqrMagnitude > DistanceFromTrianglePointSqr) {
                        all = false;

                        break;
                    }
                }
            }
            else {
                all = false;
            }

            AllNearTrianglePoints = all;
        }
    }
}