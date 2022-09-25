using UnityEngine;

namespace Quadrablaze {
    public class OverviewCamera : MonoBehaviour {
        
        [SerializeField]
        Transform _pivot;

        [SerializeField]
        ShooterCamera _shootCamera;

        [SerializeField]
        Transform _camera;

        [SerializeField]
        Vector3 _rigOverviewPosition;

        [SerializeField]
        float _cameraDistance;

        [SerializeField]
        float _defaultCameraDistance;

        [SerializeField]
        float _overviewAngle;

        [SerializeField]
        float _defaultAngle;

        public BooleanEvent OnChangedStatus;

        bool _status;

        #region Properties
        public bool Status {
            get { return _status; }
            set {
                if(Status == value) return;

                _status = value;
                SetOverviewStatus(Status);
                OnChangedStatus.InvokeEvent(Status);
            }
        }
        #endregion

        void OnEnable() {
            if(OnChangedStatus == null) OnChangedStatus = new BooleanEvent();
        }

        void LateUpdate() {
            if(!_shootCamera.enabled) {
                transform.position = _rigOverviewPosition;
                _pivot.transform.localEulerAngles = new Vector3(_overviewAngle, 0, 0);
                _camera.transform.localPosition = new Vector3(0, _cameraDistance, 0);
            }
        }

        void SetOverviewStatus(bool enable) {
            if(enable) {
                _shootCamera.enabled = false;
                transform.position = _rigOverviewPosition;

                _pivot.transform.localEulerAngles = new Vector3(_overviewAngle, 0, 0);
                _camera.transform.localPosition = new Vector3(0, _cameraDistance, 0);
            }
            else {
                if(!_shootCamera.Target) transform.position = Vector3.zero;

                _shootCamera.enabled = true;
                _pivot.transform.localEulerAngles = new Vector3(_defaultAngle, 0, 0);
                _camera.transform.localPosition = new Vector3(0, _defaultCameraDistance, 0);

                _shootCamera.InstantMove();
            }
        }

        [ContextMenu("Toggle")]
        public void ToggleStatus() {
            Status = !Status;
        }
    }
}