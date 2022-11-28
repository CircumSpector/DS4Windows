using System.Net.NetworkInformation;

using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    public JoyConCompatibleHidDevice(InputDeviceType deviceType, IHidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
        Serial = PhysicalAddress.Parse(SourceDevice.SerialNumberString);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new JoyConCompatibleInputReport();
}