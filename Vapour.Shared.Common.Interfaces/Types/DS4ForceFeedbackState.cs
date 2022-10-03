using System;

namespace Vapour.Shared.Common.Types
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
}