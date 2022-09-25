using System;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using Quadrablaze.WeaponSystem;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.EnergyBasedObjects;
using YounGenTech.PoolGen;

//TODO: Add sound again

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Weapon Skill", menuName = "Skill Layout/Executors/Weapon")]
    public class ScriptableWeaponSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        WeaponList _list;

        [SerializeField]
        string _weaponPivotTransformPath = "";

        [SerializeField]
        bool _autoShootOnEquip;

        [SerializeField]
        List<string> _hitTags;

        [SerializeField]
        LayerMask _hitMask = -1;

        public bool AutoShootOnEquip => _autoShootOnEquip;
        public LayerMask HitMask => _hitMask;
        public List<string> HitTags => _hitTags;
        public WeaponList List => _list;
        public override bool Passive => false;
        public string WeaponPivotTransformPath => _weaponPivotTransformPath;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Weapon(actorEntity, element);
        }
    }
}