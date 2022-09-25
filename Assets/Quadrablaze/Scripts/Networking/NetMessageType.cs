namespace Quadrablaze {
    public class NetMessageType {
        //Server_ messages are sent to the server from a client
        //Client_ messages are sent to clients from the server

        public const short Network_Spawn = 1001;
        public const short Client_DebugMessage = 1002;
        public const short Server_TransformSync = 1003;
        public const short Client_TransformSync = 1004;
        public const short Network_ActorSpawnInitialize = 1005;

        public const short Network_GameNetMessage = 1006;
        public const short Network_EntityNetMessage = 1007;

        public const short SendSteamID = 2000;

        public const short Client_ReceiveErrorMessage = 2500;
        
        public const short Client_OpenMultiplayerLobby = 3050;
        public const short Client_StartJoinGame = 3051;
        public const short Client_SuccessfulVerification = 3052;
        public const short Server_ReceiveVerification = 3053;
        public const short Client_EndToLobby = 3054;

        #region Upgrades
        public const short Server_UpgradeSkill = 3100;
        public const short Client_SetPlayerSkillPoints = 3101;
        public const short Client_SetPlayerLevel = 3102;
        public const short Client_SetPlayerXP = 3103;
        public const short Server_GiveSkillPoints = 3104;

        public const short Server_SendSkillData = 3201;
        public const short Server_UseAbility = 3203;
        public const short Client_UseAbility = 3204;
        public const short Client_UpgradeManagerUpgradeLevel = 3205;
        public const short Client_UpgradeManagerSetLevel = 3206;
        public const short Client_UpgradeManagerSetSkillPoints = 3207;
        public const short Client_UpgradeManagerSetXP = 3208;
        public const short Client_EnemyUseSpecialAbility = 3209;
        public const short Client_OnSkillFeedback = 3211;

        public const short Client_BarrageSkill = 3212;
        #endregion

        public const short Server_PlayerInputMovement = 3300;
        public const short Client_PlayerInputMovement = 3301;
        public const short Client_PlayerSetHealth = 3302;
        public const short Client_PlayerSetShieldHealth = 3303;
        public const short Client_PlayerSetMaxHealth = 3304;
        public const short Client_PlayerSetShieldMaxHealth = 3305;
        public const short Client_WeaponShoot = 3306;

        public const short Client_SkyboxHue = 3400;

        public const short Client_PlayCameraSound = 3500;
        public const short Client_EffectManagerInvoke = 3501;
        public const short Client_HitEffect = 3502;

        public const short Client_DeployTurrets = 3503;

        #region Triformer
        public const short Client_SetTriformerForceField = 4000;
        #endregion

        #region Deformer
        public const short Client_ShieldDrainer = 4050;
        #endregion

        #region Shockwaver
        public const short Client_GetShockwaved = 4100;
        #endregion

        #region All Enemies
        public const short Client_SetTelegraphState = 4500;
        #endregion

        #region Bosses
        public const short Client_BossStageState = 5000;
        public const short Client_SetBossHealthUI = 5001;
        //public const short Client_SetBossSpawned = 5002;
        //public const short Client_SetBossSpawnedUI = 5003;
        public const short Client_SetBossDefeated = 5004;
        public const short Client_SetBossTimer = 5005;
        public const short Client_RailRiderState = 5100;
        public const short Client_FreeTrinityState = 5101;
        public const short Client_TriCloneState = 5102;
        #endregion

        #region Ship Mesh Sync
        public const short Server_StartSyncShipData = 6000;
        public const short Server_TransmitShipData = 6001;
        public const short Client_SyncShipData = 6002;
        #endregion
    }
}