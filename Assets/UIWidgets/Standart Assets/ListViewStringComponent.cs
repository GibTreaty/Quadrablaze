using UnityEngine;
using UnityEngine.UI;

namespace UIWidgets {
    /// <summary>
    /// List view item component.
    /// </summary>
    [AddComponentMenu("UI/UIWidgets/ListViewStringComponent")]
    public class ListViewStringComponent : ListViewItem, IViewData<string> {
        /// <summary>
        /// Foreground graphics for coloring.
        /// </summary>
        public override Graphic[] GraphicsForeground {
            get {
                return new Graphic[] { Text, };
            }
        }

        /// <summary>
        /// The Text component.
        /// </summary>
        public Text Text;

        public TMPro.TMP_Text TextMeshProText;

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="item">Text.</param>
        public virtual void SetData(string item) {
            if(Text)
                Text.text = item.Replace("\\n", "\n");

            if(TextMeshProText)
                TextMeshProText.text = item.Replace("\\n", "\n");
        }
    }
}