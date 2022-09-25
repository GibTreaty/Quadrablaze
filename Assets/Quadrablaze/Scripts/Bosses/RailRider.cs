using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using YounGenTech.EnergyBasedObjects;
using YounGenTech.Entities;
using YounGenTech.PoolGen;
using YounGenTech.YounGenShooter;

namespace Quadrablaze.Boss {
    public class RailRider : BossController, IActorEntityObjectAssignedSkill, ITelegraphHandler {

        [SerializeField]
        Animator _railRiderAnimator;

        [SerializeField]
        float _speed = 1;

        [SerializeField]
        RailRiderGun _gunLeft;

        [SerializeField]
        RailRiderGun _gunMiddle;

        [SerializeField]
        RailRiderGun _gunRight;

        [SerializeField]
        LaserDamage _laserDamageComponent;

        [SerializeField]
        GameObject _laserObject;

        int _direction = 1;
        GameObject[] hitSpots = new GameObject[3];

        #region Properties
        public int Direction {
            get { return _direction; }
            set { _direction = value; }
        }

        EnemyInput EnemyInputComponent { get; set; }

        public RailRiderGun GunLeft {
            get { return _gunLeft; }
            set { _gunLeft = value; }
        }

        public RailRiderGun GunMiddle {
            get { return _gunMiddle; }
            set { _gunMiddle = value; }
        }

        public RailRiderGun GunRight {
            get { return _gunRight; }
            set { _gunRight = value; }
        }

        Health HealthComponent { get; set; }

        public Animator RailRiderAnimator {
            get { return _railRiderAnimator; }
        }

        public float Speed {
            get { return _speed; }
            set { _speed = value; }
        }

        PoolUser UserComponent { get; set; }
        #endregion

        public override void ActorEntityObjectInitialize(ActorEntity entity) {
            BaseInitialize(entity);

            if(!initialized) {
                HealthComponent = GetComponent<Health>(); // TODO: [Health]
                UserComponent = entity.UserComponent;
                EnemyInputComponent = GetComponent<EnemyInput>();

                GunLeft.HealthComponent.OnChangedHealth.AddListener(UpdateHealthAction);
                GunMiddle.HealthComponent.OnChangedHealth.AddListener(UpdateHealthAction);
                GunRight.HealthComponent.OnChangedHealth.AddListener(UpdateHealthAction);

                GunLeft.HealthComponent.OnDeath.AddListener(ExplosionAction);
                GunMiddle.HealthComponent.OnDeath.AddListener(ExplosionAction);
                GunRight.HealthComponent.OnDeath.AddListener(ExplosionAction);

                GunLeft.HealthComponent.OnDeath.AddListener(s => EnableGunLeft(false));
                GunMiddle.HealthComponent.OnDeath.AddListener(s => MiddleGunDeathEvent());
                GunRight.HealthComponent.OnDeath.AddListener(s => EnableGunRight(false));

                GunLeft.HealthComponent.OnDeath.AddListener(s => GunLeft.Conceal());
                GunRight.HealthComponent.OnDeath.AddListener(s => GunRight.Conceal());

                // TODO: Boss - Make sure this works with the new way
                //OnStage1Start.AddListener(UpdateHealth);
                //OnStage2Start.AddListener(UpdateHealth);
                //OnStage3Start.AddListener(UpdateHealth);

                hitSpots[0] = GunLeft.gameObject;
                hitSpots[1] = GunMiddle.gameObject;
                hitSpots[2] = GunRight.gameObject;
            }

            transform.GetChild(0).localPosition = Vector3.forward * (GameManager.Current.ArenaRadius + .14f);

            initialized = true;

            void ExplosionAction(HealthEvent s) => UserComponent.SpawnHere("Explosions", s.originGameObject.transform.position);
            void UpdateHealthAction(float s) => UpdateHealth();
        }

        public override GameObject[] GetHitSpots() {
            return hitSpots;
        }

