using System.Runtime.InteropServices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.SteamDeck;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public class SteamDeckCompatibleInputReport : CompatibleHidDeviceInputReport
{
    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public override void Parse(ReadOnlySpan<byte> input)
    {
        var buttons0 = (SteamDeckButtons0)MemoryMarshal.Read<ushort>(input.Slice(9, 2));

        Triangle = buttons0.HasFlag(SteamDeckButtons0.Y);
        Circle = buttons0.HasFlag(SteamDeckButtons0.B);
        Cross = buttons0.HasFlag(SteamDeckButtons0.A);
        Square = buttons0.HasFlag(SteamDeckButtons0.X);

        LeftShoulder = buttons0.HasFlag(SteamDeckButtons0.L1);
        RightShoulder = buttons0.HasFlag(SteamDeckButtons0.R1);

        Options = buttons0.HasFlag(SteamDeckButtons0.Options);
        Share = buttons0.HasFlag(SteamDeckButtons0.Menu);
        PS = buttons0.HasFlag(SteamDeckButtons0.Steam);

        SetDPad(buttons0);

        LeftTrigger = (byte)(MemoryMarshal.Read<short>(input.Slice(45, 2)) / (double)Int16.MaxValue * byte.MaxValue);
        RightTrigger = (byte)(MemoryMarshal.Read<short>(input.Slice(47, 2)) / (double)Int16.MaxValue * byte.MaxValue);

        LeftThumb = input[11] == (byte)SteamDeckButtonsStick.LeftStick;
        RightThumb = input[12] == (byte)SteamDeckButtonsStick.RightStick;

        LeftThumbX = MemoryMarshal.Read<short>(input.Slice(49, 2));
        LeftThumbY = MemoryMarshal.Read<short>(input.Slice(51, 2));
        RightThumbX = MemoryMarshal.Read<short>(input.Slice(53, 2));
        RightThumbY = MemoryMarshal.Read<short>(input.Slice(55, 2));
    }

    private void SetDPad(SteamDeckButtons0 buttons0)
    {
        if (buttons0.HasFlag(SteamDeckButtons0.DpadDown))
        {
            if (buttons0.HasFlag(SteamDeckButtons0.DpadLeft))
            {
                DPad = DPadDirection.SouthWest;
            }
            else if (buttons0.HasFlag(SteamDeckButtons0.DpadRight))
            {
                DPad = DPadDirection.SouthEast;
            }
            else
            {
                DPad = DPadDirection.South;
            }
        }
        else if (buttons0.HasFlag(SteamDeckButtons0.DpadLeft))
        {
            if (buttons0.HasFlag(SteamDeckButtons0.DpadUp))
            {
                DPad = DPadDirection.NorthWest;
            }
            else
            {
                DPad = DPadDirection.West;
            }
        }
        else if (buttons0.HasFlag(SteamDeckButtons0.DpadUp))
        {
            if (buttons0.HasFlag(SteamDeckButtons0.DpadRight))
            {
                DPad = DPadDirection.NorthEast;
            }
            else
            {
                DPad = DPadDirection.North;
            }
        }
        else if (buttons0.HasFlag(SteamDeckButtons0.DpadRight))
        {
            DPad = DPadDirection.East;
        }
        else
        {
            DPad = DPadDirection.Default;
        }
    }
}