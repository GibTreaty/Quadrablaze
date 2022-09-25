using System.Collections;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;
using YounGenTech.PoolGen;

namespace Quadrablaze.Boss {
    public class FreeTrinity : BossController, IActorEntityObjectAssignedSkill {

        const float directionChangeDelay = .5f;
        const int speedSteps = 6;
        const float speedStepMultiplier = 10;

        [SerializeField]
        MeshRenderer _renderer;

        [SerializeField]
        Transform _blazeGlow;

        [SerializeField]
        Transform _shockwaveGlow;

        [SerializeField]
        Transform _dashGlow;

        [SerializeField]
        TriggerUnityEvent _shockwaveTrigger;

        [SerializeField]
        Collider _shockwaveCollider;

        [SerializeField]
        Collider _mainCollider;

        [SerializeField]
        AudioSource _dashAudioSource;

        int speedStep;
        bool _enteredArena;
        float entitySize;
        float blazeInvokeDelay, blazeTimeLastUsed;
        float shockwaveInvokeDelay, shockwaveTimeLastUsed;
        float dashInvokeDelay, dashTimeLastUsed;

        float lastDirectionChange;

        GameObject[] hitSpot = new GameObject[1];

        #region Properties
        BaseMovementController BaseMovementControllerComponent { get; set; }
        Dash DashExecutor { get; set; }
        Vector3 Direction { get; set; }
        EnemyInput EnemyInputComponent { get; set; }
        Rigidbody RigidbodyComponent { get; set; }
        Shockwave ShockwaveExecutor { get; set; }
        #endregion

        void ActivateGlow(int value) {
            _blazeGlow.gameObject.SetActive(value == 1);
            _shockwaveGlow.gameObject.SetActive(value == 2);
            _dashGlow.gameObject.SetActive(value == 3);
        }

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            if(!initialized) {
                _shockwaveTrigger.onTrigger.AddListener(InvokeShockwave);

                void InvokeShockwave(Collider collider) {
                    if(Time.time > shockwaveTimeLastUsed + shockwaveInvokeDelay) {
                        GetEntity().CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Shockwave>().Invoke();
                        shockwaveTimeLastUsed = Time.time;
                    }
                }
            }

            base.ActorEntityObjectInitialize(entity);

            BaseMovementControllerComponent = entity.BaseMovementControllerComponent;
            EnemyInputComponent = GetComponent<EnemyInput>();

            RigidbodyComponent = entity.RigidbodyComponent;
            RigidbodyComponent.angularVelocity = new Vector3(0, 360, 0);

            hitSpot[0] = _mainCollider.gameObject;

            EnemyProxy.Targets.Add(hitSpot[0].transform);
            entitySize = entity.Size;
        }

        IEnumerator ChangeMaterialColor() {
            float time = 1;
            float startTime = time;
            Color startColor = _renderer.material.GetColor("_GlowColor");
            Color toColor = GetMaterialColor(Mathf.Clamp(Stage, 1, 3));

            while(time > 0) {
                time = Mathf.Max(time - Time.deltaTime, 0);
                _renderer.material.SetColor("_GlowColor", Color.Lerp(toColor, startColor, time / startTime));

                yield return new WaitForEndOfFrame();
            }
        }

        void FixedUpdate() {
            if(!initialized) return;

            var arenaClamp = GameManager.Current.ArenaRadius - entitySize - .1f;

            if(_enteredArena) {
                if(RigidbodyComponent.position.magnitude > arenaClamp) {
                    //if(Stage < 3)
                        RigidbodyComponent.velocity = Vector3.zero;

                    RigidbodyComponent.position = Vector3.ClampMagnitude(RigidbodyComponent.position, arenaClamp);

                    if(Stage < 3) {
                        if(Time.time - lastDirectionChange > directionChangeDelay) {
                            Direction = Quaternion.Euler(0, Mathf.Sign(Random.Range(-1f, 1)) * Random.Range(10, 60), 0) * -RigidbodyComponent.position.normalized;

                            float magnitude = Direction.magnitude;
                            
                            RigidbodyComponent.velocity = Direction.normalized * (magnitude + (speedStep * speedStepMultiplier));
                            speedStep = (speedStep + 1) % speedSteps;
                            lastDirectionChange = Time.time;
                        }
                    }
                }
            }
            else {
                if(RigidbodyComponent.position.magnitude <= arenaClamp)
                    _enteredArena = true;
            }

            if(Stage > 1)
                RigidbodyComponent.AddTorque(0, 360, 0);
        }

        Color GetMaterialColor(int value) {
            switch(value) {
                default: return _renderer.material.color = new Color(3, .774f, 0);
                case 2: return _renderer.material.color = new Color(.774f, 3, 0);
                case 3: return _renderer.material.color = new Color(0, .88f, 4);
            }
        }

        public override GameObject[] GetHitSpots() {
            return hitSpot;
        }

        public override Vector3 GetStartPosition() {
            return Quaternion.Euler(0, Random.value * 360, 0) * Vector3.forward * (GameManager.Current.ArenaRadius + 14);
        }

        //void LateUpdate() {
        //    _shockwaveGlow.rotation = _dashGlow.rotation = Quaternion.Euler(90, 0, 0);
        //}

        public void OnAssignedSkill(SkillLayoutElement element) {
            var entity = element.CurrentLayout.CurrentEntity;

            switch(element.CurrentExecutor) {
                case Blaze executor: {
                    blazeInvokeDelay = executor.CooldownTimer.Length + executor.BlazeTimer.Length + 1;
                    //blazeTimeLastUsed = Time.time - executor.BlazeTimer.Length;
                    break;
                }

                // TODO: Boss - Test Dash exeuctor stuff
                case Dash executor: {
                    //    DashExecutor = executor;
                    //    DashExecutor.OnStartDash += OnDash;
                    dashInvokeDelay = executor.CooldownTimer.Length + 1;
                    break;
                }

                // TODO: Boss - Test Shockwave exeuctor stuff
                case Shockwave executor: {
                    //    ShockwaveExecutor = executor;
                    //    ShockwaveExecutor.OnInvoke += OnShockwave;
                    shockwaveInvokeDelay = executor.CooldownTimer.Length + .05f;
                    break;
                }
            }
        }

