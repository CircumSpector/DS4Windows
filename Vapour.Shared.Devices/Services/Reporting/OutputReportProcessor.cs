using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;
public class OutputReportProcessor : IOutputReportProcessor
{
    private readonly ILogger<IOutputReportProcessor> _logger;
    private static readonly Meter Meter = new(TracingSources.AssemblyName);
    private Thread _outputReportProcessorThread;
    private CancellationTokenSource _outputReportCancellationToken;
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);
    private static readonly Counter<int> OutputReportsSentCounter =
        Meter.CreateCounter<int>("output-reports-sent", description: "The number of output reports sent.");

    private ICompatibleHidDevice _device;

    public OutputReportProcessor(ILogger<IOutputReportProcessor> logger)
    {
        _logger = logger;
    }
    
    public bool IsProcessing { get; set; }

    public void SetDevice(ICompatibleHidDevice device)
    {
        _device = device;
    }

    public void Start()
    {
        if (IsProcessing)
        {
            return;
        }

        if (_outputReportCancellationToken == null || _outputReportCancellationToken.Token.IsCancellationRequested)
        {
            _outputReportCancellationToken = new CancellationTokenSource();
        }

        _outputReportProcessorThread = new Thread(SendOutputReportLoop)
        {
            Priority = ThreadPriority.AboveNormal,
            IsBackground = true
        };
        _outputReportProcessorThread.Start();

        IsProcessing = true;
    }

    public void Stop()
    {
        if (!IsProcessing)
        {
            return;
        }

        _outputReportCancellationToken.Cancel();
        _outputReportProcessorThread.Join();
        _outputReportCancellationToken.Dispose();
        _outputReportCancellationToken = null;

        IsProcessing = false;
    }

    private async void SendOutputReportLoop()
    {
        _logger.LogDebug("Started output report sending thread");

        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(CompatibleHidDevice)}:{nameof(SendOutputReportLoop)}",
            ActivityKind.Consumer, string.Empty);

        try
        {
            while (!_outputReportCancellationToken.IsCancellationRequested)
            {
                var buffer = await _device.ReadOutputReport(_outputReportCancellationToken.Token);
                _device.SourceDevice.WriteOutputReportViaInterrupt(buffer, 500);
                OutputReportsSentCounter.Add(1);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Output report sending thread stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal failure in output report sending");
        }
    }
}
