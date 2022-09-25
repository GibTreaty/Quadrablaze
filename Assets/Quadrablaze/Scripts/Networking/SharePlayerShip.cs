using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class SharePlayerShip : NetworkBehaviour {

        [SerializeField]
        GamePlayerInfo _playerInfo;

        public void DelayedSend() {
            StartCoroutine(DelayedSendRoutine());
        }

        IEnumerator DelayedSendRoutine() {
            yield return null;

            SendData();
        }

        void FinishedSendingShip(NetworkDownloadHandler.Data data, IEnumerable<NetworkConnection> connections) {
            Debug.Log("Player finished sending ship");

            if(NetworkServer.active)
                foreach(var connection in connections)
                    if(connection != null)
                        _playerInfo.SetPlayerHasShip((ulong)QuadrablazeSteamNetworking.Current.GetSteamIDForConnection(connection));
        }

        public void SendData(IEnumerable<NetworkConnection> sendToConnections = null) {
            var authority = hasAuthority;
            var shipId = authority ? PlayerSpawnManager.Current.nextPlayerPrefabId : _playerInfo.ShipID;

            Debug.Log(string.Format("SendData netId:{0} shipId:{1}", netId, shipId));

            var shipInfo = ShipImporter.Current.GetShipData(_playerInfo);
            var shipImportSettings = authority ? ShipImporter.Current.localSettings[shipId] : _playerInfo.ShipSettings;

            if(authority) {
                var colorPreset = ShipSelectionUIManager.Current.CurrentPreset;

                shipImportSettings.primaryColor = colorPreset.PrimaryColor;
                shipImportSettings.secondaryColor = colorPreset.SecondaryColor;
                shipImportSettings.accessoryPrimaryColor = colorPreset.AccessoryPrimaryColor;
                shipImportSettings.accessorySecondaryColor = colorPreset.AccessorySecondaryColor;
                shipImportSettings.glowColor = colorPreset.GlowColor;
            }

            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream)) {
                writer.Write(DownloadMessageType.ShipDownloaded);
                writer.Write(netId.Value);

                var settingsBytes = shipImportSettings.ToBytes();

                writer.Write(settingsBytes.Length);
                writer.Write(settingsBytes);

                if(shipInfo.isBuiltinShip) {
                    writer.Write(true);
                    writer.Write(authority ? shipInfo.assetHashName : _playerInfo.ShipAssetHashName);
                }
                else {
                    writer.Write(false);

                    var shipBytes = authority ? ShipImporter.Current.PrepareShipForMuliplayerUpload(shipId) : _playerInfo.DownloadedShip.SaveToBytes();

                    writer.Write(shipBytes.Length);
                    writer.Write(shipBytes);
                }

                var bytes = stream.ToArray();

                NetworkDownloadHandler.SendData(bytes, data => FinishedSendingShip(data, sendToConnections), sendToConnections);
            }
        }
    }
}