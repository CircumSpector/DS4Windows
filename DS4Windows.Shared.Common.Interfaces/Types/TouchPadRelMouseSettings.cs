using System;
using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class TouchPadRelMouseSettings
    {
        public const double DefaultAngDegree = 0.0;
        public const double DefaultAngRad = DefaultAngDegree * Math.PI / 180.0;
        public const double DefaultMinThreshold = 1.0;

        public double Rotation { get; set; } = DefaultAngRad;

        public double MinThreshold { get; set; } = DefaultMinThreshold;

        public void Reset()
        {
            Rotation = DefaultAngRad;
            MinThreshold = DefaultMinThreshold;
        }
    }
}