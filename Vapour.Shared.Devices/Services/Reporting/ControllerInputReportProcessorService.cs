using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services.Reporting;

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
    }

    public void StartProcessing(ICompatibleHidDevice device)
    {
        device.ConfigurationChanged += Device_ConfigurationChanged;

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
        hidDevice.ConfigurationChanged -= Device_ConfigurationChanged;
        string controllerKey = hidDevice.SerialString;
        if (_controllerInputReportProcessors.ContainsKey(controllerKey))
        {
            IControllerInputReportProcessor inputReportProcessor = _controllerInputReportProcessors[controllerKey];
            inputReportProcessor.StopInputReportReader();
            IOutputProcessor outputProcessor = _outputProcessors[controllerKey];
            outputProcessor.StopOutputProcessing();
        }
    }

    private void Device_ConfigurationChanged(object sender, EventArgs e)
    {
        var controller = (ICompatibleHidDevice)sender;
        if (_controllerInputReportProcessors.ContainsKey(controller.SerialString))
        {
            ICompatibleHidDevice existingDevice = _controllerInputReportProcessors[controller.SerialString].HidDevice;

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