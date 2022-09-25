using System;
using System.Collections;
using System.Collections.Generic;
using Quadrablaze.Entities;
using StatSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using YounGenTech.EnergyBasedObjects;
using YounGenTech.PoolGen;

namespace Quadrablaze.Boss {
    public class TriClone : BossController {

        static int MinionDatabaseIndex = 0;

        [SerializeField]
        ScriptableMinionEntity _minionEntityAsset;

        [SerializeField]
        Module[] _modules = new Module[3];

        [SerializeField]
        EventTimer _moveTimer = new EventTimer(1) { Active = true };

        [SerializeField]
        EventTimer _shootTimer = new EventTimer(1) { Active = true };

        [SerializeField, ColorUsage(true, true)]
        Color _enabledShieldColor;

        [SerializeField, ColorUsage(false, true)]
        Color _enabledGlowColor;

        [SerializeField, ColorUsage(false, true)]
        Color _disabledGlowColor;

        [SerializeField]
        string _poolCloneName = "Enemy";
        PoolManager _clonePool;

        [SerializeField]
        ObjectShootingPoint _cloneGunController;

        [SerializeField]
        int _clonePoolID = 12;
        int _clonePoolIndex = -1;

        [Header("Movement")]
        [SerializeField]
        int _targetPositions = 3;

        [SerializeField]
        float _distance = 19.7f;

        [SerializeField]
        float _nearTargetDistance = .5f;

        [SerializeField]
        float _acceleration = 100;

        [SerializeField]
        float _speed = 1;

        [SerializeField, Range(0, 1)]
        float _slowDownRate = 1;

        [SerializeField]
        float _minionSpawnDelay = 5;

        [SerializeField]
        bool isNearTargetPosition;

        [SerializeField]
        GameObject[] _hitSpots = new GameObject[3];

        UnityAction onMinionDeath;

        Vector3 _targetPosition;
        int _targetPositionIndex;
        bool _spawnedMinion;
        float lastMinionDeathTime;

        RodMask _downedRods = RodMask.None;
        //Actor currentSpawnedMinion;
        int minionEntityId;

        #region Properties
        BaseMovementController BaseMovementControllerComponent { get; set; }
        public Module[] Modules {
            get { return _modules; }
            set { _modules = value; }
        }
        Rigidbody RigidbodyComponent { get; set; }
        public Vector3 TargetPosition {
            get { return GetPosition(_targetPositionIndex); }
        }
        PoolUser UserComponent { get; set; }
        #endregion

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            if(initialized) return;

            BaseInitialize(entity);

            Debug.Log("TriClone Init");

            BaseMovementControllerComponent = entity.BaseMovementControllerComponent;
            RigidbodyComponent = entity.RigidbodyComponent;

            _clonePool = PoolManager.GetPool(_poolCloneName);

            UserComponent = entity.UserComponent;

            for(int i = 0; i < Modules.Length; i++) {
                Module module = Modules[i];
                int moduleIndex = i;

                module.AttachedTriClone = this;
                module.Initialize();

                module.OutsideShieldPropertyBlock.SetColor("_PrimaryColor", _enabledShieldColor);
                module.OutsideShieldRenderer.SetPropertyBlock(module.OutsideShieldPropertyBlock);
            }

            _moveTimer.OnElapsed.AddListener(() => {
                GoToNextTarget();
                _shootTimer.Reset();

                if(_clonePool.GetSpawnedObjectCount(MinionDatabaseIndex) == 0)
                    _spawnedMinion = false;
            });

            onMinionDeath = OnMinionDeath;

            if(MinionDatabaseIndex == 0) {
                var gameMode = GameManager.Current.CurrentGameMode;

                if(gameMode is IEnemySpawnController spawnController)
                    MinionDatabaseIndex = GameManager.Current.MinionDatabase.Entities.IndexOf(_minionEntityAsset);
                //MinionDatabaseIndex = spawnController.CurrentEnemyController.EnemyDatabase.Entities.IndexOf(_minionEntityAsset);
            }

            initialized = true;
        }

        void FixedUpdate() {
            RigidbodyComponent.angularVelocity = new Vector3(0, 1, 0);

            var direction = TargetPosition - RigidbodyComponent.position;
            var distance = direction.magnitude;

            distance = Mathf.Max(distance - _nearTargetDistance, 0);
            direction = Vector3.ClampMagnitude(direction, 1) * distance;

            BaseMovementControllerComponent.Move(direction);
        }

        public override void OnDamaged(StatEvent statEvent) {
            if(statEvent.AffectedStat.Value <= 0) {
                var id = statEvent.AffectedStat.Id;
                var module = _modules[id];

                UserComponent.SpawnHere("Explosions", module.PowerRodCollider.transform.position);
                SetRodDisabledNetworked(id);
            }
        }

