using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Quadrablaze {
    public class ShieldDrainer : MonoBehaviour {

        [field: SerializeField, FormerlySerializedAs("_damage")]
        public int Damage { get; set; } = 1;

        [SerializeField]
        float _distance = 1;

        [SerializeField]
        LayerMask _hitMask;

        [SerializeField]
        MeshRenderer _mesh;

        [SerializeField, ColorUsage(true, true)]
        Color _activeColor;

        [SerializeField]
        float _colorChangeSpeed = 1;

        public FloatEvent OnChangeLerp;

        float lastDamagedTime;
        float _damageLerp;
        bool _damagedLastFrame;
        bool _activeSync;

        WeaponObject weaponObject;

        #region Properties
        public bool Active { get; set; }

        public float ColorChangeSpeed {
            get { return _colorChangeSpeed; }
            set { _colorChangeSpeed = value; }
        }

        float DamageLerp {
            get { return _damageLerp; }
            set {
                _damageLerp = value;

                ChangeColor(value);
                OnChangeLerp.InvokeEvent(value);
            }
        }

        public float Distance {
            get { return _distance; }
            set { _distance = value; }
        }

        public LayerMask HitMask {
            get { return _hitMask; }
            set { _hitMask = value; }
        }

        public MeshRenderer Mesh {
            get { return _mesh; }
            set { _mesh = value; }
        }

        public bool TargetShieldWasFull { get; set; }
        #endregion

        void Awake() {
            weaponObject = GetComponentInParent<WeaponObject>();
            ChangeColor(0);
        }

        void Update() {
            if(NetworkServer.active)
                if(Time.time > lastDamagedTime + 1) {
                    if(Active) {
                        var hits = Physics.RaycastAll(transform.position, transform.forward, Distance, HitMask);

                        foreach(RaycastHit hit in hits) {
                            if(hit.collider.attachedRigidbody != null)
                                if(hit.collider.attachedRigidbody.CompareTag("Player"))
                                    foreach(var entity in GameManager.Current.Entities)
                                        if(entity is PlayerEntity playerEntity)
                                            if(playerEntity.RigidbodyComponent == hit.collider.attachedRigidbody) {
                                                var executor = playerEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Shield>();

                                                if(executor != null) {
                                                    if(!TargetShieldWasFull && executor.WasFullyRegenerated)
                                                        TargetShieldWasFull = true;

                                                    if(TargetShieldWasFull)
                                                        if(DoDamage(executor)) {
                                                            if(!_activeSync)
                                                                SyncActive(true);

                                                            DamageLerp = Mathf.Min(DamageLerp + Time.deltaTime * ColorChangeSpeed, 1);
                                                            _damagedLastFrame = true;
                                                            lastDamagedTime = Time.time;

                                                            break;
                                                        }
                                                }
                                            }
                        }
                    }

                    if(!_damagedLastFrame && DamageLerp > 0) {
                        if(_activeSync)
                            SyncActive(false);

                        DamageLerp = Mathf.Max(DamageLerp - Time.deltaTime, 0);
                    }
                }
                else {
                    if(_activeSync) {
                        if(DamageLerp < 1)
                            DamageLerp = Mathf.Min(DamageLerp + Time.deltaTime * ColorChangeSpeed, 1);
                    }
                    else {
                        if(DamageLerp > 0)
                            DamageLerp = Mathf.Max(DamageLerp - Time.deltaTime, 0);
                    }
                }
        }

        void LateUpdate() {
            if(NetworkServer.active)
                _damagedLastFrame = false;
        }

        bool DoDamage(Shield shield) {
            return shield.DoHealthChange(-Damage, this, true);
        }

        void ChangeColor(float value) {
            var color = Color.LerpUnclamped(Color.clear, _activeColor, value);

            Mesh.material.SetColor("_TintColor", color);
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * Distance);
        }

        void SyncActive(bool enable) {
            if(QuadrablazeSteamNetworking.Current.Players.Count > 1) {
                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_ShieldDrainer);
                writer.Write(weaponObject.OwnerID);
                writer.Write(enable);
                writer.FinishMessage();

                QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
            }

            _activeSync = enable;
        }

        static void OnShieldDrain(NetworkMessage message) {
            var gameObject = message.reader.ReadGameObject();

            if(gameObject != null) {
                var activeSync = message.reader.ReadBoolean();
                var deformer = gameObject.GetComponent<DeformerController>();

                deformer.ShieldDrainerComponent._activeSync = activeSync;
            }
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_ShieldDrainer, OnShieldDrain);
        }
    }
}