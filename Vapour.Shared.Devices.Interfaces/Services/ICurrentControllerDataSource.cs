using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface ICurrentControllerDataSource
{
    List<ICompatibleHidDevice> CurrentControllers { get; }
    event EventHandler<ICompatibleHidDevice> ControllerAdded;
    event EventHandler<ICompatibleHidDevice> ControllerRemoved;
    void AddController(ICompatibleHidDevice controller);
    void RemoveController(string instanceId);
    ICompatibleHidDevice GetDeviceByInstanceId(string instanceId);
    ICompatibleHidDevice GetDeviceByControllerKey(string controllerKey);
    void Clear();
}