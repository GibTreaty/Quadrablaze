using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class ActorInputBase : NetworkBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        bool _isImmobilized;

        #region Properties
        public uint EntityId { get; private set; }

        public bool IsImmobilized {
            get { return _isImmobilized; }
            set { _isImmobilized = value; }
        }
        #endregion

        public virtual void ActorEntityObjectInitialize(ActorEntity entity) {
            EntityId = entity.Id;
        }
    }
}