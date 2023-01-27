namespace Vapour.Shared.Devices.Services.Reporting;

public interface ICustomActionProcessor
{
    void ProcessReport(IInputSource inputSource, InputSourceFinalReport inputReport);
}