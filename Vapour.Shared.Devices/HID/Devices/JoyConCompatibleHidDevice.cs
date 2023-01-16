using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    public JoyConCompatibleHidDevice(ILogger<JoyConCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
    }

    protected override void OnInitialize()
    {
        Serial = PhysicalAddress.Parse(SourceDevice.SerialNumberString);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
    }

    public override InputSourceReport InputSourceReport { get; } = new JoyConCompatibleInputReport();

    protected override InputDeviceType InputDeviceType => InputDeviceType.JoyCon;
}