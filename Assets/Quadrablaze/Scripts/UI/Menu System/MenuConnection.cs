using UnityEngine;

namespace Quadrablaze.MenuSystem {
    [System.Serializable]
    public class MenuConnection {
        [SerializeField]
        ScriptableMenu _from;

        [SerializeField]
        ScriptableMenu _to;

        public ScriptableMenu From => _from;

        public ScriptableMenu To => _to;
    }
}