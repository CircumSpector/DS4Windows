namespace Vapour.Shared.Devices.Services.Reporting;

public interface IOutputProcessor
{
    IInputSource InputSource { get; }
    void StartOutputProcessing();
    void StopOutputProcessing();
    void SetDevice(IInputSource inputSource);
}