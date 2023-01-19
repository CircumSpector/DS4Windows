namespace Vapour.Shared.Devices.Services.Reporting;

public interface IInputReportProcessorService
{
    void StartProcessing(IInputSource inputSource);

    void StopProcessing(IInputSource inputSource);
}