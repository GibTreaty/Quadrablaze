using UnityEngine;
using System;
using System.Collections.Generic;

namespace YounGenTech.PoolGen {
    /// <summary>Data to be used for an object pool</summary>
    [Serializable]
    public struct PoolPrefab {
        [SerializeField]
        [Tooltip("Activated status")]
        bool _active;

        [SerializeField]
        [Tooltip("Identification number")]
        int _id;

        [SerializeField]
        [Tooltip("Weight of random chance to be spawned. The higher the weight, the higher the chance.")]
        float _weight;

        [SerializeField]
        [Tooltip("The GameObject to be instantiated for the pool")]
        GameObject _prefab;

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_poolAmountOnStart")]
        [Tooltip("How many objects should be pooled at the start. Also will be used to decide how many objects that should be kept in the case that more objects are pooled higher than the limit. This value doesn't change when the pool expands using CanExpand.")]
        int _poolSize;

        [SerializeField]
        [Tooltip("Allows objects to be pooled past the PoolSize limit. Does not change PoolSize")]
        bool _canExpand;

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_autoPoolType")]
        [Tooltip("Decides when the ObjectPool should be initialized")]
        PoolManager.PoolInitializationType _initializationType;

        [SerializeField]
        [Tooltip("Time before automatic despawn (Uses Invoke)")]
        float _timeToDespawn;

        [SerializeField]
        [Tooltip("Reuse the oldest spawned object in the pool if the pool is empty")]
        bool _reuseSpawned;

        [SerializeField]
        [Tooltip("Will the object pool use Unity's networking system. This should be set before the object pool is initialized (or before objects are first pooled).")]
        bool _isNetworked;

        [SerializeField]
        [Tooltip("Used in NetworkHash128.Parse() to generate a unique assetId for this prefab")]
        string _customAssetHashName;

        #region Properties
        /// <summary>Activated status of this <see cref="PoolPrefab"/></summary>
        public bool Active {
            get { return _active; }
            set { _active = value; }
        }

        /// <summary>Allows objects to be pooled past the <see cref="PoolSize"/> limit. Does not change <see cref="PoolSize"/></summary>
        public bool CanExpand {
            get { return _canExpand; }
            set { _canExpand = value; }
        }

        /// <summary>Used in NetworkHash128.Parse() to generate a unique assetId for this prefab</summary>
        public string CustomAssetHashName {
            get { return _customAssetHashName; }
            set { _customAssetHashName = value; }
        }

        /// <summary>Identification number for this <see cref="PoolPrefab"/></summary>
        public int ID {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>Decides when the <see cref="ObjectPool"/> should be initialized</summary>
        public PoolManager.PoolInitializationType InitializationType {
            get { return _initializationType; }
            set { _initializationType = value; }
        }

        /// <summary>Will the object pool use Unity's networking system. This should be set before the object pool is initialized (or before objects are first pooled).</summary>
        public bool IsNetworked {
            get { return _isNetworked; }
            set { _isNetworked = value; }
        }

        /// <summary>Is the <see cref="Prefab"/> object set</summary>
        public bool IsPrefabSet {
            get { return !Prefab; }
        }

        /// <summary>How many objects should be pooled at the start. Also will be used to decide how many objects that should be kept in the case that more objects are pooled higher than the limit. This value doesn't change when the pool expands using <see cref="CanExpand"/>.</summary>
        public int PoolSize {
            get { return _poolSize; }
            set { _poolSize = value; }
        }

        /// <summary>The GameObject to be instantiated for the pool</summary>
        public GameObject Prefab {
            get { return _prefab; }
            set { _prefab = value; }
        }

        /// <summary>Reuse the oldest spawned object in the pool if the pool is empty</summary>
        public bool ReuseSpawned {
            get { return _reuseSpawned; }
            set { _reuseSpawned = value; }
        }

        /// <summary>Time before automatic despawn</summary>
        public float TimeToDespawn {
            get { return _timeToDespawn; }
            set { _timeToDespawn = value; }
        }

        /// <summary>Weight of random chance to be spawned. The higher the weight, the higher the chance.</summary>
        public float Weight {
            get { return _weight; }
            set { _weight = value; }
        }

        #endregion

        /// <summary>Despawn a <see cref="PoolUser"/> after a set amount of time</summary>
        public void TimedDespawn(PoolUser user) {
            if(TimeToDespawn > 0) user.StartTimedDespawn(TimeToDespawn);
        }

        /// <summary>Generate an ID that hasn't been used in the specified list of <see cref="PoolPrefab"/></summary>
        public static int GenerateUniqueID(IEnumerable<PoolPrefab> list) {
            var idList = new List<int>();

            foreach(PoolPrefab prefab in list)
                idList.Add(prefab.ID);

            return IDHelper.GenerateUniqueID(idList);
        }
    }
}