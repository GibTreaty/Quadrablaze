using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Boss;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors {
    // Level 1 - Single target
    // Level 2 - 3 targets
    [CreateAssetMenu(fileName = "Barrage Skill", menuName = "Skill Layout/Executors/Barrage")]
    public class ScriptableBarrageSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        byte _projectileCount = 10;

        [SerializeField]
        ushort _targetLimit = 1;

        [SerializeField]
        float _projectileLifetime = 2;

        [SerializeField]
        float _projectileStartTime = 1;

        [SerializeField]
        float _projectileLaunchGap = .1f;

        [SerializeField]
        float _cooldownTimer = 5;

        [SerializeField]
        float _damage = 1;

        [SerializeField]
        AnimationCurveAsset _missileStartCurve;

        [SerializeField]
        AnimationCurveAsset _missileHomingCurve;

        [SerializeField]
        string _projectilePoolName;

        [SerializeField]
        int _projectilePoolPrefabId;

        public float CooldownDuration => _cooldownTimer;
        public float Damage => _damage;
        public AnimationCurveAsset MissileStartCurve => _missileStartCurve;
        public AnimationCurveAsset MissileHomingCurve => _missileHomingCurve;
        public override bool Passive => false;
        public byte ProjectileCount => _projectileCount;
        public float ProjectileLifetime => _projectileLifetime;
        public int ProjectilePoolPrefabId => _projectilePoolPrefabId;
        public float ProjectileLaunchGap => _projectileLaunchGap;
        public string ProjectilePoolName => _projectilePoolName;
        public float ProjectileStartTime => _targetLimit;
        public ushort TargetLimit => _targetLimit;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Barrage(actorEntity, element);
        }
    }
}