        public override Quaternion GetStartRotation() {
            var rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            if(PlayerSpawnManager.Current && PlayerSpawnManager.Current.CurrentPlayerEntityId > 0) {
                var playerEntity = PlayerSpawnManager.Current.GetCurrentEntity();
                Vector3 direction = playerEntity.CurrentTransform.position;

                if(direction.x > float.Epsilon && direction.z > float.Epsilon)
                    rotation = Quaternion.LookRotation(-direction, Vector3.up);
            }

            return rotation;
        }

        public void OnAssignedSkill(SkillLayoutElement element) {
            if(element.CurrentExecutor is Weapon weaponExecutor) {
                element.Listener.Subscribe(EntityActions.SkillLevelChanged, _ => SetGunShootingPoint());
                // TODO: [Boss] Check that RailRider weapons work
                void SetGunShootingPoint() {
                    if(element.CurrentLevel > 0)
                        switch(element.ElementTypeIndex) {
                            case 0:
                                GunLeft.Id = 0;
                                //GunLeft.ShootingPoint = weaponExecutor.EquippedWeapon?.ShootingPoint;
                                //GunLeft.AccuracyComponent = GunLeft.ShootingPoint?.GetComponent<Accuracy>();
                                break;

                            case 1:
                                GunRight.Id = 1;
                                //GunRight.ShootingPoint = weaponExecutor.EquippedWeapon?.ShootingPoint;
                                //GunRight.AccuracyComponent = GunRight.ShootingPoint?.GetComponent<Accuracy>();
                                break;
                        }
                }
            }
        }

        void OnDisable() {
            EnableGunLeft(false);
            EnableGunMiddle(false);
            EnableGunRight(false);
        }

        protected override void OnStage(int stage) {
            switch(stage) {
                case 1:
                    GunLeft.HealthComponent.Reset();
                    GunMiddle.HealthComponent.Reset();
                    GunRight.HealthComponent.Reset();

                    EnableGunLeft(true);
                    EnableGunMiddle(false);
                    EnableGunRight(true);

                    GunLeft.Deploy();
                    GunMiddle.Conceal();
                    GunRight.Deploy();

                    UpdateHealth();

                    Direction = Random.Range(0, 2);

                    if(Direction == 0) Direction = -1;

                    BossInfoUI.Current.SetHealth(HealthComponent.NormalizedValue);

                    break;

                case 2:
                    EnableGunLeft(false);
                    EnableGunMiddle(true);
                    EnableGunRight(false);

                    GunLeft.Conceal();
                    GunMiddle.Deploy();
                    GunRight.Conceal();

                    UpdateHealth();
                    Direction = -((Direction / Mathf.Abs(Direction)) * 2);

                    break;

                case 3:
                    EnableGunLeft(true);
                    EnableGunMiddle(true);
                    EnableGunRight(true);

                    GunLeft.Deploy();
                    GunMiddle.Deploy();
                    GunRight.Deploy();

                    UpdateHealth();
                    Direction = -((Direction / Mathf.Abs(Direction)) * 3);

                    break;
            }
        }

        public override void OnStageUpdate(int stage) {
            switch(stage) {
                case 1:
                    if(!GunLeft.Deployed && !GunRight.Deployed)
                        StageUp();

                    break;

                case 2:
                    if(!GunMiddle.Deployed)
                        StageUp();

                    break;

                case 3:
                    if(!GunLeft.Deployed && !GunMiddle.Deployed && !GunRight.Deployed)
                        Defeat();

                    break;
            }
        }

        public void EnableGunLeft(bool enable) {
            RailRiderAnimator.SetBool("GunLeftOpened", enable);
            GunLeft.Active = enable;

            if(enable) EnemyProxy.Targets.Add(GunLeft.transform);
            else EnemyProxy.Targets.Remove(GunLeft.transform);
            //SendState(RailRiderState.LeftGun, writer => writer.Write(enable));
        }

        public void EnableGunMiddle(bool enable) {
            RailRiderAnimator.SetBool("GunMiddleOpened", enable);
            GunMiddle.Active = enable;

            if(enable) EnemyProxy.Targets.Add(GunMiddle.transform);
            else EnemyProxy.Targets.Remove(GunMiddle.transform);
            //SendState(RailRiderState.MiddleGun, writer => writer.Write(enable));
        }

