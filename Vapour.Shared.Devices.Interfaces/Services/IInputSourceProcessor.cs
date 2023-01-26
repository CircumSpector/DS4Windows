namespace Vapour.Shared.Devices.Services;

public interface IInputSourceProcessor : IDisposable
{
    void Start(IInputSource inputSource);
    void Stop();
}