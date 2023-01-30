namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    bool ShouldAutoRebuild { get; set; }
    bool ShouldFixupOnConfigChange { get; set; }

    void Stop();
    Task Start();
    event Action InputSourceListReady;
}