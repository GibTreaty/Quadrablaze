using System;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Projectile")]
    public class ScriptableProjectileEntity : ScriptableEntity {

        [SerializeField]
        string _poolName = "";

        [SerializeField]
        int _poolId;

        [SerializeField]
        List<ScriptableProjectileModule> _modules;

        public List<ScriptableProjectileModule> Modules => _modules;

        public override Entity CreateInstance() {
            var entity = new ProjectileEntity(0, OriginalGameObject);

            entity.PoolName = _poolName;
            entity.PoolId = _poolId;

            foreach(var module in _modules)
                entity.AddModule(module.CreateInstance(entity));

            return entity;
        }
        public ProjectileEntity CreateInstance(Quadrablaze.WeaponSystem.Weapon weaponEntity) {
            var entity = new ProjectileEntity(0, OriginalGameObject);

            entity.PoolName = _poolName;
            entity.PoolId = _poolId;
            entity.SourceWeapon = weaponEntity;

            foreach(var module in _modules)
                entity.AddModule(module.CreateInstance(entity));

            return entity;
        }
    }
}