        void OnDisable() {
            foreach(var hitSpot in _hitSpots)
                EnemyProxy.Targets.Remove(hitSpot.transform);

            _spawnedMinion = false;
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            Gizmos.color = Color.magenta * Color.blue;

            for(int i = 0; i < _targetPositions; i++) {
                var position = GetPosition(i);
                Gizmos.DrawWireCube(position, new Vector3(.25f, .25f, .25f) * UnityEditor.HandleUtility.GetHandleSize(position));
            }
        }
#endif

        protected override void OnStage(int stage) {
            switch(stage) {
                case 1:
                    GameDebug.Log("TriClone StartStage1", "Boss");

                    RigidbodyComponent.velocity = Vector3.zero;
                    RigidbodyComponent.angularVelocity = Vector3.zero;

                    _targetPositionIndex = GetFurtherestTargetFromPlayer();

                    BossObject.transform.SetPositionAndRotation(GetPosition(_targetPositionIndex), Quaternion.identity);

                    _downedRods = RodMask.None;

                    _moveTimer.Reset();
                    _shootTimer.Reset();
                    _spawnedMinion = false;
                    minionEntityId = 0;
                    _cloneGunController.AutoShoot = true;

                    break;

                case 2:
                case 3:
                    GoToNextTarget();

                    _moveTimer.Reset();
                    _shootTimer.Reset();
                    _spawnedMinion = false;

                    break;
            }
        }

        protected override void OnNetworkStageState(int stage) {
            switch(stage) {
                case 1:
                    SetRodEnabled(0, true, 0);
                    SetRodEnabled(1, true, 0);
                    SetRodEnabled(2, true, 0);

                    SetGunEnabled(0, true, 0);
                    SetGunEnabled(1, true, 0);
                    SetGunEnabled(2, true, 0);

                    SetShieldEnabled(0, true, 0);
                    SetShieldDisabled(0);
                    SetShieldEnabled(1, true, 0);
                    SetShieldEnabled(2, true, 0);
                    break;

                case 2: SetShieldDisabled(1); break;
                case 3: SetShieldDisabled(2); break;
            }
        }

        public override void OnStageUpdate(int stage) {
            bool nearTarget = (TargetPosition - BossObject.transform.position).sqrMagnitude <= _nearTargetDistance;
            int spawnedCount = _clonePool.GetSpawnedObjectCount(MinionDatabaseIndex);

            isNearTargetPosition = nearTarget;

            if(nearTarget) {
                _moveTimer.Update();

                if(Time.time > lastMinionDeathTime + _minionSpawnDelay)
                    if(spawnedCount == 0 && !_spawnedMinion) {
                        _shootTimer.Update();

                        if(_shootTimer.HasElapsed)
                            foreach(var module in Modules)
                                if(module.IsInFront(Vector3.zero))
                                    if(Mathf.Abs(module.GetPointingAngle(Vector3.zero)) < .1f) {
                                        _spawnedMinion = true;
                                        module.Shoot();
                                        break;
                                    }
                    }
            }
        }

        void ApplyColor(ColorFader options) {
            //options.renderer.material.SetColor(options.colorProperty, options.toColor);
            options.propertyBlock.SetColor(options.colorProperty, options.toColor);
            options.renderer.SetPropertyBlock(options.propertyBlock);
        }

        int CountDeadRods() {
            int count = 0;

            if((_downedRods & RodMask.Rod1) != RodMask.None) count++;
            if((_downedRods & RodMask.Rod2) != RodMask.None) count++;
            if((_downedRods & RodMask.Rod3) != RodMask.None) count++;

            return count;
        }

        IEnumerator FadeColor(ColorFader options) {
            ColorFader startOptions = options;
            Color fromColor = startOptions.propertyBlock.GetColor(options.colorProperty);
            //Color fromColor = startOptions.renderer.material.GetColor(options.colorProperty);
            float time = options.time;

            if(time == 0)
                ApplyColor(options);
            else
                while(time > 0) {
                    time = Mathf.Max(time - Time.deltaTime, 0);
                    startOptions.propertyBlock.SetColor(options.colorProperty, Color.LerpUnclamped(options.toColor, fromColor, time / options.time));
                    startOptions.renderer.SetPropertyBlock(startOptions.propertyBlock);
                    //startOptions.renderer.material.SetColor(options.colorProperty, Color.LerpUnclamped(options.toColor, fromColor, time / options.time));

                    yield return new WaitForEndOfFrame();
                }
        }

        public int GetClosestTarget() {
            int index = 0;
            float lowestDistance = Mathf.Infinity;

            for(int i = 0; i < _targetPositions; i++) {
                float distance = Vector3.Distance(GetPosition(i), BossObject.transform.position);

                if(distance < lowestDistance) {
                    index = i;
                    lowestDistance = distance;
                }
            }

            return index;
        }

