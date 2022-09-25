using System;
using System.Collections;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Quadrablaze {
    public class GameLobbyUI : MonoBehaviour {

        public static GameLobbyUI Current { get; private set; }

        [SerializeField]
        LobbyPlayerInfo _lobbyPlayerInfoPrefab;

        [SerializeField]
        Transform _playerListParent;

        [SerializeField]
        Button _exitButton;

        [SerializeField]
        Button _inviteButton;

        [SerializeField]
        Button _startButton;

        [SerializeField]
        GameObject _lobbyPrivacyOptionsGameObject;

        [SerializeField]
        Button _lobbyPublicButton;

        [SerializeField]
        Button _lobbyFriendsButton;

        [SerializeField]
        Button _lobbyPrivateButton;

        [SerializeField]
        LoadingText _connectingMessage;

        [SerializeField]
        TMPro.TMP_Text _waitingForHostText;

        UIHighlight _publicHighlight;
        UIHighlight _friendsHighlight;
        UIHighlight _privateHighlight;

        bool _startPressed = false;

        #region Properties
        public LoadingText ConnectingMessage {
            get { return _connectingMessage; }
            set { _connectingMessage = value; }
        }

        public Button ExitButton {
            get { return _exitButton; }
        }

        public Transform PlayerListParent {
            get { return _playerListParent; }
        }

        public Button StartButton {
            get { return _startButton; }
        }

        public bool StartPressed => _startPressed;

        #endregion

        public static void Close() {
            var items = LobbyPlayerInfo.players.Values.ToArray();

            foreach(var item in items)
                if(item != null)
                    NetworkServer.Destroy(item.gameObject);

            LobbyPlayerInfo.players.Clear();
        }

        public void Initialize() {
            Current = this;
            _exitButton.onClick.AddListener(GameManager.Current.QuitMultiplayerLobby);
            _inviteButton.onClick.AddListener(Invite);
            _startButton.onClick.AddListener(()=> {
                _startPressed = true;
                GameManager.Current.StartHostGame();
            });

            _lobbyPublicButton.onClick.AddListener(() => OnButtonSelected(ELobbyType.k_ELobbyTypePublic));
            _lobbyFriendsButton.onClick.AddListener(() => OnButtonSelected(ELobbyType.k_ELobbyTypeFriendsOnly));
            _lobbyPrivateButton.onClick.AddListener(() => OnButtonSelected(ELobbyType.k_ELobbyTypePrivate));

            _publicHighlight = _lobbyPublicButton.GetComponent<UIHighlight>();
            _friendsHighlight = _lobbyFriendsButton.GetComponent<UIHighlight>();
            _privateHighlight = _lobbyPrivateButton.GetComponent<UIHighlight>();
        }

        public void AddPlayer(GameParty.Player player, NetworkConnection connection) {
            var prefab = Instantiate(_lobbyPlayerInfoPrefab.gameObject, PlayerListParent);
            var lobbyPlayerInfo = prefab.GetComponent<LobbyPlayerInfo>();

            Debug.Log("Player Connection:" + player.serverToClientConnection.GetHashCode() + " NetworkConnection: " + connection.GetHashCode());

            lobbyPlayerInfo.ThisConnection = player.serverToClientConnection;
            NetworkServer.Spawn(prefab);
            var networkIdentity = prefab.GetComponent<NetworkIdentity>();
            networkIdentity.AssignClientAuthority(connection);

            player.playerInfo._lobbyPlayerInfoID = lobbyPlayerInfo.netId;
            player.playerInfo.LobbyPlayerInfoComponent = lobbyPlayerInfo;
            lobbyPlayerInfo.SetSteamId(player.steamId);

            //lobbyPlayerInfo.SetPlayerName(player.playerInfo.name);
            //lobbyPlayerInfo.SetPing(QuadrablazeNetworkManager.Current.client.GetRTT());
        }

        void EnableHostOptions(bool enable = true) {
            //_lobbyPublicButton.gameObject.SetActive(enable);
            //_lobbyFriendsButton.gameObject.SetActive(enable);
            //_lobbyPrivateButton.gameObject.SetActive(enable);
            _lobbyPrivacyOptionsGameObject.SetActive(enable);
        }

        void EnableHighlight(ELobbyType lobbyType) {
            _publicHighlight.EnableHighlight(lobbyType == ELobbyType.k_ELobbyTypePublic);
            _friendsHighlight.EnableHighlight(lobbyType == ELobbyType.k_ELobbyTypeFriendsOnly);
            _privateHighlight.EnableHighlight(lobbyType == ELobbyType.k_ELobbyTypePrivate);
        }

        public void Invite() {
            QuadrablazeSteamNetworking.Current.InviteFriend();
        }

        void OnButtonSelected(ELobbyType lobbyType) {
            SteamMatchmaking.SetLobbyType(QuadrablazeSteamNetworking.Current.SteamLobbyId, lobbyType);

            EnableHighlight(lobbyType);
        }

        public void OnOpen(NewGameOptions options) {
            bool isHost = false;
            Debug.Log("Options  "+ options.GameNetworkConnectionType.ToString());
            switch(options.GameNetworkConnectionType) {
                case GameNetworkType.Host:
                    _exitButton.gameObject.SetActive(true);
                    _startButton.gameObject.SetActive(true);
                    EnableHostOptions();
                    _startButton.Select();
                    isHost = true;

                    break;
                case GameNetworkType.Join:
                case GameNetworkType.Invited:
                case GameNetworkType.QuickMatch:
                case GameNetworkType.Search:
                    _exitButton.gameObject.SetActive(true);
                    _startButton.gameObject.SetActive(false);
                    EnableHostOptions(false);
                    _exitButton.Select();

                    break;
            }

            _publicHighlight.EnableHighlight(isHost);
            _friendsHighlight.EnableHighlight(isHost);
            _privateHighlight.EnableHighlight(isHost);

            if(isHost)
                EnableHighlight(QuadrablazeSteamNetworking.DefaultLobbyType);

            _waitingForHostText.gameObject.SetActive(!isHost);

            _startPressed = false;
        }
    }
}