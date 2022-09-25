using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using Steamworks;
using System.IO;

namespace Quadrablaze {
    public class LobbyPlayerInfo : NetworkBehaviour {

        public static Dictionary<uint, LobbyPlayerInfo> players = new Dictionary<uint, LobbyPlayerInfo>();

        [SerializeField]
        TMP_Text playerNameText;

        [SerializeField]
        TMP_Text pingText;

        [SerializeField]
        Button _kickButton;

        [SerializeField]
        GameObject _shipSyncObject;

        [SerializeField]
        TMP_Text _shipSyncText;

        [SerializeField]
        GameObject _readyOff;

        [SerializeField]
        GameObject _readyOn;

        [SerializeField]
        Image _avatarImage;

        [SerializeField, SyncVar(hook = "Sync_Ready")]
        bool _ready;

        [SyncVar(hook = "Sync_SteamId")]
        ulong _steamId;

        //[SyncVar(hook = "Sync_Username")]
        //ulong _username;

        public NetworkConnection ThisConnection { get; set; }

        public bool Ready {
            get { return _ready; }
            private set { _ready = value; }
        }

        public override void PreStartClient() {
            players[netId.Value] = this;
            _kickButton.onClick.AddListener(OnKick);
        }

        public override void OnStartClient() {
            SetReady(Ready);

            if(!NetworkServer.active) {
                _kickButton.gameObject.SetActive(false);
                transform.SetParent(GameLobbyManager.Current.LobbyUI.PlayerListParent, false);

                Sync_SteamId(_steamId);
            }
        }

        //public override void OnStartServer() {
        //    _playerName = "Player ID:" + ThisConnection.connectionId;
        //}

        public override void OnStartAuthority() {
            if(NetworkServer.active)
                _kickButton.gameObject.SetActive(false);
        }

        public override void OnNetworkDestroy() {
            players.Remove(netId.Value);
        }

        void OnKick() {
            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream)) {
                writer.Write(ChatMessageType.Kick);
                writer.Write(_steamId);

                var data = stream.ToArray();

                SteamMatchmaking.SendLobbyChatMsg(QuadrablazeSteamNetworking.Current.SteamLobbyId, data, data.Length);
            }

            //var data = new List<byte>();

            //data.AddRange(BitConverter.GetBytes(ChatMessageType.QuadrablazeMessage));
            //data.AddRange(BitConverter.GetBytes(_steamId));

            //SteamMatchmaking.SendLobbyChatMsg(QuadrablazeSteamNetworking.Current.SteamLobbyId, data.ToArray(), data.Count);
            //ThisConnection.Disconnect();
        }

        void SetPlayerName(string name) {
            playerNameText.text = name;
        }

        public void SetPing(int ping) {
            pingText.text = ping.ToString();
        }

        public void SetShipSync(int percentage) {
            _shipSyncText.text = percentage.ToString() + "%";
            _shipSyncObject.SetActive(true);
        }

        public void SetReady(bool ready) {
            //Debug.Log("SetReady " + ready);
            Ready = ready;

            UpdateReady();
        }

        public void SetSteamId(ulong steamId) {
            _steamId = steamId;
        }

        void Sync_Ready(bool ready) {
            _ready = ready;

            UpdateReady();
        }

        void Sync_SteamId(ulong steamId) {
            _steamId = steamId;

            var id = new CSteamID(_steamId);

            if(id.IsValid()) {
                SetPlayerName(SteamFriends.GetFriendPersonaName(id));
                UpdateAvatar();
            }
        }

        public void UpdateAvatar() {
            var user = SteamAvatarManager.Current.GetAvatars(new CSteamID(_steamId), SteamAvatarManager.SteamAvatarSize.Small);

            _avatarImage.sprite = user.SmallAvatar?.avatarSprite;
        }

        void UpdateReady() {
            _readyOff.SetActive(!Ready);
            _readyOn.SetActive(Ready);
        }
    }
}