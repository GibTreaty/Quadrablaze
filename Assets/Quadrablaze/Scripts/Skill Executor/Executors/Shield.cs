using Quadrablaze.Entities;
using StatSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze.SkillExecutors {
    public class Shield : SkillExecutor, ISkillUpdate, IStatChanged {

        public const int ShieldHealthMultiplier = 5;

        float lastHealTime;

        public CappedStat Health { get; }
        public float LastHealTime => lastHealTime;
        public new ScriptableShieldSkillExecutor OriginalSkillExecutor { get; }
        public EventTimer RegenerationTimer { get; }
        public bool ShieldActive { get; set; }
        public Collider ShieldCollider { get; set; }
        public MeshRenderer ShieldMesh { get; set; }
        public bool WasFullyRegenerated { get; private set; }

        public Shield(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableShieldSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            RegenerationTimer = new EventTimer(OriginalSkillExecutor.RegenerationLength) { AutoDisable = true };

            //TODO: Change shield health based on ability level
            Health = new CappedStat(0, ShieldHealthMultiplier, ShieldHealthMultiplier);
        }

        public void AnimateMaterial(float value) {
            if(!ShieldMesh) return;

            Color color = ShieldMesh.material.GetColor("_PrimaryColor");

            color.a = value;

            //TODO: Change to using a material property block instead

            ShieldMesh.material.SetColor("_PrimaryColor", color);
        }

        public override void ApplyToGameObject(GameObject gameObject) {
            var shieldCollider = CurrentActorEntity.CurrentTransform.Find(OriginalSkillExecutor.ShieldColliderPath);
            var shieldMesh = CurrentActorEntity.CurrentTransform.Find(OriginalSkillExecutor.ShieldMeshPath);

            ShieldCollider = shieldCollider.GetComponent<Collider>();
            ShieldMesh = shieldMesh.GetComponent<MeshRenderer>();

            UpdateShieldActive();
        }

        public override void LevelChanged(int level, int previousLevel) {
            RegenerationTimer.Active = false;

            var health = ShieldHealthMultiplier * level;

            Health.ModifyMaxValue(health, this, this, false);
            Health.ModifyValue(health, this, this, false);

            UpdateShieldActive();
            WasFullyRegenerated = true;
        }

        public void OnStatChanged(StatEvent statEvent) {
            var stat = statEvent.AffectedStat as CappedStat;
            var statMessage = statEvent.GetMessage();

            switch(statMessage) {
                case StatChangeValueMessage message: {
                    //if(CurrentActorEntity is PlayerEntity playerEntity) {
                    //    PlayerProxy.OnShieldChangedHealth.Invoke(playerEntity);

                    //    //TODO: Recode the health syncing
                    //    //if(NetworkServer.active)
                    //    //    if(CurrentActorEntity.CurrentGameObject != null && CurrentActorEntity.CurrentGameObject.activeInHierarchy)
                    //    //        PlayerActor.NetworkSendHealthValueToAll(CurrentActorEntity.CurrentGameObject, NetMessageType.Client_PlayerSetShieldHealth, Health.Value);
                    //}

                    if(message.AmountChanged > 0) { // Heal
                        if(stat.Value == stat.MaxValue) { //  Restored
                            WasFullyRegenerated = true;
                        }
                        //else { // Increment

                        //}
                    }
                    else { // Damage

                        WasFullyRegenerated = false;
                        RegenerationTimer.Start(true);
                        UpdateShieldActive();

                        //if(stat.Value == 0) { // Death
                        //    WasFullyRegenerated = false;
                        //}
                        //else { // Increment
                        //    RegenerationTimer.Start(true);
                        //    UpdateShieldActive();
                        //}
                    }

                    break;
                }

                case CappedChangeMaxValue message: {
                    if(CurrentActorEntity is PlayerEntity playerEntity) {
                        CurrentActorEntity.Listener.RaiseEvent(EntityActions.ChangedStatMax, new StatEventArgs(statEvent));

                        //if(NetworkServer.active)
                        //    if(CurrentActorEntity.CurrentGameObject != null && CurrentActorEntity.CurrentGameObject.activeInHierarchy)
                        //        PlayerActor.NetworkSendHealthValueToAll(CurrentActorEntity.CurrentGameObject, NetMessageType.Client_PlayerSetShieldMaxHealth, Health.MaxValue);
                    }

                    break;
                }
            }
        }

        public void SetShieldActive(bool active) {
            if(ShieldCollider != null) ShieldCollider.enabled = active;
            if(ShieldMesh != null) ShieldMesh.enabled = active;

            if(ShieldActive == active) return;

            ShieldActive = active;

            if(CameraSoundController.Current != null) {
                CameraSoundController.Current.StopSounds();

                if(active) CameraSoundController.Current.PlaySound("Shield Activate");
                else CameraSoundController.Current.PlaySound("Shield Deactivate");
            }
        }

        public void SkillUpdate() {
            UpdateShieldRegeneration();
            AnimateMaterial(Health.NormalizedValue);
        }

        public override void Unload() {
            SetShieldActive(false);
        }

        public void UpdateShieldStatus() {
            UpdateShieldRegeneration();
            UpdateShieldActive();
        }

        void UpdateShieldRegeneration() {
            RegenerationTimer.Update();

            if(!RegenerationTimer.Active && Health.NormalizedValue < 1 && Time.time > lastHealTime + (1f / CurrentLayoutElement.CurrentLevel)) {
                lastHealTime = Time.time;
                this.DoHealthChange(1, this, true);
                //ShieldHealth.Heal(new HealthEvent(CurrentActorEntity.CurrentGameObject, ShieldHealth.MaxValue * (1 / 3f) * Time.deltaTime));
                UpdateShieldActive();
            }
        }

        public void UpdateShieldActive() {
            SetShieldActive(CurrentLayoutElement.CurrentLevel > 0 && Health.Value > 0);
        }

        [GameNetworkListener]
        static void ReceiveShieldValue(NetworkMessage message) {
            var input = message.ReadMessage<ShieldMessage>();

            if(GameManager.Current.GetActorEntity(input.id) is ActorEntity entity) {
                if(entity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Shield>() is Shield executor) {
                    if(input.maxValue)
                        executor.Health.MaxValue = input.value;
                    else
                        executor.Health.Value = input.value;
                }
            }
        }

        class ShieldMessage : EntityMessageBase {

            public int value;
            public bool maxValue;

            public ShieldMessage() { }

            public ShieldMessage(NetworkMessageDelegate method, uint id, int value, bool maxValue) : base(method, id) {
                this.value = value;
                this.maxValue = maxValue;
            }

            public override void Deserialize(NetworkReader reader) {
                base.Deserialize(reader);

                value = reader.ReadInt32();
                maxValue = reader.ReadBoolean();
            }

            public override void Serialize(NetworkWriter writer) {
                base.Serialize(writer);

                writer.Write(value);
                writer.Write(maxValue);
            }
        }

    }
}