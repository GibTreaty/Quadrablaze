using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech;

#pragma warning disable CS0618

namespace Quadrablaze {
    public class GameParty : ScriptableObject {

        [SerializeField]
        HashSet<Player> _players = new HashSet<Player>();

        int _uniqueId = 0;

        #region Properties
        public HashSet<Player> Players {
            get { return _players; }
            set { _players = value; }
        }
        #endregion

        void OnDestroy() {
            if(Application.isPlaying)
                foreach(var player in Players)
                    if(player.playerInfo)
                        Destroy(player.playerInfo.gameObject);
        }

        public Player AddPlayer(NetworkConnection clientToServerConnection, NetworkConnection serverToClientConnection, bool isHost, ulong steamId, string username) {
            Debug.LogFormat("AddPlayer '{0}'", SteamFriends.GetFriendPersonaName(new CSteamID(steamId)));
            return AddPlayer(clientToServerConnection, serverToClientConnection, isHost, GetUniqueID(), steamId, username);
        }
        public Player AddPlayer(NetworkConnection clientToServerConnection, NetworkConnection serverToClientConnection, bool isHost, int playerId, ulong steamId, string username) {
            var existingPlayer = GetPlayer(steamId);

            if(existingPlayer != null) return existingPlayer;

            var player = new Player() {
                connected = true,
                serverToClientConnection = serverToClientConnection,
                clientToServerConnection = clientToServerConnection,
                isHost = isHost,
                playerId = playerId,
                steamId = steamId,
                username = username
            };

            Players.Add(player);

            return player;
        }

        public Player GetPlayer(Func<Player, bool> requirements) {
            return Players.FirstOrDefault(requirements);
        }
        public Player GetPlayer(int playerId) {
            return GetPlayer(s => s.playerId == playerId);
        }
        public Player GetPlayer(NetworkConnection connection) {
            return GetPlayer(s => s.serverToClientConnection.connectionId == connection.connectionId);
        }
        public Player GetPlayer(ulong steamID) {
            return GetPlayer(s => s.steamId == steamID);
        }

        public void RemovePlayer(NetworkConnection connection) {
            var player = GetPlayer(connection);

            if(player != null) Players.Remove(player);
        }
        public void RemovePlayer(ulong steamId) {
            var player = GetPlayer(steamId);

            if(player != null) Players.Remove(player);
        }

        public int GetUniqueID() {
            return _uniqueId++;
        }

        public class Player {
            public int playerId;
            public bool connected;
            public bool isHost;
            public NetworkConnection serverToClientConnection;
            public NetworkConnection clientToServerConnection;
            public ulong steamId;
            public string username;
            public GamePlayerInfo playerInfo;

            public CSteamID GetCSteamID() {
                return (CSteamID)steamId;
            }
        }
    }
}