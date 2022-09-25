using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    [RequireComponent(typeof(Owner))]
    public abstract class ProjectileBase : MonoBehaviour {

        static float BossDamagePercentage { get; } = .4f;

        [field: SerializeField]
        public float Damage { get; set; } = 1;

        [field: SerializeField]
        public float DamageToBoss { get; set; } = 1;

        [field: SerializeField, FormerlySerializedAs("_damageByPercentage")]
        public bool DamageByPercentage { get; set; }

        [field: SerializeField]
        public bool DamageBossByPercentage { get; set; }

        [SerializeField]
        bool _destroyOnHit = true;

        [SerializeField]
        protected bool _offlineOnly;

        [SerializeField]
        ColliderEvent _onHit;

        #region Properties
        public bool DestroyOnHit {
            get { return _destroyOnHit; }
            set { _destroyOnHit = value; }
        }

        public bool OfflineOnly {
            get { return _offlineOnly; }
            set { _offlineOnly = value; }
        }

        public ColliderEvent OnHit {
            get { return _onHit; }
            private set { _onHit = value; }
        }

        public Owner OwnerComponent { get; private set; }

        PoolUser PoolUserComponent { get; set; }
        #endregion

        void Awake() {
            PoolUserComponent = GetComponent<PoolUser>();
            OwnerComponent = GetComponent<Owner>();

            if(OnHit == null) OnHit = new ColliderEvent();

            Initialize();
        }

        protected void DoDamage(Collider hit) {
            if(_offlineOnly || NetworkServer.active) {
                if(Damage > 0) {
                    if(hit.GetComponent<EntityHitSpot>() is EntityHitSpot hitSpot) {
                        if(GameManager.Current.GetActorEntity(hit.gameObject, out ActorEntity actorEntity)) {
                            if(DamageByPercentage) {
                                float damage = actorEntity.HealthSlots[hitSpot.HealthId].MaxValue;

                                switch(actorEntity) {
                                    case Entities.BossEntity entity: { damage *= DamageToBoss; break; }
                                    default: { damage *= Damage; break; }
                                }

                                actorEntity.DoHealthChange(-(int)Mathf.Max(damage, 1), hitSpot.HealthId, this, false);
                            }
                            else {
                                int damage = (int)Damage;

                                if(actorEntity is Entities.BossEntity)
                                    damage = (int)DamageToBoss;

                                hit.gameObject.DoHealthChange(-damage, this, true);
                            }
                        }

                        //var layer = hit.GetComponent<HealthLayer>();

                        //if(layer) {
                        //    float damage = Damage;
                        //    var eventType = _damageByPercentage ? HealthEvent.EventType.Percent : HealthEvent.EventType.Normal;
                        //    var hitActor = hit.transform.root.GetComponent<Actor>();

                        //    if(eventType == HealthEvent.EventType.Percent)
                        //        if(hitActor != null)
                        //            if(hitActor.ActorType == ActorTypes.Boss)
                        //                damage *= bossDamagePercentage;

                        //    layer.Group.Damage(layer, new HealthEvent(gameObject, layer.gameObject, damage, HealthHelper.IsProjectile, eventType));
                        //}
                    }
                }
            }
        }

        protected void DoDestroy() {
            if(_offlineOnly || NetworkServer.active)
                if(DestroyOnHit)
                    if(PoolUserComponent)
                        PoolUserComponent.Despawn();
                    else
                        Destroy(gameObject);
        }

        protected void FireHitEvent(Collider hit) {
            if(_offlineOnly || NetworkServer.active)
                _onHit?.Invoke(hit);
        }

        public virtual void Initialize() { }

        public virtual void InitializeFromWeapon(GameObject weaponObject) {
            //public virtual void InitializeFromWeapon(WeaponProperties weaponProperties) {
            //Damage = weaponProperties.GetValueFromDisplayName<float>("Damage");

            Damage = (int)weaponObject.GetComponentInChildren<WeaponDamage>().Damage;
        }
    }
}