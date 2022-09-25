using UnityEngine;
using UnityEditor;

namespace Quadrablaze.Menu {
    [System.Serializable]
    public class Connection {

        [SerializeField]
        ConnectionPoint _input;

        [SerializeField]
        ConnectionPoint _output;

        public Connection(ConnectionPoint input, ConnectionPoint output) {
            _input = input;
            _output = output;
        }

        public void Draw() {
            //Handles.DrawBezier(
                //);
                //_input.
        }
    }
}