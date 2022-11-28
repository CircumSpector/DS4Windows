using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;
public interface IControllerInputReportProcessorService
{
    ICompatibleHidDevice CreateReportProcessor(IHidDevice hidDevice, CompatibleDeviceIdentification deviceIdentification);
    void StopProcessing(ICompatibleHidDevice hidDevice);
}