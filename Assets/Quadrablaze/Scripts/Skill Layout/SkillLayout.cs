using System;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using Quadrablaze.Skills;
using UnityEngine;

namespace Quadrablaze {
    public class SkillLayout {

        public List<string> ActiveSkills { get; protected set; }
        public ActorEntity CurrentEntity { get; set; }
        public ScriptableSkillLayout OriginalLayout { get; set; }
        public List<string> OrderedSkills { get; protected set; }
        public List<SkillLayoutElement> Elements { get; protected set; }

        public SkillLayout(ScriptableSkillLayout originalLayout, List<SkillLayoutElement> elements) {
            ActiveSkills = new List<string>();
            OriginalLayout = originalLayout;
            OrderedSkills = new List<string>();
            Elements = new List<SkillLayoutElement>(elements);

            foreach(var element in Elements)
                element.Initialize(this);
        }

        SkillExecutor AssignSkill(SkillLayoutElement element, bool reassigned = false) {
            if(element.CurrentExecutor != null) return element.CurrentExecutor;

            if(element.CreateExecutorInstance() is SkillExecutor executor) {
                if(!ActiveSkills.Contains(element.OriginalLayoutElement.name))
                    if(!element.Passive && !(element.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor))
                        ActiveSkills.Add(element.OriginalLayoutElement.name);

                if(!OrderedSkills.Contains(element.OriginalLayoutElement.name))
                    OrderedSkills.Add(element.OriginalLayoutElement.name);

                if(element.CurrentLevel > 0)
                    executor.LevelChanged(element.CurrentLevel, reassigned ? element.CurrentLevel : 0);

                if(CurrentEntity.CurrentGameObject != null)
                    executor.ApplyToGameObject(CurrentEntity.CurrentGameObject);

                CurrentEntity.RaiseAssignedSkillEvent(element);

                return executor;
            }

            return null;
        }

        public SkillLayoutElement GetActiveSkill(int index) {
            var skillId = ActiveSkills[index];

            foreach(var element in Elements)
                if(element.OriginalLayoutElement.name == skillId) return element;

            return null;
        }

        public SkillLayoutElement GetElement(SkillExecutor executor) {
            foreach(var element in Elements)
                if(element.CurrentExecutor == executor)
                    return element;

            return null;
        }
        public SkillLayoutElement GetElement<T>() where T : SkillExecutor {
            foreach(var element in Elements)
                if(element.CurrentExecutor is T)
                    return element;

            return null;
        }
        public SkillLayoutElement GetElement<T>(int typeIndex) where T : SkillExecutor {
            foreach(var element in Elements)
                if(element.CurrentExecutor is T)
                    if(element.ElementTypeIndex == typeIndex)
                        return element;

            return null;
        }

        public SkillExecutor GetExecutor(int skillId) {
            foreach(var element in Elements)
                if(element.ElementIndex == skillId)
                    return element.CurrentExecutor;

            return null;
        }

        public T GetFirstAvailableExecutor<T>() where T : SkillExecutor {
            foreach(var element in Elements)
                if(element.CurrentExecutor is T executor)
                    return executor;

            return null;
        }

        public SkillLayoutElement GetSkill(string id) {
            foreach(var element in Elements)
                if(element.OriginalLayoutElement.name == id) return element;

            return null;
        }

        public bool HasSkill(SkillLayoutElement element) {
            return element.CurrentExecutor != null;
        }

        public bool HasAllSkills(params string[] elementSkillNames) {
            if(elementSkillNames.Length == 0) return true;

            var list = new List<string>(elementSkillNames);

            foreach(var element in Elements)
                if(elementSkillNames.Contains(element.OriginalLayoutElement.DisplayName))
                    list.Remove(element.OriginalLayoutElement.DisplayName);

            return list.Count == 0;
        }

        public void ReloadSkillExecutors(ActorEntity actorEntity) {
            List<SkillLayoutElement> copiedElements = new List<SkillLayoutElement>(Elements);

            foreach(var skill in OrderedSkills) {
                SkillLayoutElement selectedElement = null;

                foreach(var element in copiedElements)
                    if(element.OriginalLayoutElement.name == skill) {
                        AssignSkill(element, true);
                        selectedElement = element;
                        break;
                    }

                copiedElements.Remove(selectedElement);
            }
        }

        public void SetElementLevel(SkillLayoutElement element, int level) {
            if(element.CurrentExecutor is Weapon weaponExecutor)
                level = Mathf.Clamp(level, 0, Mathf.Min(element.OriginalLayoutElement.LevelCap, weaponExecutor.OriginalSkillExecutor.List.Weapons.Length));
            else
                level = Mathf.Clamp(level, 0, element.OriginalLayoutElement.LevelCap);

            if(level != element.CurrentLevel) {
                var previousLevel = element.CurrentLevel;

                element.SetLevel(level);

                if(!HasSkill(element)) AssignSkill(element);
                else element.CurrentExecutor.LevelChanged(level, previousLevel);

                var args = new SkillArgs(CurrentEntity.Id, element.ElementIndex, level);

                element.Listener.RaiseEvent(EntityActions.SkillLevelChanged, args);
                CurrentEntity.Listener.RaiseEvent(EntityActions.SkillLevelChanged, args);
            }
        }

        public bool Upgrade(SkillLayoutElement element) {
            if(!element.IsAtMaxLevel) {
                SetElementLevel(element, element.CurrentLevel + 1);

                return true;
            }

            return false;
        }
    }
}

public static partial class EntityActions {
    public static readonly ProxyAction SkillLevelChanged = new ProxyAction();
}

public class SkillArgs : EventArgs {
    public uint EntityId { get; set; }
    public int ElementIndex { get; set; }
    public int Level { get; set; }

    public SkillArgs(uint entityId, int elementIndex, int level) {
        EntityId = entityId;
        ElementIndex = elementIndex;
        Level = level;
    }
}