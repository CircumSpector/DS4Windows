using System.Runtime.InteropServices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.DualSense;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public sealed class DualSenseCompatibleInputReport : DualShock4CompatibleInputReport
{
    public override InputAxisType AxisScaleInputType => InputAxisType.DualShock4;
    
    public int ReportDataStartIndex { get; set; }

    public override void Parse(ReadOnlySpan<byte> input)
    {
        ReportId = input[DualSense.In.ReportIdIndex];
        
        input = input.Slice(ReportDataStartIndex);

        var reportData = MemoryMarshal.AsRef<InputReportData>(input);

        LeftThumbX = reportData.SticksAndTriggers.LeftStickX;
        LeftThumbY = reportData.SticksAndTriggers.LeftStickY;
        RightThumbX = reportData.SticksAndTriggers.RightStickX;
        RightThumbY = reportData.SticksAndTriggers.RightStickY;
        LeftTrigger = reportData.SticksAndTriggers.TriggerLeft;
        RightTrigger = reportData.SticksAndTriggers.TriggerRight;
        
        Triangle = reportData.Buttons.Buttons1.HasFlag(DualSense.In.DualSenseButtons1.Triangle);
        Circle = reportData.Buttons.Buttons1.HasFlag(DualSense.In.DualSenseButtons1.Circle);
        Cross = reportData.Buttons.Buttons1.HasFlag(DualSense.In.DualSenseButtons1.Cross);
        Square = reportData.Buttons.Buttons1.HasFlag(DualSense.In.DualSenseButtons1.Square);

        DPad = reportData.Buttons.DPad;

        LeftThumb = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.L3);
        RightThumb = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.R3);
        Options = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.Options);
        Share = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.Create);
        RightTriggerButton = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.R2);
        LeftTriggerButton = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.L2);
        RightShoulder = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.R1);
        LeftShoulder = reportData.Buttons.Buttons2.HasFlag(DualSense.In.DualSenseButtons2.L1);

        PS = reportData.Buttons.Buttons3.HasFlag(DualSense.In.DualSenseButtons3.Home);
        TouchClick = reportData.Buttons.Buttons3.HasFlag(DualSense.In.DualSenseButtons3.Pad);
        Mute = reportData.Buttons.Buttons3.HasFlag(DualSense.In.DualSenseButtons3.Mute);

        var touchData = reportData.TouchData;

            TrackPadTouch1 = new TrackPadTouch
        {
            RawTrackingNum = touchData.Finger1.RawTrackingNumber,
            Id = touchData.Finger1.Index,
            IsActive = touchData.Finger1.IsActive,
            X = touchData.Finger1.FingerX,
            Y = touchData.Finger1.FingerY
        };
        TrackPadTouch2 = new TrackPadTouch
        {
            RawTrackingNum = touchData.Finger2.RawTrackingNumber,
            Id = touchData.Finger2.Index,
            IsActive = touchData.Finger2.IsActive,
            X = touchData.Finger2.FingerX,
            Y = touchData.Finger2.FingerY
        };
        TouchPacketCounter = touchData.Timestamp;
        Touch1 = touchData.Finger1.IsActive;
        Touch2 = touchData.Finger2.IsActive;
        TouchIsOnLeftSide = touchData.IsTouchOnLeftSide;
        TouchIsOnRightSide = touchData.IsTouchOnRightSide;
    }
}