using UnityEngine;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Ability Wheel")]
    public class AbilityWheelMenu : ScriptableMenu {

        public override void Close(MenuItem item, UIManager manager) {
            var pieWheel = item.MainGameObject.GetComponent<PieWheel>();

            pieWheel.EnableWheel(false);
        }

        public override void Open(MenuItem item, UIManager manager) {
            var pieWheel = item.MainGameObject.GetComponent<PieWheel>();

            pieWheel.EnableWheel(true);
        }
    }
}