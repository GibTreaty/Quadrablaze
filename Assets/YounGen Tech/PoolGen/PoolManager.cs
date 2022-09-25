using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace YounGenTech.PoolGen {
    [AddComponentMenu("YounGen Tech/PoolGen/PoolManager", -1)]
    /// <summary>Manages multi-object pools</summary>
    public class PoolManager : MonoBehaviour {

        /// <summary>The string is the name of the <see cref="PoolManager"/> and the list contains the <see cref="PoolManager"/>s with the same name</summary>
        static Dictionary<string, List<PoolManager>> multiPools = new Dictionary<string, List<PoolManager>>();

        [SerializeField]
        string _poolName = "Pool";

        [SerializeField, Tooltip("Hides objects in the hierarchy panel when spawned")]
        bool _hideSpawnedObjects = true;

        [SerializeField]
        List<PoolPrefab> _poolGenPrefabs;

        [SerializeField]
        PoolEvent _onPool;

        [SerializeField]
        PoolEvent _onSpawn;

        [SerializeField]
        PoolEvent _onDespawn;

        Dictionary<int, ObjectPool> _objectContainers = new Dictionary<int, ObjectPool>();

        #region Properties
        /// <summary>Hides objects in the hierarchy panel when spawned</summary>
        public bool HideSpawnedObjects {
            get { return _hideSpawnedObjects; }
            set { _hideSpawnedObjects = value; }
        }

        /// <summary>Containers to be used as pools. There should be one for each <see cref="PoolPrefab"/> in <see cref="PoolGenPrefabs"/></summary>
        public Dictionary<int, ObjectPool> ObjectContainers {
            get { return _objectContainers; }
            set { _objectContainers = value; }
        }

        /// <summary>Fires when an object is despawned</summary>
        public PoolEvent OnDespawn {
            get { return _onDespawn; }
            private set { _onDespawn = value; }
        }

        /// <summary>Fires when an object is pooled</summary>
        public PoolEvent OnPool {
            get { return _onPool; }
            private set { _onPool = value; }
        }

        /// <summary>Fires when an object is spawned</summary>
        public PoolEvent OnSpawn {
            get { return _onSpawn; }
            private set { _onSpawn = value; }
        }

        /// <summary>How many prefabs are in <see cref="PoolGenPrefabs"/></summary>
        public int PrefabCount {
            get { return PoolGenPrefabs.Count; }
        }

        /// <summary>How many objects are pooled total</summary>
        public int PooledObjectsCount {
            get {
                int count = 0;

                foreach(var objectContainer in ObjectContainers)
                    count += objectContainer.Value.PooledObjects.Count;

                return count;
            }
        }

        /// <summary>Poolable object data</summary>
        public List<PoolPrefab> PoolGenPrefabs {
            get { return _poolGenPrefabs; }
            set { _poolGenPrefabs = value; }
        }

        /// <summary>Name of this <see cref="PoolManager"/></summary>
        public string PoolName {
            get { return _poolName; }
            set { _poolName = value; }
        }

        /// <summary>How many objects are spawned total</summary>
        public int SpawnedObjectsCount {
            get {
                int count = 0;

                foreach(var objectContainer in ObjectContainers)
                    count += objectContainer.Value.SpawnedObjects.Count;

                return count;
            }
        }
        #endregion

        void Awake() {
            if(OnPool == null) OnPool = new PoolEvent();
            if(OnSpawn == null) OnSpawn = new PoolEvent();
            if(OnDespawn == null) OnDespawn = new PoolEvent();

            InitializePools(PoolInitializationType.Awake);
        }

        void OnDisable() {
            RemovePool(this);
        }

        void OnEnable() {
            AddPool(this);
        }

        void Start() {
            InitializePools(PoolInitializationType.Start);
        }

        /// <summary>
        ///Checks if <see cref="PoolGenPrefabs"/> contains a prefab with the specified id
        ///</summary>
        public bool ContainsPrefabID(int id) {
            for(int i = 0; i < PrefabCount; i++)
                if(PoolGenPrefabs[i].ID == id)
                    return true;

            return false;
        }

        /// <summary>Despawn all spawned objects associated with this <see cref="PoolManager"/></summary>
        public void DespawnAll() {
            UnityEngine.Debug.Log(PoolName + " DespawnAll");

            for(int prefabIndex = 0; prefabIndex < PrefabCount; prefabIndex++)
                DespawnAll(prefabIndex);
        }
        /// <summary>Despawn all spawned objects with the specified prefab index  that are associated with this <see cref="PoolManager"/></summary>
        public void DespawnAll(int prefabIndex) {
            var poolPrefab = PoolGenPrefabs[prefabIndex];
            var objectContainer = GetObjectPool(poolPrefab.ID);

            if(objectContainer != null)
                objectContainer.DespawnAll();
        }

        /// <summary>Gets an <see cref="ObjectPool"/> using the specified <see cref="PoolPrefab.ID"/></summary>
        public ObjectPool GetObjectPool(int prefabID) {
            ObjectContainers.TryGetValue(prefabID, out ObjectPool container);

            return container;
        }

        /// <summary>Gets all pooled objects</summary>
        public List<PoolUser> GetPooledObjects() {
            List<PoolUser> poolObjects = new List<PoolUser>();

            for(int i = 0; i < PrefabCount; i++)
                poolObjects.AddRange(GetPooledObjects(i));

            return poolObjects;
        }
        /// <summary>Gets all pooled objects using the specified prefab index</summary>
        public PoolUser[] GetPooledObjects(int prefabIndex) {
            var poolPrefab = PoolGenPrefabs[prefabIndex];
            var objectContainer = GetObjectPool(poolPrefab.ID);

            if(objectContainer != null)
                return objectContainer.PooledObjects.ToArray();

            return null;
        }

        /// <summary>Gets all spawned objects with the specified prefab indices</summary>
        public int GetSpawnedObjectCount(params int[] prefabIndices) {
            int count = 0;

            for(int i = 0; i < prefabIndices.Length; i++) {
                var objectContainer = GetObjectPool(PoolGenPrefabs[prefabIndices[i]].ID);

                if(objectContainer != null)
                    count += objectContainer.SpawnedObjects.Count;
            }

            return count;
        }

        /// <summary>Adds up the weight of each <see cref="PoolPrefab"/>. Empty pools are not taken into consideration.</summary>
        public float GetTotalPrefabWeight(bool includeInactive = false) {
            float weight = 0;

            for(int i = 0; i < PrefabCount; i++)
                if(includeInactive || PoolGenPrefabs[i].Active)
                    weight += PoolGenPrefabs[i].Weight;

            return weight;
        }
        /// <summary>Adds up the weight of each <see cref="PoolPrefab"/></summary>
        public float GetTotalPrefabWeight(params int[] prefabIndices) {
            float weight = 0;

            for(int i = 0; i < prefabIndices.Length; i++)
                weight += PoolGenPrefabs[prefabIndices[i]].Weight;

            return weight;
        }

        /// <summary>Gets the index of a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public int GetWeightedPrefabIndex(bool includeInactive = false) {
            return GetWeightedPrefabIndex(UnityEngine.Random.value, includeInactive);
        }
        /// <summary>Gets the index of a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public int GetWeightedPrefabIndex(float value, bool includeInactive = false) {
            if(PrefabCount == 0) return -1;
            if(value == 0 || PrefabCount == 1) return 0;

            int prefabIndex = 0;
            float maxWeight = GetTotalPrefabWeight(includeInactive);

            value = Mathf.Clamp01(value) * maxWeight;

            float sum = 0;

            for(int i = 0; i < PrefabCount; i++) {
                if(includeInactive || PoolGenPrefabs[i].Active)
                    sum += PoolGenPrefabs[i].Weight;

                if(value <= sum) {
                    prefabIndex = i;
                    break;
                }
            }

            return prefabIndex;
        }
        /// <summary>Gets the index of a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public int GetWeightedPrefabIndex(bool includeInactive, params int[] prefabIndices) {
            return GetWeightedPrefabIndex(UnityEngine.Random.value, includeInactive, prefabIndices);
        }
        /// <summary>Gets the index of a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public int GetWeightedPrefabIndex(float value, bool includeInactive, params int[] prefabIndices) {
            if(PrefabCount == 0 || prefabIndices.Length == 0) return -1;
            if(value == 0 || (PrefabCount == 1 && prefabIndices.Length == 1)) return prefabIndices[0];

            int prefabIndex = 0;
            float maxWeight = GetTotalPrefabWeight(prefabIndices);

            value = Mathf.Clamp01(value) * maxWeight;

            float sum = 0;

            for(int i = 0; i < prefabIndices.Length; i++) {
                int index = prefabIndices[i];

                if(includeInactive || PoolGenPrefabs[index].Active)
                    sum += PoolGenPrefabs[index].Weight;

                if(value <= sum) {
                    prefabIndex = index;
                    break;
                }
            }

            return prefabIndex;
        }

        /// <summary>Gets a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public PoolPrefab GetWeightedPrefab(bool includeInactive = false) {
            return GetWeightedPrefab(UnityEngine.Random.value, includeInactive);
        }
        /// <summary>Gets a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public PoolPrefab GetWeightedPrefab(float value, bool includeInactive = false) {
            int index = GetWeightedPrefabIndex(value, includeInactive);

            if(index == -1) return default;

            return PoolGenPrefabs[index];
        }
        /// <summary>Gets a <see cref="PoolPrefab"/> based on weighted random chance. Empty pools are not taken into consideration.</summary>
        public PoolPrefab GetWeightedPrefab(float value, bool includeInactive, params int[] prefabIndices) {
            int index = GetWeightedPrefabIndex(value, includeInactive, prefabIndices);

            if(index == -1) return default;

            return PoolGenPrefabs[index];
        }

        /// <summary>Gets the index of a <see cref="PoolPrefab"/> based on its index in <see cref="PoolGenPrefabs"/></summary>
        public int IndexFromPrefabID(int id) {
            int index = -1;

            for(int i = 0; i < PrefabCount; i++)
                if(PoolGenPrefabs[i].ID == id) {
                    index = i;
                    break;
                }

            return index;
        }

        /// <summary>Gets the index of a <see cref="PoolPrefab"/> based on its name</summary>
        public int IndexFromPrefabName(string name) {
            int index = -1;

            for(int i = 0; i < PrefabCount; i++)
                if(PoolGenPrefabs[i].Prefab && PoolGenPrefabs[i].Prefab.name == name) {
                    index = i;
                    break;
                }

            return index;
        }

        /// <summary>Pools a single random object</summary>
        public void Pool() {
            if(PrefabCount > 0)
                Pool(UnityEngine.Random.Range(0, PrefabCount));
        }
        /// <summary>Pools a single object</summary>
        public PoolUser Pool(int prefabIndex) {
            var poolPrefab = PoolGenPrefabs[prefabIndex];
            var objectContainer = GetObjectPool(poolPrefab.ID);
            PoolUser user = null;

            if(objectContainer != null)
                user = objectContainer.Pool();

            return user;
        }

        /// <summary>Creates an <see cref="ObjectPool"/> fills its pool if it hasn't been created already</summary>
        public void InitializePool(int prefabIndex, bool forceIfInactive = true) {
            PoolPrefab prefabInfo = PoolGenPrefabs[prefabIndex];

            if(forceIfInactive || prefabInfo.Active) {
                if(!ObjectContainers.ContainsKey(prefabInfo.ID)) {
                    var objectContainer = new ObjectPool() { PoolManagerComponent = this, PrefabIndex = prefabIndex };

                    objectContainer.Initialize();
                    ObjectContainers[prefabInfo.ID] = objectContainer;
                }
            }
        }

        /// <summary>Creates an <see cref="ObjectPool"/> for each <see cref="PoolPrefab"/> in <see cref="PoolGenPrefabs"/> set to <see cref="PoolInitializationType.Manual"/> and fills its pool</summary>
        public void InitializePools() {
            InitializePools(PoolInitializationType.Manual);
        }
        /// <summary>Creates an <see cref="ObjectPool"/> for each <see cref="PoolPrefab"/> in <see cref="PoolGenPrefabs"/> with the specified <see cref="PoolInitializationType"/> and fills its pool</summary>
        public void InitializePools(PoolInitializationType autoPoolType) {
            for(int prefabIndex = 0; prefabIndex < PoolGenPrefabs.Count; prefabIndex++)
                if(PoolGenPrefabs[prefabIndex].InitializationType == autoPoolType)
                    InitializePool(prefabIndex, false);
        }

        /// <summary>Has the <see cref="ObjectPool"/> been created</summary>
        public bool IsObjectContainerInitialized(int prefabID) {
            return GetObjectPool(prefabID) != null;
        }

        public void RefillPool(int prefabID) {
            ObjectContainers[prefabID].RefillPool();
        }

        public void RefillPools() {
            foreach(var objectPool in ObjectContainers.Values)
                objectPool.RefillPool();
        }

        public void RegisterSpawnHandlers() {
            foreach(var pool in ObjectContainers.Values)
                pool.RegisterSpawnHandler();
        }

        /// <summary>Spawns an object for each prefab</summary>
        public List<PoolUser> SpawnAll() {
            List<PoolUser> users = new List<PoolUser>();

            for(int i = 0; i < PrefabCount; i++) {
                var user = Spawn(i);

                if(user) users.Add(user);
                else break;
            }

            return users;
        }

        /// <summary>Spawns an object by name</summary>
        public void SpawnByName(string name) {
            var index = IndexFromPrefabName(name);

            if(index > -1) Spawn(index);
        }

        /// <summary>Spawns an object based on weighted random chance</summary>
        public PoolUser Spawn() {
            return Spawn(Vector3.zero, Quaternion.identity);
        }
        /// <summary>Spawns an object based on the specified prefab index</summary>
        public PoolUser Spawn(int prefabIndex) {
            return Spawn(prefabIndex, Vector3.zero, Quaternion.identity);
        }
        /// <summary>Spawns an object based on weighted random chance</summary>
        public PoolUser Spawn(Vector3 position, Quaternion rotation) {
            return Spawn(GetWeightedPrefabIndex(), position, rotation);
        }
        /// <summary>Spawns an object based on the specified prefab index</summary>
        public PoolUser Spawn(int prefabIndex, Vector3 position, Quaternion rotation) {
            if(prefabIndex < 0 || prefabIndex >= PrefabCount) return null;
            if(!PoolGenPrefabs[prefabIndex].Active) return null;

            PoolUser user = null;
            var poolPrefab = PoolGenPrefabs[prefabIndex];
            var objectContainer = GetObjectPool(poolPrefab.ID);

            if(objectContainer != null)
                user = objectContainer.Spawn(position, rotation);

            return user;
        }

        /// <summary>Spawns an object based on weighted random chance of the specified prefab indices</summary>
        public PoolUser WeightedSpawn(Vector3 position, Quaternion rotation, params int[] prefabIndices) {
            return Spawn(GetWeightedPrefabIndex(false, prefabIndices), position, rotation);
        }

        #region Static Helpers
        static void AddPool(PoolManager pool) {
            List<PoolManager> poolList = GetPoolList(pool.PoolName);

            poolList.Add(pool);
        }

        /// <summary>Gets a <see cref="PoolManager"/> by name (and index if multiple <see cref="PoolManager"/>s exist with the same name</summary>
        public static PoolManager GetPool(string poolName, int poolIndex = 0) {
            PoolManager pool = null;
            List<PoolManager> poolList = GetPoolList(poolName, false);

            if(poolList != null)
                if(poolIndex >= 0 && poolIndex < poolList.Count)
                    pool = poolList[poolIndex];

            return pool;
        }

        /// <summary>Get a list of all <see cref="PoolManager"/></summary>
        public static List<PoolManager> GetPools() {
            List<PoolManager> poolList = new List<PoolManager>();

            foreach(List<PoolManager> list in multiPools.Values)
                poolList.AddRange(list);

            return poolList;
        }

        /// <summary>Get a count of all <see cref="PoolManager"/></summary>
        public static int GetPoolCount() {
            int count = 0;

            foreach(List<PoolManager> poolList in multiPools.Values)
                count += poolList.Count;

            return count;
        }
        /// <summary>Get a count of all <see cref="PoolManager"/> with the specified name</summary>
        public static int GetPoolCount(string poolName) {
            return multiPools.ContainsKey(poolName) ? multiPools[poolName].Count : 0;
        }

        /// <summary>Get the index of the <see cref="PoolManager"/> in the list of <see cref="PoolManager"/> of the same name</summary>
        public static int GetPoolIndex(PoolManager pool) {
            var list = GetPoolList(pool.name, false);
            int index = -1;

            if(list != null)
                index = list.IndexOf(pool);

            return index;
        }

        /// <summary>Get a list of <see cref="PoolManager"/> of the same name</summary>
        public static List<PoolManager> GetPoolList(string poolName, bool createIfNotFound = true) {
            List<PoolManager> pool = null;

            if(multiPools.ContainsKey(poolName))
                pool = multiPools[poolName];
            else if(createIfNotFound) {
                pool = new List<PoolManager>();

                multiPools.Add(poolName, pool);
            }

            return pool;
        }

        public static void RefillAllPools() {
            foreach(var pool in GetPools())
                pool.RefillPools();
        }

        static void RemovePool(PoolManager pool) {
            List<PoolManager> poolList = GetPoolList(pool.PoolName, false);

            if(poolList != null) poolList.Remove(pool);
        }

        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(string poolName, int poolIndex = 0) {
            PoolManager pool = GetPool(poolName, poolIndex);

            return pool ? pool.Spawn() : null;
        }
        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(string poolName, int prefabIndex, int poolIndex = 0) {
            PoolManager pool = GetPool(poolName, poolIndex);

            return pool ? pool.Spawn(prefabIndex) : null;
        }
        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(string poolName, Vector3 position, Quaternion rotation, int poolIndex = 0) {
            PoolManager pool = GetPool(poolName, poolIndex);

            return pool ? pool.Spawn(position, rotation) : null;
        }
        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(string poolName, int prefabIndex, Vector3 position, Quaternion rotation, int poolIndex = 0) {
            PoolManager pool = GetPool(poolName, poolIndex);

            return pool ? pool.Spawn(prefabIndex, position, rotation) : null;
        }

        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(PoolManager pool) {
            return pool ? pool.Spawn() : null;
        }
        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(PoolManager pool, int prefabIndex) {
            return pool ? pool.Spawn(prefabIndex) : null;
        }
        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(PoolManager pool, Vector3 position, Quaternion rotation) {
            return pool ? pool.Spawn(position, rotation) : null;
        }
        /// <summary>Spawn an object</summary>
        public static PoolUser Spawn(PoolManager pool, int prefabIndex, Vector3 position, Quaternion rotation) {
            return pool ? pool.Spawn(prefabIndex, position, rotation) : null;
        }

        /// <summary>Spawn one of each </summary>
        public static List<PoolUser> SpawnAll(string poolName) {
            PoolManager pool = GetPool(poolName);

            return pool ? pool.SpawnAll() : null;
        }
        #endregion

        [Serializable]
        public class PoolEvent : UnityEvent<PoolUser> { }

        /// <summary>How the <see cref="ObjectPool"/> should be initialized</summary>
        public enum PoolInitializationType {
            /// <summary>The pool has to be initialized by YOU manually through code</summary>
            Manual = 0,
            /// <summary>The pool will be initialized on Awake()</summary>
            Awake = 1,
            /// <summary>The pool will be initialized on Start()</summary>
            Start = 2
        }
    }
}