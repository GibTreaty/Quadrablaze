using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Scene")]
    public class ScriptableMenuScene : ScriptableObject {

        [SerializeField]
        ScriptableMenu _firstMenu;

        [SerializeField]
        List<MenuConnection> _connections = new List<MenuConnection>();

        public List<MenuConnection> Connections => _connections;

        public ScriptableMenu FirstMenu => _firstMenu;

        public List<ScriptableMenu> GetPath(ScriptableMenu menu) {
            var currentMenu = menu;
            var path = new List<ScriptableMenu>() { menu };
            ScriptableMenu previousMenu = null;

            do {
                previousMenu = GetPreviousMenu(currentMenu);

                if(previousMenu != null) {
                    path.Insert(0, previousMenu);
                    currentMenu = previousMenu;
                }
            }
            while(previousMenu != null);

            return path;
        }

        public ScriptableMenu GetPreviousMenu(ScriptableMenu menu) {
            foreach(var connection in _connections)
                if(connection.To == menu)
                    return connection.From;

            return null;
        }
    }
}