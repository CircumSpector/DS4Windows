using System;

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
        ///     Provides a user-readable representation of <see cref="DS4Controls"/>.
        /// </summary>
        /// <param name="control">The <see cref="DS4Controls"/> to return as <see cref="string"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string ToDisplayName(this DS4Controls control)
        {
            switch (control)
            {
                case DS4Controls.None:
                    break;
                case DS4Controls.LXNeg:
                    return "LS Left";
                case DS4Controls.LXPos:
                    return "LS Right";
                case DS4Controls.LYNeg:
                    return "LS Up";
                case DS4Controls.LYPos:
                    return "LS Down";
                case DS4Controls.RXNeg:
                    return "RS Left";
                case DS4Controls.RXPos:
                    return "RS Right";
                case DS4Controls.RYNeg:
                    return "RS Up";
                case DS4Controls.RYPos:
                    return "RS Down";
                case DS4Controls.L1:
                    return "L1";
                case DS4Controls.L2:
                    return "L2";
                case DS4Controls.L3:
                    return "L3";
                case DS4Controls.R1:
                    return "R1";
                case DS4Controls.R2:
                    return "R2";
                case DS4Controls.R3:
                    return "R3";
                case DS4Controls.Square:
                    return "Square";
                case DS4Controls.Triangle:
                    return "Triangle";
                case DS4Controls.Circle:
                    return "Circle";
                case DS4Controls.Cross:
                    return "Cross";
                case DS4Controls.DpadUp:
                    return "Up";
                case DS4Controls.DpadRight:
                    return "Right";
                case DS4Controls.DpadDown:
                    return "Down";
                case DS4Controls.DpadLeft:
                    return "Left";
                case DS4Controls.PS:
                    return "PS";
                case DS4Controls.TouchLeft:
                    return "Left Touch";
                case DS4Controls.TouchUpper:
                    return "Upper Touch";
                case DS4Controls.TouchMulti:
                    return "Multitouch";
                case DS4Controls.TouchRight:
                    return "Right Touch";
                case DS4Controls.Share:
                    return "Share";
                case DS4Controls.Options:
                    return "Options";
                case DS4Controls.Mute:
                    return "Mute";
                case DS4Controls.GyroXPos:
                    return "Tilt Left";
                case DS4Controls.GyroXNeg:
                    return "Tilt Right";
                case DS4Controls.GyroZPos:
                    return "Tilt Down";
                case DS4Controls.GyroZNeg:
                    return "Tilt Up";
                case DS4Controls.SwipeLeft:
                    return "Swipe Left";
                case DS4Controls.SwipeRight:
                    return "Swipe Right";
                case DS4Controls.SwipeUp:
                    return "Swipe Up";
                case DS4Controls.SwipeDown:
                    return "Swipe Down";
                case DS4Controls.L2FullPull:
                    return "L2 Full Pull";
                case DS4Controls.R2FullPull:
                    return "R2 Full Pull";
                case DS4Controls.GyroSwipeLeft:
                    return "Gyro Swipe Left";
                case DS4Controls.GyroSwipeRight:
                    return "Gyro Swipe Right";
                case DS4Controls.GyroSwipeUp:
                    return "Gyro Swipe Up";
                case DS4Controls.GyroSwipeDown:
                    return "Gyro Swipe Down";
                case DS4Controls.Capture:
                    return "Capture";
                case DS4Controls.SideL:
                    return "Side L";
                case DS4Controls.SideR:
                    return "Side R";
                case DS4Controls.LSOuter:
                    return "LS Outer";
                case DS4Controls.RSOuter:
                    return "RS Outer";
                default:
                    throw new ArgumentOutOfRangeException(nameof(control), control, null);
            }

            return string.Empty;
        }
    }

    public enum DS4Controls : byte
    {
        None,
        LXNeg,
        LXPos,
        LYNeg,
        LYPos,
        RXNeg,
        RXPos,
        RYNeg,
        RYPos,
        L1,
        L2,
        L3,
        R1,
        R2,
        R3,
        Square,
        Triangle,
        Circle,
        Cross,
        DpadUp,
        DpadRight,
        DpadDown,
        DpadLeft,
        PS,
        TouchLeft,
        TouchUpper,
        TouchMulti,
        TouchRight,
        Share,
        Options,
        Mute,
        GyroXPos,
        GyroXNeg,
        GyroZPos,
        GyroZNeg,
        SwipeLeft,
        SwipeRight,
        SwipeUp,
        SwipeDown,
        L2FullPull,
        R2FullPull,
        GyroSwipeLeft,
        GyroSwipeRight,
        GyroSwipeUp,
        GyroSwipeDown,
        Capture,
        SideL,
        SideR,
        LSOuter,
        RSOuter
    }

    public enum X360Controls : byte
    {
        None,
        LXNeg,
        LXPos,
        LYNeg,
        LYPos,
        RXNeg,
        RXPos,
        RYNeg,
        RYPos,
        LB,
        LT,
        LS,
        RB,
        RT,
        RS,
        X,
        Y,
        B,
        A,
        DpadUp,
        DpadRight,
        DpadDown,
        DpadLeft,
        Guide,
        Back,
        Start,
        TouchpadClick,
        LeftMouse,
        RightMouse,
        MiddleMouse,
        FourthMouse,
        FifthMouse,
        WUP,
        WDOWN,
        MouseUp,
        MouseDown,
        MouseLeft,
        MouseRight,
        Unbound
    }

    public enum SASteeringWheelEmulationAxisType : byte
    {
        None = 0,
        LX,
        LY,
        RX,
        RY,
        L2R2,
        VJoy1X,
        VJoy1Y,
        VJoy1Z,
        VJoy2X,
        VJoy2Y,
        VJoy2Z
    }

    public enum OutContType : uint
    {
        None = 0,
        X360,
        DS4
    }

    public enum GyroOutMode : uint
    {
        None,
        Controls,
        Mouse,
        MouseJoystick,
        DirectionalSwipe,
        Passthru,
    }

    public enum TouchpadOutMode : uint
    {
        None,
        Mouse,
        Controls,
        AbsoluteMouse,
        Passthru,
    }

    public enum TrayIconChoice : uint
    {
        Default,
        Colored,
        White,
        Black,
    }

    public enum AppThemeChoice : uint
    {
        Default,
        Dark,
    }
}
