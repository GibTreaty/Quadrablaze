using UnityEngine;

namespace Quadrablaze.Menu {
    [System.Serializable]
    public class ConnectionPoint {

        [SerializeField]
        Vector2 _size;

        [SerializeField]
        ConnectionType _type;

        public Vector2 Size => _size;

        public ConnectionType Type => _type;

        public ConnectionPoint(Vector2 size, ConnectionType type) {
            _size = size;
            _type = type;
        }

        public void Draw(ScriptableMenuNode node, GUIStyle style) {
            var nodeRect = node.RectUI;
            var rect = new Rect(nodeRect.x, nodeRect.y + nodeRect.height * .5f, _size.x, _size.y);

            switch(_type) {
                case ConnectionType.Input:
                    rect.x -= (rect.width * .5f);
                    GUI.color = new Color(.5f, 1, .5f);
                    break;

                case ConnectionType.Output:
                    rect.x += nodeRect.width - (rect.width * .5f);
                    GUI.color = new Color(.5f, .5f, 1);
                    break;
            }

            if(GUI.Button(rect, "", style)) {

            }

            GUI.color = Color.white;
        }
    }

    public enum ConnectionType {
        Input,
        Output
    }
}