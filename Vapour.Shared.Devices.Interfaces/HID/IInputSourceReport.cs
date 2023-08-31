using Nefarius.Utilities.HID.Devices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.HID;

public interface IInputSourceReport
{
    AxisRangeType AxisScaleInputType { get; }

    /// <summary>
    ///     Gets the report ID.
    /// </summary>
    byte ReportId { get; }

    /// <summary>
    ///     Gets the battery state.
    /// </summary>
    byte? Battery { get; }

    /// <summary>
    ///     Gets the D-Pad state.
    /// </summary>
    DPadDirection DPad { get; }

    ushort Timestamp { get; }
    byte FrameCounter { get; }

    /// <summary>
    ///     Gets whether the Left Shoulder button is pressed or not.
    /// </summary>
    bool LeftShoulder { get; }

    /// <summary>
    ///     Gets whether the Right Shoulder button is pressed or not.
    /// </summary>
    bool RightShoulder { get; }

    /// <summary>
    ///     Gets whether L2 button is pressed or not.
    /// </summary>
    byte LeftTrigger { get; }

    /// <summary>
    ///     Gets whether the Left Trigger Button button is pressed or not.
    /// </summary>
    bool LeftTriggerButton { get; }

    /// <summary>
    ///     Gets whether R2 button is pressed or not.
    /// </summary>
    byte RightTrigger { get; }

    /// <summary>
    ///     Gets whether the Right Trigger Button button is pressed or not.
    /// </summary>
    bool RightTriggerButton { get; }

    /// <summary>
    ///     Gets whether L3 button is pressed or not.
    /// </summary>
    bool LeftThumb { get; }

    /// <summary>
    ///     Gets whether R3 button is pressed or not.
    /// </summary>
    bool RightThumb { get; }

    /// <summary>
    ///     Gets whether Share button is pressed or not.
    /// </summary>
    bool Share { get; }

    /// <summary>
    ///     Gets whether Options button is pressed or not.
    /// </summary>
    bool Options { get; }

    /// <summary>
    ///     Gets whether PS button is pressed or not.
    /// </summary>
    bool PS { get; }

    /// <summary>
    ///     Gets whether Square button is pressed or not.
    /// </summary>
    bool Square { get; }

    /// <summary>
    ///     Gets whether Triangle button is pressed or not.
    /// </summary>
    bool Triangle { get; }

    /// <summary>
    ///     Gets whether Circle button is pressed or not.
    /// </summary>
    bool Circle { get; }

    /// <summary>
    ///     Gets whether Cross button is pressed or not.
    /// </summary>
    bool Cross { get; }

    /// <summary>
    ///     Gets the Left Thumb X axis value.
    /// </summary>
    short LeftThumbX { get; set; }

    /// <summary>
    ///     Gets the Left Thumb Y axis value.
    /// </summary>
    short LeftThumbY { get; set; }

    /// <summary>
    ///     Gets the Right Thumb X axis value.
    /// </summary>
    short RightThumbX { get; set; }

    /// <summary>
    ///     Gets the Right Thumb Y axis value.
    /// </summary>
    short RightThumbY { get; set; }

    MultiControllerConfigurationType MultiControllerConfigurationType { get; set; }

    /// <summary>
    ///     Gets idle state.
    /// </summary>
    bool IsIdle { get; }
}