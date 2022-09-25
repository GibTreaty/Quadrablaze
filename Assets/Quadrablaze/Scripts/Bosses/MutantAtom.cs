using System;
using System.Collections;
using System.Collections.Generic;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze.Boss {
    public class MutantAtom : BossController, IActorEntityObjectSpawned {

        [SerializeField]
        string _poolName = "Mutant Atom Child Pool";

        [SerializeField]
        ScriptableMinionEntity _originalMinion;

        [SerializeField]
        int _spawnCount = 3;

        [SerializeField]
        float _spawnDistance = 14;

        [SerializeField]
        float _shotDelay = 1;

        [SerializeField]
        float _moveDelay = 5;

        [SerializeField]
        float _moveSpeed = .125f;

        float _startDistance;
        float _startMoveDelay;

        List<MutantAtomController> bosses = new List<MutantAtomController>();
        GameObject[] controlsEntities = null;

        float movePositionTime;

        IEnumerator _movePositionRoutine;

        #region Properties
        public override GameObject[] ControlsEntities {
            get { return controlsEntities; }
        }

        Health HealthComponent { get; set; }

        public int LiveBosses { get; private set; }

        public float MoveDelay {
            get { return _moveDelay; }
            set { _moveDelay = value; }
        }

        public float MoveSpeed {
            get { return _moveSpeed; }
            set { _moveSpeed = value; }
        }

        public string PoolName {
            get { return _poolName; }
            set { _poolName = value; }
        }

        public float ShotDelay {
            get { return _shotDelay; }
            set { _shotDelay = value; }
        }

        public int SpawnCount {
            get { return _spawnCount; }
            set { _spawnCount = value; }
        }

        public float SpawnDistance {
            get { return _spawnDistance; }
            set { _spawnDistance = value; }
        }
        #endregion

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            BaseInitialize(entity);

            if(!initialized) {
                HealthComponent = GetComponent<Health>(); // TODO: [Health] Get rid of this
                HealthComponent.OnChangedHealth.AddListener(s => BossInfoUI.Current.SetHealth(HealthComponent.NormalizedValue));

                _movePositionRoutine = MovePositionCoroutine();
            }

            _startDistance = SpawnDistance;
            _startMoveDelay = MoveDelay;

            initialized = true;
        }

        public void ActorEntityObjectSpawned(ActorEntity entity) {
            if(NetworkServer.active) {
                Debug.Log("ActorEntityObjectSpawned Mutant Atom");

                SpawnAll();
                StartCoroutine(ShootingHandlerCoroutine());
                StartCoroutine(MoveHandlerCoroutine());
                SetupStartHealth();
            }
        }

        protected override void OnStage(int stage) {
            switch(stage) {
                case 1:
                    SpawnDistance = _startDistance;
                    MoveDelay = _startMoveDelay;
                    break;

                case 2:
                    SpawnDistance = _startDistance * .75f;
                    MoveDelay = _startMoveDelay * .75f;
                    break;

                case 3:
                    SpawnDistance = _startDistance * .5f;
                    MoveDelay = _startMoveDelay * .5f;
                    break;
            }
        }

        void DestroyBoss() {
            foreach(var boss in bosses) {
                boss.UserComponent.Despawn();
            }

            bosses.Clear();
        }

        Vector3 GetTargetPosition(int index, float time) {
            return bosses.Count > 0 ?
                Quaternion.Euler(0, 360 * time, 0) * (Quaternion.Euler(0, (360f / bosses.Count) * index, 0) * new Vector3(0, 0, SpawnDistance)) :
                new Vector3(0, 0, SpawnDistance);
        }

        [ContextMenu("MovePosition")]
        void MovePosition() {
            StopCoroutine(_movePositionRoutine);
            StartCoroutine(_movePositionRoutine);
        }

        IEnumerator MovePositionCoroutine() {
            float direction = movePositionTime > .5f ? -1 : 1;
            float targetTime = direction == -1 ? 0 : 1;

            while(movePositionTime != targetTime) {
                movePositionTime = Mathf.MoveTowards(movePositionTime, targetTime, Time.deltaTime * MoveSpeed);

                for(int i = 0; i < bosses.Count; i++) {
                    Vector3 targetPosition = GetTargetPosition(i, movePositionTime);
                    var boss = bosses[i];

                    //boss.BaseMovementComponent.Move(targetPosition - boss.transform.position);
                    boss.EnemyInputComponent.MoveToPosition(targetPosition);

                    Debug.DrawRay(targetPosition, Vector3.up, i == 0 ? Color.green : Color.red, .05f, false);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator MoveHandlerCoroutine() {
            while(true) {
                yield return new WaitForSeconds(MoveDelay);
                yield return MovePositionCoroutine();
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, SpawnDistance);
            Gizmos.color = Color.yellow * .75f;
            Gizmos.DrawWireSphere(transform.position, SpawnDistance * .75f);
            Gizmos.color = Color.yellow * .5f;
            Gizmos.DrawWireSphere(transform.position, SpawnDistance * .5f);
        }

        void SetupStartHealth() {
            float totalValue = 0;

            foreach(var boss in bosses)
                totalValue += 1;

            HealthComponent.MaxValue = totalValue;
            HealthComponent.Value = totalValue;
        }

        IEnumerator ShootingHandlerCoroutine() {
            if(bosses.Count == 0) yield return null;

            int index = 0;

            while(true) {
                yield return new WaitForSeconds(ShotDelay);

                index %= bosses.Count;
                bosses[index].Shoot();
                index++;
            }
        }

        [ContextMenu("Spawn All")]
        void SpawnAll() {
            DestroyBoss();

            float angle = 360f / SpawnCount;

            for(int i = 0; i < SpawnCount; i++) {
                var position = new Vector3(0, 0, SpawnDistance);
                var rotation = Quaternion.Euler(0, angle * i, 0);

                SpawnBoss(rotation * position, rotation);
            }
        }

        void SpawnBoss(Vector3 position, Quaternion rotation) {
            Debug.Log("SpawnBoss Mutant Atom");
            var index = GameManager.Current.MinionDatabase.Entities.IndexOf(_originalMinion);

            if(index == -1) {
                Debug.LogError("Mutant Atom child not in the databse");
                return;
            }

            var args = new CreateEntityArgs(CreateEntityType.Minion, index, position, rotation, new Action<GameObject>(Callback));

            GameManager.Proxy.RaiseEvent(GameManagerActions.CreateEntity, args);

            void Callback(GameObject gameObject) {
                var bossComponent = gameObject.GetComponent<MutantAtomController>();

                bosses.Add(bossComponent);
                UpdateControlEntities();

                bossComponent.EnemyInputComponent.MoveToPosition(position);

                gameObject.transform.rotation *= Quaternion.Euler(0, 180, 0);
            }
        }

        void OnBossDeath(HealthEvent healthEvent) {
            var controller = healthEvent.targetGameObject.GetComponent<MutantAtomController>();

            bosses.Remove(controller);
            UpdateControlEntities();

        }

        void UpdateControlEntities() {
            var gameObjects = new List<GameObject>();

            foreach(var boss in bosses)
                gameObjects.Add(boss.gameObject);

            controlsEntities = gameObjects.ToArray();
        }
    }
}