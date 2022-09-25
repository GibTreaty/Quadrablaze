using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors {
    public class Turret : SkillExecutor, ICooldownTimer, ISkillUpdate {

        int deployedLevel;
        List<TurretEntity> turretEntities;

        public EventTimer CooldownTimer { get; }
        public Vector3 DeployPushDirection { get; set; }
        public EventTimer DeployedTimer { get; }
        public override bool IsReady => turretEntities.Count == 0 && CooldownTimer.NormalizedTime == 0;
        public new ScriptableTurretSkillExecutor OriginalSkillExecutor { get; }

        public Turret(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableTurretSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            CooldownTimer = new EventTimer(OriginalSkillExecutor.CooldownDuration) { AutoDisable = true };
            DeployedTimer = new EventTimer(OriginalSkillExecutor.DeployedDuration) { AutoDisable = true };

            turretEntities = new List<TurretEntity>();

            DeployedTimer.OnElapsed.AddListener(() => {
                if(NetworkServer.active) {
                    Recall();
                    CooldownTimer.Start(true);
                    GiveSkillFeedback(false, 1, 2);
                }
            });

            CurrentActorEntity.Listener.Subscribe(EntityActions.Despawned, ActorProxy_Despawned);

            if(CurrentActorEntity is PlayerEntity playerEntity)
                playerEntity.PlayerInputComponent.onShootDirectionInput.AddListener(v => DeployPushDirection = v);
        }

        void ActorProxy_Despawned(System.EventArgs args) {
            if(((EntityArgs)args).GetEntity<Entity>() == CurrentActorEntity)
                Recall();
        }

        public void Deploy() {
            if(turretEntities.Count > 0 || CooldownTimer.NormalizedTime > 0) return;

            deployedLevel = CurrentLayoutElement.CurrentLevel;

            var poolManager = PoolManager.GetPool(OriginalSkillExecutor.PoolManagerName);
            int iterations = deployedLevel >= 6 ? 2 : 1;

            List<GameObject> spawnedTurrets = new List<GameObject>();

            for(int i = 0; i < iterations; i++) {
                var poolUser = poolManager.Spawn(poolManager.IndexFromPrefabName(OriginalSkillExecutor.PoolPrefabName), CurrentActorEntity.CurrentTransform.position, DeployPushDirection.sqrMagnitude != 0 ? Quaternion.LookRotation(DeployPushDirection) : Quaternion.identity);

                spawnedTurrets.Add(poolUser.gameObject);

                float direction = i == 0 ? 1 : -1;

                poolUser.GetComponent<Rigidbody>().AddForce(Vector3.ClampMagnitude(DeployPushDirection * direction, 1) * OriginalSkillExecutor.DeployPushForce, ForceMode.VelocityChange);
            }

            if(spawnedTurrets.Count > 0)
                SendDeployMessage(spawnedTurrets);

            DeployedTimer.Start(true);
            GiveSkillFeedback(false, 0);
        }

        public int GetHighestWeaponLevel() {
            int level = 1;

            foreach(var element in CurrentActorEntity.CurrentUpgradeSet.CurrentSkillLayout.Elements)
                if(element.CurrentExecutor is Weapon executor)
                    if(element.CurrentLevel > level)
                        level = element.CurrentLevel;

            return level;
        }

        public override void InvokeSkillServer() {
            Deploy();
        }

        public override void LevelChanged(int level, int previousLevel) {
            foreach(var turret in turretEntities)
                turret.Turret.EnableHealRing = level > 1;
        }

        public override void OnSkillFeedback(byte[] parameters) {
            foreach(var code in parameters) {
                switch(code) {
                    case (byte)TurretNetworkCode.DeployedTimer:
                        DeployedTimer.Start(true);
                        break;
                    case (byte)TurretNetworkCode.CooldownTimer:
                        CooldownTimer.Start(true);
                        break;
                    case (byte)TurretNetworkCode.Recall:
                        Recall();
                        break;
                }
            }
        }

        public void Recall() {
            if(turretEntities.Count == 0) return;

            DeployedTimer.Reset(true);
            CooldownTimer.Reset(true);

            //TODO: Should this only be done on the server?
            foreach(var entity in turretEntities) {
                foreach(var element in entity.CurrentUpgradeSet.CurrentSkillLayout.Elements)
                    //if(element.CurrentExecutor is WeaponSkillExecutor executor) 
                    if(element.CurrentExecutor != null)
                        element.CurrentExecutor.Unload();

                entity.DestroyEntity();
                entity.UnloadEntity();
            }

            //if(NetworkServer.active)
            //    foreach(var turret in turrets)
            //        turret.GetComponent<PoolUser>().Despawn();

            turretEntities.Clear();
        }

        void SendDeployMessage(List<GameObject> gameObjects) {
            if(gameObjects.Count == 0) return;

            QuadrablazeSteamNetworking.SendGameNetworkMessage(ReceiveDeployTurrets, writer => {
                writer.Write(CurrentActorEntity.Id);
                writer.Write(gameObjects.Count);

                foreach(var gameObject in gameObjects) {
                    var entityId = GameManager.Current.GetUniqueEntityId();

                    writer.Write(entityId);
                    writer.Write(gameObject);
                }
            });
        }

        public void SkillUpdate() {
            if(NetworkServer.active)
                DeployedTimer.Update();

            CooldownTimer.Update();

            if(NetworkServer.active) {
                if(deployedLevel >= 3) {
                    for(int i = 0; i < turretEntities.Count; i++) {
                        var turret = turretEntities[i];

                        if(deployedLevel >= 6) {
                            Vector3 direction = Vector3.right;
                            Vector3 targetPosition = CurrentActorEntity.CurrentTransform.position;
                            TurretEntity otherTurret = null;

                            switch(i) {
                                case 0: otherTurret = turretEntities[1]; break;
                                case 1: otherTurret = turretEntities[0]; break;
                            }

                            var targetDirection = turret.CurrentTransform.position - otherTurret.CurrentTransform.position;

                            if(targetDirection.sqrMagnitude > .01f)
                                direction = targetDirection.normalized;
                            else
                                direction = new Vector3(i == 0 ? 1 : -1, 0, 0);

                            direction = Quaternion.Euler(0, 45, 0) * direction;

                            targetPosition += direction * 6;

                            turret.Turret.TargetMovePosition = targetPosition;
                        }
                        else {
                            turret.Turret.TargetMovePosition = CurrentActorEntity.CurrentTransform.position;
                        }
                    }
                }
            }
        }

        public override void Unload() {
            CurrentActorEntity.Listener.Unsubscribe(EntityActions.Despawned, ActorProxy_Despawned);
        }

        [GameNetworkListener]
        static void ReceiveDeployTurrets(NetworkMessage message) {
            var entityId = message.reader.ReadUInt32();
            var count = message.reader.ReadInt32();
            var entity = GameManager.Current.GetActorEntity(entityId);
            var turretSkill = entity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Turret>();

            for(int i = 0; i < count; i++) {
                var turretEntityId = message.reader.ReadUInt32();
                var gameObject = message.reader.ReadGameObject();
                var turretEntity = turretSkill.OriginalSkillExecutor.OriginalTurretEntity.CreateInstance() as TurretEntity;

                turretEntity.Id = turretEntityId;
                turretEntity.OwnerEntityId = entityId;
                turretEntity.SetGameObject(gameObject);
                turretEntity.InitializeSkillLayout();

                turretSkill.turretEntities.Add(turretEntity);
            }
        }

        enum TurretNetworkCode {
            DeployedTimer = 0,
            CooldownTimer = 1,
            Recall = 2
        }
    }
}