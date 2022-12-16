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
    private readonly IControllerFilterService _controllerFilterService;
    private readonly List<ReportProcessingItem> _processingItems = new();
    private readonly IServiceProvider _serviceProvider;

    public ControllerInputReportProcessorService(
        ILogger<ControllerInputReportProcessorService> logger,
        IServiceProvider serviceProvider,
        IControllerFilterService controllerFilterService)
    {
        _serviceProvider = serviceProvider;
        _controllerFilterService = controllerFilterService;
    }

    public void StartProcessing(ICompatibleHidDevice device)
    {
        device.ConfigurationChanged += Device_ConfigurationChanged;

        if (device.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None && _processingItems.All(i => i.Controller.SerialString != device.SerialString))
        {
            var inputReportProcessor = new ControllerInputReportProcessor(_serviceProvider);
            var outputProcessor = new OutputProcessor(inputReportProcessor, _serviceProvider);

            inputReportProcessor.SetDevice(device);
            outputProcessor.SetDevice(device);

            _processingItems.Add(new ReportProcessingItem
            {
                Controller = device,
                InputReportProcessor = inputReportProcessor,
                OutputProcessor = outputProcessor
            });

            inputReportProcessor.StartInputReportReader();
            device.OnAfterStartListening();
            outputProcessor.StartOutputProcessing();
        }
    }

    public void StopProcessing(ICompatibleHidDevice hidDevice)
    {
        hidDevice.ConfigurationChanged -= Device_ConfigurationChanged;

        var existing = _processingItems.SingleOrDefault(c => c.Controller.SerialString == hidDevice.SerialString);

        if (existing != null)
        {
            existing.InputReportProcessor.StopInputReportReader();
            existing.OutputProcessor.StopOutputProcessing();
            _processingItems.Remove(existing);
        }
    }

    private void Device_ConfigurationChanged(object sender, EventArgs e)
    {
        var controller = (ICompatibleHidDevice)sender;
        if (_controllerFilterService.FilterUnfilterIfNeeded(controller))
        {
            //stopping and starting will happen as a result of filter action needed
            return;
        }

        var existing = _processingItems.SingleOrDefault(c => c.Controller.SerialString == controller.SerialString);
        if (existing != null)
        {
            StopProcessing(existing.Controller);
            Thread.Sleep(500);
            StartProcessing(existing.Controller);
        }
        else
        {
            StartProcessing(controller);
        }
    }

    private class ReportProcessingItem
    {
        public ICompatibleHidDevice Controller { get; set; }
        public IControllerInputReportProcessor InputReportProcessor { get; set; }
        public IOutputProcessor OutputProcessor { get; set; }
    }
}