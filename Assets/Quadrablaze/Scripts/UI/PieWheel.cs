using Quadrablaze;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO Reset the wheel when the player starts a second match

public class PieWheel : MonoBehaviour {

    [SerializeField]
    GameObject _originalIcon;

    [SerializeField]
    float _iconHeightPercentage = .75f;

    [SerializeField]
    Vector2 _currentSelectDirection;

    [SerializeField]
    float _selectionDeadzone = .5f;

    [SerializeField]
    float _acceleration = 10;

    [SerializeField]
    List<GameObject> _iconList;

    [SerializeField]
    Image _pieImage;

    [SerializeField]
    RectTransform _selectionArrow;

    [SerializeField]
    RectTransform[] _actionGlyphs;

#if UNITY_EDITOR
    [SerializeField]
    int _setSlices;
#endif

    int _slices;
    bool _wheelActive;
    int currentSelectedSlice = 0;

    Material material;
    RectTransform rectTransform;

    Player rewiredPlayer;

    #region Properties
    public RectTransform[] ActionGlyphs {
        get { return _actionGlyphs; }
    }

    public int IconCount {
        get { return _iconList.Count; }
    }

    public bool WheelActive {
        get { return _wheelActive; }
    }
    #endregion

    public void AddIcon(Sprite sprite) {
        //Debug.Log("Adding sprite - " + sprite.name);
        var gameObject = InstantiateIcon(sprite);

        _iconList.Add(gameObject);

        gameObject.SetActive(_wheelActive);
        UpdateIconPositions();
    }

#if UNITY_EDITOR
    [ContextMenu("Enable Wheel")]
    void EnableWheel() {
        EnableWheel(true);
    }
#endif

    public void EnableWheel(bool enable) {
        if(_wheelActive != enable)
            ForceEnableWheel(enable);

        Cursor.visible = !enable;

        CursorLockMode unlockState = CursorLockMode.None;

        switch(Screen.fullScreenMode) {
            case FullScreenMode.ExclusiveFullScreen:
            case FullScreenMode.FullScreenWindow:
                unlockState = CursorLockMode.Confined;
                break;
        }

        Cursor.lockState = enable ? CursorLockMode.Locked : unlockState;
    }

    void ForceEnableWheel(bool enable) {
        _wheelActive = enable;
        _pieImage.enabled = enable;
        _currentSelectDirection = Vector2.zero;
        currentSelectedSlice = 0;
        material.SetInt("_HighlightSlice", 0);

        if(!enable)
            _selectionArrow.gameObject.SetActive(false);

        foreach(var icon in _iconList)
            icon.SetActive(enable);

        if(UIManager.Current != null)
            UIManager.Current.abilityWheel.UpdateActionGlyphs();
    }

    public int GetCurrentSlice() {
        float angleSize = 360f / _slices;
        float currentAngle = (Mathf.Atan2(_currentSelectDirection.x, _currentSelectDirection.y) * Mathf.Rad2Deg) + 180 + (angleSize * .5f);
        int currentSlice = (int)(((currentAngle / 360) % 1) * _slices) + 1;

        return currentSlice;
    }

    public Vector2 GetDeadzonedSelection() {
        return _currentSelectDirection.sqrMagnitude > _selectionDeadzone ? _currentSelectDirection : Vector2.zero;
    }

    public GameObject GetIcon(int index) {
        return _iconList[index];
    }

