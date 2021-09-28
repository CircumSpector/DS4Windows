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
