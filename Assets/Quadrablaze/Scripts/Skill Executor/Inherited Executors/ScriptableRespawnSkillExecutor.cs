using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Respawn Skill", menuName = "Skill Layout/Executors/Respawn")]
    public class ScriptableRespawnSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        AnimationCurveAsset _respawnTimerCurve;

        public AnimationCurveAsset RespawnTimerCurve => _respawnTimerCurve;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Respawn(actorEntity, element);
        }
    }
}