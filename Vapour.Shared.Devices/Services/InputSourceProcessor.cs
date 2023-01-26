using Microsoft.Extensions.DependencyInjection;

using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.Services;
public class InputSourceProcessor : IInputSourceProcessor
{
    private readonly IInputReportProcessor _inputReportProcessor;
    private readonly IOutputDeviceProcessor _outputDeviceProcessor;
    private readonly List<IOutputReportProcessor> _outputReportProcessors = new();
    private readonly IServiceProvider _serviceProvider;

    public InputSourceProcessor(IInputReportProcessor inputReportProcessor,
        IOutputDeviceProcessor outputDeviceProcessor,
        IServiceProvider serviceProvider)
    {
        _inputReportProcessor = inputReportProcessor;
        _outputDeviceProcessor = outputDeviceProcessor;
        _serviceProvider = serviceProvider;

        _outputDeviceProcessor.OnOutputDeviceReportReceived += _outputDeviceProcessor_OnOutputDeviceReportReceived;
    }

    public event Action<OutputDeviceReport> OnOutputDeviceReportReceived;

    public void Start(IInputSource inputSource)
    {
        _inputReportProcessor.SetInputSource(inputSource);
        _outputDeviceProcessor.SetInputSource(inputSource);
        foreach (var device in inputSource.GetControllers())
        {
            var outputReportProcessor = _serviceProvider.GetService<IOutputReportProcessor>();
            // ReSharper disable once PossibleNullReferenceException
            outputReportProcessor.SetDevice(device);
            _outputReportProcessors.Add(outputReportProcessor);
            outputReportProcessor.Start();
        }
        _outputDeviceProcessor.StartOutputProcessing(_inputReportProcessor);
        _inputReportProcessor.StartInputReportReader();
    }

    public void Stop()
    {
        _inputReportProcessor.StopInputReportReader();
        _outputDeviceProcessor.StopOutputProcessing();
        foreach (var outputReportProcessor in _outputReportProcessors)
        {
            outputReportProcessor.Stop();
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private void _outputDeviceProcessor_OnOutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        OnOutputDeviceReportReceived?.Invoke(outputDeviceReport);
    }
}
