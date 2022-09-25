namespace Quadrablaze {
    public interface IWaveBudget {
        int MaximumSpendLimit { get; }
        int MinimumSpendLimit { get; }
        int Points { get; }
        int Tier { get; }
    }

    public interface IWaveBudgetSpendable : IWaveBudget {
        bool Spend(int points);
    }
}