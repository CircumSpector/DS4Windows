namespace Vapour.Shared.Devices.Services;
public class InputSourceDataSource : IInputSourceDataSource
{
    public List<IInputSource> InputSources { get; } = new();


    public event Action<IInputSource> InputSourceCreated;
    public event Action<IInputSource> InputSourceRemoved;

    public IInputSource GetByInputSourceKey(string inputSourceKey)
    {
        return InputSources.SingleOrDefault(i => i.InputSourceKey == inputSourceKey);
    }

    public IInputSource GetByDeviceInstanceId(string instanceId)
    {
        return InputSources.SingleOrDefault(i => i.GetControllerByInstanceId(instanceId) != null);
    }

    public IInputSource GetByDeviceParentInstanceId(string instanceId)
    {
        return InputSources.SingleOrDefault(i => i.GetControllerByParentInstanceId(instanceId) != null);
    }

    public void FireCreated(IInputSource inputSource)
    {
        InputSourceCreated?.Invoke(inputSource);
    }

    public void FireRemoved(IInputSource inputSource)
    {
        InputSourceRemoved?.Invoke(inputSource);
    }
}
