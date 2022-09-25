using System.Collections.Generic;
using UnityEngine;
using Quadrablaze.Skills;
using UnityEngine.Networking;
using UnityEngine.Events;
using Quadrablaze.Entities;

namespace Quadrablaze {
    public abstract class ScriptableSkillExecutor : ScriptableObject {
        #region Properties
        public ActorEntity CurrentActorEntity { get; set; }
        public virtual bool IsReady => true;
        public SkillLayoutElement LayoutElement { get; set; }

        public virtual bool Passive => true;
        #endregion

        public virtual SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new SkillExecutor(actorEntity, element);
        }

        protected void GiveSkillFeedback(bool sendToHost, params byte[] parameters) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_OnSkillFeedback);
            writer.Write(CurrentActorEntity.Id);
            writer.Write(LayoutElement.ElementIndex);

            int length = parameters != null ? parameters.Length : 0;
            writer.Write(length);

            for(int i = 0; i < length; i++)
                writer.Write(parameters[i]);

            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
        }

        //protected void GiveSkillFeedback(Actor actor, bool sendToHost, params byte[] parameters) {
        //    var writer = new NetworkWriter();

        //    writer.StartMessage(NetMessageType.Client_OnSkillFeedback);
        //    writer.Write(actor);
        //    writer.Write(LayoutElement.SkillId);

        //    int length = parameters != null ? parameters.Length : 0;
        //    writer.Write(length);

        //    for(int i = 0; i < length; i++)
        //        writer.Write(parameters[i]);

        //    writer.FinishMessage();

        //    QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
        //}

        public void Initialize(ActorEntity actor) {
            CurrentActorEntity = actor;

            Initialize();
        }

        protected virtual void Initialize() { }

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

        public virtual void Unload() { }
    }

    public interface IExecutorActorDisable {
        void OnActorDisabled();
    }
}