        public void EnableGunRight(bool enable) {
            RailRiderAnimator.SetBool("GunRightOpened", enable);
            GunRight.Active = enable;

            if(enable) EnemyProxy.Targets.Add(GunRight.transform);
            else EnemyProxy.Targets.Remove(GunRight.transform);
            //SendState(RailRiderState.RightGun, writer => writer.Write(enable));
        }

        void MiddleGunDeathEvent() {
            EnableGunMiddle(false);
            GunMiddle.Conceal();
            _laserObject.SetActive(false);
            _laserDamageComponent.enabled = false;

            //TODO: Fix shooting
            //GunMiddle.ShootingPoint.ShootTime = 0;
            //GunMiddle.ShootingPoint.LoadTime = 0;
        }

        public static void NetworkRailRiderState(NetworkMessage networkMessage) {
            var gameObject = networkMessage.reader.ReadGameObject();
            var state = networkMessage.reader.ReadByte();
            var railRider = gameObject.GetComponent<RailRider>();

            switch(state) {
                case RailRiderState.LeftGun: {
                    var enable = networkMessage.reader.ReadBoolean();

                    railRider.EnableGunLeft(enable);

                    break;
                }

                case RailRiderState.MiddleGun: {
                    var enable = networkMessage.reader.ReadBoolean();

                    railRider.EnableGunMiddle(enable);

                    break;
                }

                case RailRiderState.RightGun: {
                    var enable = networkMessage.reader.ReadBoolean();

                    railRider.EnableGunRight(enable);

                    break;
                }
            }
        }

        void SendState(byte state, System.Action<NetworkWriter> writeAction) {
            if(!NetworkServer.active) return;

            var writer = new NetworkWriter();

            writer.StartMessage(NetMessageType.Client_RailRiderState);
            writer.Write(gameObject);
            writer.Write(state);
            writeAction?.Invoke(writer);
            writer.FinishMessage();

            NetworkServer.SendWriterToReady(gameObject, writer, Channels.DefaultReliable);
        }

        public void SetLaserState(bool enable) {
            RailRiderAnimator.SetBool("Laser Shooting", enable);
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            switch(extraData) {
                case 0: GunLeft.SetTelegraphState(enable); break;
                case 1: GunMiddle.SetTelegraphState(enable); break;
                case 2: GunRight.SetTelegraphState(enable); break;
            }
        }

        void Update() {
            if(NetworkServer.active) {
                //RailRiderAnimator.SetBool("GunLeftOpened", gunLeftOpened);
                //RailRiderAnimator.SetBool("GunMiddleOpened", gunMiddleOpened);
                //RailRiderAnimator.SetBool("GunRightOpened", gunRightOpened);

                if(Direction != 0)
                    transform.Rotate(0, Speed * Direction * Time.deltaTime, 0, Space.World);
            }
        }

        void UpdateHealth() {
            float health = 0;

            switch(Stage) {
                case 1:
                    health += 4;
                    health += (GunLeft.Active ? GunLeft.HealthComponent.NormalizedValue : 0);
                    health += (GunRight.Active ? GunRight.HealthComponent.NormalizedValue : 0);
                    break;

                case 2:
                    health += 3 + (GunMiddle.Active ? GunMiddle.HealthComponent.NormalizedValue : 0);
                    break;

                case 3:
                    health += (GunLeft.Active ? GunLeft.HealthComponent.NormalizedValue : 0);
                    health += (GunMiddle.Active ? GunMiddle.HealthComponent.NormalizedValue : 0);
                    health += (GunRight.Active ? GunRight.HealthComponent.NormalizedValue : 0);
                    break;
            }

            HealthComponent.NormalizedValue = health / 6f;
        }

        static class RailRiderState {
            public const byte LeftGun = 0;
            public const byte MiddleGun = 1;
            public const byte RightGun = 2;
        }
    }
}