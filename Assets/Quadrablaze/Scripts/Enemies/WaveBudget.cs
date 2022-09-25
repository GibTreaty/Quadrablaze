using UnityEngine;

namespace Quadrablaze {
    public class WaveBudget : IWaveBudgetSpendable {
        public int Points { get; set; }

        public int MinimumSpendLimit { get; set; }

        public int MaximumSpendLimit { get; set; }

        public int Tier { get; set; }

        public WaveBudget(int points, int minimumSpendLimit, int maximumSpendLimit, int tier) {
            this.Points = points;
            this.MinimumSpendLimit = minimumSpendLimit;
            this.MaximumSpendLimit = maximumSpendLimit;
            this.Tier = tier;
        }

        public bool HasEnough(int points) {
            return this.Points >= points;
        }

        public bool Spend(int points) {
            if(HasEnough(points)) {
                this.Points -= points;

                return true;
            }

            return false;
        }
    }

    public struct WaveBudgetData : IWaveBudget {
        public int MinimumSpendLimit { get; set; }

        public int MaximumSpendLimit { get; set; }

        public int Points { get; set; }

        public int Tier { get; set; }

        public WaveBudgetData(int points, int minimumSpendLimit, int maximumSpendLimit, int tier) {
            this.Points = points;
            this.MinimumSpendLimit = minimumSpendLimit;
            this.MaximumSpendLimit = maximumSpendLimit;
            this.Tier = tier;
        }
    }
}