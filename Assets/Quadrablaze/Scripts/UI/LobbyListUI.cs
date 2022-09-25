using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quadrablaze {
    public class LobbyListUI : MonoBehaviour {

        [SerializeField]
        LobbyDataUI _lobbyDataUIPrefab;

        [SerializeField]
        RectTransform _container;

        [SerializeField]
        ScrollRect _scrollRectComponent;

        [SerializeField]
        Button _searchButton;

        [SerializeField]
        Button _exitButton;

        [SerializeField]
        GameObject _beforeRefresh;

        [SerializeField]
        GameObject _afterRefresh;

        [SerializeField]
        GameObject _noMatchWarning;

        List<LobbyDataUI> data = new List<LobbyDataUI>();

        public bool ShowingList {
            get { return _afterRefresh.activeSelf; }
        }

        void Awake() {
            _searchButton.onClick.AddListener(() => { Open(); RequestMatchList(); });

            _exitButton.onClick.AddListener(OnExit);
        }

        void OnExit() {
            if(ShowingList)
                Close();
            else
                UIManager.Current.GoToParentMenu();
        }

        void OnDisable() {
            Close();
            _noMatchWarning.SetActive(false);
        }

        public void ClearData() {
            foreach(var value in data)
                Destroy(value.gameObject);

            data.Clear();
        }

        public void Close() {
            ClearData();
            _beforeRefresh.SetActive(true);
            _afterRefresh.SetActive(false);

            var navigation = _exitButton.navigation;

            navigation.mode = Navigation.Mode.Automatic;
            _exitButton.navigation = navigation;

            _exitButton.Select();
        }

        public void GenerateList(LobbyMatchList_t matchListCallback) {
            ClearData();

            if(matchListCallback.m_nLobbiesMatching <= 0) {
                Close();
                _noMatchWarning.SetActive(true);
            }
            else {
                _noMatchWarning.SetActive(false);

                for(int i = 0; i < matchListCallback.m_nLobbiesMatching; i++) {
                    var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);

                    if(lobbyId.IsValid()) {
                        var gameVersion = SteamMatchmaking.GetLobbyData(lobbyId, QuadrablazeSteamNetworking.LobbyData_GameMode);

                        if(gameVersion != GameManager.Current.GameVersion) continue;

                        var lobbyDataUI = GenerateData(lobbyId);

                        if(i == 0) lobbyDataUI.Select();
                    }
                }
            }
        }

        LobbyDataUI GenerateData(CSteamID lobbyId) {
            var gameObject = Instantiate(_lobbyDataUIPrefab.gameObject, _container);
            var lobbyDataUI = gameObject.GetComponent<LobbyDataUI>();

            var playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
            var title = SteamMatchmaking.GetLobbyData(lobbyId, QuadrablazeSteamNetworking.LobbyData_Title);

            lobbyDataUI.LobbyId = lobbyId;
            lobbyDataUI.SetText(string.Format("({0}/4) {1}", playerCount, title));

            data.Add(lobbyDataUI);

            return lobbyDataUI;
        }

        public void Open() {
            _beforeRefresh.SetActive(false);
            _afterRefresh.SetActive(true);
            RequestMatchList();

            var navigation = _exitButton.navigation;

            navigation.mode = Navigation.Mode.None;
            _exitButton.navigation = navigation;
        }

        public void RequestMatchList() {
            QuadrablazeSteamNetworking.Current.RequestMatchList();
        }
    }
}