        void OnDash() { // TODO: Redo OnDash
            if(!NetworkServer.active) return;

            _dashAudioSource.Play();

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_FreeTrinityState);

            writer.Write(EntityId);
            writer.Write(gameObject);
            writer.Write(FreeTrinityState.Dashed);
            writer.FinishMessage();

            foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                if(!player.isHost)
                    player.serverToClientConnection.SendWriter(writer, Channels.DefaultUnreliable);
        }

        void OnDisable() {
            if(hitSpot[0] != null)
                EnemyProxy.Targets.Remove(hitSpot[0].transform);

            // TODO: Check FreeTrinity's abilities

            //if(BlazeExecutor != null) {
            //    BlazeExecutor.BlazeTimer.OnElapsed.RemoveListener(BlazeExecutor.Recharge);
            //    BlazeExecutor.CooldownTimer.OnElapsed.RemoveListener(BlazeExecutor.Invoke);
            //    BlazeExecutor = null;
            //}

            //if(DashExecutor != null) {
            //    DashExecutor.OnStartDash -= OnDash;
            //    DashExecutor = null;
            //}

            //if(ShockwaveExecutor != null) {
            //    ShockwaveExecutor.OnInvoke -= OnShockwave;
            //    ShockwaveExecutor = null;
            //}
        }

        protected override void OnNetworkStageState(int stage) {
            if(stage > 0 && stage < 4)
                ActivateGlow(stage);

            switch(stage) {
                case 1: ResetColor(); break;
                case 3:
                    BaseMovementControllerComponent.SetPushDirection(Vector3.zero);
                    BaseMovementControllerComponent.Move(Vector3.zero);
                    break;
            }

            StopCoroutine("ChangeMaterialColor");
            StartCoroutine("ChangeMaterialColor");
        }

        protected override void OnStage(int stage) {
            _shockwaveCollider.enabled = false;
            switch(stage) {
                case 1: Direction = -RigidbodyComponent.position.normalized; break;
                case 2:
                    var blaze = GetEntity().CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Blaze>();
                    blaze.ResetSkill();
                    _shockwaveCollider.enabled = true;
                    break;
            }
        }

        public override void OnStageUpdate(int stage) {
            // TODO: Boss - Test "NetworkServer.active"
            if(NetworkServer.active) {
                var bossEntity = GetEntity();

                switch(stage) {
                    case 1: //Blaze
                        if(Time.time > blazeTimeLastUsed + blazeInvokeDelay) {
                            bossEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Blaze>().Invoke();
                            blazeTimeLastUsed = Time.time;
                        }

                        bossEntity.BaseMovementControllerComponent.Move(Direction);
                        break;

                    case 2: //Shockwave
                        bossEntity.BaseMovementControllerComponent.Move(Direction);
                        break;

                    case 3: //Dash
                        if(EnemyInputComponent.Target) {
                            if(Time.time > dashTimeLastUsed + dashInvokeDelay) {
                                var dash = bossEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Dash>();

                                dash.InvokeDash(EnemyInputComponent.Target.position - transform.position);
                                dashTimeLastUsed = Time.time;
                            }
                        }

                        break;
                }
            }
        }

        void OnShockwave() {
            if(!NetworkServer.active) return;

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_FreeTrinityState);
            writer.Write(EntityId);
            writer.Write(gameObject);
            writer.Write(FreeTrinityState.Shockwaved);
            writer.FinishMessage();

            foreach(var player in QuadrablazeSteamNetworking.Current.Players)
                if(!player.isHost)
                    player.serverToClientConnection.SendWriter(writer, Channels.DefaultUnreliable);
        }

        public void ResetColor() {
            _renderer.material.SetColor("_GlowColor", GetMaterialColor(1));
        }

        //void Update() {
        //    UpdateEffects();
        //}

        //void UpdateEffects() {
        //    _dashGlow.localScale = Vector3.one * Mathf.Lerp(1, 8.7f * 2, DashExecutor.DashesInbetween < DashExecutor.MaxDashes ? 1 : 1 - DashExecutor.CooldownTimer.NormalizedTime);
        //    _shockwaveGlow.localScale = Vector3.one * Mathf.Lerp(1, ShockwaveExecutor.OriginalSkillExecutor.Radius * 2, 1 - ShockwaveExecutor.CooldownTimer.NormalizedTime);
        //}

        public static void NetworkFreeTrinityState(NetworkMessage networkMessage) {
            var entityId = networkMessage.reader.ReadUInt32();
            var entity = GameManager.Current.GetActorEntity(entityId);
            var gameObject = networkMessage.reader.ReadGameObject();
            var state = networkMessage.reader.ReadByte();
            var freeTrinity = gameObject.GetComponent<FreeTrinity>();

            switch(state) {
                case FreeTrinityState.Shockwaved:
                    Debug.Log("Shockwaved");
                    freeTrinity.ShockwaveExecutor.InvokeSkillNonAuthority();
                    break;

                case FreeTrinityState.Dashed:
                    freeTrinity.DashExecutor.InvokeSkillNonAuthority();
                    freeTrinity._dashAudioSource.Play();
                    break;
            }
        }

        static class FreeTrinityState {
            public const byte Shockwaved = 0;
            public const byte Dashed = 1;
        }
    }
}