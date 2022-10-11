using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Threading.Channels;

using Windows.Win32.Foundation;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.Drivers.WinUSB;
using Nefarius.Utilities.DeviceManagement.PnP;
using Nefarius.ViGEm.Client.Exceptions;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Represents a <see cref="HidDevice" /> which is a compatible input device.
/// </summary>
public abstract partial class CompatibleHidDevice : ICompatibleHidDevice
{
    private const string SonyWirelessAdapterFriendlyName = "DUALSHOCK®4 USB Wireless Adaptor";

    private static readonly Meter Meter = new(TracingSources.DevicesAssemblyActivitySourceName);

    private static readonly Counter<int> ReportsReadCounter =
        Meter.CreateCounter<int>("reports-read", description: "The number of reports read.");

    private static readonly Counter<int> ReportsProcessedCounter =
        Meter.CreateCounter<int>("reports-processed", description: "The number of reports processed.");

    protected static readonly Guid UsbDeviceClassGuid = Guid.Parse("{88BAE032-5A81-49f0-BC3D-A4FF138216D6}");
    private static readonly Guid UsbCompositeDeviceClassGuid = Guid.Parse("{36fc9e60-c465-11cf-8056-444553540000}");
    private static readonly Guid BluetoothDeviceClassGuid = Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}");

    private readonly ActivitySource _coreActivity = new(TracingSources.DevicesAssemblyActivitySourceName);

    private readonly Channel<byte[]> _inputReportChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
    {
        SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true
    });

    /// <summary>
    ///     The connection type (wire, wireless).
    /// </summary>
    private ConnectionType? _connection;

    private bool _disposed;

    /// <summary>
    ///     Managed input report array.
    /// </summary>
    protected byte[] _inputReportArray;

    private Thread _inputReportProcessor;

    private Thread _inputReportReader;

    private CancellationTokenSource _inputReportToken = new();

    protected CompatibleHidDevice(InputDeviceType deviceType, IHidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider)
    {
        SourceDevice = source;

        Services = serviceProvider;
        DeviceType = deviceType;
        FeatureSet = featureSet;

        //
        // Grab new logger
        // 
        Logger = Services.GetRequiredService<ILogger<CompatibleHidDevice>>();

        if (Connection == ConnectionType.Unknown)
        {
            throw new ArgumentException("Couldn't determine connection type.");
        }

        if (FeatureSet != CompatibleHidDeviceFeatureSet.Default)
        {
            Logger.LogInformation("Controller {Device} is using custom feature set {Feature}",
                this, FeatureSet);
        }

        //
        // Open handle
        // 
        SourceDevice.OpenDevice();
    }

    /// <summary>
    ///     Service provider for injected services.
    /// </summary>
    protected IServiceProvider Services { get; }

    /// <summary>
    ///     Logger instance.
    /// </summary>
    protected ILogger<CompatibleHidDevice> Logger { get; }

    /// <summary>
    ///     The parsed input report. Depends on device type.
    /// </summary>
    protected abstract CompatibleHidDeviceInputReport InputReport { get; }

    /// <inheritdoc />
    public IHidDevice SourceDevice { get; }

    /// <summary>
    ///     The <see cref="InputDeviceType" /> of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    public InputDeviceType DeviceType { get; set; }

    /// <summary>
    ///     The <see cref="ConnectionType" /> of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    public ConnectionType? Connection => _connection ??= GetConnectionType();

    /// <inheritdoc />
    public PhysicalAddress Serial { get; protected init; }

    /// <summary>
    ///     The <see cref="CompatibleHidDeviceFeatureSet" /> flags this device has been created with.
    /// </summary>
    public CompatibleHidDeviceFeatureSet FeatureSet { get; }

    /// <summary>
    ///     Gets whether <see cref="InputReportAvailable" /> will be invoked in the processing loop.
    /// </summary>
    public bool IsInputReportAvailableInvoked { get; set; } = true;

    /// <summary>
    ///     Fired when this device has been disconnected/unplugged.
    /// </summary>
    public event Action<ICompatibleHidDevice> Disconnected;

    /// <summary>
    ///     Fired when a new input report is read for further processing.
    /// </summary>
    public event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    ///     Determine <see cref="ConnectionType" /> of this device.
    /// </summary>
    /// <returns>The discovered <see cref="ConnectionType" />.</returns>
    private ConnectionType GetConnectionType()
    {
        try
        {
            PnPDevice device = PnPDevice.GetDeviceByInterfaceId(SourceDevice.Path);

            //
            // Walk up device tree
            // 
            while (device is not null)
            {
                Guid deviceClass = device.GetProperty<Guid>(DevicePropertyKey.Device_ClassGuid);

                //
                // Parent is Bluetooth device
                // 
                if (Equals(deviceClass, BluetoothDeviceClassGuid))
                {
                    return ConnectionType.Bluetooth;
                }

                //
                // USB or via Sony Wireless Adapter
                // 
                if (Equals(deviceClass, UsbCompositeDeviceClassGuid))
                {
                    //
                    // Check if we find the composite audio device
                    // 
                    List<string> children = device.GetProperty<string[]>(DevicePropertyKey.Device_Children).ToList();

                    if (children.Count != 2)
                    {
                        return ConnectionType.Usb;
                    }

                    PnPDevice audioDevice = PnPDevice.GetDeviceByInstanceId(children.First());

                    string friendlyName = audioDevice.GetProperty<string>(DevicePropertyKey.Device_FriendlyName);

                    if (string.IsNullOrEmpty(friendlyName))
                    {
                        return ConnectionType.Usb;
                    }

                    //
                    // Match friendly name reported by Wireless Adapter
                    // 
                    return friendlyName.Equals(SonyWirelessAdapterFriendlyName, StringComparison.OrdinalIgnoreCase)
                        ? ConnectionType.SonyWirelessAdapter
                        : ConnectionType.Usb;
                }

                string parentId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);

                if (parentId is null)
                {
                    break;
                }

                device = PnPDevice.GetDeviceByInstanceId(parentId);
            }

            return ConnectionType.Unknown;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Connection type lookup failed");

            return ConnectionType.Unknown;
        }
    }

    /// <summary>
    ///     Process the input report read from the device.
    /// </summary>
    /// <param name="input">The raw report buffer.</param>
    protected abstract void ProcessInputReport(ReadOnlySpan<byte> input);

    /// <summary>
    ///     Start the asynchronous input report reading logic.
    /// </summary>
    protected void StartInputReportReader()
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

    /// <summary>
    ///     Stop the asynchronous input report reading logic.
    /// </summary>
    private void StopInputReportReader()
    {
        _inputReportToken.Cancel();

        _inputReportReader.Join();
        _inputReportProcessor.Join();
    }

    /// <summary>
    ///     Continuous input report processing thread.
    /// </summary>
    protected async void ProcessInputReportLoop()
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
                ProcessInputReport(buffer);

                ReportsProcessedCounter.Add(1);

                if (IsInputReportAvailableInvoked)
                {
                    InputReportAvailable?.Invoke(this, InputReport);
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

    /// <summary>
    ///     Continuous input report reader thread.
    /// </summary>
    protected async void ReadInputReportLoop()
    {
        Logger.LogDebug("Started input report reading thread");

        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(CompatibleHidDevice)}:{nameof(ReadInputReportLoop)}",
            ActivityKind.Producer, string.Empty);

        try
        {
            while (!_inputReportToken.IsCancellationRequested)
            {
                SourceDevice.ReadInputReport(_inputReportArray);

                ReportsReadCounter.Add(1);

                await _inputReportChannel.Writer.WriteAsync(_inputReportArray, _inputReportToken.Token);
            }
        }
        catch (HidDeviceException win32)
        {
            if (win32.ErrorCode != (uint)WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
            {
                throw;
            }

            _inputReportToken.Cancel();

            Disconnected?.Invoke(this);
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

            if (win32Exception.NativeErrorCode != (int)WIN32_ERROR.ERROR_NO_SUCH_DEVICE)
            {
                throw;
            }

            _inputReportToken.Cancel();

            Disconnected?.Invoke(this);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report reading");
        }
    }

    /// <summary>
    ///     Invokes a GET_FEATURE request to query for the device serial (MAC address).
    /// </summary>
    /// <param name="featureId">The report ID of the GET_REPORT request.</param>
    /// <returns>The MAC address of the device.</returns>
    [CanBeNull]
    protected PhysicalAddress ReadSerial(byte featureId)
    {
        switch (SourceDevice.Service)
        {
            case InputDeviceService.HidUsb:
            case InputDeviceService.WinUsb:
                if (((HidDevice)SourceDevice).Capabilities.InputReportByteLength == 64)
                {
                    Span<byte> buffer = stackalloc byte[64];
                    buffer[0] = featureId;

                    if (SourceDevice.ReadFeatureData(buffer))
                    {
                        Span<byte> serialBytes = buffer.Slice(1, 6);
                        serialBytes.Reverse();
                        return new PhysicalAddress(serialBytes.ToArray());
                    }
                }
                else
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(SourceDevice.SerialNumberString))
                        {
                            return PhysicalAddress.Parse(SourceDevice.SerialNumberString.ToUpper());
                        }
                    }
                    catch
                    {
                        return GenerateFakeHwSerial();
                    }
                }

                break;
            default:
                return GenerateFakeHwSerial();
        }

        return GenerateFakeHwSerial();
    }

    /// <summary>
    ///     Generate <see cref="Serial" /> from <see cref="HidDevice.Path" />.
    /// </summary>
    /// <returns></returns>
    private PhysicalAddress GenerateFakeHwSerial()
    {
        string address = string.Empty;

        // Substring: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030} -> \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#
        int endPos = SourceDevice.Path.LastIndexOf('{');
        if (endPos < 0)
        {
            endPos = SourceDevice.Path.Length;
        }

        // String array: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001# -> [0]=\\?\hidvid_054c, [1]=pid_09cc, [2]=mi_037, [3]=1f882A25, [4]=0, [5]=0001
        string[] devPathItems = SourceDevice.Path.Substring(0, endPos).Replace("#", "").Replace("-", "")
            .Replace("{", "")
            .Replace("}", "").Split('&');

        address = devPathItems.Length switch
        {
            >= 3 => devPathItems[^3].ToUpper() // 1f882A25
                    + devPathItems[^2].ToUpper() // 0
                    + devPathItems[^1].TrimStart('0').ToUpper(),
            // Device and usb hub and port identifiers missing in devicePath string. Fallback to use vendor and product ID values and 
            // take a number from the last part of the devicePath. Hopefully the last part is a usb port number as it usually should be.
            >= 1 => SourceDevice.VendorId.ToString("X4") + SourceDevice.ProductId.ToString("X4") +
                    devPathItems[^1].TrimStart('0').ToUpper(),
            _ => address
        };

        if (string.IsNullOrEmpty(address))
        {
            return PhysicalAddress.Parse(address);
        }

        address = address.PadRight(12, '0');
        address =
            $"{address[0]}{address[1]}:{address[2]}{address[3]}:{address[4]}{address[5]}:{address[6]}{address[7]}:{address[8]}{address[9]}:{address[10]}{address[11]}";

        return PhysicalAddress.Parse(address);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _coreActivity.Dispose();

                _inputReportToken?.Dispose();
            }

            _disposed = true;
        }
    }

    public override string ToString()
    {
        return $"{SourceDevice.DisplayName} ({Serial}) via {Connection}";
    }
}