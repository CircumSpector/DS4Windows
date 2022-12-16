using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

public interface IOutputProcessor
{
    ICompatibleHidDevice HidDevice { get; }
    void StartOutputProcessing();
    void StopOutputProcessing();
    void SetDevice(ICompatibleHidDevice device);
}