        public int GetFurtherestTargetFromPlayer() {
            int index = 0;
            float highestDistance = 0;

            if(PlayerSpawnManager.Current.CurrentPlayerEntityId > 0) {
                var playerEntity = PlayerSpawnManager.Current.GetCurrentEntity();

                for(int i = 0; i < _targetPositions; i++) {
                    float distance = Vector3.Distance(GetPosition(i), playerEntity.CurrentTransform.position);

                    if(distance > highestDistance) {
                        index = i;
                        highestDistance = distance;
                    }
                }
            }

            return index;
        }

        public override GameObject[] GetHitSpots() {
            return _hitSpots;
        }

        public Vector3 GetPosition(int index) {
            var angle = (360f / _targetPositions) * index;

            return Quaternion.Euler(0, angle, 0) * new Vector3(0, 0, _distance);
        }

        public void GoToNextTarget() {
            _targetPositionIndex++;
            _targetPositionIndex %= _targetPositions;
        }

        void OnMinionDeath() {
            if(minionEntityId != 0) {
                _shootTimer.Reset();
                //TODO: Fix this
                //currentSpawnedMinion.OnDeath.RemoveListener(onMinionDeath);
                //currentSpawnedMinion = null;
                minionEntityId = 0;

                lastMinionDeathTime = Time.time;
            }
        }

        public void RodDown(int index) {
            switch(index) {
                case 0: _downedRods |= RodMask.Rod1; break;
                case 1: _downedRods |= RodMask.Rod2; break;
                case 2: _downedRods |= RodMask.Rod3; break;
            }

            if(_downedRods == RodMask.All)
                Defeat();
            else
                StageUp();
        }

        public void SetGunDisabled(int index) {
            SetGunEnabled(index, false);
        }

        public void SetGunEnabled(int index) {
            SetGunEnabled(index, true);
        }
        public void SetGunEnabled(int index, bool value, float time = .5f) {
            Color color = value ? _enabledGlowColor : _disabledGlowColor;

            //Modules[index].CloneGun.autoLoadOnShoot = value;
            Modules[index].CloneGun.Force = new Vector3(0, 0, GameManager.Current.ArenaRadius);

            if(gameObject.activeInHierarchy)
                StartCoroutine("FadeColor", new ColorFader(Modules[index].GunRenderer, "_GlowColor", time, color, Modules[index].GunPropertyBlock));
        }

        public void SetRodDisabled(int index) {
            GameDebug.Log("Disabled " + index, "Power Rod");
            SetRodEnabled(index, false);
        }

