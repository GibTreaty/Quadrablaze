using Quadrablaze.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class GameNetworkSelectionButton : MonoBehaviour {
        [SerializeField]
        GameNetworkType _buttonType;

        [SerializeField]
        Button _button;

        [SerializeField]
        Image _highlightImage;

        [SerializeField]
        bool _playImmediately = true;

        [SerializeField]
        ScriptableMenu _searchLobbiesMenu;

        #region Properties
        public Button Button {
            get { return _button; }
            set { _button = value; }
        }

        public GameNetworkType ButtonType {
            get { return _buttonType; }
            set { _buttonType = value; }
        }

        public Image HighlightImage {
            get { return _highlightImage; }
            set { _highlightImage = value; }
        }
        #endregion

        public void Highlight(bool enable, float time) {
            if(HighlightImage) HighlightImage.CrossFadeAlpha(enable ? 1 : 0, time, true);
        }

        public void SetGameNetworkConnectionType() {
            switch(_buttonType) {
                case GameNetworkType.SinglePlayer:
                case GameNetworkType.Host:
                case GameNetworkType.QuickMatch:
                    GameManager.Current.CurrentGameModeID = 0; // TODO: Set CurrentGameModeID differently here
                    break;
            }

            switch(_buttonType) {
                case GameNetworkType.SinglePlayer:
                case GameNetworkType.Host:
                case GameNetworkType.Join:
                case GameNetworkType.Invited: {
                    NewGameOptions options = GameManager.Current.Options;
                    options.GameNetworkConnectionType = ButtonType;
                    GameManager.Current.Options = options;

                    if(_playImmediately)
                        GameManager.Current.NewGame();
                }

                break;

                case GameNetworkType.Search:
                    UIManager.Current.GoToMenu(_searchLobbiesMenu);
                    UIManager.Current.lobbyListUI.Open();

                    break;

                case GameNetworkType.QuickMatch: {
                    UIManager.Current.GoToMenu("Lobby");
                    UIManager.Current.IsJoiningGame = true;
                    GameLobbyManager.Current.LobbyUI.ConnectingMessage.StartAnimation();

                    NewGameOptions options = GameManager.Current.Options;
                    options.GameNetworkConnectionType = GameNetworkType.QuickMatch;
                    GameManager.Current.Options = options;

                    QuadrablazeSteamNetworking.Current.QuickMatch();
                }

                break;
            }
        }
    }
}