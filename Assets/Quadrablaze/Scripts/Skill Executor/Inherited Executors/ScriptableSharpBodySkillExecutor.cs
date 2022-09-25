using Quadrablaze.Entities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "SharpBody Skill", menuName = "Skill Layout/Executors/SharpBody")]
    public class ScriptableSharpBodySkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        float _damage;

        [SerializeField]
        float _damageMultiplier = 1;

        [SerializeField]
        float _damageLevelMultiplier = .25f; // damage = damage + ((Level - 1) * damage * _damageLevelMultiplier)

        [SerializeField]
        AnimationCurveAsset _damageLevelCurve;

        [SerializeField]
        float _hitDelay = 1;

        [SerializeField]
        float _force = 1;

        [SerializeField]
        float _nonRigidbodyForce = 10;

        [SerializeField]
        LayerMask _hitMask = -1;

        public float Damage => _damage;
        public float DamageMultiplier => _damageMultiplier;
        public AnimationCurveAsset DamageLevelCurve => _damageLevelCurve;
        public float DamageLevelMultiplier => _damageLevelMultiplier;
        public float Force => _force;
        public LayerMask HitMask => _hitMask;
        public float HitDelay => _hitDelay;
        public float NonRigidbodyForce => _nonRigidbodyForce;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new SharpBody(actorEntity, element);
        }
    }
}