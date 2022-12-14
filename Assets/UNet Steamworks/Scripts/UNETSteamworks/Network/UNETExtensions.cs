using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class UNETExtensions {

    private static int nextConnectionId = -1;

    /// Because we fake the UNET connection, connection initialization is not handled by UNET internally. 
    /// Connections must be manually initialized with this function.
    public static void ForceInitialize(this NetworkConnection conn) {
        ForceInitialize(conn, NetworkServer.hostTopology);
        //conn.Initialize("localhost", id, id, SteamNetworkManager.hostTopology);
    }
    public static void ForceInitialize(this NetworkConnection conn, HostTopology hostTopology) {
        int id = ++nextConnectionId;

        conn.Initialize("localhost", id, id, hostTopology);
    }

    public static void ResetIdCounter() {
        nextConnectionId = 0;
    }
}
