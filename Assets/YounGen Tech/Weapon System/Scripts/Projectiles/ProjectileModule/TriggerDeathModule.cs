using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class TriggerDeathModule : ProjectileModule {

        public List<string> TriggerTags { get; set; }

        GameObject attachedGameObject;

        public TriggerDeathModule(ProjectileEntity attachedProjectile, params string[] triggerTags) : base(attachedProjectile) {
            attachedProjectile.OnGameObjectSet += SetupTriggerEvent;

            TriggerTags = new List<string>(triggerTags);

            attachedProjectile.OnDestroyed += OnProjectileDestroyed;
        }

        void CheckDeath(int layer, string tag) {
            bool flag = false;

            flag |= AttachedProjectile.HitMask != 0 && (AttachedProjectile.HitMask & (1 << layer)) > 0;
            flag |= AttachedProjectile.HitTags != null && AttachedProjectile.HitTags.Contains(tag);

            if(flag) {
                AttachedProjectile.SetDeathFlag("Trigger");

                if(attachedGameObject.GetComponent<TriggerBehaviour>() is TriggerBehaviour trigger) {
                    trigger.OnTrigger2DEntered -= OnTriggerEnter;
                    trigger.OnTrigger3DEntered -= OnTriggerEnter;
                }
            }
        }

        void OnProjectileDestroyed() {
            if(attachedGameObject.GetComponent<TriggerBehaviour>() is TriggerBehaviour trigger) {
                trigger.OnTrigger2DEntered -= OnTriggerEnter;
                trigger.OnTrigger3DEntered -= OnTriggerEnter;
            }
        }

        void OnTriggerEnter(Collider collider) {
            CheckDeath(collider.gameObject.layer, collider.tag);
        }

        void OnTriggerEnter(Collider2D collider) {
            CheckDeath(collider.gameObject.layer, collider.tag);
        }

        void SetupTriggerEvent(GameObject gameObject) {
            attachedGameObject = gameObject;

            if(attachedGameObject.GetComponent<TriggerBehaviour>() is TriggerBehaviour trigger) {
                trigger.OnTrigger2DEntered += OnTriggerEnter;
                trigger.OnTrigger3DEntered += OnTriggerEnter;
            }
        }
    }
}