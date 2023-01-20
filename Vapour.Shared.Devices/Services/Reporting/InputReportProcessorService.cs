using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services.Reporting;

/// <summary>
///     Processes input reports for <see cref="IInputSource" />s.
/// </summary>
internal sealed class InputReportProcessorService : IInputReportProcessorService
{
    private readonly IFilterService _filterService;
    private readonly List<ReportProcessingItem> _processingItems = new();
    private readonly IServiceProvider _serviceProvider;

    public InputReportProcessorService(
        ILogger<InputReportProcessorService> logger,
        IServiceProvider serviceProvider,
        IFilterService filterService)
    {
        _serviceProvider = serviceProvider;
        _filterService = filterService;
    }

    public void StartProcessing(IInputSource inputSource)
    {
        if (inputSource.Configuration.OutputDeviceType != OutputDeviceType.None && _processingItems.All(i => i.InputSource.InputSourceKey != inputSource.InputSourceKey))
        {
            var inputReportProcessor = new InputReportProcessor(_serviceProvider);
            var outputProcessor = new OutputProcessor(inputReportProcessor, _serviceProvider);

            inputReportProcessor.SetDevice(inputSource);
            outputProcessor.SetInputSource(inputSource);

            _processingItems.Add(new ReportProcessingItem
            {
                InputSource = inputSource,
                InputReportProcessor = inputReportProcessor,
                OutputProcessor = outputProcessor
            });

            inputReportProcessor.StartInputReportReader();
            inputSource.OnAfterStartListening();
            outputProcessor.StartOutputProcessing();
        }
    }

    public void StopProcessing(IInputSource inputSource)
    {
        var existing = _processingItems.SingleOrDefault(c => c.InputSource.InputSourceKey == inputSource.InputSourceKey);

        if (existing != null)
        {
            existing.InputReportProcessor.StopInputReportReader();
            existing.OutputProcessor.StopOutputProcessing();
            _processingItems.Remove(existing);
        }
    }

    private class ReportProcessingItem
    {
        public IInputSource InputSource { get; set; }
        public IInputReportProcessor InputReportProcessor { get; set; }
        public IOutputProcessor OutputProcessor { get; set; }
    }
}