using System;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;

// TODO: Skill descriptions are showing up wrong in the upgrade menu
// TODO: Get active skills in an better, indexed, way

namespace Quadrablaze {
    [CreateAssetMenu(fileName = "Skill Layout", menuName = "Skill Layout/Layout")]
    public class ScriptableSkillLayout : ScriptableObject {

        [SerializeField]
        List<ScriptableLayoutElement> _elements = new List<ScriptableLayoutElement>();

        #region Properties
        public List<ScriptableLayoutElement> Elements => _elements;
        #endregion

        public SkillLayout CreateInstance() {
            var elements = new List<SkillLayoutElement>();
            var subIndices = new Dictionary<Type, int>();

            for(int i = 0; i < Elements.Count; i++) {
                var element = Elements[i];
                var elementInstance = element.CreateInstance();

                elementInstance.ElementIndex = i;
                elements.Add(elementInstance);

                var type = element.CreatesExecutor.GetType();

                if(subIndices.TryGetValue(type, out int index)) {
                    index++;
                    elementInstance.ElementTypeIndex = index;
                    subIndices[type] = index;
                }
                else
                    subIndices.Add(type, 0);
            }

            return new SkillLayout(this, elements);
        }
    }

    public class SkillLayoutElementEvent : UnityEvent<SkillLayoutElement> { }

    public class SkillLayoutAndElementEvent : UnityEvent<SkillLayout, SkillLayoutElement> { }
}