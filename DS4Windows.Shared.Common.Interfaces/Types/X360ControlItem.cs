using System.ComponentModel;

namespace DS4Windows.Shared.Common.Types
{
    /// <summary>
    ///     Describes a gamepad item (button, axis, slider, etc.) specific to the Xbox 360 feature set.
    /// </summary>
    public enum X360ControlItem : byte
    {
        [Description("Unassigned")]
        None,
        [Description("Left X-Axis-")]
        LXNeg,
        [Description("Left X-Axis+")]
        LXPos,
        [Description("Left Y-Axis-")]
        LYNeg,
        [Description("Left Y-Axis+")]
        LYPos,
        [Description("Right X-Axis-")]
        RXNeg,
        [Description("Right X-Axis+")]
        RXPos,
        [Description("Right Y-Axis-")]
        RYNeg,
        [Description("Right Y-Axis+")]
        RYPos,
        [Description("Left Bumper")]
        LB,
        [Description("Left Trigger")]
        LT,
        [Description("Left Stick")]
        LS,
        [Description("Right Bumper")]
        RB,
        [Description("Right Trigger")]
        RT,
        [Description("Right Stick")]
        RS,
        [Description("X Button")]
        X,
        [Description("Y Button")]
        Y,
        [Description("B Button")]
        B,
        [Description("A Button")]
        A,
        [Description("Up Button")]
        DpadUp,
        [Description("Right Button")]
        DpadRight,
        [Description("Down Button")]
        DpadDown,
        [Description("Left Button")]
        DpadLeft,
        [Description("Guide")]
        Guide,
        [Description("Back")]
        Back,
        [Description("Start")]
        Start,
        [Description("Touchpad Click")]
        TouchpadClick,
        [Description("Left Mouse Button")]
        LeftMouse,
        [Description("Right Mouse Button")]
        RightMouse,
        [Description("Middle Mouse Button")]
        MiddleMouse,
        [Description("4th Mouse Button")]
        FourthMouse,
        [Description("5th Mouse Button")]
        FifthMouse,
        [Description("Mouse Wheel Up")]
        WUP,
        [Description("Mouse Wheel Down")]
        WDOWN,
        [Description("Mouse Up")]
        MouseUp,
        [Description("Mouse Down")]
        MouseDown,
        [Description("Mouse Left")]
        MouseLeft,
        [Description("Mouse Right")]
        MouseRight,
        [Description("Unbound")]
        Unbound
    }
}