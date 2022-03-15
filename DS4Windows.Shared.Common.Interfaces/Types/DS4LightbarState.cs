using System;

namespace DS4Windows.Shared.Common.Types
{
    public class DS4LightbarState : IEquatable<DS4LightbarState>, ICloneable
    {
        public bool LightBarExplicitlyOff;

        public byte LightBarFlashDurationOn, LightBarFlashDurationOff;
        public DS4Color LightBarColor { get; set; } = new();

        public object Clone()
        {
            var state = (DS4LightbarState)MemberwiseClone();
            state.LightBarColor = (DS4Color)LightBarColor.Clone();

            return state;
        }

        public bool Equals(DS4LightbarState other)
        {
            return LightBarColor.Equals(other.LightBarColor) &&
                   LightBarExplicitlyOff == other.LightBarExplicitlyOff &&
                   LightBarFlashDurationOn == other.LightBarFlashDurationOn &&
                   LightBarFlashDurationOff == other.LightBarFlashDurationOff;
        }

        public bool IsLightBarSet()
        {
            return LightBarExplicitlyOff || LightBarColor.Red != 0 || LightBarColor.Green != 0 ||
                   LightBarColor.Blue != 0;
        }
    }
}
