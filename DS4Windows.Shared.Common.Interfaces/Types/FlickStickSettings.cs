using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class FlickStickSettings
    {
        public delegate void FlickStickSettingsEventHandler(FlickStickSettings sender,
            EventArgs args);

        public const double DefaultFlickThreshold = 0.9;
        public const double DefaultFlickTime = 0.1; // In seconds
        public const double DefaultRealWorldCalibration = 5.3;
        public const double DefaultMinAngleThreshold = 0.0;
        public const double DefaultMinCutoff = 0.4;
        public const double DefaultBeta = 0.4;

        public double FlickThreshold { get; set; } = DefaultFlickThreshold;

        public double FlickTime { get; set; } = DefaultFlickTime; // In seconds

        public double RealWorldCalibration { get; set; } = DefaultRealWorldCalibration;

        public double MinAngleThreshold { get; set; } = DefaultMinAngleThreshold;

        public double MinCutoff { get; set; } = DefaultMinCutoff;

        public double Beta { get; set; } = DefaultBeta;

        [UsedImplicitly]
        private void OnMinCutoffChanged()
        {
            MinCutoffChanged?.Invoke(this, EventArgs.Empty);
        }

        public event FlickStickSettingsEventHandler MinCutoffChanged;

        [UsedImplicitly]
        private void OnBetaChanged()
        {
            BetaChanged?.Invoke(this, EventArgs.Empty);
        }

        public event FlickStickSettingsEventHandler BetaChanged;

        public void Reset()
        {
            FlickThreshold = DefaultFlickThreshold;
            FlickTime = DefaultFlickTime;
            RealWorldCalibration = DefaultRealWorldCalibration;
            MinAngleThreshold = DefaultMinAngleThreshold;

            MinCutoff = DefaultMinCutoff;
            Beta = DefaultBeta;
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