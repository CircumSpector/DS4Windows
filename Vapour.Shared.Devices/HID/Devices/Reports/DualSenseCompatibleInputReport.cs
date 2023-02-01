using System.Runtime.InteropServices;

using Vapour.Shared.Common.Util;
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

        LeftThumbX = input[DualSense.In.LeftThumbXIndex];
        LeftThumbY = input[DualSense.In.LeftThumbYIndex];
        RightThumbX = input[DualSense.In.RightThumbXIndex];
        RightThumbY = input[DualSense.In.RightThumbYIndex];
        LeftTrigger = input[DualSense.In.LeftTriggerIndex];
        RightTrigger = input[DualSense.In.RightTriggerIndex];
        
        var buttons1Byte = input[DualSense.In.Buttons1Index];
        var buttons1 = (DualSenseButtons1)buttons1Byte;
        Triangle = buttons1.HasFlag(DualSenseButtons1.Triangle);
        Circle = buttons1.HasFlag(DualSenseButtons1.Circle);
        Cross = buttons1.HasFlag(DualSenseButtons1.Cross);
        Square = buttons1.HasFlag(DualSenseButtons1.Square);

        DPad = (DPadDirection)buttons1Byte.GetBitsAsByte(0, 4);

        var buttons2 = (DualSenseButtons2)input[DualSense.In.Buttons2Index];
        LeftThumb = buttons2.HasFlag(DualSenseButtons2.L3);
        RightThumb = buttons2.HasFlag(DualSenseButtons2.R3);
        Options = buttons2.HasFlag(DualSenseButtons2.Options);
        Share = buttons2.HasFlag(DualSenseButtons2.Create);
        RightTriggerButton = buttons2.HasFlag(DualSenseButtons2.R2);
        LeftTriggerButton = buttons2.HasFlag(DualSenseButtons2.L2);
        RightShoulder = buttons2.HasFlag(DualSenseButtons2.R1);
        LeftShoulder = buttons2.HasFlag(DualSenseButtons2.L1);

        var buttons3 = (DualSenseButtons3)input[DualSense.In.Buttons3Index];
        PS = buttons3.HasFlag(DualSenseButtons3.Home);
        TouchClick = buttons3.HasFlag(DualSenseButtons3.Pad);
        Mute = buttons3.HasFlag(DualSenseButtons3.Mute);

        var touchData =
            MemoryMarshal.AsRef<TouchData>(input.Slice(DualSense.In.Touch1Index,
                DualSense.In.TouchDataLength));

        TrackPadTouch1 = new TrackPadTouch
        {
            RawTrackingNum = input[DualSense.In.Touch1Index],
            Id = touchData.Finger1.Index,
            IsActive = touchData.Finger1.IsActive,
            X = touchData.Finger1.FingerX,
            Y = touchData.Finger1.FingerY
        };
        TrackPadTouch2 = new TrackPadTouch
        {
            RawTrackingNum = input[DualSense.In.Touch2Index],
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