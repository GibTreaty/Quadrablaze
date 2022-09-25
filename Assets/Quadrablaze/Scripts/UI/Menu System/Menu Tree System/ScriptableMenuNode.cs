using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze.Menu {
    public class ScriptableMenuNode : ScriptableObject {

        [SerializeField]
        Vector2 _position;

        //[SerializeField]
        //ScriptableMenu _menu;

        [SerializeField]
        ConnectionPoint _input = new ConnectionPoint(new Vector2(10, 20), ConnectionType.Input);

        [SerializeField]
        ConnectionPoint _output = new ConnectionPoint(new Vector2(10, 20), ConnectionType.Output);

        public Vector2 Position => _position;

        public ConnectionPoint Input => _input;

        public ConnectionPoint Output => _output;


        public Rect RectUI {
            get { return new Rect(Position.x, Position.y, 150, 150); }
        }

        public void DrawGUI(GUIStyle nodeStyle, GUIStyle inputStyle, GUIStyle outputStyle) {
            _input.Draw(this, inputStyle);
            _output.Draw(this, outputStyle);

            //GUI.color = new Color(1,1,1,.2f);
            GUI.Box(RectUI, name, nodeStyle);
            //GUI.color = Color.white;
        }
    }
}