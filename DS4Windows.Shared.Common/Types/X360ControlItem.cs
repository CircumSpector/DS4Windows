namespace DS4Windows.Shared.Common.Types
{
    /// <summary>
    ///     Describes a gamepad item (button, axis, slider, etc.) specific to the Xbox 360 feature set.
    /// </summary>
    public enum X360ControlItem : byte
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
}