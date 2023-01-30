namespace Vapour.Shared.Devices.Services;

public interface IInputSourceDataSource
{
    List<IInputSource> InputSources { get; }
    event Action<IInputSource> InputSourceCreated;
    event Action<IInputSource> InputSourceRemoved;
    IInputSource GetByInputSourceKey(string inputSourceKey);
    void FireCreated(IInputSource inputSource);
    void FireRemoved(IInputSource inputSource);
    IInputSource GetByDeviceInstanceId(string instanceId);
}