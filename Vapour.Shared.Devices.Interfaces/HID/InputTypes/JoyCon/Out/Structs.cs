using System.Runtime.InteropServices;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.JoyCon.Out;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SubCommand
{
    public SubCommand()
    {

    }

    public byte ReportId;
    public byte CommandCount;
    public StructArray8<byte> RumbleData = new (OutConstants.DefaultRumbleData);
    public byte SubCommandId;
    public StructArray5<byte> Data = new();
}