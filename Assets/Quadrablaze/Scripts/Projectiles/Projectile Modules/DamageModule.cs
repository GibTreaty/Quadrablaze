using System.Collections.Generic;
using UnityEngine;
using YounGenTech.Entities.Weapon;

namespace Quadrablaze.WeaponSystem {
    public class DamageModule : ProjectileModule, IProjectileModuleDestroyed { //IProjectileModuleGameObjectSet

        public float DamageAmount { get; set; }
        public float DamageMultiplier { get; set; }
        public HealthEvent.EventType HealthEventType { get; }

        TriggerBehaviour triggerBehaviour;

        public DamageModule(ProjectileEntity attachedProjectile, float damage, HealthEvent.EventType eventType) : base(attachedProjectile) {
            attachedProjectile.OnGameObjectSet += SetupTriggerEvent;

            DamageAmount = damage;
            DamageMultiplier = attachedProjectile.SourceWeapon.DamageMultiplier;
            HealthEventType = eventType;
        }

        void CheckDamage(Collider collider) {
            bool damageFlag = false;

            damageFlag |= AttachedProjectile.HitMask != 0 && (AttachedProjectile.HitMask & (1 << collider.gameObject.layer)) > 0;
            damageFlag |= AttachedProjectile.HitTags != null && AttachedProjectile.HitTags.Contains(collider.tag);

            if(damageFlag) {
                //collider.Damage(new HealthEvent(AttachedProjectile.SourceTransform.gameObject, DamageAmount * DamageMultiplier, "Projectile Damage", HealthEventType));
                collider.gameObject.DoHealthChange((int)-(DamageAmount * DamageMultiplier), AttachedProjectile, false);
            }
        }

        void OnTriggerEnter(Collider collider) {
            CheckDamage(collider);
        }

        void SetupTriggerEvent(GameObject gameObject) {
            triggerBehaviour = gameObject.GetComponent<TriggerBehaviour>();

            if(triggerBehaviour != null)
                triggerBehaviour.OnTrigger3DEntered += OnTriggerEnter;
        }
         
        public void ModuleDestroyed() {
            triggerBehaviour.OnTrigger3DEntered -= OnTriggerEnter;
        }
    }
}