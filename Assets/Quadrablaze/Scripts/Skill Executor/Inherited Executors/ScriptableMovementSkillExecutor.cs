using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Movement Skill", menuName = "Skill Layout/Executors/Movement")]
    public class ScriptableMovementSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        float[] _speedUpgrades;

        public float[] SpeedUpgrades => _speedUpgrades;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Movement(actorEntity, element);
        }
    }
}