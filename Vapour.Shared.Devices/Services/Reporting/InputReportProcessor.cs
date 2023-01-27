using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

using Windows.Win32.Foundation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.Drivers.WinUSB;
using Nefarius.ViGEm.Client.Exceptions;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

/// <summary>
///     Handles reading input reports from a compatible input device and dispatches them.
/// </summary>
internal sealed class InputReportProcessor : IInputReportProcessor
{
    private readonly ICustomActionProcessor _customActionProcessor;
    private static readonly Meter Meter = new(TracingSources.AssemblyName);

    private static readonly Counter<int> ReportsReadCounter =
        Meter.CreateCounter<int>("reports-read", description: "The number of reports read.");

    private static readonly Counter<int> ReportsProcessedCounter =
        Meter.CreateCounter<int>("reports-processed", description: "The number of reports processed.");

    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);
    
    private Channel<byte[]> _inputReportChannel;
    private Channel<InputSourceFinalReport> _customActionChannel;
    
    private Thread _inputReportProcessor;
    private Thread _inputReportReader;
    private Thread _customActionThread;

    private CancellationTokenSource _inputReportToken;

    public InputReportProcessor(ILogger<InputReportProcessor> logger, ICustomActionProcessor customActionProcessor)
    {
        _customActionProcessor = customActionProcessor;
        Logger = logger;
    }
    
    private ILogger<InputReportProcessor> Logger { get; }
    public IInputSource InputSource { get; private set; }
    public bool IsInputReportAvailableInvoked { get; set; } = true;
    public bool IsProcessing { get; private set; }
    public event Action<IInputSource, InputSourceFinalReport> InputReportAvailable;

    public void SetInputSource(IInputSource inputSource)
    {
        InputSource = inputSource;
    }
    
    /// <inheritdoc />
    public void StartInputReportReader()
    {
        if (IsProcessing)
        {
            return;
        }

        _inputReportChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
        });

        _customActionChannel = Channel.CreateUnbounded<InputSourceFinalReport>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
        });

        if (_inputReportToken == null || _inputReportToken.Token.IsCancellationRequested)
        {
            _inputReportToken = new CancellationTokenSource();
        }

        _inputReportReader = new Thread(ReadInputReportLoop)
        {
            Priority = ThreadPriority.AboveNormal, IsBackground = true
        };
        _inputReportReader.Start();

        _inputReportProcessor = new Thread(ProcessInputReportLoop)
        {
            Priority = ThreadPriority.AboveNormal, IsBackground = true
        };
        _inputReportProcessor.Start();

        _customActionThread = new Thread(ProcessCustomActionLoop)
        {
            Priority = ThreadPriority.AboveNormal,
            IsBackground = true
        };
        _customActionThread.Start();

        IsProcessing = true;
    }

    /// <inheritdoc />
    public void StopInputReportReader()
    {
        if (!IsProcessing)
        {
            return;
        }

        _inputReportToken.Cancel();

        _inputReportReader.Join();
        _inputReportProcessor.Join();
        _customActionThread.Join();

        _inputReportChannel.Writer.Complete();
        _customActionChannel.Writer.Complete();

        _inputReportToken.Dispose();
        _inputReportToken = null;

        IsProcessing = false;
    }

    /// <summary>
    ///     Background thread dequeueing and dispatching read input reports.
    /// </summary>
    private async void ProcessInputReportLoop()
    {
        Logger.LogDebug("Started input report processing thread");

        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(CompatibleHidDevice)}:{nameof(ProcessInputReportLoop)}",
            ActivityKind.Consumer, string.Empty);

        try
        {
            while (!_inputReportToken.IsCancellationRequested)
            {
                byte[] buffer = await _inputReportChannel.Reader.ReadAsync(_inputReportToken.Token);

                //
                // Implementation depends on derived object
                // 
                var report = InputSource.ProcessInputReport(buffer);

                ReportsProcessedCounter.Add(1);

                if (IsInputReportAvailableInvoked)
                {
                    InputReportAvailable?.Invoke(InputSource, report);
                }

                _customActionChannel.Writer.WriteAsync(report);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Input report processing thread stopped");
        }
        // TODO: possibly handle this somewhere else
        catch (VigemBusNotFoundException)
        {
            StopInputReportReader();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report processing");
        }
    }

    /// <summary>
    ///     Background thread reading input reports from the device.
    /// </summary>
    private async void ReadInputReportLoop()
    {
        Logger.LogDebug("Started input report reading thread");

        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(CompatibleHidDevice)}:{nameof(ReadInputReportLoop)}",
            ActivityKind.Producer, string.Empty);

        try
        {
            while (!_inputReportToken.IsCancellationRequested)
            {
                var reportData = InputSource.ReadInputReport();

                ReportsReadCounter.Add(1);

                await _inputReportChannel.Writer.WriteAsync(reportData, _inputReportToken.Token);
            }
        }
        // general cancellation case
        catch (TaskCanceledException)
        {
            if (!_inputReportToken.IsCancellationRequested)
            {
                _inputReportToken.Cancel();
            }
        }
        // general cancellation case
        catch (ObjectDisposedException)
        {
            if (!_inputReportToken.IsCancellationRequested)
            {
                _inputReportToken.Cancel();
            }
        }
        // HID-API errors
        catch (HidDeviceException win32)
        {
            if (win32.ErrorCode != (uint)WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
            {
                throw;
            }

            _inputReportToken.Cancel();
        }
        // WinUSB errors
        catch (USBException ex)
        {
            Exception apiException = ex.InnerException;

            if (apiException is null)
            {
                throw;
            }

            if (apiException.InnerException is not Win32Exception win32Exception)
            {
                throw;
            }

            if (win32Exception.NativeErrorCode == (int)WIN32_ERROR.ERROR_SEM_TIMEOUT)
            {
                throw new HidDeviceException("Device communication timed out.",
                    (WIN32_ERROR)win32Exception.NativeErrorCode);
            }

            // expected error when the device got surprise-removed (unplugged)
            //if (win32Exception.NativeErrorCode != (int)WIN32_ERROR.ERROR_NO_SUCH_DEVICE)
            //{
            //    throw;
            //}

            _inputReportToken.Cancel();
        }
        // unaccounted error
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report reading");
        }
    }

    private async void ProcessCustomActionLoop()
    {
        Logger.LogDebug("Started custom action report processing thread");

        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(CompatibleHidDevice)}:{nameof(ProcessInputReportLoop)}",
            ActivityKind.Consumer, string.Empty);

        try
        {
            while (!_inputReportToken.IsCancellationRequested)
            {
                var inputSourceReport = await _customActionChannel.Reader.ReadAsync(_inputReportToken.Token);
                _customActionProcessor.ProcessReport(InputSource, inputSourceReport);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Input report processing thread stopped");
        }
        // TODO: possibly handle this somewhere else
        catch (VigemBusNotFoundException)
        {
            StopInputReportReader();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report processing");
        }
    }
}