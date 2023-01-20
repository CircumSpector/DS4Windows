using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="CompatibleHidDevice" />.
/// </summary>
internal class InputSource : IInputSource
{
    private Dictionary<ICompatibleHidDevice, byte[]> _controllers = new();
    private byte[] _allReportBytes;

    public event EventHandler<InputSourceConfiguration> ConfigurationChanged;
    public InputSourceConfiguration Configuration { get; private set; }

    public string InputSourceKey { get; private set; }

    public void SetConfiguration(InputSourceConfiguration configuration)
    {
        Configuration = configuration;
        foreach (var controller in _controllers)
        {
            controller.Key.SetConfiguration(Configuration);
        }
        ReorderControllers();
        SetInputSourceKey();
        ConfigurationChanged?.Invoke(this, configuration);
    }

    public InputSourceReport ProcessInputReport(ReadOnlySpan<byte> buffers)
    {
        InputSourceReport finalReport = null;
        var previousLength = 0;
        foreach (var controller in _controllers)
        {  
            var bytes = buffers.Slice(previousLength, controller.Value.Length);
            controller.Key.ProcessInputReport(bytes);
            previousLength = previousLength + controller.Value.Length - 1;
            
            if (finalReport == null)
            {
                finalReport = controller.Key.InputSourceReport;
            }
            else
            {
                //combine controller report with previous one
            }
        }

        return finalReport;
    }

    public byte[] ReadInputReport()
    {
        var previousLength = 0;
        foreach (var controller in _controllers)
        {
            controller.Key.ReadInputReport(controller.Value);

            Buffer.BlockCopy(controller.Value, 0, _allReportBytes, previousLength, controller.Value.Length);
            previousLength = previousLength + controller.Value.Length - 1;
        }

        return _allReportBytes;
    }

    public void OnAfterStartListening()
    {
        foreach (var controller in _controllers)
        {
            controller.Key.OnAfterStartListening();
        }
    }

    public List<ICompatibleHidDevice> GetControllers()
    {
        return _controllers.Keys.ToList();
    }

    public void AddController(ICompatibleHidDevice controller)
    {
        if (_controllers.Keys.All(c => c.DeviceKey != controller.DeviceKey))
        {
            var reportBuffer = new byte[controller.SourceDevice.InputReportByteLength];
            _controllers.Add(controller, reportBuffer);
            ReorderControllers();
            SetAllReportBytes();
            SetInputSourceKey();
        }
    }
    
    public void RemoveController(string instanceId)
    {
        var existing = _controllers.Keys.SingleOrDefault(c => c.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());
        if (existing != null)
        {
            _controllers.Remove(existing);
            ReorderControllers();
            SetAllReportBytes();
            SetInputSourceKey();
        }
    }

    private void ReorderControllers()
    {
        if (Configuration != null)
        {
            foreach (var controller in _controllers)
            {
                var configurationIndex = Configuration.Controllers
                    .SingleOrDefault(c => c.DeviceKey == controller.Key.DeviceKey)?.Index;
                controller.Key.Index = configurationIndex.HasValue ? configurationIndex.Value : 0;
            }
        }

        _controllers = _controllers.OrderBy(c => c.Key.Index).ToDictionary(i => i.Key, v => v.Value);
    }

    private void SetInputSourceKey()
    {
        InputSourceKey = string.Join("::::", _controllers.Keys.OrderBy(c => c.Index).Select(c => c.DeviceKey));
    }

    private void SetAllReportBytes()
    {
        _allReportBytes = new byte[_controllers.Values.Select(b => b.Length).Sum()];
    }
}