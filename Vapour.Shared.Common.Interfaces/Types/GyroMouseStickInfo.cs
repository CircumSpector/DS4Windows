using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class GyroMouseStickInfo
    {
        public delegate void GyroMouseStickInfoEventHandler(GyroMouseStickInfo sender,
            EventArgs args);

        public enum OutputStick : byte
        {
            None,
            LeftStick,
            RightStick
        }

        public enum OutputStickAxes : byte
        {
            None,
            XY,
            X,
            Y
        }

        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage
        }

        public const double DefaultMinCutoff = 0.4;
        public const double DefaultBeta = 0.7;
        public const string DefaultSmoothTechnique = "one-euro";
        public const OutputStick DefaultOutputStick = OutputStick.RightStick;
        public const OutputStickAxes DefaultOutputStickAxes = OutputStickAxes.XY;

        public int DeadZone { get; set; }

        public int MaxZone { get; set; }

        public double AntiDeadX { get; set; }

        public double AntiDeadY { get; set; }

        public int VerticalScale { get; set; }

        public bool MaxOutputEnabled { get; set; }

        public double MaxOutput { get; set; } = 100.0;

        // Flags representing invert axis choices
        public uint Inverted { get; set; }

        public bool UseSmoothing { get; set; }

        public double SmoothWeight { get; set; }

        public SmoothingMethod Smoothing { get; set; }

        public double MinCutoff { get; set; } = DefaultMinCutoff;

        public double Beta { get; set; } = DefaultBeta;

        public OutputStick OutStick { get; set; } = DefaultOutputStick;

        public OutputStickAxes OutputStickDir { get; set; } = DefaultOutputStickAxes;

        [UsedImplicitly]
        private void OnMinCutoffChanged()
        {
            MinCutoffChanged?.Invoke(this, EventArgs.Empty);
        }

        public event GyroMouseStickInfoEventHandler MinCutoffChanged;

        private void OnBetaChanged()
        {
            BetaChanged?.Invoke(this, EventArgs.Empty);
        }

        public event GyroMouseStickInfoEventHandler BetaChanged;

        public void Reset()
        {
            DeadZone = 30;
            MaxZone = 830;
            AntiDeadX = 0.4;
            AntiDeadY = 0.4;
            Inverted = 0;
            VerticalScale = 100;
            MaxOutputEnabled = false;
            MaxOutput = 100.0;
            OutStick = DefaultOutputStick;
            OutputStickDir = DefaultOutputStickAxes;

            MinCutoff = DefaultMinCutoff;
            Beta = DefaultBeta;
            Smoothing = SmoothingMethod.None;
            UseSmoothing = false;
            SmoothWeight = 0.5;
        }

        public void ResetSmoothing()
        {
            UseSmoothing = false;
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
            switch (Smoothing)
            {
                case SmoothingMethod.WeightedAverage:
                    result = "weighted-average";
                    break;
                case SmoothingMethod.OneEuro:
                    result = "one-euro";
                    break;
            }

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

        public bool OutputHorizontal()
        {
            return OutputStickDir == OutputStickAxes.XY ||
                   OutputStickDir == OutputStickAxes.X;
        }

        public bool OutputVertical()
        {
            return OutputStickDir == OutputStickAxes.XY ||
                   OutputStickDir == OutputStickAxes.Y;
        }
    }
}