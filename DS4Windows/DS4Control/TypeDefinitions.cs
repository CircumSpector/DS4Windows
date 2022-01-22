using System;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    [Flags]
    public enum DS4KeyType : byte
    {
        None = 0,
        ScanCode = 1,
        Toggle = 2,
        Unbound = 4,
        Macro = 8,
        HoldMacro = 16,
        RepeatMacro = 32
    } // Increment by exponents of 2*, starting at 2^0

    public enum Ds3PadId : byte
    {
        None = 0xFF,
        One = 0x00,
        Two = 0x01,
        Three = 0x02,
        Four = 0x03,
        All = 0x04
    }

    public static class DS4ControlsExtensions
    {
        /// <summary>
        ///     Provides a user-readable representation of <see cref="DS4ControlItem" />.
        /// </summary>
        /// <param name="control">The <see cref="DS4ControlItem" /> to return as <see cref="string" />.</param>
        /// <returns>A <see cref="string" />.</returns>
        public static string ToDisplayName(this DS4ControlItem control)
        {
            switch (control)
            {
                case DS4ControlItem.None:
                    break;
                case DS4ControlItem.LXNeg:
                    return "LS Left";
                case DS4ControlItem.LXPos:
                    return "LS Right";
                case DS4ControlItem.LYNeg:
                    return "LS Up";
                case DS4ControlItem.LYPos:
                    return "LS Down";
                case DS4ControlItem.RXNeg:
                    return "RS Left";
                case DS4ControlItem.RXPos:
                    return "RS Right";
                case DS4ControlItem.RYNeg:
                    return "RS Up";
                case DS4ControlItem.RYPos:
                    return "RS Down";
                case DS4ControlItem.L1:
                    return "L1";
                case DS4ControlItem.L2:
                    return "L2";
                case DS4ControlItem.L3:
                    return "L3";
                case DS4ControlItem.R1:
                    return "R1";
                case DS4ControlItem.R2:
                    return "R2";
                case DS4ControlItem.R3:
                    return "R3";
                case DS4ControlItem.Square:
                    return "Square";
                case DS4ControlItem.Triangle:
                    return "Triangle";
                case DS4ControlItem.Circle:
                    return "Circle";
                case DS4ControlItem.Cross:
                    return "Cross";
                case DS4ControlItem.DpadUp:
                    return "Up";
                case DS4ControlItem.DpadRight:
                    return "Right";
                case DS4ControlItem.DpadDown:
                    return "Down";
                case DS4ControlItem.DpadLeft:
                    return "Left";
                case DS4ControlItem.PS:
                    return "PS";
                case DS4ControlItem.TouchLeft:
                    return "Left Touch";
                case DS4ControlItem.TouchUpper:
                    return "Upper Touch";
                case DS4ControlItem.TouchMulti:
                    return "Multitouch";
                case DS4ControlItem.TouchRight:
                    return "Right Touch";
                case DS4ControlItem.Share:
                    return "Share";
                case DS4ControlItem.Options:
                    return "Options";
                case DS4ControlItem.Mute:
                    return "Mute";
                case DS4ControlItem.GyroXPos:
                    return "Tilt Left";
                case DS4ControlItem.GyroXNeg:
                    return "Tilt Right";
                case DS4ControlItem.GyroZPos:
                    return "Tilt Down";
                case DS4ControlItem.GyroZNeg:
                    return "Tilt Up";
                case DS4ControlItem.SwipeLeft:
                    return "Swipe Left";
                case DS4ControlItem.SwipeRight:
                    return "Swipe Right";
                case DS4ControlItem.SwipeUp:
                    return "Swipe Up";
                case DS4ControlItem.SwipeDown:
                    return "Swipe Down";
                case DS4ControlItem.L2FullPull:
                    return "L2 Full Pull";
                case DS4ControlItem.R2FullPull:
                    return "R2 Full Pull";
                case DS4ControlItem.GyroSwipeLeft:
                    return "Gyro Swipe Left";
                case DS4ControlItem.GyroSwipeRight:
                    return "Gyro Swipe Right";
                case DS4ControlItem.GyroSwipeUp:
                    return "Gyro Swipe Up";
                case DS4ControlItem.GyroSwipeDown:
                    return "Gyro Swipe Down";
                case DS4ControlItem.Capture:
                    return "Capture";
                case DS4ControlItem.SideL:
                    return "Side L";
                case DS4ControlItem.SideR:
                    return "Side R";
                case DS4ControlItem.LSOuter:
                    return "LS Outer";
                case DS4ControlItem.RSOuter:
                    return "RS Outer";
                default:
                    throw new ArgumentOutOfRangeException(nameof(control), control, null);
            }

            return string.Empty;
        }
    }
}