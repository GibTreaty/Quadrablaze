using UnityEngine;

namespace Quadrablaze {
    public class GameLobbyManager : MonoBehaviour {

        public static GameLobbyManager Current { get; private set; }

        [SerializeField]
        GameLobbyUI _lobbyUI;

        public GameLobbyUI LobbyUI {
            get { return _lobbyUI; }
            set { _lobbyUI = value; }
        }

        public void Initialize() {
            Current = this;

            LobbyUI.Initialize();
        }
    }
}