using System.Windows.Media;

using Microsoft.Extensions.DependencyInjection;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="CompatibleHidDevice" />.
/// </summary>
internal class InputSource : IInputSource
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private byte[] _allReportBytes;
    private readonly Dictionary<ICompatibleHidDevice, byte[]> _controllerInputReportData = new();
    private InputSourceFinalReport _finalReport = new();
    private IInputSourceProcessor _inputSourceProcessor;

    public InputSource(IServiceProvider serviceProvider,
        IInputSourceConfigurationService inputSourceConfigurationService)
    {
        _serviceProvider = serviceProvider;
        _inputSourceConfigurationService = inputSourceConfigurationService;
    }

    public event EventHandler<InputSourceConfiguration> ConfigurationChanged;
    public InputSourceConfiguration Configuration { get; private set; }

    public string InputSourceKey { get; private set; }
    public List<ICompatibleHidDevice> Controllers { get; private set; } = new();

    public void Start()
    {
        if (_inputSourceProcessor != null)
        {
            return;
        }

        foreach (var device in Controllers)
        {
            device.SourceDevice.OpenDevice();
        }

        _inputSourceProcessor = _serviceProvider.GetService<IInputSourceProcessor>();
        _inputSourceProcessor.OnOutputDeviceReportReceived += OnOutputDeviceReportReceived;
        _inputSourceProcessor.Start(this);
        OnAfterStartListening();
    }

    public void Stop()
    {
        if (_inputSourceProcessor == null)
        {
            return;
        }

        _inputSourceProcessor.OnOutputDeviceReportReceived -= OnOutputDeviceReportReceived;
        _inputSourceProcessor.Dispose();
        _inputSourceProcessor = null;
    }

    public void LoadInputSourceConfiguration()
    {
        SetInputSourceKey();

        var configurations = _inputSourceConfigurationService.GetInputSourceConfigurations(InputSourceKey);

        if (_inputSourceConfigurationService.GetCurrentGameRunning != null)
        {
            string currentGameRunning = _inputSourceConfigurationService.GetCurrentGameRunning();

            if (!string.IsNullOrWhiteSpace(currentGameRunning))
            {
                InputSourceConfiguration gameConfiguration =
                    configurations.SingleOrDefault(c => c.IsGameConfiguration && c.GameInfo.GameId == currentGameRunning);

                if (gameConfiguration != null)
                {
                    SetConfiguration(gameConfiguration);
                    return;
                }
            }
        }

        SetConfiguration(configurations.Single(c => !c.IsGameConfiguration));
    }

    private void SetConfiguration(InputSourceConfiguration configuration)
    {
        Configuration = configuration;

        foreach (var device in Controllers)
        {
            device.SetConfiguration(Configuration);
        }

        ReorderControllers();
        SetFinalReport();
        ConfigurationChanged?.Invoke(this, configuration);
    }

    public InputSourceFinalReport ProcessInputReport(ReadOnlySpan<byte> buffers)
    {
        int previousLength = 0;
        foreach (KeyValuePair<ICompatibleHidDevice, byte[]> controller in _controllerInputReportData)
        {
            ReadOnlySpan<byte> bytes = buffers.Slice(previousLength, controller.Value.Length);
            controller.Key.ProcessInputReport(bytes);
            previousLength = previousLength + controller.Value.Length - 1;

            if (controller.Key.MultiControllerConfigurationType == MultiControllerConfigurationType.None ||
                controller.Key.MultiControllerConfigurationType == MultiControllerConfigurationType.Left)
            {
                _finalReport.LeftThumbX = controller.Key.InputSourceReport.LeftThumbX;
                _finalReport.LeftThumbY = controller.Key.InputSourceReport.LeftThumbY;
                _finalReport.DPad = controller.Key.InputSourceReport.DPad;
                _finalReport.LeftShoulder = controller.Key.InputSourceReport.LeftShoulder;
                _finalReport.LeftTrigger = controller.Key.InputSourceReport.LeftTrigger;
                _finalReport.LeftThumb = controller.Key.InputSourceReport.LeftThumb;
                _finalReport.Share = controller.Key.InputSourceReport.Share;
                _finalReport.PS = controller.Key.InputSourceReport.PS;
            }

            if (controller.Key.MultiControllerConfigurationType == MultiControllerConfigurationType.None ||
                controller.Key.MultiControllerConfigurationType == MultiControllerConfigurationType.Right)
            {
                _finalReport.RightThumbX = controller.Key.InputSourceReport.RightThumbX;
                _finalReport.RightThumbY = controller.Key.InputSourceReport.RightThumbY;
                _finalReport.RightShoulder = controller.Key.InputSourceReport.RightShoulder;
                _finalReport.RightTrigger = controller.Key.InputSourceReport.RightTrigger;
                _finalReport.RightThumb = controller.Key.InputSourceReport.RightThumb;
                _finalReport.Cross = controller.Key.InputSourceReport.Cross;
                _finalReport.Square = controller.Key.InputSourceReport.Square;
                _finalReport.Triangle = controller.Key.InputSourceReport.Triangle;
                _finalReport.Circle = controller.Key.InputSourceReport.Circle;
                _finalReport.Options = controller.Key.InputSourceReport.Options;
            }
        }

        return _finalReport;
    }

    public byte[] ReadInputReport()
    {
        int previousLength = 0;
        foreach ((ICompatibleHidDevice device, byte[] reportBuffer) in _controllerInputReportData)
        {
            device.ReadInputReport(reportBuffer);

            Buffer.BlockCopy(reportBuffer, 0, _allReportBytes, previousLength, reportBuffer.Length);
            previousLength = previousLength + reportBuffer.Length - 1;
        }

        return _allReportBytes;
    }

    public void OnAfterStartListening()
    {
        foreach (var device in Controllers)
        {
            device.OnAfterStartListening();
        }
    }

    public void AddController(ICompatibleHidDevice controller)
    {
        if (Controllers.All(c => c.DeviceKey != controller.DeviceKey))
        {
            byte[] reportBuffer = new byte[controller.SourceDevice.InputReportByteLength];
            Controllers.Add(controller);
            _controllerInputReportData.Add(controller, reportBuffer);
            ReorderControllers();
            SetAllReportBytes();
            SetInputSourceKey();
        }
    }

    public void RemoveController(string instanceId)
    {
        var existing =
            Controllers.SingleOrDefault(c => c.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());

        if (existing == null)
        {
            return;
        }

        Controllers.Remove(existing);
        ReorderControllers();
        SetAllReportBytes();
        SetInputSourceKey();
    }

    public void SetPlayerNumberAndColor(int playerNumber)
    {
        Configuration.PlayerNumber = playerNumber;
        if (string.IsNullOrWhiteSpace(Configuration.CustomLightbar))
        {
            Configuration.LoadedLightbar = Configuration.PlayerNumber switch
            {
                1 => DefaultPlayerNumberColors.Player1,
                2 => DefaultPlayerNumberColors.Player2,
                3 => DefaultPlayerNumberColors.Player3,
                _ => DefaultPlayerNumberColors.Player4
            };
        }

        foreach (var device in Controllers)
        {
            device.OnAfterStartListening();
        }
    }

    public async Task DisconnectControllers()
    {
        var controllers = Controllers.Where(c => c.Connection == ConnectionType.Bluetooth).ToList();
        if (controllers.Any())
        {
            Stop();

            foreach (var device in controllers)
            {
                await device.DisconnectBTController();
            }
        }
    }

    public ICompatibleHidDevice GetControllerByInstanceId(string instanceId)
    {
        return Controllers.SingleOrDefault(c => c.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());
    }

    public ICompatibleHidDevice GetControllerByParentInstanceId(string instanceId)
    {
        return Controllers.SingleOrDefault(c => c.SourceDevice.ParentInstance.ToLower() == instanceId.ToLower());
    }

    private void ReorderControllers()
    {
        if (Configuration != null)
        {
            foreach (var device in Controllers)
            {
                int? configurationIndex = Configuration.Controllers
                    .SingleOrDefault(c => c.DeviceKey == device.DeviceKey)?.Index;
                device.Index = configurationIndex ?? 0;
            }
        }

        Controllers = Controllers.OrderBy(c => c.Index).ToList();
    }

    private void SetInputSourceKey()
    {
        InputSourceKey = string.Join(InputSourceConfigurationService.MultiControllerKeySeparator, Controllers.OrderBy(c => c.Index).Select(c => c.DeviceKey));
    }

    private void SetAllReportBytes()
    {
        _allReportBytes = new byte[_controllerInputReportData.Values.Select(b => b.Length).Sum()];
    }

    private void SetFinalReport()
    {
        InputSourceFinalReport finalReport = new();
        if (Controllers.Count == 1)
        {
            ICompatibleHidDevice controller = Controllers.Single();
            finalReport.LThumbAxisScaleInputType = controller.InputSourceReport.AxisScaleInputType;
            finalReport.RThumbAxisScaleInputType = controller.InputSourceReport.AxisScaleInputType;
        }
        else
        {
            ICompatibleHidDevice leftController = Controllers.Single(c =>
                c.MultiControllerConfigurationType == MultiControllerConfigurationType.Left);
            ICompatibleHidDevice rightController = Controllers.Single(c =>
                c.MultiControllerConfigurationType == MultiControllerConfigurationType.Right);
            finalReport.LThumbAxisScaleInputType = leftController.InputSourceReport.AxisScaleInputType;
            finalReport.RThumbAxisScaleInputType = rightController.InputSourceReport.AxisScaleInputType;
        }

        _finalReport = finalReport;
    }

    private void OnOutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        foreach (var device in Controllers)
        {
            device.OutputDeviceReportReceived(outputDeviceReport);
        }
    }

    public static class DefaultPlayerNumberColors
    {
        public static string Player1 = Colors.Blue.ToString();
        public static string Player2 = Colors.Red.ToString();
        public static string Player3 = Colors.Green.ToString();
        public static string Player4 = Colors.Purple.ToString();
    }
}