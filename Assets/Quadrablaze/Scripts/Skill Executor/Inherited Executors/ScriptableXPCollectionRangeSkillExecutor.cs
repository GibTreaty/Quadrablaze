using Quadrablaze.Entities;
using System;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "XP Collection Range", menuName = "Skill Layout/Executors/XP Collection Range")]
    public class ScriptableXPCollectionRangeSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        float _rangeMultiplier = 1;

        public float RangeMultiplier => _rangeMultiplier;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new XPCollectionRange(actorEntity, element);
        }
    }
}