using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Quadrablaze/Entities/Boss Entity")]
    public class BossEntity : ScriptableObject {

        [SerializeField]
        GameObject _prefab;

        [SerializeField]
        string _id;

        [Header("Object Pool Settings")]
        [SerializeField]
        int _prefabId;

        public string Id {
            get { return _id; }
            set { _id = value; }
        }

        public GameObject Prefab {
            get { return _prefab; }
            set { _prefab = value; }
        }

        public int PrefabId {
            get { return _prefabId; }
            set { _prefabId = value; }
        }
    }
}