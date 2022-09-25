using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class BlazeExploderController : MonoBehaviour, IActorEntityObjectInitialize, IActorEntityObjectAssignedSkill, ISpecialAbility, ITelegraphHandler {

        [SerializeField]
        TriggerUnityEvent _blazeTrigger;

        [SerializeField]
        GameObject _telegraphObject;

        [SerializeField]
        Transform _blazeGlow;

        #region Properties
        EnemyInput EnemyInputComponent { get; set; }

        Blaze BlazeExecutor { get; set; }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            EnemyInputComponent = GetComponent<EnemyInput>();

            _blazeTrigger.onTrigger.AddListener(s => BlazeTriggered());
            EnemyInputComponent.onTelegraphStart.AddListener(StartTelegraph);
            EnemyInputComponent.onTelegraphEnd.AddListener(EndTelegraph);

            SetTelegraphState(false, 1);
        }

        void BlazeTriggered() {
            if(NetworkServer.active)
                if(BlazeExecutor != null)
                    if(BlazeExecutor.CooldownTimer == 0 && EnemyInputComponent.TelegraphTimer == 0)
                        EnemyInputComponent.DelayTelegraphTimer();
        }

        void EndTelegraph() {
            if(BlazeExecutor != null)
                BlazeExecutor.Invoke();
            else
                Debug.LogError("BlazeExecutor does not exist :(");

            this.SendEnemyUsedSpecialAbility();

            SetTelegraphState(false, 1);
            TelegraphStateHandler.SendTelegraphState(gameObject, false, 1);
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Blaze executor)
                BlazeExecutor = executor;
        }

        void OnDisable() {
            BlazeExecutor = null;
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            if(extraData == 1)
                _telegraphObject.SetActive(enable);
        }

        void StartTelegraph() {
            SetTelegraphState(true, 1);
            TelegraphStateHandler.SendTelegraphState(gameObject, true, 1);
        }

        void Update() {
            if(BlazeExecutor != null)
                _blazeGlow.localScale = Vector3.one * Mathf.Lerp(1, BlazeExecutor.OriginalSkillExecutor.Radius * 2, 1 - (BlazeExecutor.CooldownTimer.NormalizedTime > .1f ? 1 : BlazeExecutor.CooldownTimer.NormalizedTime / .1f));
        }

        public void UseSpecialAbility() {
            if(BlazeExecutor != null)
                BlazeExecutor.InvokeSkillNonAuthority();
        }
    }
}