using UnityEngine;
using UnityEngine.EventSystems;

namespace Quadrablaze {
    public class UISelectableSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IScrollHandler, IMoveHandler, ISubmitHandler {

        [SerializeField]
        UISoundDatabase _database;

        [SerializeField]
        SoundToggles _options;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            if(_options.onEnter) PlaySound(_database.SelectableEnter);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            if(_options.onExit) PlaySound(_database.SelectableExit);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if(_options.onDown) PlaySound(_database.SelectableDown);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            if(_options.onUp) PlaySound(_database.SelectableUp);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            if(_options.onClick) PlaySound(_database.SelectableSelect);
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData) {
            if(_options.onSubmit) PlaySound(_database.SelectableSubmit);
        }

        void IScrollHandler.OnScroll(PointerEventData eventData) {
            if(_options.onScroll) PlaySound(_database.SelectableScroll);
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
        }

        void IMoveHandler.OnMove(AxisEventData eventData) {
            if(_options.onMove) PlaySound(_database.SelectableMove);
        }

        void PlaySound(AudioClip clip) {
            if(Camera.main != null)
                Camera.main.GetComponent<AudioSource>().PlayOneShot(clip);
        }

        [System.Serializable]
        public struct SoundToggles {
            public bool onEnter;
            public bool onExit;
            public bool onDown;
            public bool onUp;
            public bool onClick;
            public bool onSubmit;
            public bool onScroll;
            public bool onMove;
        }
    }
}