    public void Initialize() {
        if(material == null) {
            var imageComponent = _pieImage.GetComponent<Image>();

            if(imageComponent != null) {
                material = Instantiate(imageComponent.material);

                imageComponent.material = material;
            }
            else {
                Debug.LogError("Pie Wheel could not find an Image component");
                enabled = false;
            }
        }

        if(rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if(ReInput.isReady)
            rewiredPlayer = ReInput.players.GetPlayer(0);

        //_wheelActive = _pieImage != null ? _pieImage.enabled : false;
        ForceEnableWheel(false);
        SetSlices(0);
    }

    public void InsertIcon(int index, Sprite sprite) {
        index = Mathf.Clamp(index, 0, _iconList.Count - 1);

        var gameObject = InstantiateIcon(sprite);

        _iconList.Insert(index, gameObject);

        gameObject.SetActive(true);
        UpdateIconPositions();
    }

    GameObject InstantiateIcon(Sprite sprite) {
        var gameObject = Instantiate(_originalIcon, transform, false);
        var image = gameObject.GetComponent<Image>();

        image.sprite = sprite;

        return gameObject;
    }

    public void RemoveAllIcons() {
        foreach(var gameObject in _iconList)
            Destroy(gameObject);

        _iconList.Clear();
    }

    public void RemoveIcon(int index) {
        if(index > 0 && index < _iconList.Count) {
            Destroy(_iconList[index]);
            _iconList.RemoveAt(index);
        }
    }

    public void SetSelectDirection(Vector2 direction) {
        _currentSelectDirection = Vector2.ClampMagnitude(direction, 1);
    }

    public void SetSlices(int count) {
        _slices = count;
        _pieImage.transform.rotation = Quaternion.Euler(0, 0, count > 1 ? 180 / count : 0);
        material.SetInt("_Slices", count);
    }

#if UNITY_EDITOR
    [ContextMenu("Set Slices")]
    void SetSlices() {
        if(Application.isPlaying)
            SetSlices(_setSlices);
    }

    [ContextMenu("Initialize Material")]
    void InitMaterial() {
        if(material == null) {
            var imageComponent = _pieImage.GetComponent<Image>();

            if(imageComponent != null) {
                material = imageComponent.material;
            }
            else {
                Debug.LogError("Pie Wheel could not find an Image component");
                enabled = false;
            }
        }

        if(rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }
#endif

    void Update() {
        if(_wheelActive) {
            //rewiredPlayer.GetAxis("");
            //var mouseSpeed = ReInput.controllers.Mouse.GetAxis2DRaw(0);
            //var mouseSpeed = rewiredPlayer.GetAxis2D(RewiredActions.Default_Aim_Horizontal, RewiredActions.Default_Aim_Vertical);
            var direction = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            direction += rewiredPlayer.GetAxis2D(RewiredActions.Default_Aim_Horizontal, RewiredActions.Default_Aim_Vertical) * Time.unscaledDeltaTime * _acceleration;

            _currentSelectDirection -= direction * Time.unscaledDeltaTime * _acceleration;

            if(_currentSelectDirection.sqrMagnitude > _selectionDeadzone && _slices > 0) {
                _currentSelectDirection = Vector2.ClampMagnitude(_currentSelectDirection, 1);

                Vector2 scale = transform.lossyScale;

                scale = Vector2.Scale(scale, new Vector2(rectTransform.rect.width, rectTransform.rect.height) * .5f);
                scale = Vector2.Scale(scale, _currentSelectDirection);

#if UNITY_EDITOR
                Debug.DrawRay(rectTransform.position, scale, Color.yellow, 0, false);
#endif

                float angleSize = 360f / _slices;
                float halfAngleSize = angleSize * .5f;
                float cursorAngle = Mathf.Atan2(-_currentSelectDirection.x, _currentSelectDirection.y) * Mathf.Rad2Deg;
                float highlightAngle = cursorAngle + 180 + halfAngleSize;
                int currentSlice = (int)(((highlightAngle / 360) % 1) * _slices) + 1;

                _selectionArrow.transform.localRotation = Quaternion.Euler(0, 0, (cursorAngle + 180));
                //_selectionArrow.transform.localRotation = Quaternion.Euler(0, 0, currentAngle + halfAngleSize - 90);

                if(!_selectionArrow.gameObject.activeSelf)
                    _selectionArrow.gameObject.SetActive(true);

                if(currentSelectedSlice != currentSlice) {
                    material.SetInt("_HighlightSlice", currentSlice);
                    currentSelectedSlice = currentSlice;
                }
            }
            else {
                if(_selectionArrow.gameObject.activeSelf)
                    _selectionArrow.gameObject.SetActive(false);

                if(currentSelectedSlice > 0) {
                    material.SetInt("_HighlightSlice", 0);
                    currentSelectedSlice = 0;
                }
            }
        }
    }

    void UpdateIconPosition(int index, GameObject icon) {
        float angleSize = 360f / _slices;
        float angle = index * angleSize;

        var iconTransform = icon.transform;
        Vector3 direction = Vector3.up * _iconHeightPercentage;
        Vector2 scale = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        direction = Vector3.Scale(direction, scale);
        direction = Quaternion.Euler(0, 0, -angle) * direction;
        direction.z = -.5f;

        iconTransform.position = transform.TransformPoint(direction);
    }

    [ContextMenu("Update Icon Positions")]
    public void UpdateIconPositions() {
        if(_slices <= 0) return;

        for(int i = 0; i < _iconList.Count; i++)
            UpdateIconPosition(i, _iconList[i]);
    }

    public void UpdateGlyphs() {
        for(int i = 0; i < _actionGlyphs.Length; i++)
            _actionGlyphs[i].gameObject.SetActive(false);


        for(int i = 0; i < _actionGlyphs.Length; i++) {
            if(UIManager.Current.abilityWheel.HasAssignedSkill(i)) {
                var skillId = UIManager.Current.abilityWheel.GetSkillId(i);

                _actionGlyphs[i].gameObject.SetActive(true);
            }
        }
    }
}