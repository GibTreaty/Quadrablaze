using Quadrablaze.Entities;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Regenerate Skill", menuName = "Skill Layout/Executors/Regenerate")]
    public class ScriptableRegenerateSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        float _healRatePercent = .05f;

        [SerializeField]
        float _healDelay = 2;

        public float HealDelay => _healDelay;
        public float HealRatePercent => _healRatePercent;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Regenerate(actorEntity, element);
        }
    }
}