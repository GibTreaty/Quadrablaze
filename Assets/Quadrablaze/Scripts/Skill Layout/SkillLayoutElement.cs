using System;
using System.Collections.Generic;
using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;

namespace Quadrablaze {
    public class SkillLayoutElement {
        #region Properties
        public SkillExecutor CurrentExecutor { get; set; }

        public SkillLayout CurrentLayout { get; private set; }

        public int CurrentLevel { get; set; }

        public float CurrentSkillAmount {
            get { return GetSkillAmount(Mathf.Max(CurrentLevel - 1, 0)); }
        }

        /// <summary>Index of this element within the <see cref="CurrentLayout"/></summary>
        public int ElementIndex { get; set; }

        /// <summary>Index of this element with similar executors in the <see cref="CurrentLayout"/></summary>
        public int ElementTypeIndex { get; set; }

        public bool IsAtMaxLevel {
            get {
                return CurrentLevel >=
                  ((OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor weaponExecutor) ?
                  Mathf.Min(OriginalLayoutElement.LevelCap, weaponExecutor.List.Weapons.Length) :
                  OriginalLayoutElement.LevelCap);
            }
        }

        public ScriptableSkillLayout Layout { get; private set; }

        public ProxyListener<ProxyAction> Listener { get; private set; } = new ProxyListener<ProxyAction>();

        public float Modifier { get; set; }

        public ScriptableLayoutElement OriginalLayoutElement { get; set; }

        public bool Passive {
            get { return OriginalLayoutElement.CreatesExecutor != null ? OriginalLayoutElement.CreatesExecutor.Passive : true; }
        }
        #endregion

        public SkillExecutor CreateExecutorInstance() {
            if(OriginalLayoutElement.CreatesExecutor == null || CurrentExecutor != null) return null;

            CurrentExecutor = OriginalLayoutElement.CreatesExecutor.CreateInstance(CurrentLayout.CurrentEntity, this);

            return CurrentExecutor;
        }

        public string GetLevelDescription(int level) {
            return level > 0 && level <= OriginalLayoutElement.LevelDescriptions.Count ? OriginalLayoutElement.LevelDescriptions[level - 1] : "";
        }

        public float GetSkillAmount(int index) {
            switch(OriginalLayoutElement.SkillModifierType) {
                default: return OriginalLayoutElement.BaseSkillAmount + (Modifier * index);
                case ModifierType.Multiplier: return OriginalLayoutElement.BaseSkillAmount * (Modifier * index);
                case ModifierType.Set: return index < OriginalLayoutElement.SetSkillAmounts.Length ? OriginalLayoutElement.SetSkillAmounts[index].Amount : 0;
            }
        }

        public void Initialize(SkillLayout layout) {
            CurrentLayout = layout;
        }

        public void SetLevel(int level) {
            CurrentLevel = Mathf.Clamp(level, 0, OriginalLayoutElement.LevelCap);
        }

        //public bool Upgrade() {
        //    if(!IsAtMaxLevel) {
        //        SetLevel(_skillData.CurrentLevel + 1);

        //        return true;
        //    }

        //    return false;
        //}
    }
}