using UnityEngine;

namespace Quadrablaze.MenuSystem {
    [CreateAssetMenu(menuName = "Quadrablaze/Menu/Menu Logic/Lobby")]
    public class LobbyMenu : ScriptableMenu {

        public override void Close(MenuItem item, UIManager manager) {
            base.Close(item, manager);

            if(GameLobbyUI.Current != null && !GameLobbyUI.Current.StartPressed)
                if(QuadrablazeSteamNetworking.Current.SteamLobbyId.IsValid())
                    QuadrablazeSteamNetworking.Current.Stop();
        }

        public override void Open(MenuItem item, UIManager manager) {
            base.Open(item, manager);
        }
    }
}