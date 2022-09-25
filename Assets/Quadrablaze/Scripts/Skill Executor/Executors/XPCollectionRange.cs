using Quadrablaze.Entities;
using UnityEngine;

namespace Quadrablaze.SkillExecutors {
    public class XPCollectionRange : SkillExecutor {

        public new ScriptableXPCollectionRangeSkillExecutor OriginalSkillExecutor { get; }

        public XPCollectionRange(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableXPCollectionRangeSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
        }

        public float GetRange() {
            return CurrentLayoutElement.CurrentLevel * OriginalSkillExecutor.RangeMultiplier;
        }
    }
}