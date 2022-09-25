using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class PlayerSpawnManager : MonoBehaviour {

        public static PlayerSpawnManager Current { get; private set; }

        public static bool IsPlayerAlive { get; set; }
        //public static bool IsPlayerAlive => Current
        //               && Current.CurrentPlayerEntity != null
        //               && Current.CurrentPlayerEntity.CurrentGameObject != null
        //               && Current.CurrentPlayerEntity.CurrentGameObject.activeInHierarchy
        //               && Current.CurrentPlayerEntity.HealthComponents[0].Value > 0;

        public ScriptablePlayerEntity originalPlayerEntity;

        public int currentPlayerPrefabId;
        public int nextPlayerPrefabId;

        public PoolManager playerShipPool;
        public PoolManager activeShipPool;
        public PoolManager playerEffectsPool;

        public bool shootOnAim;

        public UnityEvent OnPlayerGainedSkillPoint;
        public UnityEvent OnPlayerHasNoSkillPoints;
        public GameObjectEvent OnPlayerSpawned;
        public GameObjectEvent OnPlayerDespawned;

        #region Properties
        //public PlayerEntity CurrentPlayerEntity { get; set; }
        public uint CurrentPlayerEntityId { get; set; }
        public GamePlayerInfo CurrentPlayerInfo { get; set; }
        #endregion

        void OnEnable() {
            Current = this;
        }

        public void Initialize() {
            Current = this;

            if(OnPlayerGainedSkillPoint == null) OnPlayerGainedSkillPoint = new UnityEvent();
            if(OnPlayerHasNoSkillPoints == null) OnPlayerHasNoSkillPoints = new UnityEvent();
            if(OnPlayerSpawned == null) OnPlayerSpawned = new GameObjectEvent();
            if(OnPlayerDespawned == null) OnPlayerDespawned = new GameObjectEvent();

            nextPlayerPrefabId = currentPlayerPrefabId = ShipSelectionUIManager.Current.CurrentShipSelection;

            RefreshPlayerHUD();
        }

        public PlayerEntity GetCurrentEntity() {
            if(CurrentPlayerEntityId > 0)
                return GameManager.Current.GetActorEntity(CurrentPlayerEntityId) as PlayerEntity;

            return null;
        }

        public void Despawn() {
            GlobalTargetController.GetController("Global Player Target").Target = null;
            playerEffectsPool.DespawnAll();

            foreach(var player in PlayerProxy.Players)
                OnPlayerDespawned.InvokeEvent(player.CurrentGameObject);
        }

        public void GiveSkillPoints(int amount) {
            if(NetworkServer.active || NetworkClient.active)
                NetworkUpgradeManager.Current.GiveSkillPoints(CurrentPlayerInfo.gameObject, amount);
        }

        public void RefreshPlayerHUD() {
            InGameUIManager.Current.RefreshXPInfo();
            InGameUIManager.Current.RefreshGameLevelText();

            PlayerEntity entity = null;

            if(QuadrablazeSteamNetworking.Current.MyPlayerInfo != null)
                entity = QuadrablazeSteamNetworking.Current.MyPlayerInfo.AttachedEntity;

            InGameUIManager.Current.RefreshShieldInfo(entity);
            InGameUIManager.Current.RefreshSkillPointText();
            InGameUIManager.Current.RefreshHealthInfo(entity);
        }

        public GameObject SpawnPlayerGameObject(NetworkConnection connection, GamePlayerInfo playerInfo, Vector3 position = default, Quaternion rotation = default) {
            int id = playerInfo.IsBuiltinShip ? playerInfo.ShipID : (int)playerInfo.netId.Value;

            PoolManager pool = playerInfo.IsBuiltinShip ? playerShipPool : activeShipPool;

            var prefabIndex = pool.IndexFromPrefabID(id);
            var playerPoolUser = pool.Spawn(prefabIndex, position, rotation);
            var networkIdentity = playerPoolUser.GetComponent<NetworkIdentity>();

            networkIdentity.AssignClientAuthority(connection);

            return playerPoolUser.gameObject;
        }
    }
}