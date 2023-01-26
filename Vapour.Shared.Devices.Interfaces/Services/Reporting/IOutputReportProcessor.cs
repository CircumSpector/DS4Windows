using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

public interface IOutputReportProcessor
{
    bool IsProcessing { get; set; }
    void SetDevice(ICompatibleHidDevice device);
    void Start();
    void Stop();
}