using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services.Reporting;

public interface ICustomActionProcessor
{
    Task ProcessReport(IInputSource inputSource, InputSourceFinalReport inputReport);
    event Action<ICustomAction> OnCustomActionDetected;
}