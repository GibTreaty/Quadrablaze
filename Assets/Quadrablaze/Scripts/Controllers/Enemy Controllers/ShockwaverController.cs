using System;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class ShockwaverController : MonoBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill, ISpecialAbility, ITelegraphHandler {

        [SerializeField]
        Transform _shockwaveGlow;

        [SerializeField]
        TriggerUnityEvent _shockwaveTrigger;

        [SerializeField]
        GameObject _telegraphObject;

        #region Properties
        EnemyInput EnemyInputComponent { get; set; }

        Shockwave ShockwaveExecutor { get; set; }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            EnemyInputComponent = GetComponent<EnemyInput>();

            _shockwaveTrigger.onTrigger.AddListener(s => ShockwaveTriggered());
            EnemyInputComponent.onTelegraphStart.AddListener(StartTelegraph);
            EnemyInputComponent.onTelegraphEnd.AddListener(EndTelegraph);

            SetTelegraphState(false);
        }

        void EndTelegraph() {
            if(ShockwaveExecutor != null)
                ShockwaveExecutor.Invoke();
            else
                Debug.LogError("ShockwaveExecutor does not exist :(");

            this.SendEnemyUsedSpecialAbility();

            SetTelegraphState(false);
            TelegraphStateHandler.SendTelegraphState(gameObject, false);
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Shockwave executor)
                ShockwaveExecutor = executor;
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            _telegraphObject.SetActive(enable);
        }

        void ShockwaveTriggered() {
            if(NetworkServer.active)
                if(ShockwaveExecutor != null)
                    if(ShockwaveExecutor.CooldownTimer == 0 && EnemyInputComponent.TelegraphTimer == 0)
                        EnemyInputComponent.DelayTelegraphTimer();
        }

        void StartTelegraph() {
            SetTelegraphState(true);
            TelegraphStateHandler.SendTelegraphState(gameObject, true);
        }

        void Update() {
            if(ShockwaveExecutor != null)
                _shockwaveGlow.localScale = Vector3.one * Mathf.Lerp(1, ShockwaveExecutor.OriginalSkillExecutor.Radius * 2, 1 - (ShockwaveExecutor.CooldownTimer.NormalizedTime > .1f ? 1 : ShockwaveExecutor.CooldownTimer.NormalizedTime / .1f));
        }

        public void UseSpecialAbility() {
            if(ShockwaveExecutor != null)
                ShockwaveExecutor.InvokeSkillNonAuthority();
        }
    }
}