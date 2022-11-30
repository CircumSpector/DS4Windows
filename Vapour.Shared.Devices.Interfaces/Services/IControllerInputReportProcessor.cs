using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IControllerInputReportProcessor
{
    ICompatibleHidDevice HidDevice { get; }
    bool IsInputReportAvailableInvoked { get; set; }

    event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

    void StartInputReportReader();
    void StopInputReportReader();
}