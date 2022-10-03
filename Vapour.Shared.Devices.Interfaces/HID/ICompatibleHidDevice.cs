using System.Net.NetworkInformation;

namespace Vapour.Shared.Devices.Interfaces.HID
{
    public interface ICompatibleHidDevice : IHidDevice
    {
        ConnectionType? Connection { get; }
        InputDeviceType DeviceType { get; set; }
        CompatibleHidDeviceFeatureSet FeatureSet { get; }
        bool IsInputReportAvailableInvoked { get; }
        PhysicalAddress Serial { get; }

        event Action<ICompatibleHidDevice> Disconnected;
        event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;
    }
}