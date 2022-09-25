using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

//TODO:Remove ReachedRadius component
namespace Quadrablaze {
    public class MineLayerController : MonoBehaviour, IActorEntityObjectInitialize {

        const int disappearRadius = 50;
        const int disappearSqrRadius = disappearRadius * disappearRadius;

        static PoolManager pool;
        static int minePoolId;

        uint entityId;

        EnemyInput InputComponent { get; set; }
        BaseMovementController MovementController { get; set; }

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            entityId = entity.Id;

            if(NetworkServer.active) {
                var randomPosition = Random.insideUnitSphere * (GameManager.Current.ArenaRadius - 1);
                randomPosition.y = 0;

                InputComponent.MoveToPosition(randomPosition);
            }
        }

        void OnDisable() {
            entityId = 0;
        }

        void Start() {
            InputComponent = GetComponent<EnemyInput>();
            MovementController = GetComponent<BaseMovementController>();

            InputComponent.onReachedPosition.AddListener(ReachedPosition);
            InputComponent.onMovementInputChanged.AddListener(MovementController.Move);
            InputComponent.onRandomMovementInput.AddListener(MovementController.SetVelocity);

            if(pool == null) {
                pool = PoolManager.GetPool("Enemy");

                foreach(var prefab in pool.PoolGenPrefabs)
                    if(prefab.Prefab.name == "Mine") {
                        minePoolId = prefab.ID;
                        break;
                    }
            }
        }

        void ReachedPosition() {
            Debug.Log($"[Mine Layer] ReachedPosition");
            SpawnMine();
            SetMoveStatus();
        }

        void SetMoveStatus() {
            InputComponent.SetMoveForwardStatus();
            MovementController.Speed = 12;
        }

        void SpawnMine() {
            if(GameManager.Current.CurrentGameMode is IEnemySpawnController enemySpawn) {
                Debug.Log($"[Mine Layer] Spawn Mine");
                enemySpawn.CurrentEnemyController.SpawnEnemy(minePoolId, transform.position);
            }
        }

        void Update() {
            if(NetworkServer.active)
                if(entityId != 0)
                    if(transform.position.sqrMagnitude >= disappearSqrRadius)
                        if(GameManager.Current.GetActorEntity(entityId) is ActorEntity actorEntity) {
                            Debug.Log($"[Mine Layer] Destroy and Unload");
                            actorEntity.DestroyEntity();
                            actorEntity.UnloadEntity();
                        }
        }
    }
}