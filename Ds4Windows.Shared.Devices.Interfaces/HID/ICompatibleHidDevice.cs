using System.Net.NetworkInformation;

namespace Ds4Windows.Shared.Devices.Interfaces.HID
{
    public interface ICompatibleHidDevice : IHidDevice
    {
        ConnectionType? Connection { get; }
        InputDeviceType DeviceType { get; set; }
        CompatibleHidDeviceFeatureSet FeatureSet { get; }
        bool IsInputReportAvailableInvoked { get; }
        int ReportsPerSecondProcessed { get; }
        int ReportsPerSecondRead { get; }
        PhysicalAddress Serial { get; }

        event Action<ICompatibleHidDevice> Disconnected;
        event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

        void Dispose();
        string ToString();
    }
}