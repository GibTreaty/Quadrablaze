using System;
using System.Collections.Generic;
using Quadrablaze.Entities;
using StatSystem;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

//TODO: Sometimes the telegraph will show after player touches this entity but the explosion+damage never happens
namespace Quadrablaze {
    public class ExplosiveController : MonoBehaviour, IActorEntityObjectInitialize, ITelegraphHandler, IStatDeath {

        [SerializeField]
        int _damage = 1;

        [SerializeField]
        float _damageRadius = 2;

        [SerializeField]
        LayerMask _collisionMask = -1;

        [SerializeField]
        float _damageDelay = 1;

        [SerializeField]
        GameObject _telegraphObject;

        [SerializeField]
        float _telegraphLength;

        float _telegraphTimer;
        float _spawnTime;

        uint _entityId;

        //bool initialized;

        #region Properties
        bool CanDamage => Time.time > _spawnTime;
        public LayerMask CollisionMask => _collisionMask;
        public int Damage => _damage;
        public float DamageRadius => _damageRadius;
        public float TelegraphLength => _telegraphLength;
        public float TelegraphTimer {
            get { return _telegraphTimer; }
            set { _telegraphTimer = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            _entityId = entity.Id;
            _telegraphObject.SetActive(false);

            if(NetworkServer.active)
                _spawnTime = Time.time + _damageDelay;

            _telegraphTimer = 0;
            SetTelegraphState(false);

            //if(initialized) return;
            //initialized = true;
        }

        //void OnCausedDamage(HealthEvent healthEvent) {
        //    if(ActorComponent.UserComponent && !healthEvent.description.Contains(HealthHelper.ContinuousDamage)) {
        //        ActorComponent.UserComponent.SpawnHere("Explosions");
        //        ActorComponent.UserComponent.Despawn();
        //    }
        //}

        void DamageCollider(Collider collider) {
            if(!CanDamage) return;

            collider.gameObject.DoHealthChange(-Damage, this, false);
        }

        void DoDamageToRadius() {
            var hit = Physics.OverlapSphere(transform.position, _damageRadius, -1, QueryTriggerInteraction.Ignore);
            var hitList = new List<Transform>();

            foreach(var collider in hit)
                if(!hitList.Contains(collider.transform.root)) {
                    DamageCollider(collider);
                    hitList.Add(collider.transform.root);
                }
        }

        void DelayTelegraphTimer() {
            _telegraphTimer = _telegraphLength;
        }

        void EndTelegraph() {
            if(NetworkServer.active) {
                var entity = GameManager.Current.GetActorEntity(_entityId);

                if(entity != null) {
                    var selfDamage = entity.HealthSlots[0].Value;

                    entity.DoHealthChange(-selfDamage, 0, entity, false);
                }
            }
        }

        void OnCollisionEnter(Collision collision) {
            if(NetworkServer.active)
                if(_telegraphTimer == 0)
                    if((CollisionMask & (1 << collision.collider.gameObject.layer)) > 0) {
                        DelayTelegraphTimer();
                        SetTelegraphState(true);
                        TelegraphStateHandler.SendTelegraphState(gameObject, true);
                    }
        }

        public void OnDeath(StatEvent statEvent) {
            if(NetworkServer.active)
                DoDamageToRadius();

            SetTelegraphState(false);
        }

        void OnDrawGizmos() {
            Gizmos.color = new Color(1, .3f, .3f, .4f);
            Gizmos.DrawWireSphere(transform.position, _damageRadius);
        }

        public void SetTelegraphState(bool enable, byte extraData = 0) {
            if(extraData == 0)
                _telegraphObject.SetActive(enable);
        }

        void Update() {
            if(TelegraphTimer > 0) {
                TelegraphTimer = Mathf.Max(TelegraphTimer - Time.deltaTime, 0);

                if(TelegraphTimer == 0)
                    EndTelegraph();
            }
        }
    }
}