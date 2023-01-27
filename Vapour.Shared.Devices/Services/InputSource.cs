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
    private byte[] _allReportBytes;
    private Dictionary<ICompatibleHidDevice, byte[]> _controllers = new();
    private InputSourceFinalReport _finalReport = new();
    private IInputSourceProcessor _inputSourceProcessor;

    public InputSource(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public event EventHandler<InputSourceConfiguration> ConfigurationChanged;
    public InputSourceConfiguration Configuration { get; private set; }

    public string InputSourceKey { get; private set; }

    public void Start()
    {
        if (_inputSourceProcessor != null)
        {
            return;
        }

        _inputSourceProcessor = _serviceProvider.GetService<IInputSourceProcessor>();
        _inputSourceProcessor.OnOutputDeviceReportReceived += OnOutputDeviceReportReceived;
        // ReSharper disable once PossibleNullReferenceException
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

    public void SetConfiguration(InputSourceConfiguration configuration)
    {
        Configuration = configuration;

        foreach ((ICompatibleHidDevice device, _) in _controllers)
        {
            device.SetConfiguration(Configuration);
        }

        ReorderControllers();
        SetFinalReport();
        SetInputSourceKey();
        ConfigurationChanged?.Invoke(this, configuration);
    }

    public InputSourceFinalReport ProcessInputReport(ReadOnlySpan<byte> buffers)
    {
        int previousLength = 0;
        foreach (KeyValuePair<ICompatibleHidDevice, byte[]> controller in _controllers)
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
        foreach ((ICompatibleHidDevice device, byte[] reportBuffer) in _controllers)
        {
            device.ReadInputReport(reportBuffer);

            Buffer.BlockCopy(reportBuffer, 0, _allReportBytes, previousLength, reportBuffer.Length);
            previousLength = previousLength + reportBuffer.Length - 1;
        }

        return _allReportBytes;
    }

    public void OnAfterStartListening()
    {
        foreach ((ICompatibleHidDevice device, _) in _controllers)
        {
            device.OnAfterStartListening();
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
            byte[] reportBuffer = new byte[controller.SourceDevice.InputReportByteLength];
            _controllers.Add(controller, reportBuffer);
            ReorderControllers();
            SetAllReportBytes();
            SetInputSourceKey();
        }
    }

    public void RemoveController(string instanceId)
    {
        ICompatibleHidDevice existing =
            _controllers.Keys.SingleOrDefault(c => c.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());

        if (existing == null)
        {
            return;
        }

        _controllers.Remove(existing);
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

        foreach ((ICompatibleHidDevice device, _) in _controllers)
        {
            device.RefreshConfiguration();
        }
    }

    private void ReorderControllers()
    {
        if (Configuration != null)
        {
            foreach ((ICompatibleHidDevice device, _) in _controllers)
            {
                int? configurationIndex = Configuration.Controllers
                    .SingleOrDefault(c => c.DeviceKey == device.DeviceKey)?.Index;
                device.Index = configurationIndex ?? 0;
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

    private void SetFinalReport()
    {
        InputSourceFinalReport finalReport = new();
        if (_controllers.Keys.Count == 1)
        {
            ICompatibleHidDevice controller = _controllers.Keys.Single();
            finalReport.LThumbAxisScaleInputType = controller.InputSourceReport.AxisScaleInputType;
            finalReport.RThumbAxisScaleInputType = controller.InputSourceReport.AxisScaleInputType;
        }
        else
        {
            ICompatibleHidDevice leftController = _controllers.Keys.Single(c =>
                c.MultiControllerConfigurationType == MultiControllerConfigurationType.Left);
            ICompatibleHidDevice rightController = _controllers.Keys.Single(c =>
                c.MultiControllerConfigurationType == MultiControllerConfigurationType.Right);
            finalReport.LThumbAxisScaleInputType = leftController.InputSourceReport.AxisScaleInputType;
            finalReport.RThumbAxisScaleInputType = rightController.InputSourceReport.AxisScaleInputType;
        }

        _finalReport = finalReport;
    }

    private void OnOutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        foreach ((ICompatibleHidDevice device, _) in _controllers)
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