using Quadrablaze.Boss;
using Quadrablaze.Skills;
using StatSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze.Entities {
    public class BossEntity : EnemyEntity {
        const float StageUpDelay = .1f;

        public int NextStage { get; set; }
        public int Stage { get; set; }
        public float StageUpTime { get; set; }
        public float TotalHealth { get; private set; }

        public BossEntity(GameObject gameObject, string name, uint id, UpgradeSet upgradeSet, float size) : base(gameObject, name, id, upgradeSet, size) {
            BossInfoUI.Current.SetHealth(1);
        }

        public void Defeat() {
            SetStage(4);
            EnemyProxy.SpawnedBoss = null;
        }

        public override void EntityUpdate() {
            base.EntityUpdate();

            if(NetworkServer.active) {
                if(StageUpTime > 0) {
                    StageUpTime = Mathf.Max(StageUpTime - Time.deltaTime, 0);

                    if(StageUpTime == 0)
                        SetStage(NextStage);
                }

                if(CurrentGameObject != null)
                    if(StageUpTime == 0)
                        if(CurrentGameObject.GetComponent<BossController>() is BossController controller)
                            controller.OnStageUpdate(Stage);
            }
        }

        /// <summary>Returns stage number based on health</summary>
        int GetStage() {
            return Mathf.FloorToInt(((1 - TotalHealth) * 3f) + 1);
        }

        public void GiveReward() {
            GameManager.Current.LevelUp();
        }

        public override void OnDamaged(StatEvent statEvent) {
            var healthSlotCount = HealthSlots.Length;
            var totalHealth = 0;
            var maxHealth = 0;

            for(int i = 0; i < healthSlotCount; i++) {
                var slot = HealthSlots[i];

                totalHealth += slot.Value;
                maxHealth += slot.MaxValue;
            }

            TotalHealth = (float)totalHealth / (float)maxHealth;

            BossInfoUI.Current.SetHealth(TotalHealth);

            int stageUpdate = GetStage();
            bool stageFlag = false;

            if(stageUpdate > Stage)
                if(NextStage != stageUpdate)
                    stageFlag = true;

            if(CurrentGameObject.GetComponent<BossController>() is BossController controller) {
                controller.OnDamaged(statEvent);

                if(stageFlag)
                    StartStageUp(stageUpdate);
            }

            if(TotalHealth <= 0) {
                UserComponent.SpawnHere("Explosions");
                CameraSoundController.Current.PlaySound("Boss Defeated", true);
                Defeat();
            }
        }

        // TODO: Boss - Send stage over network
        public void SetStage(int stage) {
            Stage = stage;

            var controller = CurrentGameObject.GetComponent<BossController>();

            if(controller != null)
                controller.SetStage(stage);

            if(stage == 4) {
                GiveReward();
                BossInfoUI.Current.OnBossDefeated();
                TimedBossSpawnController.InvokeBossDefeated();
                Kill();
            }
        }

        public virtual void StartStageUp(int nextStage) {
            if(StageUpTime > 0) return;
            if(Stage >= 3) return;
            if(Stage == nextStage) return;
            if(NextStage == nextStage) return;

            NextStage = nextStage;
            StageUpTime = StageUpDelay;
        }
    }
}