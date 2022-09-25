using System.Collections.Generic;
using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.PoolGen;

namespace Quadrablaze.Boss {
    public class Supplier : BossController {

        [SerializeField]
        Animator _animatorComponent;

        [SerializeField]
        PoolManager _enemyPool;

        [SerializeField]
        Transform _spawner;

        [SerializeField]
        float _rotationSpeed = 1;

        [SerializeField]
        EventTimer _delayedStartTimer = new EventTimer(1) { Active = true, AutoDisable = true, AutoReset = true };

        [SerializeField]
        EventTimer _moveTimer = new EventTimer(5) { AutoDisable = true, AutoReset = true };

        [SerializeField]
        EventTimer _vulnerableTimer = new EventTimer(4) { AutoDisable = true, AutoReset = true };

        [SerializeField]
        EventTimer _spawnerTimer = new EventTimer(.25f) { AutoReset = true };

        [SerializeField]
        EventTimer _movingSpawnTimer = new EventTimer(2.5f) { AutoDisable = true, AutoReset = true };

        [SerializeField]
        float _targetRotation;

        [SerializeField]
        GameObject[] _hitSpots = new GameObject[1];

        int[] prefabIndices = null;

        #region Properties
        Animator AnimatorComponent => _animatorComponent;
        Rigidbody RigidbodyComponent { get; set; }
        PoolUser UserComponent { get; set; }
        #endregion

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            BaseInitialize(entity);

            if(!initialized) {
                UserComponent = entity.UserComponent;
                RigidbodyComponent = entity.RigidbodyComponent;

                SetupTimers();
                SetupPrefabSpawning();

            }

            _moveTimer.Active = false;
            _vulnerableTimer.Active = false;
            _delayedStartTimer.Start(true);
            _spawnerTimer.Active = false;
            _movingSpawnTimer.Active = false;

            transform.rotation = Quaternion.identity;
            EnableHitSpots(false);
        }

        void EnableHitSpots(bool enable) {
            foreach(var hitSpot in _hitSpots)
                hitSpot.SetActive(enable);
        }

        public override GameObject[] GetHitSpots() {
            return _hitSpots;
        }

        void OnDisable() {
            EnemyProxy.Targets.Remove(_hitSpots[0].transform);
        }

        protected override void OnStage(int stage) {
            switch(stage) {
                case 1:
                    prefabIndices = new int[] {
                _enemyPool.IndexFromPrefabName("Exploder T1")
            };
                    break;

                case 2:
                    prefabIndices = new int[] {
                _enemyPool.IndexFromPrefabName("Exploder T1"),
                _enemyPool.IndexFromPrefabName("Shotformer")
            };
                    break;

                case 3:
                    prefabIndices = new int[] {
                _enemyPool.IndexFromPrefabName("Shotformer")
            };
                    break;
            }
        }

        public override void OnStageUpdate(int stage) {
            _delayedStartTimer.Update();
            _moveTimer.Update();
            _vulnerableTimer.Update();
            _movingSpawnTimer.Update();
            _spawnerTimer.Update();

            if(_moveTimer.Active) {
                Quaternion targtRotation = Quaternion.Euler(0, _targetRotation, 0);

                RigidbodyComponent.rotation = Quaternion.RotateTowards(transform.rotation, targtRotation, Time.deltaTime * _rotationSpeed);
            }
        }

        void SetupPrefabSpawning() {
            _enemyPool = PoolManager.GetPool("Enemy");
        }

        void SetupTimers() {
            _delayedStartTimer.OnElapsed.AddListener(Move);

            _vulnerableTimer.OnElapsed.AddListener(Move);
            _vulnerableTimer.OnElapsed.AddListener(() => AnimatorComponent.SetBool("Vulnerable", false));
            _vulnerableTimer.OnElapsed.AddListener(() => GameManager.Current.GetActorEntity(EntityId).HealthSlots[0].Invincible = true);
            //_vulnerableTimer.OnElapsed.AddListener(() => HealthGroupComponent.MainHealthLayer.Active = false);
            _vulnerableTimer.OnElapsed.AddListener(() => {
                EnemyProxy.Targets.Remove(_hitSpots[0].transform);
                EnableHitSpots(false);
            });

            _moveTimer.OnElapsed.AddListener(() => _vulnerableTimer.Start(true));
            _moveTimer.OnElapsed.AddListener(() => _movingSpawnTimer.Start(true));
            _moveTimer.OnElapsed.AddListener(() => AnimatorComponent.SetBool("Vulnerable", true));
            _moveTimer.OnElapsed.AddListener(() => GameManager.Current.GetActorEntity(EntityId).HealthSlots[0].Invincible = false);
            //_moveTimer.OnElapsed.AddListener(() => HealthGroupComponent.MainHealthLayer.Active = true);
            _moveTimer.OnElapsed.AddListener(() => {
                EnemyProxy.Targets.Add(_hitSpots[0].transform);
                EnableHitSpots(true);
            });

            _movingSpawnTimer.OnElapsed.AddListener(_spawnerTimer.Stop);
            _movingSpawnTimer.OnElapsed.AddListener(_movingSpawnTimer.Stop);

            _spawnerTimer.OnElapsed.AddListener(SpawnEnemy);
        }

        [ContextMenu("Move")]
        void Move() {
            _moveTimer.Start(true);
            _movingSpawnTimer.Start(true);
            _spawnerTimer.Start(true);

            float _direction = Random.Range(0, 2) == 0 ? 1 : -1;

            _targetRotation = (Mathf.Floor(transform.rotation.eulerAngles.y / 180f) * 180) + (_direction * 180);

            if(_targetRotation < -360) _targetRotation = -180;
            else if(_targetRotation > 360) _targetRotation = 180;
        }

        void SpawnEnemy() {
            if(_enemyPool.GetSpawnedObjectCount(prefabIndices) < 5) {
                var gameObject = _enemyPool.WeightedSpawn(_spawner.position, Quaternion.identity, prefabIndices);

                //TODO: Do ActorEntity-related shiz here
                //gameObject.GetComponent<DropXP>().CanDrop = false;
            }
        }
    }
}