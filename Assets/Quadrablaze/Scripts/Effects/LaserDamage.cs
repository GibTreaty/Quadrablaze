using System;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class LaserDamage : MonoBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        float _damage = 1;

        [SerializeField]
        float _distance = 1;

        [SerializeField]
        LayerMask _hitMask;

        [SerializeField]
        LineRenderer _laserLine;

        float lastDamageTime;

        #region Properties
        public float Damage {
            get { return _damage; }
            set { _damage = value; }
        }

        public float Distance {
            //get { return _distance; }
            get { return (GameManager.Current.ArenaRadius * 2) - 2.3f; }
            set { _distance = value; }
        }

        public LayerMask HitMask {
            get { return _hitMask; }
            set { _hitMask = value; }
        }

        public LineRenderer LaserLine {
            get { return _laserLine; }
            set { _laserLine = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            LaserLine.SetPosition(0, Vector3.zero);
            LaserLine.SetPosition(1, Vector3.forward * Distance);
        }

        void Update() {
            if(!NetworkServer.active) return;

            if(Time.time > lastDamageTime + 1) {
                if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Distance, HitMask)) {
                    DoDamage(hit.collider.attachedRigidbody ? hit.collider.attachedRigidbody.gameObject : hit.collider.gameObject);
                    LaserLine.SetPosition(1, Vector3.forward * hit.distance);

                    lastDamageTime = Time.time;
                }
                else
                    LaserLine.SetPosition(1, Vector3.forward * Distance);
            }
        }

        void DoDamage(GameObject gameObject) {
            gameObject.DoHealthChange((int)-Damage, this, true);
        }
    }
}