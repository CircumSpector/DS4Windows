using Microsoft.Extensions.DependencyInjection;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Reporting;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services;

public sealed class InputSourceProcessor : IInputSourceProcessor
{
    private readonly IInputReportProcessor _inputReportProcessor;
    private readonly IOutputDeviceProcessor _outputDeviceProcessor;
    private readonly List<IOutputReportProcessor> _outputReportProcessors = new();
    private readonly IServiceProvider _serviceProvider;

    public InputSourceProcessor(
        IInputReportProcessor inputReportProcessor,
        IOutputDeviceProcessor outputDeviceProcessor,
        IServiceProvider serviceProvider
        )
    {
        _inputReportProcessor = inputReportProcessor;
        _outputDeviceProcessor = outputDeviceProcessor;
        _serviceProvider = serviceProvider;
    }

    public event Action<OutputDeviceReport> OnOutputDeviceReportReceived;
    public event Action<ICustomAction> OnCustomActionDetected;

    public void Start(IInputSource inputSource)
    {
        _inputReportProcessor.SetInputSource(inputSource);
        _outputDeviceProcessor.SetInputSource(inputSource);

        foreach (ICompatibleHidDevice device in inputSource.Controllers)
        {
            IOutputReportProcessor outputReportProcessor =
                _serviceProvider.GetRequiredService<IOutputReportProcessor>();

            outputReportProcessor.SetDevice(device);
            _outputReportProcessors.Add(outputReportProcessor);
            outputReportProcessor.Start();
        }

        _outputDeviceProcessor.OnOutputDeviceReportReceived += OutputDeviceProcessorOnOutputDeviceReportReceived;
        _outputDeviceProcessor.StartOutputProcessing(_inputReportProcessor);
        _inputReportProcessor.OnCustomActionDetected += OnCustomAction;
        _inputReportProcessor.StartInputReportReader();
    }

    public void Stop()
    {
        _outputDeviceProcessor.OnOutputDeviceReportReceived -= OutputDeviceProcessorOnOutputDeviceReportReceived;
        _inputReportProcessor.OnCustomActionDetected -= OnCustomAction;

        _inputReportProcessor.StopInputReportReader();
        _outputDeviceProcessor.StopOutputProcessing();

        foreach (IOutputReportProcessor outputReportProcessor in _outputReportProcessors)
        {
            outputReportProcessor.Stop();
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private void OutputDeviceProcessorOnOutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        OnOutputDeviceReportReceived?.Invoke(outputDeviceReport);
    }

    private void OnCustomAction(ICustomAction customAction)
    {
        OnCustomActionDetected?.Invoke(customAction);
    }
}
