using System;
using System.Collections.Generic;
using Quadrablaze.Skills;
using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Skill Layout/Layout Element")]
    public class ScriptableLayoutElement : ScriptableObject {

        [SerializeField]
        Sprite _skillIcon;

        [SerializeField]
        ScriptableSkillExecutor _createsExecutor;

        [Header("Level Options")]
        [SerializeField]
        int _baseLevel;

        [SerializeField]
        float _baseModifier;

        [SerializeField]
        float _baseSkillAmount;

        [SerializeField]
        int _levelCap;

        [Header("Modifier Options")]
        [SerializeField]
        ModifierType _skillModifierType;

        [SerializeField]
        SetSkillAmount[] _setSkillAmounts;

        [Header("Descriptions")]
        [SerializeField, TextArea(true, true)]
        List<string> _levelDescriptions;

        public int BaseLevel => _baseLevel;
        public float BaseModifier => _baseModifier;
        public float BaseSkillAmount => _baseSkillAmount;
        public ScriptableSkillExecutor CreatesExecutor => _createsExecutor;
        public int LevelCap => _levelCap;
        public List<string> LevelDescriptions => _levelDescriptions;
        public SetSkillAmount[] SetSkillAmounts => _setSkillAmounts;
        public Sprite SkillIcon => _skillIcon;
        public string DisplayName => _createsExecutor.name;
        public ModifierType SkillModifierType => _skillModifierType;

        public SkillLayoutElement CreateInstance() {
            var element = new SkillLayoutElement() {
                //CurrentLevel = _baseLevel,
                OriginalLayoutElement = this
            };

            return element;
        }
    }
}