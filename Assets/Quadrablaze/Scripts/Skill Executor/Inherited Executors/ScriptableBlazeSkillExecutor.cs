using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Blaze Skill", menuName = "Skill Layout/Executors/Blaze")]
    public class ScriptableBlazeSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        float _afterburnerSpeed = 32;

        [SerializeField]
        BlazeAfterburner _blazeAfterburnerPrefab;

        [SerializeField]
        BlazeRing _blazeRingPrefab;

        [SerializeField, FormerlySerializedAs("_blazeRadiusTransformPath")]
        string _graphicsRadiusTransformPath;

        [SerializeField]
        float _blazeDuration = 3;

        [SerializeField]
        float _blazeRechargeDuration = 3;

        [SerializeField]
        float _rotationSpeed = 45;

        [SerializeField]
        float _radius = 1;

        [SerializeField]
        float _radiusLevelMultiplier = .4f;

        [SerializeField]
        float _damagePerSecond = .2f; // = 2; // damage = damage + ((Level - 1) * damage * _damageLevelMultiplier)

        [SerializeField]
        float _bossDamagePerSecond = .1f; // Does a different rate of damage to boss hitspots

        [SerializeField]
        float _damageLevelMultiplier = .25f; // damage = damage + ((Level - 1) * damage * _damageLevelMultiplier)

        [SerializeField]
        LayerMask _hitMask = -1;

        public float AfterburnerSpeed => _afterburnerSpeed;
        public float BlazeDuration => _blazeDuration;
        public BlazeAfterburner BlazeAfterburnerPrefab => _blazeAfterburnerPrefab;
        public float BlazeRechargeDuration => _blazeRechargeDuration;
        public BlazeRing BlazeRingPrefab => _blazeRingPrefab;
        public float BossDamagePerSecond => _bossDamagePerSecond;
        public float DamageLevelMultiplier => _damageLevelMultiplier;
        public float DamagePerSecond => _damagePerSecond;
        public string GraphicsRadiusTransformPath => _graphicsRadiusTransformPath;
        public LayerMask HitMask => _hitMask;
        public float RadiusLevelMultiplier => _radiusLevelMultiplier;
        public float RotationSpeed => _rotationSpeed;
        public override bool Passive => false;
        public float Radius => _radius;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Blaze(actorEntity, element);
        }
    }
}