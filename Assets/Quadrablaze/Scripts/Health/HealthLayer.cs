using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class HealthLayer : MonoBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        bool _active = true;

        [SerializeField]
        Health _healthComponent;

        [SerializeField]
        string _description;

        #region Properties
        public bool Active {
            get { return _active; }
            set { _active = value; }
        }

        public string Description {
            get { return _description; }
            set { _description = value; }
        }

        public uint EntityId { get; private set; }

        public HealthGroup Group { get; set; }

        public Health HealthComponent {
            get { return _healthComponent; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            EntityId = entity.Id;
        }

        public bool Damage(HealthEvent healthEvent) {
            var entity = GameManager.Current.GetActorEntity(EntityId);

            if(entity.IsInvincible) return false;
            if(!HealthComponent) return false;

            healthEvent.description += Description;

            var damaged = HealthComponent.Damage(healthEvent);

            if(damaged) Group.OnDamaged.InvokeEvent();

            return damaged;
        }

        /// <summary>
        /// Checks if the <see cref="Health"/> component is alive. If no <see cref="Health"/> component exists, then is assumed to be alive.
        /// </summary>
        public bool HealthStatusAlive() {
            return gameObject.activeInHierarchy && Active && (!HealthComponent || HealthComponent.NormalizedValue > 0);
        }
    }
}