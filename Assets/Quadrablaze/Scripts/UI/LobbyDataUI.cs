using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class LobbyDataUI : MonoBehaviour {

        [SerializeField]
        TMP_Text _text;

        [SerializeField]
        Button _joinButton;

        [SerializeField]
        CSteamID _lobbyId;

        #region Properties
        public CSteamID LobbyId {
            get { return _lobbyId; }
            set { _lobbyId = value; }
        }
        #endregion

        void OnEnable() {
            _joinButton.onClick.AddListener(Join);
        }

        public void Join() {
            //UIManager.Current.lobbyListUI.gameObject.SetActive(false);

            //UIManager.Current.CloseMenus();
            //UIManager.Current.EnableMainMenu(false);
            //UIManager.Current.difficultyMenu.SetActive(false);
            UIManager.Current.IsJoiningGame = true;
            GameLobbyManager.Current.LobbyUI.ConnectingMessage.StartAnimation();

            NewGameOptions options = GameManager.Current.Options;
            options.GameNetworkConnectionType = GameNetworkType.Search;
            GameManager.Current.Options = options;

            GameManager.Current.NewGame();

            SteamMatchmaking.JoinLobby(LobbyId);
        }

        public void SetText(string text) {
            _text.text = text;
        }

        public void Select() {
            _joinButton.Select();
        }
    }
}