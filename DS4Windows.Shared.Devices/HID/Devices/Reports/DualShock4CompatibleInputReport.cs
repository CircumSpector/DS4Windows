using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Shared.Devices.HID.Devices.Reports;

public struct TrackPadTouch
{
    public bool IsActive;
    public byte Id;
    public short X;
    public short Y;
    public byte RawTrackingNum;
}

public class DualShock4CompatibleInputReport : CompatibleHidDeviceInputReport
{
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

    /// <inheritdoc />
    public override void Parse(ReadOnlySpan<byte> input)
    {
        // Eliminate bounds checks
        input = input.Slice(0, 43);

        ReportId = input[0];

        LeftThumbX = input[1];
        LeftThumbY = input[2];
        RightThumbX = input[3];
        RightThumbY = input[4];
        LeftTrigger = input[8];
        RightTrigger = input[9];

        Triangle = (input[5] & (1 << 7)) != 0;
        Circle = (input[5] & (1 << 6)) != 0;
        Cross = (input[5] & (1 << 5)) != 0;
        Square = (input[5] & (1 << 4)) != 0;

        DPad = (DPadDirection)(input[5] & 0x0F);

        LeftThumb = (input[6] & (1 << 6)) != 0;
        RightThumb = (input[6] & (1 << 7)) != 0;
        Options = (input[6] & (1 << 5)) != 0;
        Share = (input[6] & (1 << 4)) != 0;
        RightTriggerButton = (input[6] & (1 << 3)) != 0;
        LeftTriggerButton = (input[6] & (1 << 2)) != 0;
        RightShoulder = (input[6] & (1 << 1)) != 0;
        LeftShoulder = (input[6] & (1 << 0)) != 0;

        PS = (input[7] & (1 << 0)) != 0;
        TouchClick = (input[7] & (1 << 1)) != 0;

        FrameCounter = (byte)(input[7] >> 2);

        var touchX = ((input[35] & 0xF) << 8) | input[34];
        
        TrackPadTouch1 = new TrackPadTouch
        {
            RawTrackingNum = input[35],
            Id = (byte)(input[35] & 0x7f),
            IsActive = (input[35] & 0x80) == 0,
            X = (short)(((ushort)(input[37] & 0x0f) << 8) |
                        input[36]),
            Y = (short)((input[39] << 4) |
                        ((ushort)(input[37] & 0xf0) >> 4))
        };
        TrackPadTouch2 = new TrackPadTouch
        {
            RawTrackingNum = input[39],
            Id = (byte)(input[39] & 0x7f),
            IsActive = (input[39] & 0x80) == 0,
            X = (short)(((ushort)(input[41] & 0x0f) << 8) |
                        input[40]),
            Y = (short)((input[42] << 4) |
                        ((ushort)(input[41] & 0xf0) >> 4))
        };
        TouchPacketCounter = input[34];
        Touch1 = input[35] >> 7 == 0;
        Touch2 = input[39] >> 7 == 0;
        TouchIsOnLeftSide = !(touchX >= 1920 * 2 / 5); // TODO: port const
        TouchIsOnRightSide = !(touchX < 1920 * 2 / 5); // TODO: port const
    }

    /// <inheritdoc />
    public override bool GetIsIdle()
    {
        if (!base.GetIsIdle())
            return false;

        if (Touch1 || Touch2 || TouchClick)
            return false;

        return true;
    }
}