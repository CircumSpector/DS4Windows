namespace Vapour.Shared.Devices.Services.Reporting;

public interface IOutputDeviceProcessor
{
    IInputSource InputSource { get; }
    void StartOutputProcessing(IInputReportProcessor inputReportProcessor);
    void StopOutputProcessing();
    void SetInputSource(IInputSource inputSource);
}