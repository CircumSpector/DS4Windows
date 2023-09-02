using System.Diagnostics.CodeAnalysis;

using Nefarius.Utilities.HID.Devices;
using Nefarius.Utilities.HID.Devices.DualShock4.In;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public struct TrackPadTouch
{
    public bool IsActive;
    
    public byte Id;
    
    public short X;
    
    public short Y;
    
    public byte RawTrackingNum;
}

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class DualShock4CompatibleInputReport : InputSourceReport, IStructInputSourceReport<InputReportData>
{
    public override AxisRangeType AxisScaleInputType => AxisRangeType.Byte;

    public TrackPadTouch TrackPadTouch1 { get; protected set; }

    public TrackPadTouch TrackPadTouch2 { get; protected set; }

    public byte TouchPacketCounter { get; protected set; }

    public bool TouchOneFingerActive => Touch1 || Touch2;

    public bool TouchTwoFingersActive => Touch1 && Touch2;

    public bool Mute { get; protected set; }

    /// <summary>
    ///     First (one finger) touch is registered.
    /// </summary>
    public bool Touch1 { get; protected set; }

    /// <summary>
    ///     Second (two fingers) touch is registered.
    /// </summary>
    public bool Touch2 { get; protected set; }

    public bool TouchIsOnLeftSide { get; protected set; }

    public bool TouchIsOnRightSide { get; protected set; }

    public bool TouchClick { get; protected set; }

    public int ReportDataStartIndex { get; set; }

    /// <inheritdoc />
    public void Parse(ref InputReportData reportData)
    {
        var sticks = reportData.Sticks;
        LeftThumbX = sticks.LeftStickX;
        LeftThumbY = sticks.LeftStickY;
        RightThumbX = sticks.RightStickX;
        RightThumbY = sticks.RightStickY;

        var buttons1 = reportData.Buttons.Buttons1;
        Triangle = buttons1.HasFlag(DualShock4Buttons1.Triangle);
        Circle = buttons1.HasFlag(DualShock4Buttons1.Circle);
        Cross = buttons1.HasFlag(DualShock4Buttons1.Cross);
        Square = buttons1.HasFlag(DualShock4Buttons1.Square);

        DPad = reportData.Buttons.DPad;

        var buttons2 = reportData.Buttons.Buttons2;
        LeftThumb = buttons2.HasFlag(DualShock4Buttons2.L3);
        RightThumb = buttons2.HasFlag(DualShock4Buttons2.R3);
        Options = buttons2.HasFlag(DualShock4Buttons2.Options);
        Share = buttons2.HasFlag(DualShock4Buttons2.Create);
        RightTriggerButton = buttons2.HasFlag(DualShock4Buttons2.R2);
        LeftTriggerButton = buttons2.HasFlag(DualShock4Buttons2.L2);
        RightShoulder = buttons2.HasFlag(DualShock4Buttons2.R1);
        LeftShoulder = buttons2.HasFlag(DualShock4Buttons2.L1);

        var buttons3 = reportData.Buttons.Buttons3;
        PS = buttons3.HasFlag(DualShock4Buttons3.Home);
        TouchClick = buttons3.HasFlag(DualShock4Buttons3.Pad);
        FrameCounter = reportData.Buttons.FrameCounter;

        LeftTrigger = reportData.Buttons.LeftTrigger;
        RightTrigger = reportData.Buttons.RightTrigger;

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

    /// <inheritdoc />
    public override bool IsIdle
    {
        get
        {
            if (!base.IsIdle)
            {
                return false;
            }

            if (Touch1 || Touch2 || TouchClick)
            {
                return false;
            }

            return true;
        }
    }
}