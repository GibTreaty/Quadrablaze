using UnityEngine;
using Quadrablaze.Entities;
using Quadrablaze;

public class EntityCollisionTrigger : MonoBehaviour {

    public uint EntityId { get; set; }

    void Invoke(Collision collision) {
        if(EntityId > 0)
            if(GameManager.Current.GetActorEntity(EntityId) is ActorEntity entity)
                if(entity.CurrentUpgradeSet != null && entity.CurrentUpgradeSet.CurrentSkillLayout != null)
                    foreach(var element in entity.CurrentUpgradeSet.CurrentSkillLayout.Elements)
                        if(element.CurrentExecutor is IEntityCollisionTrigger collisionTrigger)
                            collisionTrigger.OnEntityCollision(collision);
    }

    void OnCollisionEnter(Collision collision) {
        Invoke(collision);
    }

    void OnCollisionStay(Collision collision) {
        Invoke(collision);
    }
}

public interface IEntityCollisionTrigger {
    void OnEntityCollision(Collision collision);
}