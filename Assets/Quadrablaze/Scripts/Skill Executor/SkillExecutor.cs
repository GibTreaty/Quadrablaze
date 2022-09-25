using UnityEngine;
using Quadrablaze.Skills;
using UnityEngine.Networking;
using UnityEngine.Events;
using Quadrablaze.Entities;

namespace Quadrablaze {
    public class SkillExecutor {

        public ActorEntity CurrentActorEntity { get; set; }
        public SkillLayoutElement CurrentLayoutElement { get; set; }
        public virtual bool IsReady => true;
        public ScriptableSkillExecutor OriginalSkillExecutor => CurrentLayoutElement.OriginalLayoutElement.CreatesExecutor;

        public SkillExecutor(ActorEntity actorEntity, SkillLayoutElement element) {
            CurrentActorEntity = actorEntity;
            CurrentLayoutElement = element;
        }

        public virtual void ApplyToGameObject(GameObject gameObject) { } //TODO: Call this when the actor sets their GameObject

        public virtual void ClearGameObject(GameObject gameObject) { }

        protected void GiveSkillFeedback(bool sendToHost, params byte[] parameters) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_OnSkillFeedback);
            writer.Write(CurrentActorEntity.Id);
            writer.Write(CurrentLayoutElement.ElementIndex);

            int length = parameters != null ? parameters.Length : 0;
            writer.Write(length);

            for(int i = 0; i < length; i++)
                writer.Write(parameters[i]);

            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
        }

        public virtual void Invoke() {
            if(NetworkServer.active) {
                InvokeSkillServer();
            }
            else if(NetworkClient.active) {
                if((CurrentActorEntity is PlayerEntity playerEntity) && playerEntity.Owner)
                    InvokeSkillAuthority();
                else
                    InvokeSkillNonAuthority();
            }
        }
        public virtual void InvokeSkillAuthority() { }
        public virtual void InvokeSkillNonAuthority() { }
        public virtual void InvokeSkillServer() { }

        public virtual void LevelChanged(int level, int previousLevel) { }

        public virtual void OnSkillFeedback(byte[] parameters) { }

        /// <summary>Remove attachments from GameObject and unsubscribe events</summary>
        public virtual void Unload() { } //TODO: Call this when the actor loses their GameObject

        static void SkillFeedback(NetworkMessage message) {
            var entityId = message.reader.ReadUInt32();
            var entity = GameManager.Current.GetActorEntity(entityId);
            var upgradeSet = entity.CurrentUpgradeSet;

            var elementIndex = message.reader.ReadInt32();
            var executor = upgradeSet.CurrentSkillLayout.GetExecutor(elementIndex);

            var parameterLength = message.reader.ReadInt32();
            var parameters = new byte[parameterLength];

            for(int i = 0; i < parameterLength; i++)
                parameters[i] = message.reader.ReadByte();

            if(executor != null)
                executor.OnSkillFeedback(parameters);
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_OnSkillFeedback, SkillFeedback);
        }

        [System.Serializable]
        public class SkillExecutorEvent : UnityEvent<SkillExecutor> { }
    }
}