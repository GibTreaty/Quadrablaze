using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze.MenuSystem {
    [System.Serializable]
    public class MenuItem {

        [SerializeField]
        ScriptableMenu _menuLogic;

        [SerializeField]
        GameObject _mainGameObject;

        [SerializeField]
        GameObject[] _subGameObjects;

        [SerializeField]
        Selectable[] _primarySelectables;

        public GameObject MainGameObject => _mainGameObject;

        public ScriptableMenu MenuLogic => _menuLogic;

        public string Name => _menuLogic != null ? _menuLogic.MenuName : "";

        public Selectable[] PrimarySelectables => _primarySelectables;

        public GameObject[] SubGameObjects => _subGameObjects;

        public Selectable GetActiveSelectable() {
            Selectable output = null;

            foreach(var item in _primarySelectables)
                if(item.gameObject.activeSelf) {
                    output = item;
                    break;
                }

            return output;
        }
    }

    [System.Serializable]
    public partial struct MenuOptions {
        public bool alwaysOnTop;
        public bool closableRoot;
        public bool closableOutOfRound;
        public bool closableDuringRound;
        public bool disableCancelButton;
        public bool pauseWhileOpen;
        public bool rootIsContainer;
        public bool showBackground;
    }
}