using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quadrablaze.Boss;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors {
    public class Barrage : SkillExecutor, ICooldownTimer, IExecutorActorDisable, ISkillUpdate {

        public EventTimer CooldownTimer { get; }
        public new ScriptableBarrageSkillExecutor OriginalSkillExecutor { get; }

        PoolManager poolManager;
        MissileInfo[] missileArray;
        int missileCount;

        public Barrage(ActorEntity actorEntity, SkillLayoutElement element) : base(actorEntity, element) {
            OriginalSkillExecutor = (ScriptableBarrageSkillExecutor)element.OriginalLayoutElement.CreatesExecutor;
            CooldownTimer = new EventTimer(OriginalSkillExecutor.CooldownDuration) { AutoDisable = true };

            poolManager = PoolManager.GetPool(OriginalSkillExecutor.ProjectilePoolName);
            missileArray = new MissileInfo[OriginalSkillExecutor.ProjectileCount];
        }

        Coroutine AddTarget(Transform target, float damage, int missileStartIndex, int missileCount) {
            return GameCoroutine.BeginCoroutine(InvokeDelayed(missileCount, OriginalSkillExecutor.ProjectileLaunchGap, (int rep) => AddMissile(missileStartIndex + rep, target, damage), CheckCancel));

            bool CheckCancel() {
                return !EnemyProxy.Targets.Contains(target);
            }
        }

        void AddTargets(List<Transform> targets) {
            var targetCount = targets.Count;

            if(targetCount > 1)
                GameCoroutine.BeginCoroutine(AddTargetRoutine(targets));
            else
                AddTarget(targets[0], OriginalSkillExecutor.Damage / missileArray.Length, 0, missileArray.Length);
        }

        IEnumerator AddTargetRoutine(List<Transform> targets) {
            var targetCount = targets.Count;
            var targetAllocations = new int[targetCount];
            int targetIndex = 0;

            for(int i = 0; i < missileArray.Length; i++) {
                targetAllocations[targetIndex]++;

                if(targetCount > 1) {
                    targetIndex++;
                    targetIndex %= targetCount;
                }
            }

            int currentStartIndex = 0;

            for(int i = 0; i < targetAllocations.Length; i++) {
                var damage = OriginalSkillExecutor.Damage / targetAllocations[i];

                yield return AddTarget(targets[i], damage, currentStartIndex, targetAllocations[i]);

                currentStartIndex += targetAllocations[i];
            }
        }

        void AddMissile(int rep, Transform target, float damage) {
            var startPosition = CurrentActorEntity.CurrentTransform.position;
            var newDirection = Quaternion.Euler(0, rep * (360f / OriginalSkillExecutor.ProjectileCount), 0) * Vector3.forward;

            CreateMissile(rep, target, startPosition + (newDirection * CurrentActorEntity.Size), Quaternion.LookRotation(newDirection), damage);
        }

        void CreateMissile(int missileIndex, Transform target, Vector3 position, Quaternion rotation, float damage) {
            var index = poolManager.IndexFromPrefabID(OriginalSkillExecutor.ProjectilePoolPrefabId);

            var missileUser = poolManager.Spawn(index, position, rotation);
            var targetGameObject = target.gameObject;
            var travelAudioSource = missileUser.transform.Find("Travel Audio").GetComponent<AudioSource>();

            missileArray[missileIndex] = new MissileInfo() {
                alive = true,
                damage = damage,
                missile = missileUser,
                missileLaunchAudioSource = missileUser.GetComponent<AudioSource>(),
                missileTravelAudioSource = travelAudioSource,
                mode = false,
                startPosition = position,
                startTime = Time.time,
                targetPosition = targetGameObject.transform.position,
                targetEntity = CurrentActorEntity,
                targetGameObject = targetGameObject
            };

            missileCount++;
        }

        public void ClearMissiles() {
            for(int i = 0; i < missileArray.Length; i++) {
                var missileInfo = missileArray[i];

                missileArray[i] = RemoveMissile(missileInfo);
            }
        }

        Transform GetNearestTarget(ActorEntity entity, List<Transform> excludeTargets) {
            var position = entity.CurrentTransform.position;
            var closestDistance = Mathf.Infinity;
            Transform closestTarget = null;

            foreach(var target in EnemyProxy.Targets) {
                if(excludeTargets.Contains(target)) continue;

                float distance = (target.transform.position - position).sqrMagnitude;

                if(distance < closestDistance) {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        List<Transform> GetTargets() {
            var targetLimit = OriginalSkillExecutor.TargetLimit;

            if(CurrentLayoutElement.OriginalLayoutElement.SkillModifierType == ModifierType.Set)
                targetLimit = (ushort)CurrentLayoutElement.CurrentSkillAmount;

            var targets = new List<Transform>();

            if(CurrentLayoutElement.CurrentLevel < 5) {
                var bossRoots = new HashSet<Transform>();

                foreach(var target in EnemyProxy.Targets) //TODO: Test if this still works
                    if(GameManager.Current.GetActorEntity(target.root.gameObject, out ActorEntity actorEntity))
                        if(bossRoots.Contains(target.root))
                            continue;
                        else if(actorEntity is Entities.BossEntity)
                            bossRoots.Add(target.root);
                        else
                            targets.Add(target);
            }
            else {
                targets.AddRange(EnemyProxy.Targets);
            }

            targets = targets.OrderBy(target => (CurrentActorEntity.CurrentTransform.position - target.position).sqrMagnitude).ToList();

            if(targets.Count > targetLimit) {
                var overlimit = targets.Count - targetLimit;

                targets.RemoveRange(OriginalSkillExecutor.TargetLimit, overlimit);
            }

            return targets;
        }

        IEnumerator InvokeDelayed(int repetitions, float delay, System.Action<int> action, Func<bool> cancel) {
            for(int i = 0; i < repetitions; i++) {
                if(cancel()) yield break;

                var rep = i;

                action(rep);

                if(i < repetitions - 1) {
                    //Debug.Log("InvokeDelayed " + Time.time);
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        public override void InvokeSkillServer() {
            if(CooldownTimer.Active) return;
            if(missileCount > 0) return;

            var targets = GetTargets();

            if(targets.Count > 0) {
                CooldownTimer.Start(true);

                AddTargets(targets);
                SendTargets(targets);
                GiveSkillFeedback(false);
            }
        }

        void IExecutorActorDisable.OnActorDisabled() {
            ClearMissiles();
            //RemoveTarget(actor.netId);
        }

        public override void OnSkillFeedback(byte[] parameters) {
            CooldownTimer.Start(true);
        }

        void PlayHomingSound(MissileInfo missileInfo) {
            missileInfo.missileTravelAudioSource.Play();
        }

        MissileInfo RemoveMissile(MissileInfo missileInfo) {
            if(missileInfo.Destroy())
                missileCount--;

            return missileInfo;
        }

        void SendTargets(List<Transform> targets) {
            if(true) {
                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_BarrageSkill);
                writer.Write(CurrentActorEntity.Id);
                writer.Write(targets.Count);

                foreach(var target in targets) {
                    if(target.root == target) {
                        writer.Write(true);
                        writer.Write(target);
                    }
                    else {
                        writer.Write(false);
                        writer.Write(target.root);

                        var healthGroup = target.GetComponentInParent<HealthGroup>();
                        var healthLayer = target.GetComponent<HealthLayer>();
                        (var tierIndex, var layerIndex) = healthGroup.GetHealthLayerIndex(healthLayer);

                        writer.Write(tierIndex);
                        writer.Write(layerIndex);
                    }
                }

                writer.FinishMessage();

                QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
            }
        }

        public void SkillUpdate() {
            CooldownTimer.Update();

            //if(NetworkServer.active) {
            if(missileCount > 0)
                for(int i = 0; i < missileArray.Length; i++) {
                    var missileInfo = missileArray[i];

                    if(!missileInfo.alive) continue;

                    var target = missileInfo.targetGameObject;
                    //var target = NetworkServer.FindLocalObject(missileInfo.targetNetId);

                    if(target != null && target.activeInHierarchy && EnemyProxy.Targets.Contains(target.transform)) {
                        missileInfo.targetPosition = target.transform.position;
                    }
                    else {
                        if(target != null) {
                            missileInfo.targetEntity = null;
                            missileInfo.targetGameObject = null;
                        }
                    }

                    var currentLifetime = missileInfo.CurrentLifetime;

                    if(currentLifetime <= OriginalSkillExecutor.ProjectileLifetime) {
                        var startTime = OriginalSkillExecutor.ProjectileStartTime;
                        var currentHalflifetime = currentLifetime / startTime;
                        var missileTransform = missileInfo.missile.transform;

                        if(currentLifetime < startTime) {
                            var currentAnimationTime = OriginalSkillExecutor.MissileStartCurve.Evaluate(currentHalflifetime);

                            missileTransform.position = Vector3.Lerp(missileInfo.startPosition, missileInfo.startPosition + missileTransform.forward * 2, currentAnimationTime);
                            //missileTransform.Translate(new Vector3(0,0,4) * Time.deltaTime * (2 + missileInfo.CurrentLifetime), Space.Self);
                        }
                        else {
                            var homingTime = OriginalSkillExecutor.ProjectileLifetime - startTime;
                            var currentTime = (currentLifetime - startTime) / homingTime;
                            var currentAnimationTime = OriginalSkillExecutor.MissileHomingCurve.Evaluate(currentTime);

                            if(!missileInfo.mode) {
                                missileInfo.mode = true;
                                missileInfo.startPosition = missileTransform.position;

                                PlayHomingSound(missileInfo);
                            }

                            missileTransform.position = Vector3.Lerp(missileInfo.startPosition, missileInfo.targetPosition, currentAnimationTime);

                            var directionToTarget = missileInfo.targetPosition - missileInfo.missile.transform.position;

                            missileTransform.forward = directionToTarget;
                        }
                    }
                    else {
                        if(target != null && target.activeInHierarchy && EnemyProxy.Targets.Contains(target.transform)) {
                            if(NetworkServer.active) {
                                target.DoHealthChange((int)-OriginalSkillExecutor.Damage, this, false);
                                //if(target.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot) {

                                //}
                                //var layer = target.GetComponent<HealthLayer>();

                                //if(layer)
                                //    layer.Group.Damage(layer, new HealthEvent(CurrentActorEntity.CurrentGameObject, layer.gameObject, OriginalSkillExecutor.Damage, HealthHelper.Executor_Barrage));
                            }

                            PoolManager.Spawn("Explosions", missileInfo.targetPosition, Quaternion.identity);
                        }

                        missileInfo = RemoveMissile(missileInfo);
                    }

                    missileArray[i] = missileInfo;
                }
            //}
        }

        private static void OnTarget(NetworkMessage netMsg) {
            var reader = netMsg.reader;
            var entityId = reader.ReadUInt32();
            var entity = GameManager.Current.GetActorEntity(entityId);

            if(entity == null) {
                Debug.LogError($"Entity Id '{entityId}' not found");
                return;
            }

            if(entity.CurrentGameObject == null) {
                Debug.LogError("Owner not found for BarrageSkillExecutor. They could've been disconnected/destroyed.");
                return;
            }

            var barrageSkill = entity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Barrage>();

            if(barrageSkill == null) {
                Debug.Log("No <color=Red>Barrage Skill</color>");
                return;
            }

            var targetCount = reader.ReadInt32();
            var targets = new List<Transform>();

            for(int i = 0; i < targetCount; i++) {
                var isRoot = reader.ReadBoolean();
                var target = reader.ReadTransform();

                if(isRoot) {
                    targets.Add(target);
                }
                else {
                    var healthGroup = target.GetComponent<HealthGroup>();
                    var tierIndex = reader.ReadInt32();
                    var layerIndex = reader.ReadInt32();
                    var healthLayer = healthGroup.GetHealthLayer(tierIndex, layerIndex);

                    targets.Add(healthLayer.transform);
                }
            }

            if(targets.Count > 0) {
                barrageSkill.CooldownTimer.Start(true);
                barrageSkill.AddTargets(targets);
            }
        }

        [RegisterNetworkHandlers]
        public new static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_BarrageSkill, OnTarget);
        }

        [Serializable]
        struct MissileInfo {
            public PoolUser missile;
            public AudioSource missileLaunchAudioSource;
            public AudioSource missileTravelAudioSource;
            public Vector3 startPosition;
            public Vector3 targetPosition;
            public float startTime;
            public bool alive;
            public bool mode;
            public float damage;
            public ActorEntity targetEntity;
            public GameObject targetGameObject;

            public float CurrentLifetime {
                get { return Mathf.Max(0, Time.time - startTime); }
            }

            public bool Destroy() {
                bool flag = false;

                if(alive) {
                    alive = false;
                    flag = true;
                }

                if(missile != null)
                    missile.Despawn();

                return flag;
            }
        }
    }
}