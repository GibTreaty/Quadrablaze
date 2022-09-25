using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YounGenTech.PoolGen {
    /// <summary>Handles the pooling of objects. It will apply a <see cref="PoolUser"/> to each of its pooled objects.</summary>
    public class ObjectPool {
        PoolManager _poolManagerComponent;

        int _prefabIndex;

        Stack<PoolUser> _pooledObjects;
        List<PoolUser> _spawnedObjects;

        NetworkIdentity _prefabNetworkIdentity;

        #region Properties
        /// <summary>How many spawned objects can fit back into the pool</summary>
        public int CapacityLeft {
            get { return PoolPrefab.PoolSize - SpawnedObjects.Count; }
        }

        /// <summary>Will the object pool use Unity's networking system. Should be set before the object pool is initialized (or before objects are first pooled). If true, Despawn and Spawn can only be called by the Host (on the Server).</summary>
        public bool IsNetworked {
            get { return PoolPrefab.IsNetworked; }
        }

        /// <summary>If true, the <see cref="TotalCount"/> is equal or greater than the <see cref="PoolPrefab.PoolSize"/> and it cannot expand</summary>
        public bool IsPoolFull {
            get { return !PoolPrefab.CanExpand && TotalCount >= PoolPrefab.PoolSize; }
        }

        /// <summary>If true, the <see cref="TotalCount"/> is greater than the <see cref="PoolPrefab.PoolSize"/> and it cannot expand</summary>
        public bool IsPoolOverflowing {
            get { return !PoolPrefab.CanExpand && TotalCount > PoolPrefab.PoolSize; }
        }

        /// <summary>The <see cref="PoolGen.PoolManager"/> that this <see cref="ObjectPool"/> belongs to</summary>
        public PoolManager PoolManagerComponent {
            get { return _poolManagerComponent; }
            set { _poolManagerComponent = value; }
        }

        /// <summary>A stack of pooled objects. Takes advantage of <see cref="Stack{T}"/> for better memory usage.</summary>
        public Stack<PoolUser> PooledObjects {
            get { return _pooledObjects; }
            set { _pooledObjects = value; }
        }

        /// <summary> <see cref="PoolGen.PoolPrefab"/> data from the <see cref="PoolManagerComponent"/></summary>
        public PoolPrefab PoolPrefab {
            get { return PoolManagerComponent.PoolGenPrefabs[PrefabIndex]; }
        }

        /// <summary> ID of the <see cref="PoolGen.PoolPrefab"/></summary>
        public int PrefabID {
            get { return PoolPrefab.ID; }
        }

        /// <summary> Index of the <see cref="PoolGen.PoolPrefab"/> in <see cref="PoolManagerComponent"/></summary>
        public int PrefabIndex {
            get { return _prefabIndex; }
            set { _prefabIndex = value; }
        }

        /// <summary> List of spawned <see cref="PoolUser"/>s</summary>
        public List<PoolUser> SpawnedObjects {
            get { return _spawnedObjects; }
            set { _spawnedObjects = value; }
        }

        /// <summary> Total number of pooled and spawned objects</summary>
        public int TotalCount {
            get { return PooledObjects.Count + SpawnedObjects.Count; }
        }
        #endregion

        /// <summary>Destroys all pooled and spawned objects</summary>
        public void ClearAll() {
            while(_pooledObjects.Count > 0)
                Object.Destroy(_pooledObjects.Pop().gameObject);

            if(_spawnedObjects.Count > 0) {
                for(int i = 0; i < _spawnedObjects.Count; i++)
                    if(_spawnedObjects[i])
                        Object.Destroy(_spawnedObjects[i].gameObject);

                _spawnedObjects.Clear();
            }
        }

        /// <summary>Despawns a <see cref="PoolUser"/></summary>
        //        public void Despawn(PoolUser user) {
        //            UnityEngine.Debug.LogFormat(user.gameObject, "Despawn({0})", user.name);

        //            if(user && user.PrefabIndex == PrefabIndex) {
        //                if(IsNetworked) {
        //                    UnityEngine.Debug.Log(" NetworkServer.UnSpawn()");
        //                    NetworkServer.UnSpawn(user.gameObject);
        //                }
        //                else {
        //                    if(SpawnedObjects.Remove(user)) {
        //                        if(IsPoolFull && !PoolPrefab.ReuseSpawned) {
        //                            PoolManagerComponent.OnDespawn.Invoke(user);
        //                            user.OnDespawn.Invoke();
        //                            Object.Destroy(user.gameObject);
        //                        }
        //                        else {
        //                            PooledObjects.Push(user);
        //                            user.StopTimedDespawn();

        //                            PoolManagerComponent.OnDespawn.Invoke(user);
        //                            user.OnDespawn.Invoke();

        //                            //user.gameObject.hideFlags = HideFlags.HideInHierarchy;

        //#if UNITY_EDITOR
        //                            //UnityEditor.EditorApplication.DirtyHierarchyWindowSorting();
        //#endif

        //                            user.gameObject.SetActive(false);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        public void Despawn(PoolUser user) {
            if(user && user.PrefabIndex == PrefabIndex) {
                if(SpawnedObjects.Remove(user)) {
                    if(IsPoolFull && !PoolPrefab.ReuseSpawned) {
                        PoolManagerComponent.OnDespawn.Invoke(user);
                        user.OnDespawn.Invoke();
                        Object.Destroy(user.gameObject);
                    }
                    else {
                        PooledObjects.Push(user);
                        user.StopTimedDespawn();

                        PoolManagerComponent.OnDespawn.Invoke(user);
                        user.OnDespawn.Invoke();

                        user.gameObject.hideFlags = HideFlags.HideInHierarchy;

#if UNITY_EDITOR
                        //UnityEditor.EditorApplication.DirtyHierarchyWindowSorting();
#endif

                        //if(IsNetworked && NetworkServer.active)
                        //if(IsNetworked && ((NetworkClient.active || NetworkServer.active) ? NetworkServer.active : true))
                        if(IsNetworked)
                            NetworkServer.UnSpawn(user.gameObject);

                        user.gameObject.SetActive(false);
                        user.spawned = false;
                    }
                }
            }
        }

        /// <summary>Despawns the oldest spawned object</summary>
        public void Despawn() {
            if(SpawnedObjects.Count > 0)
                Despawn(SpawnedObjects[0]);
        }

        /// <summary>Despawns all objects</summary>
        public void DespawnAll() {
            var list = SpawnedObjects.ToArray();

            for(int i = 0; i < list.Length; i++)
                Despawn(list[i]);
        }

        void DespawnInternal(PoolUser user) {
            SpawnedObjects.Remove(user);
            PooledObjects.Push(user);
            user.StopTimedDespawn();
            PoolManagerComponent.OnDespawn.Invoke(user);
            user.OnDespawn.Invoke();

            NetworkServer.UnSpawn(user.gameObject);

            user.gameObject.hideFlags = HideFlags.HideInHierarchy;
            user.gameObject.SetActive(false);
            user.spawned = false;
        }

        void DespawnNetworked(PoolUser user) {
            DespawnInternal(user);
        }

        PoolUser GetObjectFromPool(Vector3 position, Quaternion rotation) {
            PoolUser user = null;

            if(PooledObjects.Count == 0) {
                if(PoolPrefab.ReuseSpawned && SpawnedObjects.Count > 0 && !PoolPrefab.CanExpand) {
                    Despawn(SpawnedObjects[0]);
                }
                else {
                    Pool();
                }
            }

            if(PooledObjects.Count > 0) {
                user = PooledObjects.Pop();
                //UnityEngine.Debug.Log("Pop: " + PoolPrefab.Prefab.name);
            }

            if(user) {
                SpawnedObjects.Add(user);

                user.transform.position = position;
                user.transform.rotation = rotation;
                user.gameObject.SetActive(true);
                user.gameObject.hideFlags = PoolManagerComponent.HideSpawnedObjects ? HideFlags.HideInHierarchy : HideFlags.None;
                user.spawned = true;

                PoolManagerComponent.OnSpawn.Invoke(user);
                user.OnSpawn.Invoke();
            }

            return user;
        }

        /// <summary>Sets up the necessary variables and fills the object pool</summary>
        public void Initialize() {
            if(_pooledObjects == null) _pooledObjects = new Stack<PoolUser>(PoolPrefab.PoolSize);
            if(_spawnedObjects == null) _spawnedObjects = new List<PoolUser>(PoolPrefab.PoolSize);

            ClearAll();

            RegisterSpawnHandler();
            PoolCount(PoolPrefab.PoolSize);
        }

        /// <summary>Instantiates an object for usage with the pool with default position and rotation</summary>
        public GameObject Instantiate() {
            return Instantiate(Vector3.zero, Quaternion.identity);
        }
        /// <summary>Instantiates an object for usage with the pool</summary>
        public GameObject Instantiate(Vector3 position, Quaternion rotation) {
            GameObject gameObject = null;

            if(PoolPrefab.Prefab) {
                gameObject = Object.Instantiate(PoolPrefab.Prefab, position, rotation);
                //gameObject = Object.Instantiate(PoolPrefab.Prefab);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
                gameObject.hideFlags = HideFlags.HideInHierarchy;

                gameObject.SetActive(false);
                //gameObject.transform.position = position;
                //gameObject.transform.rotation = rotation;

                PoolUser.BecomeUser(gameObject, PoolManagerComponent, PrefabIndex);
                gameObject.SetActive(true);
            }

            return gameObject;
        }

        void NetworkedDespawnHandler(GameObject spawned) {
            if(NetworkManager.singleton && NetworkManager.singleton.logLevel <= LogFilter.FilterLevel.Debug)
                UnityEngine.Debug.Log(string.Format("NetworkedDespawnHandler({0})", spawned), PoolManagerComponent);

            DespawnNetworked(spawned.GetComponent<PoolUser>());
        }

        GameObject NetworkedSpawnHandler(Vector3 position, NetworkHash128 assetId) {
            if(NetworkManager.singleton && NetworkManager.singleton.logLevel <= LogFilter.FilterLevel.Debug)
                UnityEngine.Debug.Log(string.Format("NetworkedSpawnHandler({0})", assetId), PoolManagerComponent);

            return SpawnNetworked(position, Quaternion.identity).gameObject;
        }

        /// <summary>Attempts to add an object to the pool then returns it. If the pool is full then it returns null.</summary>
        public PoolUser Pool() {
            if(IsPoolFull) return null;

            var gameObject = Instantiate();

            if(!gameObject) return null;

            var user = gameObject.GetComponent<PoolUser>();
            var initializationComponents = user.GetComponentsInChildren<IPoolInstantiate>(false);

            PooledObjects.Push(user);

            for(int i = 0; i < initializationComponents.Length; i++)
                initializationComponents[i].PoolInstantiate(user);

            PoolManagerComponent.OnPool.Invoke(user);
            user.OnPool.Invoke();

            gameObject.SetActive(false);

            return user;
        }

        /// <summary>Attempts to pool a specified amount of objects</summary>
        public void PoolCount(int count) {
            if(!PoolPrefab.CanExpand)
                count = Mathf.Min(Mathf.Max(0, PoolPrefab.PoolSize - PooledObjects.Count), count);

            for(int i = 0; i < count; i++)
                Pool();
        }

        public void RefillPool() {
            PoolCount(PoolPrefab.PoolSize - PooledObjects.Count);
        }

        public void RegisterSpawnHandler() {
            if(IsNetworked) {
                _prefabNetworkIdentity = PoolPrefab.Prefab.GetComponent<NetworkIdentity>();

                if(!_prefabNetworkIdentity) {
                    if(NetworkManager.singleton && NetworkManager.singleton.logLevel <= LogFilter.FilterLevel.Debug)
                        UnityEngine.Debug.LogError(string.Format("NetworkIdentity is required on pooled object ({0}) for networking", PoolPrefab.Prefab + ":" + PoolPrefab.ID), PoolManagerComponent);
                }
                else {
                    var assetHash = string.IsNullOrEmpty(PoolPrefab.CustomAssetHashName) ? _prefabNetworkIdentity.assetId : NetworkHash128.Parse(PoolPrefab.CustomAssetHashName);

                    ClientScene.RegisterSpawnHandler(assetHash, NetworkedSpawnHandler, NetworkedDespawnHandler);
                }
            }
        }

        /// <summary>Attempts to spawn an object from the pool with default position and rotation</summary>
        public PoolUser Spawn() {
            return Spawn(Vector3.zero, Quaternion.identity);
        }

        /// <summary>Attempts to spawn an object from the pool</summary>
        public PoolUser Spawn(Vector3 position, Quaternion rotation) {
            if(!PoolPrefab.Active) return null;
            if(IsNetworked && !NetworkServer.active) return null;

            PoolUser user = GetObjectFromPool(position, rotation);

            if(user) {
                PoolPrefab.TimedDespawn(user);

                if(IsNetworked) {
                    var assetHash = string.IsNullOrEmpty(PoolPrefab.CustomAssetHashName) ? _prefabNetworkIdentity.assetId : NetworkHash128.Parse(PoolPrefab.CustomAssetHashName);

                    if(user.gameObject == null) 
                        UnityEngine.Debug.LogError("Spawn object is null!?");

                    NetworkServer.Spawn(user.gameObject, assetHash);
                }
            }

            return user;
        }

        PoolUser SpawnNetworked(Vector3 position, Quaternion rotation) {
            //UnityEngine.Debug.Log(PoolPrefab.Prefab.name + ":SpawnNetworked");
            return GetObjectFromPool(position, rotation);
        }

        public override string ToString() {
            return string.Format("{0} - Pooled({1}) - Spawned({2}) - CapacityLeft({3}) - Full({4}) - Overflowing({5})", PoolPrefab.Prefab ? PoolPrefab.Prefab.name : "", PooledObjects.Count, SpawnedObjects.Count, CapacityLeft, IsPoolFull, IsPoolOverflowing);
        }
    }
}