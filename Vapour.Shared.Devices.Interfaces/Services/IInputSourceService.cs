using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    bool ShouldAutoFixup { get; set; }
    bool ShouldFixupOnConfigChange { get; set; }

    void Stop();
    
    Task AddController(ICompatibleHidDevice device);
    
    Task RemoveController(string instanceId);
    
    Task Clear();
    
    Task FixupInputSources();
}