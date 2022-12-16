using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;
public class CurrentControllerDataSource : ICurrentControllerDataSource
{
    public List<ICompatibleHidDevice> CurrentControllers { get; } = new();
    public event EventHandler<ICompatibleHidDevice> ControllerAdded;
    public event EventHandler<ICompatibleHidDevice> ControllerRemoved; 

    public void AddController(ICompatibleHidDevice controller)
    {
        if (CurrentControllers.All(c => c.SourceDevice.InstanceId != controller.SourceDevice.InstanceId))
        {
            CurrentControllers.Add(controller);
            ControllerAdded?.Invoke(this, controller);
        }
    }

    public void RemoveController(string instanceId)
    {
        var existing = CurrentControllers.SingleOrDefault(c => c.SourceDevice.InstanceId == instanceId);
        if (existing != null)
        {
            CurrentControllers.Remove(existing);
            existing.Dispose();
            ControllerRemoved?.Invoke(this, existing);
        }
    }

    public ICompatibleHidDevice GetDeviceByInstanceId(string instanceId)
    {
        return CurrentControllers.SingleOrDefault(c => c.SourceDevice.InstanceId == instanceId);
    }

    public ICompatibleHidDevice GetDeviceByControllerKey(string controllerKey)
    {
        return CurrentControllers.SingleOrDefault(c => c.SerialString == controllerKey);
    }
    
    public void Clear()
    {
        foreach (var controller in CurrentControllers.ToList())
        {
            RemoveController(controller.SourceDevice.InstanceId);
        }
    }
}
