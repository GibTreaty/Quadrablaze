using System;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.EnergyBasedObjects;
using YounGenTech.YounGenShooter;

namespace Quadrablaze {
    public class WeaponObject : MonoBehaviour {

        [SerializeField]
        int _layoutElementIndex;

        [SerializeField]
        float _cameraShake = 6;

        [SerializeField]
        bool _cameraShakeCapped = true;

        NetworkInstanceId _ownerID;

        #region Properties
        public Accuracy AccuracyComponent { get; protected set; }

        public float CameraShake {
            get { return _cameraShake; }
            set { _cameraShake = value; }
        }

        public bool CameraShakeCapped {
            get { return _cameraShakeCapped; }
            set { _cameraShakeCapped = value; }
        }

        public WeaponDamage Damage { get; protected set; }

        public ParticleSystem EffectParticles { get; protected set; }

        public NetworkInstanceId OwnerID {
            get { return _ownerID; }
            set { _ownerID = value; }
        }

        public ObjectShootingPoint ShootingPoint { get; protected set; }

        public ObjectSpawnerPusher SpawnerPusherShootingPoint { get; protected set; }

        public RandomizeAudioPitch SoundRandomizer { get; protected set; }

        public AudioSource SoundSource { get; protected set; }

        public int LayoutElementIndex {
            get { return _layoutElementIndex; }
            set { _layoutElementIndex = value; }
        }
        #endregion

        public void Initialize(int weaponIndex, int layoutElementIndex, NetworkInstanceId ownerNetId) {
            ShootingPoint = GetComponentInChildren<ObjectShootingPoint>();

            if(ShootingPoint is ObjectSpawnerPusher) {
                SpawnerPusherShootingPoint = ShootingPoint as ObjectSpawnerPusher;
                SpawnerPusherShootingPoint.OnWeaponLoad += OnProjectilesLoaded;
            }

            AccuracyComponent = GetComponentInChildren<Accuracy>();
            Damage = GetComponentInChildren<WeaponDamage>();
            EffectParticles = GetComponentInChildren<ParticleSystem>();
            SoundRandomizer = GetComponentInChildren<RandomizeAudioPitch>();
            SoundSource = GetComponentInChildren<AudioSource>();

            ShootingPoint?.onWeaponShoot.AddListener(OnShootServer);

            LayoutElementIndex = layoutElementIndex;
            OwnerID = ownerNetId;
        }

        public void Shoot() {
            ShootingPoint?.Shoot();
        }

        void OnProjectilesLoaded(ObjectShootingPoint shootingPoint) {
            foreach(var projectile in SpawnerPusherShootingPoint.pushProjectiles) {
                var projectileBase = projectile.GetComponent<ProjectileBase>();

                if(projectileBase != null)
                    projectileBase.InitializeFromWeapon(gameObject);
            }
        }

        void OnShootServer(ObjectShootingPoint shootingPoint) {
            if(NetworkServer.active) {
                //AccuracyComponent?.AddToInaccuracy();

                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_WeaponShoot);
                writer.Write(OwnerID);
                writer.Write(LayoutElementIndex);
                writer.FinishMessage();

                QuadrablazeSteamNetworking.SendWriterToAll(writer);
                // TODO: Store reference to the Actor component to avoid sending enemy shots to the clients
                //NetworkServer.SendWriterToReady(transform.root.gameObject, writer, Channels.DefaultUnreliable);
            }
        }

        protected void OnShoot() {
            if(CameraShake > 0)
                if(CameraShakeCapped)
                    ShakeyCamera.Current.ShakeCapped(CameraShake);
                else
                    ShakeyCamera.Current.Shake(CameraShake);

            AccuracyComponent?.AddToInaccuracy();

            if(SoundSource != null) SoundSource.Play();
            if(EffectParticles != null) EffectParticles.Play();
            if(SoundRandomizer != null) SoundRandomizer.RandomizePitch();
        }

        public static void NetworkWeaponShoot(NetworkMessage networkMessage) {
            var ownerObject = networkMessage.reader.ReadGameObject();

            if(!ownerObject) return;

            var actor = ownerObject.GetComponent<Actor>();

            var layoutElementIndex = networkMessage.reader.ReadInt32();
            //var layoutElement = actor.CurrentUpgradeSet.CurrentSkillLayout.SkillElements[layoutElementIndex];

            //if(actor.CurrentUpgradeSet.CurrentSkillLayout.SkillLookupTable.TryGetValue(layoutElement, out SkillExecutor skillExecutor)) {
            //    var weaponExecutor = skillExecutor as WeaponSkillExecutor;

            //    weaponExecutor?.EquippedWeapon?.OnShoot();
            //}
        }
    }
}