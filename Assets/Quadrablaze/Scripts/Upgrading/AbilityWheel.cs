using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Networking;

// TODO Abilities in the Upgrade Menu need to show which button goes to which skill. They are currently hard-set to use certain input actions.

namespace Quadrablaze {
    public class AbilityWheel : MonoBehaviour {

        public static AbilityWheel Current { get; private set; }

        public static event Action<int, string> OnUpdateActionInput;

        Player RewiredPlayer { get; set; }

        public string[] SkillIds { get; private set; }

        public void Initialize() {
            Current = this;
            RewiredPlayer = ReInput.players.GetPlayer(0);

            SkillIds = new string[4];
            /*OnUpdateActionInput = new Action<string>()*/
        }

        public int GetUnusedActionInputIndex() {
            for(int i = 0; i < SkillIds.Length; i++)
                if(string.IsNullOrEmpty(SkillIds[i]))
                    return i;

            return -1;
        }

        public string GetSkillId(int index) {
            return SkillIds[index];
        }

        public int GetSkillIndex(string skillId) {
            if(!string.IsNullOrEmpty(skillId))
                for(int i = 0; i < SkillIds.Length; i++)
                    if(SkillIds[i] == skillId) return i;

            return -1;
        }

        SkillLayoutElement GetSelectedAbilityElement() {
            SkillLayoutElement element = null;

            var captureSelectedAbility = UIManager.Current.abilityPieWheel.GetCurrentSlice();

            if(captureSelectedAbility > 0)
                if(UIManager.Current.abilityPieWheel.GetDeadzonedSelection().sqrMagnitude > 0)
                    element = QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.GetActiveSkill(captureSelectedAbility - 1);
            //element = QuadrablazeSteamNetworking.Current.GetPlayer(0).playerInfo.CurrentUpgradeSet.CurrentSkillLayout.GetActiveSkill(captureSelectedAbility - 1);

            return element;
        }

        public bool HasAssignedSkill(int index) {
            return !string.IsNullOrEmpty(SkillIds[index]);
        }

        static void InvokeUpdateActionInput(int index, string skillId) {
            Debug.Log("InvokeUpdateActionInput(" + skillId + ")");
            OnUpdateActionInput?.Invoke(index, skillId);
        }

        public void PollForBind() {
            int index = -1;

            if(RewiredPlayer.GetButtonDown("Action 1"))
                index = 0;
            else if(RewiredPlayer.GetButtonDown("Action 2"))
                index = 1;
            else if(RewiredPlayer.GetButtonDown("Action 3"))
                index = 2;
            else if(RewiredPlayer.GetButtonDown("Action 4"))
                index = 3;

            if(index > -1) {
                var currentSkillIndex = -1;
                var skill = GetSelectedAbilityElement();

                if(skill == null) return;

                var skillId = skill.OriginalLayoutElement.name;
                var previousSkillId = SkillIds[index];

                for(int i = 0; i < SkillIds.Length; i++)
                    if(SkillIds[i] == skillId) { // Currently selected ability on the wheel
                        currentSkillIndex = i;
                        break;
                    }

                if(currentSkillIndex > -1) { // Swap two used action keys
                    ResetSkill(currentSkillIndex);

                    var existingSkillId = SkillIds[index];

                    if(!string.IsNullOrEmpty(existingSkillId))
                        SetInputToAbility(currentSkillIndex, existingSkillId);
                }
                else if(!string.IsNullOrEmpty(previousSkillId)) {
                    ResetSkill(index);
                    InvokeUpdateActionInput(-1, previousSkillId);
                }

                SetInputToAbility(index, skillId);
                UpdateActionGlyphs();
            }
        }

        public void ResetSkill(int index) {
            SkillIds[index] = "";
        }

        public void ResetSkills() {
            for(int i = 0; i < SkillIds.Length; i++)
                SkillIds[i] = "";
        }

        public void SetInputToAbility(int index, SkillLayoutElement element) {
            SetInputToAbility(index, element.OriginalLayoutElement.name);
        }
        public void SetInputToAbility(int index, string skillId) {
            SkillIds[index] = skillId;
            InvokeUpdateActionInput(index, skillId);
        }

        // TODO This will need to be called when using Ability Layout Sheets later on
        public void UpdateActionGlyphs() {
            if(!RoundManager.RoundInProgress) {
                for(int i = 0; i < 4; i++) {
                    var glyphTransform = UIManager.Current.abilityPieWheel.ActionGlyphs[i];

                    if(glyphTransform != null)
                        glyphTransform.gameObject.SetActive(false);
                }

                return;
            }

            if(!(NetworkServer.active || NetworkClient.active)) return;
            if(QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity == null) return;

            var activeSkills = QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.ActiveSkills;

            for(int i = 0; i < SkillIds.Length; i++) {
                var glyphTransform = UIManager.Current.abilityPieWheel.ActionGlyphs[i];

                glyphTransform.gameObject.SetActive(false);

                if(activeSkills != null)
                    if(!string.IsNullOrEmpty(SkillIds[i])) {
                        var skillId = SkillIds[i];
                        var skillIndex = activeSkills.IndexOf(skillId);
                        var iconTransform = UIManager.Current.abilityPieWheel.GetIcon(skillIndex).transform as RectTransform;
                        var iconRect = GetWorldRect(iconTransform);

                        glyphTransform.position = iconRect.min + new Vector2(iconRect.size.x * .5f, glyphTransform.rect.size.y * .5f);

                        if(UIManager.Current.AbilityWheelOpen)
                            glyphTransform.gameObject.SetActive(true);
                    }
            }
        }


        static public Rect GetWorldRect(RectTransform rectTransform) {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector3 topLeft = corners[0];
            //var scaledSize = new Vector2(scale.x * rectTransform.rect.size.x, scale.y * rectTransform.rect.size.y);
            //var scaledSize = new Vector2(rectTransform.rect.size.x, rectTransform.rect.size.y);

            var scaledSize = new Vector2(Mathf.Abs(corners[0].x - corners[2].x), Mathf.Abs(corners[0].y - corners[1].y));

            return new Rect(topLeft, scaledSize);
        }
    }
}