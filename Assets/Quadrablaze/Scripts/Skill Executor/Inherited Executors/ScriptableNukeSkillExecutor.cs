using System.Collections;
using System.Collections.Generic;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Nuke Skill", menuName = "Skill Layout/Executors/Nuke")]
    public class ScriptableNukeSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        AnimationCurveAsset _rangeCurve;

        [SerializeField]
        AnimationCurveAsset _repeatDelayCurve;

        [SerializeField]
        AnimationCurveAsset _repeatVariationCurve;

        public AnimationCurveAsset RangeCurve => _rangeCurve;
        public AnimationCurveAsset RepeatDelayCurve => _repeatDelayCurve;
        public AnimationCurveAsset RepeatVariationCurve => _repeatVariationCurve;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Nuke(actorEntity, element);
        }
    }
}