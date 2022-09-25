using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

namespace YounGenTech.PoolGen {
    [AddComponentMenu("YounGen Tech/PoolGen/PoolUser", -1)]
    /// <summary>An object that is pooled in an <see cref="ObjectPool"/></summary>
    public class PoolUser : MonoBehaviour {
        [SerializeField]
        [Tooltip("The user belongs to this PoolManager")]
        PoolManager _pool;

        [SerializeField]
        [Tooltip("Index of the PoolPrefab for the PoolManager")]
        int _prefabIndex = -1;

        [SerializeField]
        UnityEvent _onPool;

        [SerializeField]
        UnityEvent _onSpawn;

        [SerializeField]
        UnityEvent _onDespawn;

        public bool spawned;

        #region Properties
        /// <summary>The <see cref="PoolPrefab"/> that this object is related to</summary>
        public PoolPrefab Prefab {
            get { return InPool.PoolGenPrefabs[PrefabIndex]; }
        }

        /// <summary>Index of the <see cref="PoolPrefab"/></summary>
        public int PrefabIndex {
            get { return _prefabIndex; }
            set { _prefabIndex = value; }
        }

        /// <summary>The user belongs to this <see cref="PoolManager"/> for the PoolManager</summary>
        public PoolManager InPool {
            get { return _pool; }
            set { _pool = value; }
        }

        /// <summary>An object from the pool has despawned</summary>
        public UnityEvent OnDespawn {
            get { return _onDespawn; }
            private set { _onDespawn = value; }
        }

        /// <summary>An object from the pool has been pooled</summary>
        public UnityEvent OnPool {
            get { return _onPool; }
            private set { _onPool = value; }
        }

        /// <summary>An object from the pool has spawned</summary>
        public UnityEvent OnSpawn {
            get { return _onSpawn; }
            private set { _onSpawn = value; }
        }
        #endregion

        void Awake() {
            if(OnPool == null) OnPool = new UnityEvent();
            if(OnSpawn == null) OnSpawn = new UnityEvent();
            if(OnDespawn == null) OnDespawn = new UnityEvent();
        }

        /// <summary>Returns the object to the pool. If <see cref="InPool"/> is null, this GameObject will be destroyed.</summary>
        public void Despawn() {
            if(!InPool) Destroy(gameObject);

            var container = InPool.GetObjectPool(Prefab.ID);

            if(container != null)
                container.Despawn(this);
        }
        /// <summary>Spawns an object from a specified <see cref="PoolManager"/> at this position</summary>
        public void SpawnHere(string poolManager) {
            SpawnHere(poolManager, transform.position, Quaternion.identity);
        }
        /// <summary>Spawns an object from a specified <see cref="PoolManager"/> at a specified position</summary>
        public void SpawnHere(string poolManager, Vector3 position) {
            SpawnHere(poolManager, position, Quaternion.identity);
        }
        /// <summary>Spawns an object from a specified <see cref="PoolManager"/> at a specified position and rotation</summary>
        public void SpawnHere(string poolManager, Vector3 position, Quaternion rotation) {
            PoolManager pool = PoolManager.GetPool(poolManager);

            if(pool)
                pool.Spawn(position, rotation);
        }

        /// <summary>Starts a timer to despawn this GameObject</summary>
        public void StartTimedDespawn(float time) {
            Invoke("TimedDespawn", time);
        }
        /// <summary>Stops the self destruction (despawnation?) timer</summary>
        public void StopTimedDespawn() {
            CancelInvoke("TimedDespawn");
        }

        void TimedDespawn() {
            Despawn();
        }

        /// <summary>Adds the <see cref="PoolUser"/> component to specified GameObject with the necessary pool info</summary>
        public static PoolUser BecomeUser(GameObject gameObject, PoolManager pool, int prefabIndex) {
            var user = gameObject.GetComponent<PoolUser>();

            if(!user)
                user = gameObject.AddComponent<PoolUser>();

            user.InPool = pool;
            user.PrefabIndex = prefabIndex;

            return user;
        }

        void OnDestroy() {
            if(InPool != null) {
                var networkIdentity = GetComponent<UnityEngine.Networking.NetworkIdentity>();

                //UnityEngine.Networking.NetworkServer.spawn.ContainsKey(networkIdentity.assetId);
                if(PrefabIndex < InPool.PoolGenPrefabs.Count) {
                    //print("PoolUser OnDestroy - " + name);

                    var objectContainer = InPool.GetObjectPool(Prefab.ID);

                    if(objectContainer != null)
                        objectContainer.SpawnedObjects.Remove(this);

                    //UnityEngine.Debug.Log(name + " has been Destroyed", gameObject);
                }
            }
        }
    }
}