        void SetRodDisabledNetworked(int index) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_TriCloneState);
            writer.Write(TriCloneState.RodDisabled);
            writer.Write(gameObject);
            writer.Write(index);
            writer.FinishMessage();

            foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
                connection.SendWriter(writer, Channels.DefaultReliable);
        }

        public void SetRodEnabled(int index) {
            SetRodEnabled(index, true);
        }
        public void SetRodEnabled(int index, bool value, float time = .5f) {
            Color color = value ? _enabledGlowColor : _disabledGlowColor;

            if(!value)
                RodDown(index);

            if(gameObject.activeInHierarchy)
                StartCoroutine("FadeColor", new ColorFader(Modules[index].PowerRodRenderer, "_GlowColor", time, color, Modules[index].PowerRodPropertyBlock));
        }

        public void SetShieldDisabled(int index) {
            SetShieldEnabled(index, false);
        }

        public void SetShieldEnabled(int index) {
            SetShieldEnabled(index, true);
        }
        public void SetShieldEnabled(int index, bool value, float time = .5f) {
            Color color = value ? _enabledShieldColor : Color.clear;

            if(value) EnemyProxy.Targets.Remove(_hitSpots[index].transform);
            else EnemyProxy.Targets.Add(_hitSpots[index].transform);

            if(gameObject.activeInHierarchy) {
                Modules[index].PowerRodCollider.enabled = !value;
                Modules[index].ShieldCollider.enabled = value;

                StartCoroutine("FadeColor", new ColorFader(Modules[index].ShieldRenderer, "_PrimaryColor", time, color, Modules[index].ShieldPropertyBlock));
            }
        }

        GameObject SpawnClone(Vector3 position, Quaternion rotation) {
            //GameObject clone = _clonePool.Spawn(_clonePoolIndex, position, Quaternion.identity).gameObject;

            //currentSpawnedMinion = clone.GetComponent<Actor>();
            //currentSpawnedMinion.OnDeath.AddListener(onMinionDeath);

            //return clone;
            //return null;

            //TODO: Do ActorEntity-related shiz here
            var gameMode = GameManager.Current.CurrentGameMode;

            if(gameMode is IEnemySpawnController spawnController) {
                var gameObject = spawnController.CurrentEnemyController.SpawnEnemy(MinionDatabaseIndex, position);

                if(gameObject.GetComponent<Rigidbody>() is Rigidbody rigidbody)
                    rigidbody.AddForce(-position.normalized * 20, ForceMode.VelocityChange);

                return gameObject;
            }

            return null;
        }

        [RegisterNetworkHandlers]
        public static void RegisterTriCloneHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_TriCloneState, NetworkTriCloneState);
        }

        static void NetworkTriCloneState(NetworkMessage networkMessage) {
            var state = networkMessage.reader.ReadByte();
            var gameObject = networkMessage.reader.ReadGameObject();

            if(gameObject == null) return;

            var triClone = gameObject.GetComponent<TriClone>();

            switch(state) {
                case TriCloneState.RodDisabled:
                    var index = networkMessage.reader.ReadInt32();

                    triClone.SetRodDisabled(index);
                    break;
            }
        }

        [Serializable]
        public class Module {
            [SerializeField]
            Renderer _powerRodRenderer;

            [SerializeField]
            Health _powerRodHealth;

            [SerializeField]
            Collider _powerRodCollider;

            [SerializeField]
            Renderer _gunRenderer;

            [SerializeField]
            GameObject _shield;

            [SerializeField]
            Renderer _shieldRenderer;

            [SerializeField]
            Collider _shieldCollider;

            [SerializeField]
            GameObject _outsideShield;

            [SerializeField]
            Renderer _outsideShieldRenderer;

            [SerializeField]
            ObjectSpawnerPusher _cloneGun;

            [SerializeField]
            Transform _shootingOrigin;

            #region Properties
            public TriClone AttachedTriClone { get; set; }

            public ObjectSpawnerPusher CloneGun {
                get { return _cloneGun; }
                set { _cloneGun = value; }
            }

            public MaterialPropertyBlock GunPropertyBlock { get; set; }

            public Renderer GunRenderer {
                get { return _gunRenderer; }
                set { _gunRenderer = value; }
            }

            public GameObject OutsideShield {
                get { return _outsideShield; }
                set { _outsideShield = value; }
            }

            public MaterialPropertyBlock OutsideShieldPropertyBlock { get; set; }

            public Renderer OutsideShieldRenderer {
                get { return _outsideShieldRenderer; }
                set { _outsideShieldRenderer = value; }
            }

            public Collider PowerRodCollider {
                get { return _powerRodCollider; }
                set { _powerRodCollider = value; }
            }

            public Health PowerRodHealth {
                get { return _powerRodHealth; }
                set { _powerRodHealth = value; }
            }

            public MaterialPropertyBlock PowerRodPropertyBlock { get; set; }

            public Renderer PowerRodRenderer {
                get { return _powerRodRenderer; }
                set { _powerRodRenderer = value; }
            }

            public GameObject Shield {
                get { return _shield; }
                set { _shield = value; }
            }

            public Collider ShieldCollider {
                get { return _shieldCollider; }
                set { _shieldCollider = value; }
            }

            public MaterialPropertyBlock ShieldPropertyBlock { get; set; }

            public Renderer ShieldRenderer {
                get { return _shieldRenderer; }
                set { _shieldRenderer = value; }
            }

            public Transform ShootingOrigin => _shootingOrigin;
            #endregion

            public void Initialize() {
                GunPropertyBlock = new MaterialPropertyBlock();
                OutsideShieldPropertyBlock = new MaterialPropertyBlock();
                PowerRodPropertyBlock = new MaterialPropertyBlock();
                ShieldPropertyBlock = new MaterialPropertyBlock();
            }

            public float GetPointingAngle(Vector3 point) {
                return Vector3.Dot(CloneGun.transform.right, (point - CloneGun.transform.position).normalized);
            }

            public bool IsInFront(Vector3 point) {
                return Vector3.Dot(CloneGun.transform.forward, (point - CloneGun.transform.position).normalized) > 0;
            }

            public void Shoot() {
                //TODO: Spawn the minion with a different method
                AttachedTriClone.SpawnClone(ShootingOrigin.position, default);
                //CloneGun.Load();
                //CloneGun.Shoot();
            }
        }

        struct ColorFader {
            public Renderer renderer;
            public float time;
            public Color toColor;
            public string colorProperty;
            public MaterialPropertyBlock propertyBlock;

            public ColorFader(Renderer renderer, string colorProperty, float time, Color toColor, MaterialPropertyBlock propertyBlock) {
                this.renderer = renderer;
                this.time = time;
                this.toColor = toColor;
                this.colorProperty = colorProperty;
                this.propertyBlock = propertyBlock;
            }
        }

        [Flags]
        enum RodMask : byte {
            None = 0,
            All = Rod1 | Rod2 | Rod3,
            Rod1 = 1,
            Rod2 = 2,
            Rod3 = 4
        }

        static class TriCloneState {
            public const byte RodDisabled = 0;
        }
    }
}