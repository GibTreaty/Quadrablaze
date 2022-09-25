using Quadrablaze.Entities;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Radblast Skill", menuName = "Skill Layout/Executors/Radblast")]
    public class ScriptableRadblastSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        float _cooldownDuration = 1;

        [SerializeField]
        string _prefabPoolName = "Projectiles";

        [SerializeField]
        string[] _prefabNamePerLevel;

        [SerializeField]
        float _speed = 1;

        public float CooldownDuration => _cooldownDuration;
        public override bool Passive => false;
        public string PrefabPoolName => _prefabPoolName;
        public string[] PrefabNamePerLevel => _prefabNamePerLevel;
        public float Speed => _speed;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Radblast(actorEntity, element);
        }
    }
}