using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze.GameModes {
    public abstract class GameMode {
        public bool IsHost => NetworkServer.active;

        public event Action OnWin;

        public GameMode(Action onWinListener) {
            if(onWinListener != null)
                OnWin += onWinListener;
        }

        public abstract void EndGame();

        public abstract void UpdateMode();
    }
}