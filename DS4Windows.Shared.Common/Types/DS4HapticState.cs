using System;

namespace DS4Windows.Shared.Common.Types
{
    public class DS4HapticState : IEquatable<DS4HapticState>, ICloneable
    {
        public DS4LightbarState LightbarState { get; set; } = new();
        public DS4ForceFeedbackState RumbleState { get; set; } = new();

        public object Clone()
        {
            var state = (DS4HapticState)MemberwiseClone();
            state.LightbarState = (DS4LightbarState)LightbarState.Clone();
            state.RumbleState = (DS4ForceFeedbackState)RumbleState.Clone();

            return state;
        }

        public bool Equals(DS4HapticState other)
        {
            return LightbarState.Equals(other.LightbarState) &&
                   RumbleState.Equals(other.RumbleState);
        }

        public bool IsLightBarSet()
        {
            return LightbarState.IsLightBarSet();
        }

        public bool IsRumbleSet()
        {
            return RumbleState.IsRumbleSet();
        }
    }
}