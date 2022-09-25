using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze.Menu {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu Tree")]
    public class ScriptableMenuTree : ScriptableObject {

        [SerializeField]
        List<ScriptableMenuNode> _nodes = new List<ScriptableMenuNode>();

        public List<ScriptableMenuNode> Nodes => _nodes;

    }
}