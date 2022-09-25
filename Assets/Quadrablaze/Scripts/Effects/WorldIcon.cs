using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Quadrablaze {
    [AddComponentMenu("Quadrablaze/Effects/World Icon"), ExecuteInEditMode, RequireComponent(typeof(SpriteRenderer))]
    public class WorldIcon : MonoBehaviour {

        [SerializeField]
        LayerMask _raycastMask = 0;

        [SerializeField]
        float _visibleDistance = 1;

        //[SerializeField]
        //float _size = 1;

        public BooleanEvent onChangeVisibility;

        #region Properties
        /// <summary>
        /// Can this icon be obstructed by anything.
        /// </summary>
        public LayerMask RaycastMask {
            get { return _raycastMask; }
            set { _raycastMask = value; }
        }

        //public float Size {
        //    get { return _size; }
        //    set { _size = value; }
        //}

        public SpriteRenderer SpriteRendererComponent { get; private set; }

        /// <summary>
        /// The distance that this icon will be completely in visible.
        /// </summary>
        public float VisibleDistance {
            get { return _visibleDistance; }
            set { _visibleDistance = value; }
        }
        #endregion

        void Awake() {
            SpriteRendererComponent = GetComponent<SpriteRenderer>();
        }

        void OnWillRenderObject() {
            Color color = SpriteRendererComponent.color;

            if(Camera.current.transform.InverseTransformPoint(transform.position).z > 0) {
                color.a = RaycastMask != 0 && Physics.Linecast(transform.position, Camera.current.transform.position, RaycastMask) ?
                    0 :
                    Mathf.Clamp01(VisibleDistance - Vector3.Distance(transform.position, Camera.current.transform.position));
            }
            else
                color.a = 0;

            if(SpriteRendererComponent.color.a != color.a)
                onChangeVisibility.InvokeEvent(color.a > 0);

            SpriteRendererComponent.color = color;

            transform.rotation = Camera.current.transform.rotation;
        }
    }
}