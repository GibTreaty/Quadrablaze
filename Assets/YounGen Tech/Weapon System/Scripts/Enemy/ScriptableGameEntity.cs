using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Weapon Example/Game Entity")]
    public class ScriptableGameEntity : ScriptableEntity {

        [SerializeField]
        protected string _tag;

        public override Entity CreateInstance() {
            var entity = new GameEntity(0, OriginalGameObject);

            entity.Tag = _tag;

            return entity;
        }
    }
}