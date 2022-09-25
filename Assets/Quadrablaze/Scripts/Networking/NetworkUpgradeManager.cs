using System;
using System.Collections;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class NetworkUpgradeManager : NetworkBehaviour {

        public static NetworkUpgradeManager Current { get; private set; }

        public void Initialize() {
            Current = this;

            QuadrablazeSteamNetworking.Current.OnConnect.AddListener(OnConnect);
        }

        void OnConnect() {
            if(NetworkServer.active) {
                NetworkServer.RegisterHandler(NetMessageType.Server_UpgradeSkill, UpgradePlayerSkill);
                NetworkServer.RegisterHandler(NetMessageType.Server_UseAbility, Server_UseAbility);
                NetworkServer.RegisterHandler(NetMessageType.Server_GiveSkillPoints, Server_GiveSkillPoints);
            }

            if(NetworkClient.active) {
                QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(NetMessageType.Client_UseAbility, Client_UseAbility);

                QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(NetMessageType.Client_UpgradeManagerUpgradeLevel, Client_UpgradeLevel);
                QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(NetMessageType.Client_UpgradeManagerSetLevel, Client_SetLevel);
                QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(NetMessageType.Client_UpgradeManagerSetSkillPoints, Client_SetSkillPoints);
                QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(NetMessageType.Client_UpgradeManagerSetXP, Client_SetXP);

                if(!NetworkServer.active)
                    QuadrablazeSteamNetworking.Current.MyClient.RegisterHandler(NetMessageType.Client_EnemyUseSpecialAbility, EnemyUseSpecialAbility);
            }
        }

        void Client_SetLevel(NetworkMessage message) {
            var amount = message.reader.ReadInt32();

            GameManager.Current.SetLevel(amount);
        }

        void Client_SetSkillPoints(NetworkMessage message) {
            var playerObject = message.reader.ReadGameObject();
            var amount = message.reader.ReadInt32();

            SetPlayerSkillPoints(playerObject, amount);
        }

        void Client_SetXP(NetworkMessage message) {
            var amount = message.reader.ReadUInt32();

            GameManager.Current.SetXP(amount);
        }

        void Client_UpgradeLevel(NetworkMessage message) {
            var entityId = message.reader.ReadUInt32();
            var index = message.reader.ReadByte();
            var level = message.reader.ReadInt32();

            SetUpgradeLevel(entityId, index, level);
        }

        void Client_UseAbility(NetworkMessage message) {
            //var abilityType = message.reader.ReadByte();
            //var playerObject = message.reader.ReadGameObject();

            //var elementIndex = message.reader.ReadInt32();
            //var actor = playerObject.GetComponent<Actor>();
            //var upgradeSet = actor.CurrentUpgradeSet;

            //TODO: Does this still work?
            //if(upgradeSet != null) {
            //    var element = upgradeSet.CurrentSkillLayout.SkillElements[elementIndex];

            //    if(upgradeSet.CurrentSkillLayout.SkillLookupTable.TryGetValue(element, out SkillExecutor skillExecutor))
            //        if(skillExecutor.IsReady)
            //            skillExecutor.Invoke();
            //}
        }

        void Client_EnemyEquipWeapon(NetworkMessage netMsg) {

        }

        void EnemyUseSpecialAbility(NetworkMessage netMsg) {
            var gameObject = netMsg.reader.ReadGameObject();

            gameObject?.GetComponent<ISpecialAbility>()?.UseSpecialAbility();
        }

        void Client_EnemyUsedAbility(NetworkMessage netMsg) {
            var gameObject = netMsg.reader.ReadGameObject();

            if(gameObject != null)
                foreach(var messagedObject in gameObject.GetComponentsInChildren<INetworkObjectMessaged>())
                    messagedObject.InvokeNetworkMessage(netMsg);
        }

        [Server]
        public void UpgradePlayerSkill(NetworkMessage netMsg) {
            var gameObject = netMsg.reader.ReadGameObject();
            var index = netMsg.reader.ReadByte();

            var playerInfo = gameObject.GetComponent<GamePlayerInfo>();
            var upgradeSet = playerInfo.AttachedEntity.CurrentUpgradeSet;

            if(playerInfo != null) {
                var element = upgradeSet.CurrentSkillLayout.Elements[index];

                if(upgradeSet.SkillPoints > 0) {
                    if(upgradeSet.CurrentSkillLayout.Upgrade(element))
                        playerInfo.RemoveSkillPoints(1);

                    Server_SetUpgradeLevel(playerInfo.AttachedEntity, index, element.CurrentLevel);
                    Server_SetPlayerSkillPoints(gameObject, playerInfo.AttachedEntity.CurrentUpgradeSet.SkillPoints);
                }
            }
            //TODO: Does this even get called anyways?
            //else {
            //    var actor = gameObject.GetComponent<Actor>();

            //    if(actor != null) {
            //        var element = actor.CurrentUpgradeSet.CurrentSkillLayout.SkillElements[index];

            //        if(actor.CurrentUpgradeSet.CurrentSkillLayout.Upgrade(element, actor, actor.CurrentUpgradeSet))
            //            Server_SetUpgradeLevel(gameObject, index, element.CurrentLevel);
            //    }
            //}
        }

        public void SetUpgradeLevel(uint entityId, int index, int level) {
            SetUpgradeLevel(GameManager.Current.GetActorEntity(entityId), index, level);
        }
        public void SetUpgradeLevel(ActorEntity entity, int index, int level) {
            if(entity.CurrentUpgradeSet == null) return;

            var skillLayout = entity.CurrentUpgradeSet.CurrentSkillLayout;
            var element = skillLayout.Elements[index];

            if(!NetworkServer.active)
                skillLayout.SetElementLevel(element, level);

            bool ownerFlag = false;

            if(QuadrablazeSteamNetworking.Current.MyPlayerInfo != null)
                if(QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity != null)
                    if(entity == QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity)
                        ownerFlag = true;

            if(ownerFlag) {
                var button = InGameUIManager.Current.GetSkillButton(element);

                if(button != null) {
                    bool alreadyAcquired = button.Acquired;

                    button.Acquired = true;

                    if(!alreadyAcquired)
                        button.UpdateAbilityIcon();

                    if(entity is PlayerEntity playerEntity)
                        playerEntity.UpdateAbilityWheelIcons();
                }
            }
        }

        public void SetPlayerSkillPoints(GameObject gameObject, int skillPoints) {
            if(!gameObject) return;

            var playerInfo = gameObject.GetComponent<GamePlayerInfo>();

            if(!NetworkServer.active) {
                playerInfo.AttachedEntity.CurrentUpgradeSet.SkillPoints = skillPoints;
                playerInfo.AttachedEntity?.Listener.RaiseEvent(PlayerActions.ChangedSkillPoints, playerInfo.AttachedEntity.ToArgs());
            }

            if(playerInfo.hasAuthority)
                InGameUIManager.Current.RefreshSkillPointText();
        }

        void Server_GiveSkillPoints(NetworkMessage message) {
            var playerObject = message.reader.ReadGameObject();
            var amount = message.reader.ReadInt32();

            if(amount > 0) {
                var playerInfo = playerObject.GetComponent<GamePlayerInfo>();

                if(playerInfo.AttachedEntity == null) return;

                int currentSkillPoints = playerInfo.AttachedEntity.CurrentUpgradeSet.SkillPoints;

                playerInfo.SetSkillPoints(currentSkillPoints + amount);
            }
        }

        public void Server_SetLevel(int level) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_UpgradeManagerSetLevel);
            writer.Write(level);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
        }

        public void Server_SetPlayerSkillPoints(GameObject gameObject, int skillPoints) {
            var writer = new NetworkWriter();
            //Debug.Log("Server_SetPlayerSkillPoints = " + skillPoints);
            writer.StartMessage(NetMessageType.Client_UpgradeManagerSetSkillPoints);
            writer.Write(gameObject);
            writer.Write(skillPoints);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer);
        }

        public void Server_SetXP(uint xp) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_UpgradeManagerSetXP);
            writer.Write(xp);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
        }

        public void Server_SetUpgradeLevel(ActorEntity entity, int index, int level) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_UpgradeManagerUpgradeLevel);
            writer.Write(entity.Id);
            writer.Write((byte)index);
            writer.Write(level);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriterToAll(writer);
        }

        public void GiveSkillPoints(GameObject playerObject, int amount) {
            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Server_GiveSkillPoints);
            writer.Write(playerObject);
            writer.Write(amount);
            writer.FinishMessage();

            QuadrablazeSteamNetworking.SendWriter(writer, Channels.DefaultReliable);
        }

        //[Command]
        //public void Cmd_GiveSkillPoints(GameObject playerObject, int amount) {
        //    Debug.Log("Cmd_GiveSkillPoints " + amount);
        //    var playerLevel = playerObject.GetComponent<PlayerLevel>();
        //    playerLevel.SkillPoints += amount;
        //    Rpc_SetPlayerSkillPoints(playerObject, playerLevel.SkillPoints);
        //}

        void Server_UseAbility(NetworkMessage message) {
            //Debug.Log("Server_UseAbility");

            var playerObject = message.reader.ReadGameObject();

            var elementIndex = message.reader.ReadInt32();
            //var actor = playerObject.GetComponent<Actor>();
            //var upgradeSet = actor.CurrentUpgradeSet;

            //TODO: Does this still work?
            //if(upgradeSet != null) {
            //    var element = upgradeSet.CurrentSkillLayout.SkillElements[elementIndex];

            //    if(upgradeSet.CurrentSkillLayout.SkillLookupTable.TryGetValue(element, out SkillExecutor skillExecutor)) {
            //        if(skillExecutor.IsReady) {
            //            skillExecutor.Invoke();

            //            if(QuadrablazeSteamNetworking.Current.SteamConnections.Count > 1) {
            //                var writer = new NetworkWriter();

            //                writer.StartMessage(NetMessageType.Client_UseAbility);
            //                writer.Write((byte)1);
            //                writer.Write(playerObject);
            //                writer.Write(elementIndex);
            //                writer.FinishMessage();

            //                QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
            //            }
            //        }
            //    }
            //}
        }
    }

    public static class NetworkUpgradeManagerHelper {
        public static void SendEnemyUsedSpecialAbility(this ISpecialAbility abilityUser) {
            if(!NetworkServer.active) return;

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_EnemyUseSpecialAbility);
            writer.Write(abilityUser.gameObject);
            writer.FinishMessage();

            foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                if(!player.isHost)
                    player.serverToClientConnection.SendWriter(writer, 0);
        }
    }
}