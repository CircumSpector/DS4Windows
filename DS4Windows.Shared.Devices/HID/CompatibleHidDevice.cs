using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DS4Windows.Shared.Common.Telemetry;
using DS4Windows.Shared.Common.Util;
using DS4Windows.Shared.Emulator.ViGEmGen1.Types;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;
using PInvoke;

namespace DS4Windows.Shared.Devices.HID;

/// <summary>
///     Represents a <see cref="HidDevice" /> which is a compatible input device.
/// </summary>
public abstract partial class CompatibleHidDevice : HidDevice, ICompatibleHidDevice
{
    protected const string SonyWirelessAdapterFriendlyName = "DUALSHOCK®4 USB Wireless Adaptor";
    protected static readonly Guid UsbDeviceClassGuid = Guid.Parse("{88BAE032-5A81-49f0-BC3D-A4FF138216D6}");

    protected static readonly Guid UsbCompositeDeviceClassGuid =
        Guid.Parse("{36fc9e60-c465-11cf-8056-444553540000}");

    protected static readonly Guid BluetoothDeviceClassGuid = Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}");

    protected readonly ActivitySource CoreActivity = new(TracingSources.DevicesAssemblyActivitySourceName);

    protected readonly Channel<byte[]> InputReportChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = true,
        AllowSynchronousContinuations = true
    });
  
    private ConnectionType? connection;

    /// <summary>
    ///     Managed input report array.
    /// </summary>
    protected byte[] InputReportArray;

    /// <summary>
    ///     Unmanaged input report buffer.
    /// </summary>
    protected IntPtr InputReportBuffer = IntPtr.Zero;

    private Thread inputReportProcessor;

    private Thread inputReportReader;

    private CancellationTokenSource inputReportToken = new();

    protected CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider)
    {
        //
        // This makes this instance independent
        // 
        source.DeepCloneTo(this);

        Services = serviceProvider;
        DeviceType = deviceType;
        FeatureSet = featureSet;

        //
        // Grab new logger
        // 
        Logger = Services.GetRequiredService<ILogger<CompatibleHidDevice>>();

        if (Connection == ConnectionType.Unknown)
            throw new ArgumentException("Couldn't determine connection type.");

        if (FeatureSet != CompatibleHidDeviceFeatureSet.Default)
            Logger.LogInformation("Controller {Device} is using custom feature set {Feature}",
                this, FeatureSet);

        //
        // Open handle
        // 
        OpenDevice();
    }

    /// <summary>
    ///     The <see cref="InputDeviceType" /> of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    public InputDeviceType DeviceType { get; set; }

    /// <summary>
    ///     The <see cref="ConnectionType" /> of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    public ConnectionType? Connection
    {
        get { return connection ??= GetConnectionType(); }
    }

    /// <summary>
    ///     The serial number (MAC address) of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    public PhysicalAddress Serial { get; protected set; }

    /// <summary>
    ///     The <see cref="CompatibleHidDeviceFeatureSet" /> flags this device has been created with.
    /// </summary>
    public CompatibleHidDeviceFeatureSet FeatureSet { get; }

    /// <summary>
    ///     Service provider for injected services.
    /// </summary>
    protected IServiceProvider Services { get; }

    /// <summary>
    ///     Logger instance.
    /// </summary>
    protected ILogger<CompatibleHidDevice> Logger { get; }

    /// <summary>
    ///     Metrics of how many input reports were read in a second.
    /// </summary>
    public int ReportsPerSecondRead { get; private set; }

    /// <summary>
    ///     Metrics of how many input reports were processed in a second.
    /// </summary>
    public int ReportsPerSecondProcessed { get; private set; }

    /// <summary>
    ///     Gets whether <see cref="InputReportAvailable" /> will be invoked in the processing loop.
    /// </summary>
    public bool IsInputReportAvailableInvoked { get; set; }

    /// <summary>
    ///     The parsed input report. Depends on device type.
    /// </summary>
    protected abstract CompatibleHidDeviceInputReport InputReport { get; }

    /// <summary>
    ///     Determine <see cref="ConnectionType" /> of this device.
    /// </summary>
    /// <returns>The discovered <see cref="ConnectionType" />.</returns>
    private ConnectionType GetConnectionType()
    {
        try
        {
            var device = PnPDevice.GetDeviceByInterfaceId(Path);

            //
            // Walk up device tree
            // 
            while (device is not null)
            {
                var deviceClass = device.GetProperty<Guid>(DevicePropertyDevice.ClassGuid);

                //
                // Parent is Bluetooth device
                // 
                if (Equals(deviceClass, BluetoothDeviceClassGuid))
                    return ConnectionType.Bluetooth;

                //
                // USB or via Sony Wireless Adapter
                // 
                if (Equals(deviceClass, UsbCompositeDeviceClassGuid))
                {
                    //
                    // Check if we find the composite audio device
                    // 
                    var children = device.GetProperty<string[]>(DevicePropertyDevice.Children).ToList();

                    if (children.Count != 2)
                        return ConnectionType.Usb;

                    var audioDevice = PnPDevice.GetDeviceByInstanceId(children.First());

                    var friendlyName = audioDevice.GetProperty<string>(DevicePropertyDevice.FriendlyName);

                    if (string.IsNullOrEmpty(friendlyName))
                        return ConnectionType.Usb;

                    //
                    // Match friendly name reported by Wireless Adapter
                    // 
                    return friendlyName.Equals(SonyWirelessAdapterFriendlyName, StringComparison.OrdinalIgnoreCase)
                        ? ConnectionType.SonyWirelessAdapter
                        : ConnectionType.Usb;
                }

                var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

                if (parentId is null)
                    break;

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
    ///     Fired when this device has been disconnected/unplugged.
    /// </summary>
    public event Action<ICompatibleHidDevice> Disconnected;

    /// <summary>
    ///     Fired when a new input report is read for further processing.
    /// </summary>
    public event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

    /// <summary>
    ///     Process the input report read from the device.
    /// </summary>
    /// <param name="inputReport">The raw report buffer.</param>
    protected abstract void ProcessInputReport(byte[] inputReport);

    /// <summary>
    ///     Start the asynchronous input report reading logic.
    /// </summary>
    protected void StartInputReportReader()
    {
        if (inputReportToken.Token.IsCancellationRequested)
            inputReportToken = new CancellationTokenSource();

        inputReportReader = new Thread(ReadInputReportLoop)
        {
            Priority = ThreadPriority.AboveNormal,
            IsBackground = true
        };
        inputReportReader.Start();

        inputReportProcessor = new Thread(ProcessInputReportLoop)
        {
            Priority = ThreadPriority.AboveNormal,
            IsBackground = true
        };
        inputReportProcessor.Start();
    }

    /// <summary>
    ///     Stop the asynchronous input report reading logic.
    /// </summary>
    protected void StopInputReportReader()
    {
        inputReportToken.Cancel();

        inputReportReader.Join();
        inputReportProcessor.Join();
    }

    /// <summary>
    ///     Continuous input report processing thread.
    /// </summary>
    protected async void ProcessInputReportLoop()
    {
        Logger.LogDebug("Started input report processing thread");

        Stopwatch sw = new();

        var counter = 0;

        try
        {
            sw.Start();

            while (!inputReportToken.IsCancellationRequested)
            {
                using var activity = CoreActivity.StartActivity(
                    $"{nameof(CompatibleHidDevice)}:{nameof(ProcessInputReportLoop)}",
                    ActivityKind.Consumer, string.Empty);

                activity?.SetTag(nameof(ReportsPerSecondProcessed), ReportsPerSecondProcessed);

                if (sw.Elapsed >= TimeSpan.FromSeconds(1))
                {
                    ReportsPerSecondProcessed = counter;
                    counter = 0;
                    sw.Restart();
                }

                var buffer = await InputReportChannel.Reader.ReadAsync();

                //
                // Implementation depends on derived object
                // 
                ProcessInputReport(buffer);

                if (IsInputReportAvailableInvoked)
                    InputReportAvailable?.Invoke(this, InputReport);

                counter++;
            }
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

        Stopwatch sw = new();

        var counter = 0;

        try
        {
            sw.Start();

            while (!inputReportToken.IsCancellationRequested)
            {
                using var activity = CoreActivity.StartActivity(
                    $"{nameof(CompatibleHidDevice)}:{nameof(ReadInputReportLoop)}",
                    ActivityKind.Producer, string.Empty);

                activity?.SetTag(nameof(ReportsPerSecondRead), ReportsPerSecondRead);

                if (sw.Elapsed >= TimeSpan.FromSeconds(1))
                {
                    ReportsPerSecondRead = counter;
                    counter = 0;
                    sw.Restart();
                }

                ReadInputReport(InputReportBuffer, InputReportArray.Length, out var written);

                Marshal.Copy(InputReportBuffer, InputReportArray, 0, written);

                await InputReportChannel.Writer.WriteAsync(InputReportArray, inputReportToken.Token);

                counter++;
            }
        }
        catch (Win32Exception win32)
        {
            if (win32.NativeErrorCode != Win32ErrorCode.ERROR_DEVICE_NOT_CONNECTED) throw;

            inputReportToken.Cancel();

            Disconnected?.Invoke(this);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fatal failure in input report reading");
        }
    }

    [CanBeNull]
    protected PhysicalAddress ReadSerial(byte featureId)
    {
        PhysicalAddress serial = null;

        if (Capabilities.InputReportByteLength == 64)
        {
            var buffer = new byte[64];
            buffer[0] = featureId;

            if (ReadFeatureData(buffer))
                serial = PhysicalAddress.Parse(
                    $"{buffer[6]:X02}:{buffer[5]:X02}:{buffer[4]:X02}:{buffer[3]:X02}:{buffer[2]:X02}:{buffer[1]:X02}"
                );
        }
        else
        {
            try
            {
                if (!string.IsNullOrEmpty(SerialNumberString))
                    serial = PhysicalAddress.Parse(SerialNumberString.ToUpper());
            }
            catch
            {
                serial = GenerateFakeHwSerial();
            }
        }

        return serial;
    }

    /// <summary>
    ///     Generate <see cref="Serial" /> from <see cref="HidDevice.Path" />.
    /// </summary>
    /// <returns></returns>
    protected PhysicalAddress GenerateFakeHwSerial()
    {
        var address = string.Empty;

        // Substring: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030} -> \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#
        var endPos = Path.LastIndexOf('{');
        if (endPos < 0)
            endPos = Path.Length;

        // String array: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001# -> [0]=\\?\hidvid_054c, [1]=pid_09cc, [2]=mi_037, [3]=1f882A25, [4]=0, [5]=0001
        var devPathItems = Path.Substring(0, endPos).Replace("#", "").Replace("-", "").Replace("{", "")
            .Replace("}", "").Split('&');

        address = devPathItems.Length switch
        {
            >= 3 => devPathItems[^3].ToUpper() // 1f882A25
                    + devPathItems[^2].ToUpper() // 0
                    + devPathItems[^1].TrimStart('0').ToUpper(),
            // Device and usb hub and port identifiers missing in devicePath string. Fallback to use vendor and product ID values and 
            // take a number from the last part of the devicePath. Hopefully the last part is a usb port number as it usually should be.
            >= 1 => Attributes.VendorId.ToString("X4") + Attributes.ProductId.ToString("X4") +
                    devPathItems[^1].TrimStart('0').ToUpper(),
            _ => address
        };

        if (string.IsNullOrEmpty(address)) return PhysicalAddress.Parse(address);

        address = address.PadRight(12, '0');
        address =
            $"{address[0]}{address[1]}:{address[2]}{address[3]}:{address[4]}{address[5]}:{address[6]}{address[7]}:{address[8]}{address[9]}:{address[10]}{address[11]}";

        return PhysicalAddress.Parse(address);
    }

    public override void Dispose()
    {
        StopInputReportReader();

        if (InputReportBuffer != IntPtr.Zero) Marshal.FreeHGlobal(InputReportBuffer);

        base.Dispose();
    }

    public override string ToString()
    {
        return $"{DisplayName} ({Serial}) via {Connection}";
    }
}