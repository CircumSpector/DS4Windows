using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    public JoyConCompatibleHidDevice(ILogger<JoyConCompatibleHidDevice> logger) : base(logger)
    {
    }

    protected override void OnInitialize()
    {
        Serial = PhysicalAddress.Parse(SourceDevice.SerialNumberString);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new JoyConCompatibleInputReport();

    public override List<IDeviceInfo> KnownDevices { get; } = new()
    {
        new JoyConLeftDeviceInfo(),
        new JoyConRightDeviceInfo()
    };
}