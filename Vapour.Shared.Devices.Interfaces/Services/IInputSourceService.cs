namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    void Stop();
    Task Start(CancellationToken ct = default);
    event Action InputSourceListReady;
}