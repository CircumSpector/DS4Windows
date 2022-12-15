using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Output;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Processes input reports for <see cref="ICompatibleHidDevice" />s.
/// </summary>
internal sealed class ControllerInputReportProcessorService : IControllerInputReportProcessorService
{
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly IControllerFilterService _controllerFilterService;
    private readonly Dictionary<string, IControllerInputReportProcessor> _controllerInputReportProcessors;
    private readonly Dictionary<string, IOutputProcessor> _outputProcessors;
    private readonly IServiceProvider _serviceProvider;

    public ControllerInputReportProcessorService(
        ILogger<ControllerInputReportProcessorService> logger,
        IServiceProvider serviceProvider,
        IControllerConfigurationService controllerConfigurationService,
        IControllerFilterService controllerFilterService)
    {
        _serviceProvider = serviceProvider;
        _controllerConfigurationService = controllerConfigurationService;
        _controllerFilterService = controllerFilterService;
        _controllerInputReportProcessors = new Dictionary<string, IControllerInputReportProcessor>();
        _outputProcessors = new Dictionary<string, IOutputProcessor>();

        _controllerConfigurationService.OnActiveConfigurationChanged +=
            OnActiveConfigurationChanged;
    }

    public void StartProcessing(ICompatibleHidDevice device)
    {
        device.SetConfiguration(_controllerConfigurationService.GetActiveControllerConfiguration(device.SerialString));

        IControllerInputReportProcessor inputReportProcessor;
        IOutputProcessor outputProcessor;
        string controllerKey = device.SerialString;
        if (!_controllerInputReportProcessors.ContainsKey(controllerKey))
        {
            inputReportProcessor = new ControllerInputReportProcessor(_serviceProvider);
            outputProcessor = new OutputProcessor(inputReportProcessor, _serviceProvider);
            _controllerInputReportProcessors.Add(controllerKey, inputReportProcessor);
            _outputProcessors.Add(controllerKey, outputProcessor);
        }
        else
        {
            inputReportProcessor = _controllerInputReportProcessors[controllerKey];
            outputProcessor = _outputProcessors[controllerKey];
        }

        inputReportProcessor.SetDevice(device);
        outputProcessor.SetDevice(device);

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
            IOutputProcessor outputProcessor = _outputProcessors[controllerKey];
            outputProcessor.StopOutputProcessing();
        }
    }

    private void OnActiveConfigurationChanged(object sender,
        ControllerConfigurationChangedEventArgs e)
    {
        if (_controllerInputReportProcessors.ContainsKey(e.ControllerKey))
        {
            ICompatibleHidDevice existingDevice = _controllerInputReportProcessors[e.ControllerKey].HidDevice;

            if (_controllerFilterService.FilterUnfilterIfNeeded(existingDevice))
            {
                return;
            }

            //otherwise just restart that faster way
            StopProcessing(existingDevice);
            //without this going from one output device type to another does not always work
            Thread.Sleep(500);
            StartProcessing(existingDevice);
        }
    }
}