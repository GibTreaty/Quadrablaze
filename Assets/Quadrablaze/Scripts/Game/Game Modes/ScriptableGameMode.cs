using UnityEngine;
using Quadrablaze.GameModes;
using System;

namespace Quadrablaze.GameModes {
    public abstract class ScriptableGameMode : ScriptableObject {
        public abstract GameMode InstantiateMode(Action onWinListener);
    }
}