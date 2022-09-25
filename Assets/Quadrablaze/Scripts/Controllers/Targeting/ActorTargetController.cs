using UnityEngine;
using System.Linq;

namespace Quadrablaze {
    public class ActorTargetController : TargetController {

        #region Properties
        public override Transform Target {
            set {
                if(Target == value) return;

                _target = value;

                if(Target) {
                    var collider = _target.GetComponent<Collider>();
                    TargetBody = collider.attachedRigidbody ? collider.attachedRigidbody.transform : collider.transform;
                }
                else
                    TargetBody = null;

                if(onTargetChanged != null) onTargetChanged.Invoke(Target);
            }
        }
        #endregion

        public override Transform FindTarget() {
            var colliders = GetColliders();

            if(colliders.Length == 0) return null;

            if(colliders.Length > 1)
                colliders = colliders.OrderBy(s => Mathf.Abs((s.transform.position - transform.position).sqrMagnitude)).ToArray();

            var outputTarget = colliders.FirstOrDefault(TargetIsEntity);

            TargetBody = (outputTarget && outputTarget.attachedRigidbody) ? outputTarget.attachedRigidbody.transform : null;

            return outputTarget ? outputTarget.transform : null;

            bool TargetIsEntity(Collider collider) {
                if(collider.attachedRigidbody != null)
                    //TODO: Get life-status from ActorEntity
                    //if(GameManager.Current.GetActorEntity(collider.attachedRigidbody.gameObject, out Entities.ActorEntity actorEntity))
                    if(GameManager.Current.GetActorEntity(collider.attachedRigidbody.gameObject))
                        if(collider.GetComponent<HealthLayer>() is HealthLayer layer)
                            if(layer.HealthStatusAlive())
                                if(!layer.HealthComponent.Invincible)
                                    return true;

                return false;
            }
        }

        public override void UpdateTarget() {
            base.UpdateTarget();

            if(Target) {
                var healthLayer = Target.GetComponent<HealthLayer>();

                if(!healthLayer.HealthStatusAlive() || healthLayer.HealthComponent.Invincible)
                    Target = null;
            }
        }
    }
}