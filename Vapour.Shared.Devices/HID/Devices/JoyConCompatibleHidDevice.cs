using System.Net.NetworkInformation;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    public JoyConCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
        Serial = PhysicalAddress.Parse(SerialNumberString);
    }

    protected override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }

    protected override CompatibleHidDeviceInputReport InputReport { get; } = new JoyConCompatibleInputReport();
}