using System;

namespace DS4Windows
{
    

    /**
     * The haptics engine uses a stack of these states representing the light bar and rumble motor settings.
     * It (will) handle composing them and the details of output report management.
     */
    public class DS4ForceFeedbackState : IEquatable<DS4ForceFeedbackState>, ICloneable
    {
        public byte RumbleMotorStrengthLeftHeavySlow { get; set; }

        public byte RumbleMotorStrengthRightLightFast { get; set; }

        public bool RumbleMotorsExplicitlyOff { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(DS4ForceFeedbackState other)
        {
            return RumbleMotorStrengthLeftHeavySlow == other.RumbleMotorStrengthLeftHeavySlow &&
                   RumbleMotorStrengthRightLightFast == other.RumbleMotorStrengthRightLightFast &&
                   RumbleMotorsExplicitlyOff == other.RumbleMotorsExplicitlyOff;
        }

        public bool IsRumbleSet()
        {
            const byte zero = 0;
            return RumbleMotorsExplicitlyOff || RumbleMotorStrengthLeftHeavySlow != zero ||
                   RumbleMotorStrengthRightLightFast != zero;
        }
    }

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