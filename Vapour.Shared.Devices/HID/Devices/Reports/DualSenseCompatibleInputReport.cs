using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.DualSense.In;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public sealed class DualSenseCompatibleInputReport : DualShock4CompatibleInputReport
{
    public override InputAxisType AxisScaleInputType => InputAxisType.DualShock4;

    public override void Parse(ReadOnlySpan<byte> input)
    {
        ReportId = input[InConstants.ReportIdIndex];
        input = input.Slice(ReportDataStartIndex);

        var reportData = input.ToStruct<InputReportData>();

        var sticksAndTriggers = reportData.SticksAndTriggers;
        LeftThumbX = sticksAndTriggers.LeftStickX;
        LeftThumbY = sticksAndTriggers.LeftStickY;
        RightThumbX = sticksAndTriggers.RightStickX;
        RightThumbY = sticksAndTriggers.RightStickY;
        LeftTrigger = sticksAndTriggers.TriggerLeft;
        RightTrigger = sticksAndTriggers.TriggerRight;

        var buttons1 = reportData.Buttons.Buttons1;
        Triangle = buttons1.HasFlag(DualSenseButtons1.Triangle);
        Circle = buttons1.HasFlag(DualSenseButtons1.Circle);
        Cross = buttons1.HasFlag(DualSenseButtons1.Cross);
        Square = buttons1.HasFlag(DualSenseButtons1.Square);

        DPad = reportData.Buttons.DPad;

        var buttons2 = reportData.Buttons.Buttons2;
        LeftThumb = buttons2.HasFlag(DualSenseButtons2.L3);
        RightThumb = buttons2.HasFlag(DualSenseButtons2.R3);
        Options = buttons2.HasFlag(DualSenseButtons2.Options);
        Share = buttons2.HasFlag(DualSenseButtons2.Create);
        RightTriggerButton = buttons2.HasFlag(DualSenseButtons2.R2);
        LeftTriggerButton = buttons2.HasFlag(DualSenseButtons2.L2);
        RightShoulder = buttons2.HasFlag(DualSenseButtons2.R1);
        LeftShoulder = buttons2.HasFlag(DualSenseButtons2.L1);

        var buttons3 = reportData.Buttons.Buttons3;
        PS = buttons3.HasFlag(DualSenseButtons3.Home);
        TouchClick = buttons3.HasFlag(DualSenseButtons3.Pad);
        Mute = buttons3.HasFlag(DualSenseButtons3.Mute);

        var finger1 = reportData.TouchData.Finger1;
        TrackPadTouch1 = new TrackPadTouch
        {
            RawTrackingNum = finger1.RawTrackingNumber,
            Id = finger1.Index,
            IsActive = finger1.IsActive,
            X = finger1.FingerX,
            Y = finger1.FingerY
        };

        var finger2 = reportData.TouchData.Finger2;
        TrackPadTouch2 = new TrackPadTouch
        {
            RawTrackingNum = finger2.RawTrackingNumber,
            Id = finger2.Index,
            IsActive = finger2.IsActive,
            X = finger2.FingerX,
            Y = finger2.FingerY
        };

        var touchData = reportData.TouchData;
        TouchPacketCounter = touchData.Timestamp;
        Touch1 = finger1.IsActive;
        Touch2 = finger2.IsActive;
        TouchIsOnLeftSide = touchData.IsTouchOnLeftSide;
        TouchIsOnRightSide = touchData.IsTouchOnRightSide;
    }
}