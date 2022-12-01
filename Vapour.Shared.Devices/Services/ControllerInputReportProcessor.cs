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
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public sealed class ControllerInputReportProcessor : IControllerInputReportProcessor
{
    private static readonly Meter Meter = new(TracingSources.AssemblyName);

    private static readonly Counter<int> ReportsReadCounter =
        Meter.CreateCounter<int>("reports-read", description: "The number of reports read.");

    private static readonly Counter<int> ReportsProcessedCounter =
        Meter.CreateCounter<int>("reports-processed", description: "The number of reports processed.");

    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

    /// <summary>
    ///     Managed input report array.
    /// </summary>
    private readonly byte[] _inputReportArray;

    private readonly Channel<byte[]> _inputReportChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
    {
        SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true
    });

    private IProfile _currentProfile;

    private Thread _inputReportProcessor;
    private Thread _inputReportReader;
    private CancellationTokenSource _inputReportToken = new();

    public ControllerInputReportProcessor(ICompatibleHidDevice hidDevice, IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        Logger = Services.GetRequiredService<ILogger<ControllerInputReportProcessor>>();
        HidDevice = hidDevice;

        ushort inputReportSize = ((HidDevice)HidDevice.SourceDevice).Capabilities.InputReportByteLength;

        _inputReportArray = new byte[inputReportSize];
    }

    private IServiceProvider Services { get; }
    private ILogger<ControllerInputReportProcessor> Logger { get; }
    public ICompatibleHidDevice HidDevice { get; }
    public bool IsInputReportAvailableInvoked { get; set; } = true;
    public event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

    public void StartInputReportReader()
    {
        if (_inputReportToken.Token.IsCancellationRequested)
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
    }

    public void StopInputReportReader()
    {
        _inputReportToken.Cancel();
        _inputReportToken.Dispose();

        _inputReportReader.Join();
        _inputReportProcessor.Join();
    }

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
                byte[] buffer = await _inputReportChannel.Reader.ReadAsync();

                //
                // Implementation depends on derived object
                // 
                HidDevice.ProcessInputReport(buffer);

                ReportsProcessedCounter.Add(1);

                if (IsInputReportAvailableInvoked)
                {
                    InputReportAvailable?.Invoke(HidDevice, HidDevice.InputReport);
                }
            }
        }
        catch (VigemBusNotFoundException)
        {
            StopInputReportReader();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report processing");
        }
    }

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
                HidDevice.SourceDevice.ReadInputReport(_inputReportArray);

                ReportsReadCounter.Add(1);

                await _inputReportChannel.Writer.WriteAsync(_inputReportArray, _inputReportToken.Token);
            }
        }
        catch (ObjectDisposedException)
        {
            if (!_inputReportToken.IsCancellationRequested)
            {
                _inputReportToken.Cancel();
            }

            HidDevice.FireDisconnected();
        }
        catch (HidDeviceException win32)
        {
            if (win32.ErrorCode != (uint)WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
            {
                throw;
            }

            _inputReportToken.Cancel();

            HidDevice.FireDisconnected();
        }
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
            if (win32Exception.NativeErrorCode != (int)WIN32_ERROR.ERROR_NO_SUCH_DEVICE)
            {
                throw;
            }

            _inputReportToken.Cancel();

            HidDevice.FireDisconnected();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report reading");
        }
    }
}