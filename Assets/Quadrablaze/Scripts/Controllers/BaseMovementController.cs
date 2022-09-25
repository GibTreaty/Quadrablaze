using System;
using System.Collections;
using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class BaseMovementController : MonoBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        float _speed = 1;

        [SerializeField]
        float _speedMultiplier = 1;

        [SerializeField]
        float _acceleration = 100;

        [SerializeField]
        float _accelerationMultiplier = 1;

        [SerializeField, Range(0, 1)]
        float _slowDownRate = 1;

        [SerializeField]
        float _turnSpeed = 20;

        [SerializeField]
        float _angularDrag = 1;

        [SerializeField]
        float _pushRecoveryRate = 1;

        [SerializeField]
        SpeedSettings _movementSpeedSettings;

        [SerializeField]
        MovementType _movementStyle = MovementType.Directional;

        [SerializeField]
        bool _enableMovementUpdate = true;

        [SerializeField]
        bool _enableMovementOvertime = true;

        float _rotation;
        float _turnForce;
        Vector3 _moveToPosition;
        Rigidbody _rigidbodyComponent = null;
        Vector3 _movementDirection;
        Vector3 _pushDirection;

        float moveOvertimeEndTime;
        Vector3 moveTargetPosition;
        Vector3 moveTargetDirection;

        bool initialized;

        #region Properties
        public float Acceleration {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        public float AccelerationMultiplier {
            get { return _accelerationMultiplier; }
            set { _accelerationMultiplier = value; }
        }

        public float AngularDrag {
            get { return _angularDrag; }
            set { _angularDrag = value; }
        }

        public bool EnableMovementOvertime {
            get { return _enableMovementOvertime; }
            set { _enableMovementOvertime = value; }
        }

        public bool EnableMovementUpdate {
            get { return _enableMovementUpdate; }
            set { _enableMovementUpdate = value; }
        }

        public MovementType MovementStyle {
            get { return _movementStyle; }
            set { _movementStyle = value; }
        }

        Rigidbody RigidbodyComponent {
            get { return _rigidbodyComponent; }
            set { _rigidbodyComponent = value; }
        }

        public float Speed {
            get { return _speed; }
            set { _speed = value; }
        }

        public float SpeedMultiplier {
            get { return _speedMultiplier; }
            set { _speedMultiplier = value; }
        }

        public float SlowDownRate {
            get { return _slowDownRate; }
            set { _slowDownRate = value; }
        }

        public float TurnSpeed {
            get { return _turnSpeed; }
            set { _turnSpeed = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            RigidbodyComponent = entity.RigidbodyComponent;

            _moveToPosition = transform.position;
            _rotation = RigidbodyComponent.rotation.eulerAngles.y;
            initialized = true;
        }

        //void Update() {
        //    if(RigidbodyComponent)
        //        RigidbodyComponent.drag = Mathf.Clamp(RigidbodyComponent.velocity.magnitude * DragVelocityMultiplier, 0, Drag);
        //    //RigidbodyComponent.drag = DragMultiplierVelocityCurve.Evaluate(RigidbodyComponent.velocity.magnitude) * Drag;
        //}

        void FixedUpdate() {
            if(initialized)
                if(EnableMovementUpdate) {
                    UpdatePush();
                    UpdateMovement();
                    UpdateTurning();
                }
        }

        // TODO: Make sure this works as intended
        void HandleCollision(Collision collision) {
            //if(collision.gameObject.layer != LayerMask.NameToLayer("Small Projectile") && collision.gameObject.layer != LayerMask.NameToLayer("Large Projectile"))
            if(collision.rigidbody != null) {
                if(RigidbodyComponent == null) {
                    Debug.LogError($"[Base Movement] Rigidbody null - {name}");
                    return;
                }

                var direction = transform.position - collision.contacts[0].point;
                var massForce = collision.rigidbody.mass / RigidbodyComponent.mass; //TODO: Null reference exception 

                _pushDirection = direction * massForce;
            }
        }

        public void Move(Vector3 direction) {
            switch(MovementStyle) {
                case MovementType.Directional:
                    _movementDirection = direction;
                    break;

                case MovementType.Arcade:
                    _movementDirection = RigidbodyComponent.rotation * new Vector3(0, 0, direction.z);
                    _turnForce = Mathf.Clamp(direction.x, -1, 1);
                    break;
            }
        }

        public void MoveTo(Vector3 position) {
            _moveToPosition = position;
        }

        public void MoveToOvertime(float time, Vector3 targetPosition, float slowDownRate, float acceleration) {
            if(!EnableMovementOvertime) return;
            Debug.Log("MoveToOvertime");

            moveOvertimeEndTime = Time.time + time;
            moveTargetPosition = targetPosition;
            moveTargetDirection = targetPosition - transform.position;
        }

        public void PushDirection(Vector3 direction) {
            _pushDirection += direction;
        }

        void OnCollisionEnter(Collision collision) {
            HandleCollision(collision);
        }

        void OnCollisionStay(Collision collision) {
            HandleCollision(collision);
        }

        void OnDisable() {
            initialized = false;
        }

        public void PointAt(Vector3 point) {
            Vector3 direction = point - transform.position;
            direction.y = 0;

            if(direction != Vector3.zero)
                RigidbodyComponent.rotation = Quaternion.LookRotation(direction);
        }

        public void SetPushDirection(Vector3 direction) {
            _pushDirection = direction;
            RigidbodyComponent.AddForce(_pushDirection, ForceMode.Impulse);
        }

        public void SetVelocity(Vector3 direction) {
            RigidbodyComponent.velocity = direction;
        }

        void UpdatePush() {
            if(_pushDirection.sqrMagnitude > 0) {
                RigidbodyComponent.AddForce(_pushDirection, ForceMode.Acceleration);

                _pushDirection -= _pushDirection * _pushRecoveryRate * Time.deltaTime;
            }
        }

        void UpdateMovement() {
            var speedInDirection = Vector3.zero;

            if(_pushDirection.magnitude < _pushRecoveryRate)
                if(_movementDirection.magnitude == 0) {
                    if(_pushDirection.sqrMagnitude <= .01f) {
                        float stopSpeed = _movementSpeedSettings.stopSpeed;
                        var stopVector = -RigidbodyComponent.velocity;

                        stopVector.y = 0;

                        RigidbodyComponent.AddForce(stopVector * stopSpeed, ForceMode.Acceleration);
                    }
                }
                else {
                    float acceleration = _movementSpeedSettings.acceleration;

                    var movementDirection = _movementDirection;

                    #region Sideways friction
                    var angle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;

                    var velocity = RigidbodyComponent.velocity;

                    velocity.y = 0;

                    var velocityMagnitude = velocity.magnitude;
                    var angleSigned = Vector3.SignedAngle(movementDirection, velocity, Vector3.up);

                    var localVelocity = Quaternion.Euler(0, angleSigned, 0) * Vector3.forward * velocityMagnitude;

                    if(_movementSpeedSettings.sidewaysForceMultiplier > 0)
                        RigidbodyComponent.AddForce((Quaternion.Euler(0, angle + 90, 0) * Vector3.forward) * -localVelocity.x * _movementSpeedSettings.sidewaysForceMultiplier, ForceMode.Acceleration);
                    #endregion

                    float brake = 0;

                    if(Mathf.Abs(angleSigned) > 90) {
                        brake = _movementSpeedSettings.changeDirectionSlowdownSpeed;
                        acceleration = 0;
                    }

                    if(Mathf.Abs(localVelocity.z) >= _movementSpeedSettings.maxSpeed * SpeedMultiplier)
                        acceleration = 0;

                    var applyForce = movementDirection * (acceleration + (brake * Mathf.Sign(-localVelocity.z)));

                    RigidbodyComponent.AddForce(applyForce, ForceMode.Acceleration);
                }
        }

        void UpdateTurning() {
            if(MovementStyle == MovementType.Arcade) {
                _rotation += _turnForce;
                RigidbodyComponent.rotation = Quaternion.Euler(0, _rotation, 0);
            }
        }

        public static Vector3 GetForceToPosition(Vector3 position, Vector3 velocity, Vector3 targetPosition, float slowDownRate, float acceleration) {
            Vector3 velocityCompensation = new Vector3(velocity.x, 0, velocity.z) * Mathf.Clamp01(slowDownRate);
            Vector3 endPosition = (position + velocityCompensation);
            Vector3 targetDirection = targetPosition - endPosition;
            float targetModifiedDistance = targetDirection.magnitude;
            float isWithinVelocityRadius = 1 - Mathf.Clamp01((targetModifiedDistance / velocityCompensation.magnitude));
            Vector3 finalVector = Vector3.ClampMagnitude(targetDirection, 1);

            finalVector = finalVector * acceleration - Vector3.ClampMagnitude(velocityCompensation, isWithinVelocityRadius);

            return finalVector;
        }

        [Serializable]
        public struct SpeedSettings {
            public float acceleration;
            public float maxSpeed;
            public float stopSpeed;
            public float changeDirectionSlowdownSpeed;
            public float sidewaysForceMultiplier;
        }

        public enum MovementType {
            Directional = 0,
            Arcade = 1
        }
    }
}