using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.Events;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using YounGenTech.Entities;

namespace Quadrablaze.Skills {
    public class UISkillButton : MonoBehaviour {

        public SkillLayout skillLayout;
        public SkillLayoutElement skillLayoutElement;
        public GamePlayerInfo playerInfo;

        public Toggle toggleSkill;
        public Button selectSkillButton;
        public Button upgradeSkillButton;

        public TextMeshProUGUI skillText;
        public TextMeshProUGUI levelText;

        public Image icon;

        [Header("Acquiring")]
        public Image glowImage;

        [SerializeField]
        bool _acquired;

        public Color acquiredColor = Color.green;
        public Color notAcquiredColor = Color.red;
        public Color canNotAcquireColor = Color.gray;

        bool initialized;
        UnityAction<PlayerEntity> onChangedSkillPointsMethod;

        #region Properties
        public bool Acquired {
            get { return _acquired; }
            set {
                _acquired = value;

                RefreshAll();
            }
        }

        public bool CanAcquire {
            get { return skillLayout != null && !skillLayout.HasSkill(skillLayoutElement) && playerInfo && playerInfo.AttachedEntity.CurrentUpgradeSet.SkillPoints > 0; }
        }
        #endregion

        void OnDisable() {
            if(onChangedSkillPointsMethod != null) {
                PlayerProxy.Proxy.Unsubscribe(PlayerActions.ChangedSkillPoints, PlayerProxy_ChangedSkillPoints);
                onChangedSkillPointsMethod = null;
            }
        }

        void OnEnable() {
            if(initialized) return;

            initialized = true;

            onChangedSkillPointsMethod = entity => { if(entity.Owner) RefreshAll(); }; // TODO: This should probably be done somewhere else

            PlayerProxy.Proxy.Subscribe(PlayerActions.ChangedSkillPoints, PlayerProxy_ChangedSkillPoints);

            if(glowImage)
                if(CanAcquire)
                    glowImage.color = Acquired ? acquiredColor : notAcquiredColor;
                else
                    glowImage.color = canNotAcquireColor;

            RefreshIcon();
        }

        void PlayerProxy_ChangedSkillPoints(System.EventArgs args) {
            var entity = ((EntityArgs)args).GetEntity<PlayerEntity>();

            if(entity.Owner) 
                RefreshAll();
        }

        public void RefreshAll() {
            RefreshText();
            RefreshGlowImage();
            RefreshIcon();
        }

        public void RefreshText() {
            if(skillLayoutElement != null)
                SetText(skillLayoutElement.OriginalLayoutElement.DisplayName, skillLayoutElement.CurrentLevel, skillLayoutElement.OriginalLayoutElement.LevelCap);
        }

        public void RefreshGlowImage() {
            if(glowImage)
                if(Acquired)
                    glowImage.color = acquiredColor;
                else
                    glowImage.color = CanAcquire ? notAcquiredColor : canNotAcquireColor;
        }

        public void RefreshIcon() {
            if(icon && skillLayoutElement != null) {
                icon.sprite = skillLayoutElement.OriginalLayoutElement.SkillIcon;
                icon.color = Color.white;
            }
        }

        public void Reset() {
            skillLayout = null;
            skillLayoutElement = null;
            playerInfo = null;
            glowImage.color = canNotAcquireColor;
            Acquired = false;
        }

        public void SetText(string skill, int level, int levelCap) {
            if(skillText) skillText.text = skill;

            if(levelText)
                if(skillLayoutElement.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor)
                    levelText.text = level.ToString();
                else
                    levelText.text = level.ToString() + "/" + levelCap.ToString();
        }

        public void UpdateAbilityIcon(bool setInput = true) {
            if(Acquired && !UIAbilityBar.Current.ContainsIcon(skillLayoutElement)) {
                UIAbilityBar.Current.AddIcon(skillLayout, skillLayoutElement);

                if(setInput) {
                    var actionIndex = UIManager.Current.abilityWheel.GetUnusedActionInputIndex();

                    if(actionIndex == -1) {
                        var activeSkillCount = playerInfo.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.ActiveSkills.Count;

                        if(activeSkillCount > 0 && activeSkillCount <= 4)
                            actionIndex = activeSkillCount - 1;
                    }

                    if(actionIndex > -1)
                        if(!skillLayoutElement.Passive && !(skillLayoutElement.OriginalLayoutElement.CreatesExecutor is ScriptableWeaponSkillExecutor))
                            UIManager.Current.abilityWheel.SetInputToAbility(actionIndex, skillLayoutElement);
                }
            }
        }

        public void UpgradeSkill() {
            var networkWriter = new NetworkWriter();

            networkWriter.StartMessage(NetMessageType.Server_UpgradeSkill);
            networkWriter.Write(playerInfo.gameObject);
            networkWriter.Write(skillLayout.Elements.IndexOf(skillLayoutElement));
            networkWriter.FinishMessage();

            QuadrablazeSteamNetworking.Current.MyClient.connection.SendWriter(networkWriter, 0);
        }

        //#if UNITY_EDITOR
        //        void OnValidate() {
        //            RefreshAll();
        //        }
        //#endif
    }
}