using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class GyroMouseInfo
    {
        public delegate void GyroMouseInfoEventHandler(GyroMouseInfo sender, EventArgs args);

        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage
        }

        public const double DefaultMinCutoff = 1.0;
        public const double DefaultBeta = 0.7;
        public const string DefaultSmoothTechnique = "one-euro";
        public const double DefaultMinThreshold = 1.0;

        public bool EnableSmoothing { get; set; }

        public double SmoothingWeight { get; set; } = 0.5;

        public SmoothingMethod Smoothing { get; set; }

        public double MinCutoff { get; set; } = DefaultMinCutoff;

        public double Beta { get; set; } = DefaultBeta;

        public double MinThreshold { get; set; } = DefaultMinThreshold;

        [UsedImplicitly]
        private void OnMinCutoffChanged()
        {
            MinCutoffChanged?.Invoke(this, EventArgs.Empty);
        }

        public event GyroMouseInfoEventHandler MinCutoffChanged;

        [UsedImplicitly]
        private void OnBetaChanged()
        {
            BetaChanged?.Invoke(this, EventArgs.Empty);
        }

        public event GyroMouseInfoEventHandler BetaChanged;

        public void Reset()
        {
            MinCutoff = DefaultMinCutoff;
            Beta = DefaultBeta;
            EnableSmoothing = false;
            Smoothing = SmoothingMethod.None;
            SmoothingWeight = 0.5;
            MinThreshold = DefaultMinThreshold;
        }

        public void ResetSmoothing()
        {
            EnableSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            Smoothing = SmoothingMethod.None;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    Smoothing = SmoothingMethod.WeightedAverage;
                    break;
                case "one-euro":
                    Smoothing = SmoothingMethod.OneEuro;
                    break;
                default:
                    Smoothing = SmoothingMethod.None;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            var result = "none";
            if (Smoothing == SmoothingMethod.OneEuro)
                result = "one-euro";
            else if (Smoothing == SmoothingMethod.WeightedAverage) result = "weighted-average";

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) => { euroFilter.Beta = Beta; };

            MinCutoffChanged += (sender, args) => { euroFilter.MinCutoff = MinCutoff; };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }
}