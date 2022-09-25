using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

//TODO: Shockwave skill not effecting player wtf
namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Shockwave Skill", menuName = "Skill Layout/Executors/Shockwave")]
    public class ScriptableShockwaveSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        string _graphicsRadiusTransformPath;

        [SerializeField]
        float _cooldownDuration = 1;

        [SerializeField]
        float _baseForce = 2;

        [SerializeField]
        float _baseRigidbodyForce = 4000;

        //[SerializeField]
        //float _damageLevelMultiplier = .25f; // damage = damage + ((Level - 1) * damage * _damageLevelMultiplier)

        [SerializeField]
        AnimationCurveAsset _damageLevelMultiplierCurve;

        [SerializeField]
        AnimationCurveAsset _bossDamageLevelMultiplierCurve;

        [SerializeField]
        float _radius = 5;

        [SerializeField]
        LayerMask _hitMask = -1;

        public float BaseForce => _baseForce;
        public float BaseRigidbodyForce => _baseRigidbodyForce;
        public AnimationCurveAsset BossDamageLevelMultiplierCurve => _bossDamageLevelMultiplierCurve;
        public float CooldownDuration => _cooldownDuration;
        public AnimationCurveAsset DamageLevelMultiplierCurve => _damageLevelMultiplierCurve;
        public string GraphicsRadiusTransformPath => _graphicsRadiusTransformPath;
        public LayerMask HitMask => _hitMask;
        public override bool Passive => false;
        public float Radius => _radius;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Shockwave(actorEntity, element);
        }
    }
}