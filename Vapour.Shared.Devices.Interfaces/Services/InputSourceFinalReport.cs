using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.InputTypes;

namespace Vapour.Shared.Devices.Services;
public class InputSourceFinalReport 
{
    public InputAxisType LThumbAxisScaleInputType { get; set; }
    public InputAxisType RThumbAxisScaleInputType { get; set; }
    
    /// <summary>
    ///     Gets the battery state.
    /// </summary>
    public byte? Battery { get; set; }

    /// <summary>
    ///     Gets the D-Pad state.
    /// </summary>
    public DPadDirection DPad { get; set; } = DPadDirection.Default;

    public ushort Timestamp { get; set; }

    public byte FrameCounter { get; set; }

    /// <summary>
    ///     Gets whether the Left Shoulder button is pressed or not.
    /// </summary>
    public bool LeftShoulder { get; set; }

    /// <summary>
    ///     Gets whether the Right Shoulder button is pressed or not.
    /// </summary>
    public bool RightShoulder { get; set; }

    /// <summary>
    ///     Gets whether L2 button is pressed or not.
    /// </summary>
    public byte LeftTrigger { get; set; }

    /// <summary>
    ///     Gets whether the Left Trigger Button button is pressed or not.
    /// </summary>
    public bool LeftTriggerButton { get; set; }

    /// <summary>
    ///     Gets whether R2 button is pressed or not.
    /// </summary>
    public byte RightTrigger { get; set; }

    /// <summary>
    ///     Gets whether the Right Trigger Button button is pressed or not.
    /// </summary>
    public bool RightTriggerButton { get; set; }

    /// <summary>
    ///     Gets whether L3 button is pressed or not.
    /// </summary>
    public bool LeftThumb { get; set; }

    /// <summary>
    ///     Gets whether R3 button is pressed or not.
    /// </summary>
    public bool RightThumb { get; set; }

    /// <summary>
    ///     Gets whether Share button is pressed or not.
    /// </summary>
    public bool Share { get; set; }

    /// <summary>
    ///     Gets whether Options button is pressed or not.
    /// </summary>
    public bool Options { get; set; }

    /// <summary>
    ///     Gets whether PS button is pressed or not.
    /// </summary>
    public bool PS { get; set; }

    /// <summary>
    ///     Gets whether Square button is pressed or not.
    /// </summary>
    public bool Square { get; set; }

    /// <summary>
    ///     Gets whether Triangle button is pressed or not.
    /// </summary>
    public bool Triangle { get; set; }

    /// <summary>
    ///     Gets whether Circle button is pressed or not.
    /// </summary>
    public bool Circle { get; set; }

    /// <summary>
    ///     Gets whether Cross button is pressed or not.
    /// </summary>
    public bool Cross { get; set; }

    /// <summary>
    ///     Gets the Left Thumb X axis value.
    /// </summary>
    public short LeftThumbX { get; set; } = 128;

    /// <summary>
    ///     Gets the Left Thumb Y axis value.
    /// </summary>
    public short LeftThumbY { get; set; } = 128;

    /// <summary>
    ///     Gets the Right Thumb X axis value.
    /// </summary>
    public short RightThumbX { get; set; } = 128;

    /// <summary>
    ///     Gets the Right Thumb Y axis value.
    /// </summary>
    public short RightThumbY { get; set; } = 128;
}
