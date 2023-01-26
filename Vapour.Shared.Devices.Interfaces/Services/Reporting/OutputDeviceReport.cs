using Vapour.Shared.Common.Types;

namespace Vapour.Shared.Devices.Services.Reporting;
public class OutputDeviceReport
{
    public OutputDeviceType OutputDeviceType { get; set; }
    public byte[] Packet { get; set; }
}
