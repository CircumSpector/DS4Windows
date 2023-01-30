using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="ICompatibleHidDevice" />.
/// </summary>
public interface IInputSource
{
    InputSourceConfiguration Configuration { get; }
    string InputSourceKey { get; }
    List<ICompatibleHidDevice> Controllers { get; }
    event EventHandler<InputSourceConfiguration> ConfigurationChanged;
    InputSourceFinalReport ProcessInputReport(ReadOnlySpan<byte> buffers);
    byte[] ReadInputReport();
    void OnAfterStartListening();
    void AddController(ICompatibleHidDevice controller);
    void RemoveController(string instanceId);
    void SetPlayerNumberAndColor(int playerNumber);
    void Start();
    void Stop();
    Task DisconnectControllers();
    ICompatibleHidDevice GetControllerByInstanceId(string instanceId);
    ICompatibleHidDevice GetControllerByParentInstanceId(string instanceId);
    void LoadInputSourceConfiguration();
    event Action<ICustomAction> OnCustomActionDetected;
}