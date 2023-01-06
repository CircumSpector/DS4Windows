using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.VisualBasic.CompilerServices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.SteamDeck;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public class SteamDeckCompatibleInputReport : CompatibleHidDeviceInputReport
{
    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public override void Parse(ReadOnlySpan<byte> input)
    {
        SteamDeckInput report = MemoryMarshal.AsRef<SteamDeckInput>(input);

        Triangle = report.buttons0.HasFlag(SteamDeckButtons0.BTN_Y);
        Circle = report.buttons0.HasFlag(SteamDeckButtons0.BTN_B);
        Cross = report.buttons0.HasFlag(SteamDeckButtons0.BTN_A);
        Square = report.buttons0.HasFlag(SteamDeckButtons0.BTN_X);

        LeftShoulder = report.buttons0.HasFlag(SteamDeckButtons0.BTN_L1);
        RightShoulder = report.buttons0.HasFlag(SteamDeckButtons0.BTN_R1);

        LeftTrigger = (byte)(MemoryMarshal.Read<short>(input.Slice(45, 2)) / (double)Int16.MaxValue * byte.MaxValue);
        RightTrigger = (byte)(MemoryMarshal.Read<short>(input.Slice(47, 2)) / (double)Int16.MaxValue * byte.MaxValue);

        LeftThumbX = MemoryMarshal.Read<short>(input.Slice(49, 2));
        LeftThumbY = MemoryMarshal.Read<short>(input.Slice(51, 2));
        RightThumbX = MemoryMarshal.Read<short>(input.Slice(53, 2));
        RightThumbY = MemoryMarshal.Read<short>(input.Slice(55, 2));

        Options = report.buttons0.HasFlag(SteamDeckButtons0.BTN_OPTIONS);
        Share = report.buttons0.HasFlag(SteamDeckButtons0.BTN_MENU);
        PS = report.buttons0.HasFlag(SteamDeckButtons0.BTN_STEAM);
    }
}