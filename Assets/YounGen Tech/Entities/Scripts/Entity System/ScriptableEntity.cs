using UnityEngine;

namespace YounGenTech.Entities {
    [CreateAssetMenu(menuName = "YounGen Tech/Entities/Basic Entity")]
    public abstract class ScriptableEntity : ScriptableObject {

        [SerializeField]
        GameObject _originalGameObject;

        public GameObject OriginalGameObject => _originalGameObject;

        public virtual Entity CreateInstance() {
            return new Entity(0, OriginalGameObject);
        }
    }
}