using UnityEngine.Events;

public static class RoundManager {

    static bool _roundInProgress;

    #region Properties
    public static UnityEvent OnRoundEnded { get; private set; }

    public static UnityEvent OnRoundStarted { get; private set; }

    public static bool RoundInProgress {
        get { return _roundInProgress; }
        private set {
            if(_roundInProgress == value) return;

            _roundInProgress = value;

            if(RoundInProgress) OnRoundStarted.InvokeEvent();
            else OnRoundEnded.InvokeEvent();
        }
    }
    #endregion

    public static void EndRound() {
        RoundInProgress = false;
    }

    public static void Initialize() {
        OnRoundEnded = new UnityEvent();
        OnRoundStarted = new UnityEvent();
    }

    public static void StartRound() {
        RoundInProgress = true;
    }
}