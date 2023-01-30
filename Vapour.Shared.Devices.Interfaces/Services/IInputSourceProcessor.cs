using Vapour.Shared.Devices.Services.Reporting;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceProcessor : IDisposable
{
    void Start(IInputSource inputSource);

    void Stop();

    event Action<OutputDeviceReport> OnOutputDeviceReportReceived;
    event Action<ICustomAction> OnCustomActionDetected;
}