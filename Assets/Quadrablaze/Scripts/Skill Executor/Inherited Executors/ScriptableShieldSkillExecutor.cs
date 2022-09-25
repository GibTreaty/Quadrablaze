using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Shield Skill", menuName = "Skill Layout/Executors/Shield")]
    public class ScriptableShieldSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        string _shieldHealthPath = "";

        [SerializeField]
        string _shieldColliderPath = "";

        [SerializeField]
        string _shieldMeshPath = "";

        [SerializeField]
        string _shieldHealthLayerPath = "";

        [SerializeField]
        float _regenerationLength = 3;

        public float RegenerationLength => _regenerationLength;
        public string ShieldColliderPath => _shieldColliderPath;
        public string ShieldHealthPath => _shieldHealthPath;
        public string ShieldHealthLayerPath => _shieldHealthLayerPath;
        public string ShieldMeshPath => _shieldMeshPath;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Shield(actorEntity, element);
        }
    }
}