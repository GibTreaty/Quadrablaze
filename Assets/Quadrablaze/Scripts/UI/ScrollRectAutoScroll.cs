using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectAutoScroll : MonoBehaviour {

    //public Bounds bounds;

    [SerializeField]
    float _autoScrollSpeed = 10;

    List<Selectable> selectables = new List<Selectable>();

    int elementCount;
    //RectTransform scrollRectTransform;

    //public float distanceTop;
    //public float distanceBottom;

    bool reachedTarget = true;
    float targetScrollPosition;

    #region Properties
    Player RewiredPlayer { get; set; }

    ScrollRect ScrollRectComponent { get; set; }
    #endregion

    void OnEnable() {
        if(ScrollRectComponent) {
            ScrollRectComponent.content.GetComponentsInChildren(selectables);
            elementCount = selectables.Count;
        }
    }

    void OnDisable() {
        reachedTarget = true;
    }

    void Awake() {
        RewiredPlayer = ReInput.players.GetPlayer(0);
        ScrollRectComponent = GetComponent<ScrollRect>();
        //scrollRectTransform = ScrollRectComponent.transform as RectTransform;
    }

    void Start() {
        ScrollRectComponent.content.GetComponentsInChildren(selectables);
        elementCount = selectables.Count;
        targetScrollPosition = ScrollRectComponent.verticalNormalizedPosition;
    }

    void Update() {
        if(elementCount > 0) {
            var selectedElement = EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;

            if(selectedElement == null) return;


            bool navigateVertical = RewiredPlayer.GetButtonDown("Menu Vertical") ||
                RewiredPlayer.GetNegativeButtonDown("Menu Vertical") ||
                RewiredPlayer.GetButton("Menu Vertical") ||
                RewiredPlayer.GetNegativeButton("Menu Vertical");

            bool navigateHorizontal = RewiredPlayer.GetButtonDown("Menu Horizontal") ||
                RewiredPlayer.GetNegativeButtonDown("Menu Horizontal") ||
                RewiredPlayer.GetButton("Menu Horizontal") ||
                RewiredPlayer.GetNegativeButton("Menu Horizontal");

            if(navigateVertical ^ navigateHorizontal)
                if(selectedElement.transform.IsChildOf(ScrollRectComponent.transform))
                    ScrollTo(selectedElement);
        }
    }

    void LateUpdate() {
        if(!reachedTarget) {
            float direction = targetScrollPosition - ScrollRectComponent.verticalNormalizedPosition;

            if(Mathf.Abs(direction) > .001f) {
                ScrollRectComponent.verticalNormalizedPosition += direction * Time.unscaledDeltaTime * _autoScrollSpeed;

                if(Mathf.Abs(targetScrollPosition - ScrollRectComponent.verticalNormalizedPosition) <= .05f) {
                    ScrollRectComponent.verticalNormalizedPosition = targetScrollPosition;
                    reachedTarget = true;
                }
            }
        }
    }

    void ScrollTo(Selectable selectable) {
        var rectTransform = selectable.transform as RectTransform;

        Vector3[] selectableCorners = new Vector3[4];
        Vector3[] contentCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];

        float verticalSliderPosition = ScrollRectComponent.verticalNormalizedPosition;

        rectTransform.GetWorldCorners(selectableCorners);
        ScrollRectComponent.content.GetWorldCorners(contentCorners);
        ScrollRectComponent.viewport.GetWorldCorners(viewportCorners);

        //float selectableHeight = Mathf.Abs(selectableCorners[0].y - selectableCorners[1].y);
        float contentHeight = Mathf.Abs(contentCorners[0].y - contentCorners[1].y);
        float viewportHeight = Mathf.Abs(viewportCorners[0].y - viewportCorners[1].y);

        //switch(ScrollRectComponent.verticalScrollbar.direction) {
        //    case Scrollbar.Direction.BottomToTop:
        //        break;
        //    case Scrollbar.Direction.TopToBottom:
        //        break;
        //}

        if(selectableCorners[0].y < viewportCorners[0].y) { // Bottom of selectable is under viewport
            //float distanceFromBottom = contentCorners[0].y - selectableCorners[0].y;
            float distanceFromTop = contentCorners[1].y - selectableCorners[0].y;
            //float normalizedPosition = distanceFromTop / (contentHeight - viewportHeight);
            float normalizedPosition = (distanceFromTop - viewportHeight) / (contentHeight - viewportHeight);

            verticalSliderPosition = 1 - normalizedPosition;
        }
        else if(selectableCorners[1].y > viewportCorners[1].y) { // Top of selectable is over viewport
            //float distanceFromBottom = contentCorners[0].y - selectableCorners[1].y;
            float distanceFromTop = contentCorners[1].y - selectableCorners[1].y;
            float normalizedPosition = distanceFromTop / (contentHeight - viewportHeight);
            //float normalizedPosition = (distanceFromTop - viewportHeight) / (contentHeight - viewportHeight);

            verticalSliderPosition = 1 - normalizedPosition;
        }

        targetScrollPosition = verticalSliderPosition;
        reachedTarget = false;

        //float direction = verticalSliderPosition - ScrollRectComponent.verticalNormalizedPosition;

        //ScrollRectComponent.verticalNormalizedPosition += direction * Time.unscaledDeltaTime;
        //ScrollRectComponent.verticalNormalizedPosition = verticalSliderPosition;
    }

    #region @Pulni#5958's way
    private void ScrollToTransform(ScrollRect scrollRect, RectTransform target) {
        var localPosition = scrollRect.content.InverseTransformPoint(target.TransformPoint(target.rect.max));
        var contentRect = scrollRect.content.rect;
        var scrollRectHeight = ((RectTransform)scrollRect.transform).rect.height;
        var ratio = (localPosition.y - contentRect.yMin - scrollRectHeight) / (contentRect.height - scrollRectHeight);

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(ratio);
    }
    #endregion
}
