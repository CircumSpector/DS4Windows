﻿namespace Vapour.Shared.Devices.HID;

public enum DPadDirection
{
    Default = 0x8,
    NorthWest = 0x7,
    West = 0x6,
    SouthWest = 0x5,
    South = 0x4,
    SouthEast = 0x3,
    East = 0x2,
    NorthEast = 0x1,
    North = 0x0
}

/// <summary>
///     Describes the bare minimum common properties an input report of any compatible device can deliver.
/// </summary>
public abstract class CompatibleHidDeviceInputReport
{
    public byte ReportId { get; protected set; }

    public byte? Battery { get; protected set; }

    public DPadDirection DPad { get; protected set; }

    public ushort Timestamp { get; protected set; }

    public byte FrameCounter { get; protected set; }

    public bool LeftShoulder { get; protected set; }

    public bool RightShoulder { get; protected set; }

    public byte LeftTrigger { get; protected set; }

    public bool LeftTriggerButton { get; protected set; }

    public byte RightTrigger { get; protected set; }

    public bool RightTriggerButton { get; protected set; }

    public bool LeftThumb { get; protected set; }

    public bool RightThumb { get; protected set; }

    public bool Share { get; protected set; }

    public bool Options { get; protected set; }

    public bool PS { get; protected set; }

    public bool Square { get; protected set; }

    public bool Triangle { get; protected set; }

    public bool Circle { get; protected set; }

    public bool Cross { get; protected set; }

    public byte LeftThumbX { get; protected set; } = 128;

    public byte LeftThumbY { get; protected set; } = 128;

    public byte RightThumbX { get; protected set; } = 128;

    public byte RightThumbY { get; protected set; } = 128;

    /// <summary>
    ///     Parse a raw byte buffer into <see cref="CompatibleHidDeviceInputReport" />.
    /// </summary>
    /// <param name="input">The raw input report buffer.</param>
    public abstract void Parse(ReadOnlySpan<byte> input);

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