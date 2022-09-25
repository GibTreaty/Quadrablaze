using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Quadrablaze {
    [CustomPreview(typeof(QuadrablazeTransformSync))]
    public class QuadrablazeTransformSyncPreview : ObjectPreview {

        QuadrablazeTransformSync component;
        QuadrablazeTransformSync[] components;

        public override void Initialize(Object[] targets) {
            base.Initialize(targets);

            component = target as QuadrablazeTransformSync;

            //components = new QuadrablazeTransformSync[targets.Length];

            //for(int i = 0; i < targets.Length; i++)
            //    components[i] = targets[i] as QuadrablazeTransformSync;
        }

        public override GUIContent GetPreviewTitle() {
            return new GUIContent("Transform Sync");
        }

        public override bool HasPreviewGUI() {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            GUI.BeginGroup(r);
            {
                var rect = new Rect(0, 0, r.width, 15);

                GUI.Label(rect, "Last Sync Time:" + component.LastSyncTime);

                rect.y += rect.height + 5;
                GUI.Label(rect, "Last Send Time:" + component.LastSendTime);
            }
            GUI.EndGroup();
        }
    }
}