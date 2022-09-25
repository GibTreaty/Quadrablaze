using System;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class XPRing : NetworkBehaviour, IPoolInstantiate {
        [SerializeField]
        float _targetFindFrequency = 1;

        [SerializeField]
        float _pullDistance = 5;

        [SerializeField]
        [SyncVar(hook = "Sync_SetXP")]
        uint _xp;

        [SerializeField]
        SpriteRenderer _xpSprite;

        [SerializeField]
        AnimationCurveAsset _movementCurve;

        [SyncVar(hook = "Sync_SetTargetID")]
        NetworkInstanceId _targetId;

        [SyncVar(hook = "Sync_StartPosition")]
        Vector3 _startPosition;

        [SyncVar(hook = "Sync_MoveToPosition")]
        Vector2 _moveToPosition;

        GameObject _target;
        float _lastTargetFindTime;

        float _movementLerp;
        Vector3 _caughtPosition;

        PoolUser _user;

        bool _enableMove;

        #region Properties
        public uint XP {
            get { return _xp; }
            set { _xp = value; }
        }
        #endregion

        void Initialize() {

        }

        //public override void OnStartServer() {
        //    _startPosition = transform.position;
        //}

        //public override void OnStartClient() {
        //    Sync_StartPosition(_startPosition);
        //}

        void OnSpawn() {
            _movementLerp = 0;
            _lastTargetFindTime = Time.realtimeSinceStartup;
        }

        void OnDespawn() {
            _targetId = new NetworkInstanceId(0);
            _target = null;
            _xpSprite.enabled = false;
        }

        public void PoolInstantiate(PoolUser user) {
            Initialize();

            _user = user;

            user.OnSpawn.AddListener(OnSpawn);
            user.OnDespawn.AddListener(OnDespawn);
        }

        public void SetPosition(Vector3 position) {
            _startPosition = position;
            _moveToPosition = new Vector2(position.x, position.z);
        }

        void CatchXP() {
            if(NetworkServer.active) {
                GameManager.Current.AddXP(XP);
                _user.StartTimedDespawn(1);
            }

            XPParticleManager.Current?.PlayEffect(transform.position);

            _xpSprite.enabled = false;
            _enableMove = false;
        }

        void Sync_MoveToPosition(Vector2 position) {
            _moveToPosition = position;

            if(_moveToPosition != new Vector2(_startPosition.x, _startPosition.z))
                _enableMove = true;
        }

        void Sync_SetXP(uint xp) {
            _xp = xp;
        }

        void Sync_SetTargetID(NetworkInstanceId netId) {
            _targetId = netId;
            _target = _targetId.IsEmpty() ? null : ClientScene.FindLocalObject(_targetId);
            _caughtPosition = transform.position;
            _movementLerp = 0;
        }

        void Sync_StartPosition(Vector3 position) {
            _startPosition = position;
            transform.position = _startPosition;
            _xpSprite.enabled = true;

            _moveToPosition = new Vector2(position.x, position.z);
            _enableMove = false;
        }

        void Update() {
            if(!XPParticleManager.Current) return;

            // TODO Only update if sprite is enabled

            if(_target) {
                if(_movementLerp < 1) {
                    _movementLerp = Mathf.Min(_movementLerp + Time.deltaTime, 1);

                    transform.position = Vector3.Lerp(_caughtPosition, _target.transform.position, _movementCurve ? _movementCurve.Evaluate(_movementLerp) : _movementLerp);

                    if(_movementLerp == 1)
                        CatchXP();
                }
            }
            else {
                if(_enableMove) {
                    Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
                    Vector2 direction = _moveToPosition - currentPosition;
                    float distance = direction.magnitude;
                    var lerp = Vector2.MoveTowards(currentPosition, _moveToPosition, Time.deltaTime * Mathf.Max(2, distance));

                    transform.position = new Vector3(lerp.x, transform.position.y, lerp.y);

                    if(distance <= .05f)
                        _enableMove = false;
                }
            }

            if(NetworkServer.active) {
                if(!_enableMove && transform.position.magnitude > GameManager.Current.ArenaRadius - 1) {
                    _moveToPosition = Vector2.ClampMagnitude(new Vector2(transform.position.x, transform.position.z), GameManager.Current.ArenaRadius - 1.1f);
                    _enableMove = true;
                }

                if(_targetId.IsEmpty()) {
                    if(Time.realtimeSinceStartup - _lastTargetFindTime < _targetFindFrequency) return;

                    _lastTargetFindTime = Time.realtimeSinceStartup;

                    var attractionEntity = XPParticleManager.Current.GetFirstNearestAttractionTransform(transform.position, _pullDistance);

                    if(attractionEntity != null)
                        _targetId = attractionEntity.NetworkIdentityComponent.netId;
                }
                else {
                    if(ClientScene.FindLocalObject(_targetId) == null) {
                        _targetId = new NetworkInstanceId(0);
                        _target = null;
                    }

                    if(_target != null) {
                        //var targetObject = ClientScene.FindLocalObject(_targetId);

                        //if(targetObject == null || !targetObject.activeInHierarchy)
                        if(!_target.activeInHierarchy) {
                            _targetId = new NetworkInstanceId(0);
                            _target = null;
                        }
                    }
                }
            }
        }
    }
}