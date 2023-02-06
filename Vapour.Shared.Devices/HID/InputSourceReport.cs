using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Describes the bare minimum common properties an input report of any <see cref="ICompatibleHidDevice"/> can deliver.
/// </summary>
public abstract class InputSourceReport : IInputSourceReport
{
    public abstract InputAxisType AxisScaleInputType { get; }

    /// <summary>
    ///     Gets the report ID.
    /// </summary>
    public byte ReportId { get; set; }

    /// <summary>
    ///     Gets the battery state.
    /// </summary>
    public byte? Battery { get; protected set; }

    /// <summary>
    ///     Gets the D-Pad state.
    /// </summary>
    public DPadDirection DPad { get; protected set; } = DPadDirection.Default;

    public ushort Timestamp { get; protected set; }

    public byte FrameCounter { get; protected set; }

    /// <summary>
    ///     Gets whether the Left Shoulder button is pressed or not.
    /// </summary>
    public bool LeftShoulder { get; protected set; }

    /// <summary>
    ///     Gets whether the Right Shoulder button is pressed or not.
    /// </summary>
    public bool RightShoulder { get; protected set; }

    /// <summary>
    ///     Gets whether L2 button is pressed or not.
    /// </summary>
    public byte LeftTrigger { get; protected set; }

    /// <summary>
    ///     Gets whether the Left Trigger Button button is pressed or not.
    /// </summary>
    public bool LeftTriggerButton { get; protected set; }

    /// <summary>
    ///     Gets whether R2 button is pressed or not.
    /// </summary>
    public byte RightTrigger { get; protected set; }

    /// <summary>
    ///     Gets whether the Right Trigger Button button is pressed or not.
    /// </summary>
    public bool RightTriggerButton { get; protected set; }

    /// <summary>
    ///     Gets whether L3 button is pressed or not.
    /// </summary>
    public bool LeftThumb { get; protected set; }

    /// <summary>
    ///     Gets whether R3 button is pressed or not.
    /// </summary>
    public bool RightThumb { get; protected set; }

    /// <summary>
    ///     Gets whether Share button is pressed or not.
    /// </summary>
    public bool Share { get; protected set; }

    /// <summary>
    ///     Gets whether Options button is pressed or not.
    /// </summary>
    public bool Options { get; protected set; }

    /// <summary>
    ///     Gets whether PS button is pressed or not.
    /// </summary>
    public bool PS { get; protected set; }

    /// <summary>
    ///     Gets whether Square button is pressed or not.
    /// </summary>
    public bool Square { get; protected set; }

    /// <summary>
    ///     Gets whether Triangle button is pressed or not.
    /// </summary>
    public bool Triangle { get; protected set; }

    /// <summary>
    ///     Gets whether Circle button is pressed or not.
    /// </summary>
    public bool Circle { get; protected set; }

    /// <summary>
    ///     Gets whether Cross button is pressed or not.
    /// </summary>
    public bool Cross { get; protected set; }

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

    public MultiControllerConfigurationType MultiControllerConfigurationType
    {
        get;
        set;
    } = MultiControllerConfigurationType.None;

    /// <summary>
    ///     Gets idle state.
    /// </summary>
    public virtual bool IsIdle
    {
        get
        {
            if (Square || Cross || Circle || Triangle)
                return false;
            if (DPad != DPadDirection.Default)
                return false;
            if (LeftShoulder || RightShoulder || LeftThumb || RightThumb || Share || Options || PS)
                return false;
            if (LeftTriggerButton || RightTriggerButton)
                return false;
            if (LeftTrigger != 0 || RightTrigger != 0)
                return false;

            const int slop = 64;
            if (LeftThumbX is <= 127 - slop or >= 128 + slop || LeftThumbY is <= 127 - slop or >= 128 + slop)
                return false;
            if (RightThumbX is <= 127 - slop or >= 128 + slop || RightThumbY is <= 127 - slop or >= 128 + slop)
                return false;

            return true;
        }
    }
}