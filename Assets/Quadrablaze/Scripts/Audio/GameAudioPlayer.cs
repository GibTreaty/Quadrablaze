using UnityEngine;

namespace Quadrablaze {
    public class GameAudioPlayer : MonoBehaviour {

        [SerializeField]
        Camera _mainCamera;

        [SerializeField]
        AudioListener _listener;

        [SerializeField]
        OverviewCamera _overviewCamera;

        #region Properties
        public bool IsInOverheadMode {
            get { return OverviewCamera.Status; }
        }

        public AudioListener Listener {
            get { return _listener; }
            set { _listener = value; }
        }

        public Camera MainCamera {
            get { return _mainCamera; }
            set { _mainCamera = value; }
        }

        public OverviewCamera OverviewCamera {
            get { return _overviewCamera; }
            set { _overviewCamera = value; }
        }
        #endregion

        //void Awake() {
        //    Listener.transform.SetParent(null);
        //}

        //void LateUpdate() { //TODO: This may need to be constrained to something else, potentially other players while in spectating mode
        //    if(PlayerSpawnManager.IsPlayerAlive)
        //        Listener.transform.position = PlayerSpawnManager.Current.CurrentPlayerEntity.CurrentTransform.position;
        //}

        //public void FollowPosition(Vector3 position) {
        //    Listener.transform.position = position;
        //}
    }
}