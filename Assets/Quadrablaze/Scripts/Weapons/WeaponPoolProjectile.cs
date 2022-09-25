using UnityEngine;
using YounGenTech.PoolGen;
using YounGenTech.EnergyBasedObjects;
using System.Collections.Generic;
using UnityEngine.Networking;

[RequireComponent(typeof(ObjectSpawnerPusher))]
public class WeaponPoolProjectile : MonoBehaviour {

    [SerializeField]
    PoolManager _poolManager;

    [SerializeField]
    string _findPoolOnAwake = "Projectile";

    [SerializeField]
    string[] _projectileNames;

    [SerializeField]
    PoolManager.PoolEvent _onProjectileSpawned;

    int[] _projectileIndices;

    public PoolManager PoolManagerComponent {
        get { return _poolManager; }
        protected set { _poolManager = value; }
    }

    public PoolManager.PoolEvent OnProjectileSpawned {
        get { return _onProjectileSpawned; }
        private set { _onProjectileSpawned = value; }
    }

    void Awake() {
        if(OnProjectileSpawned == null) OnProjectileSpawned = new PoolManager.PoolEvent();

        if(!string.IsNullOrEmpty(_findPoolOnAwake))
            PoolManagerComponent = PoolManager.GetPool(_findPoolOnAwake);

        if(!PoolManagerComponent)
            PoolManagerComponent = GetComponent<PoolManager>();

        if(PoolManagerComponent)
            GetComponent<ObjectSpawnerPusher>().OverrideSpawnThisObject = GetProjectile;

        List<int> indexList = new List<int>();

        for(int i = 0; i < _projectileNames.Length; i++)
            indexList.Add(PoolManagerComponent.IndexFromPrefabName(_projectileNames[i]));

        _projectileIndices = indexList.ToArray();
    }

    GameObject GetProjectile(Vector3 position, Quaternion rotation) {
        PoolUser user = null;

        if(_projectileNames.Length > 0) {
            user = _projectileNames.Length > 1 ?
                 PoolManagerComponent.WeightedSpawn(position, rotation, _projectileIndices) :
                 PoolManagerComponent.Spawn(_projectileIndices[0], position, rotation);
        }
        else
            user = PoolManagerComponent.Spawn();

        OnProjectileSpawned.Invoke(user);

        return user.gameObject;
    }
}