using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderMoveHandler : MonoBehaviour, IMoveHandler, IEndDragHandler {

    [SerializeField]
    float _step = 0.1f;

    [SerializeField]
    float _repeatStep = 1;

    [SerializeField]
    float _repeatDelay = .4f;

    Slider slider;
    float previousSliderValue = 0f;
    float previousActionTime;
    int consecutiveMoveCount;

    #region Properties
    public float RepeatDelay {
        get { return _repeatDelay; }
        set { _repeatDelay = value; }
    }

    public float RepeatStep {
        get { return _repeatStep; }
        set { _repeatStep = value; }
    }

    public float Step {
        get { return _step; }
        set { _step = value; }
    }
    #endregion

    void Awake() {
        slider = GetComponent<Slider>();

        if(slider)
            previousSliderValue = slider.value;
    }

    public void OnMove(AxisEventData eventData) {
        //0 = horizontal
        //1 = vertical
        int axis = slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft ? 0 : 1;

        float time = Time.unscaledTime;
        float step = Step;

        //if(RepeatDelay > 0)
        //    if(time > previousActionTime + RepeatDelay) {
        //        step = RepeatStep;
        //    }

        switch(eventData.moveDir) {
            case MoveDirection.Left:
                if(axis == 0 && slider.FindSelectableOnLeft() == null) {
                    slider.value = previousSliderValue - step;
                    previousActionTime = time;
                }

                break;
            case MoveDirection.Right:
                if(axis == 0 && slider.FindSelectableOnRight() == null) {
                    slider.value = previousSliderValue + step;
                    previousActionTime = time;
                }

                break;
            case MoveDirection.Down:
                if(axis == 1 && slider.FindSelectableOnDown() == null) {
                    slider.value = previousSliderValue - step;
                    previousActionTime = time;
                }

                break;
            case MoveDirection.Up:
                if(axis == 1 && slider.FindSelectableOnUp() == null) {
                    slider.value = previousSliderValue + step;
                    previousActionTime = time;
                }

                break;
        }

        previousSliderValue = slider.value;
    }

    public void OnEndDrag(PointerEventData eventData) {
        // keep the last slider value if the slider was dragged by mouse
        previousSliderValue = slider.value;
    }
}