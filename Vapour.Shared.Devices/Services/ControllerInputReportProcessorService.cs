using Microsoft.Extensions.Logging;

using Nefarius.ViGEm.Client;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Output;

namespace Vapour.Shared.Devices.Services;

public sealed class ControllerInputReportProcessorService : IControllerInputReportProcessorService
{
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly Dictionary<string, IControllerInputReportProcessor> _controllerInputReportProcessors;
    private readonly Dictionary<string, IOutputProcessor> _outputProcessors;
    private readonly IServiceProvider _serviceProvider;

    public ControllerInputReportProcessorService(
        ILogger<ControllerInputReportProcessorService> logger,
        IServiceProvider serviceProvider,
        IControllerConfigurationService controllerConfigurationService)
    {
        _serviceProvider = serviceProvider;
        _controllerConfigurationService = controllerConfigurationService;
        _controllerInputReportProcessors = new Dictionary<string, IControllerInputReportProcessor>();
        _outputProcessors = new Dictionary<string, IOutputProcessor>();

        _controllerConfigurationService.OnActiveConfigurationChanged += _controllerConfigurationService_OnActiveConfigurationChanged;
    }

    public void StartProcessing(ICompatibleHidDevice device)
    {
        device.SetConfiguration(_controllerConfigurationService.GetActiveControllerConfiguration(device.SerialString));

        IControllerInputReportProcessor inputReportProcessor;
        IOutputProcessor outputProcessor;
        string controllerKey = device.SerialString;
        if (!_controllerInputReportProcessors.ContainsKey(controllerKey))
        {
            inputReportProcessor = new ControllerInputReportProcessor(device, _serviceProvider);
            outputProcessor = new OutputProcessor(device, inputReportProcessor, _serviceProvider);
            _controllerInputReportProcessors.Add(controllerKey, inputReportProcessor);
            _outputProcessors.Add(controllerKey, outputProcessor);
        }
        else
        {
            inputReportProcessor = _controllerInputReportProcessors[controllerKey];
            outputProcessor = _outputProcessors[controllerKey];
        }

        if (device.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None)
        {
            inputReportProcessor.StartInputReportReader();
            device.OnAfterStartListening();
            outputProcessor.StartOutputProcessing();
        }
    }

    public void StopProcessing(ICompatibleHidDevice hidDevice)
    {
        string controllerKey = hidDevice.SerialString;
        if (_controllerInputReportProcessors.ContainsKey(controllerKey))
        {
            IControllerInputReportProcessor inputReportProcessor = _controllerInputReportProcessors[controllerKey];
            inputReportProcessor.StopInputReportReader();
            var outputProcessor = _outputProcessors[controllerKey];
            outputProcessor.StopOutputProcessing();
        }
    }

    private void _controllerConfigurationService_OnActiveConfigurationChanged(object sender, ControllerConfigurationChangedEventArgs e)
    {
        if (_controllerInputReportProcessors.ContainsKey(e.ControllerKey))
        {
            var existingDevice = _controllerInputReportProcessors[e.ControllerKey].HidDevice;
            StopProcessing(existingDevice);
            StartProcessing(existingDevice);
        }
    }
}