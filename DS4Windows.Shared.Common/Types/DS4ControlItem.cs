using System.ComponentModel;

namespace DS4Windows.Shared.Common.Types
{
    /// <summary>
    ///     Describes a gamepad item (button, axis, slider, etc.) specific to the DualShock 4 feature set.
    /// </summary>
    public enum DS4ControlItem : byte
    {
        [Description("Unbound")] None,
        [Description("Left X-Axis-")] LXNeg,
        [Description("Left X-Axis+")] LXPos,
        [Description("Left Y-Axis-")] LYNeg,
        [Description("Left Y-Axis+")] LYPos,
        [Description("Right X-Axis-")] RXNeg,
        [Description("Right X-Axis+")] RXPos,
        [Description("Right Y-Axis-")] RYNeg,
        [Description("Right Y-Axis+")] RYPos,
        [Description("L1")] L1,
        [Description("L2")] L2,
        [Description("L3")] L3,
        [Description("R1")] R1,
        [Description("R2")] R2,
        [Description("R3")] R3,
        [Description("Square")] Square,
        [Description("Triangle")] Triangle,
        [Description("Circle")] Circle,
        [Description("Cross")] Cross,
        [Description("Dpad Up")] DpadUp,
        [Description("Dpad Right")] DpadRight,
        [Description("Dpad Down")] DpadDown,
        [Description("Dpad Left")] DpadLeft,
        [Description("PS")] PS,
        [Description("Left Touch")] TouchLeft,
        [Description("Upper Touch")] TouchUpper,
        [Description("Multitouch")] TouchMulti,
        [Description("Right Touch")] TouchRight,
        [Description("Share")] Share,
        [Description("Options")] Options,
        [Description("Mute")] Mute,
        [Description("Gyro X+")] GyroXPos,
        [Description("Gyro X-")] GyroXNeg,
        [Description("Gyro Z+")] GyroZPos,
        [Description("Gyro Z-")] GyroZNeg,
        [Description("Swipe Left")] SwipeLeft,
        [Description("Swipe Right")] SwipeRight,
        [Description("Swipe Up")] SwipeUp,
        [Description("Swipe Down")] SwipeDown,
        [Description("L2 Full Pull")] L2FullPull,
        [Description("R2 Full Pull")] R2FullPull,
        [Description("Gyro Swipe Left")] GyroSwipeLeft,
        [Description("Gyro Swipe Right")] GyroSwipeRight,
        [Description("Gyro Swipe Up")] GyroSwipeUp,
        [Description("Gyro Swipe Down")] GyroSwipeDown,
        [Description("Capture")] Capture,
        [Description("Side L")] SideL,
        [Description("Side R")] SideR,
        [Description("LS Outer")] LSOuter,
        [Description("RS Outer")] RSOuter
    }
}