using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    void Stop();
    void AddController(ICompatibleHidDevice device);
    void RemoveController(string deviceKey);
    void Clear();
}