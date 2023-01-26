using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="ICompatibleHidDevice" />.
/// </summary>
public interface IInputSource
{
    //ICompatibleHidDevice Controller1 { get; set; }
    //ICompatibleHidDevice Controller2 { get; set; }
    InputSourceConfiguration Configuration { get; }
    string InputSourceKey { get; }
    event EventHandler<InputSourceConfiguration> ConfigurationChanged;
    InputSourceFinalReport ProcessInputReport(ReadOnlySpan<byte> buffers);
    byte[] ReadInputReport();
    void OnAfterStartListening();
    void SetConfiguration(InputSourceConfiguration configuration);
    List<ICompatibleHidDevice> GetControllers();
    void AddController(ICompatibleHidDevice controller);
    void RemoveController(string instanceId);
    void SetPlayerNumberAndColor(int playerNumber);
    void Start();
    void Stop();
}