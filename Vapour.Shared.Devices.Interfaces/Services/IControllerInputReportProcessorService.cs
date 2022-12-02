using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IControllerInputReportProcessorService
{
    void StartProcessing(ICompatibleHidDevice device);
    void StopProcessing(ICompatibleHidDevice hidDevice);
}