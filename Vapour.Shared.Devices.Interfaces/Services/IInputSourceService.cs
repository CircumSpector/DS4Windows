using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    void Stop();
    Task AddController(ICompatibleHidDevice device);
    Task RemoveController(string instanceId);
    Task Clear();
    Task FixupInputSources();
    bool ShouldAutoFixup { get; set; }
}