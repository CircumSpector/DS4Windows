using Vapour.Shared.Common.Types;

namespace Vapour.Shared.Devices.Services.Reporting;

public interface IOutDevice
{
    void ConvertAndSendReport(InputSourceFinalReport state, int device = 0);
    void Connect();
    void Disconnect();
    void ResetState(bool submit = true);
    OutputDeviceType GetDeviceType();
    event Action<OutputDeviceReport> OnOutputDeviceReportReceived;
}