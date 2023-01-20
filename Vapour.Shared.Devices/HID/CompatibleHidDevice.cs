using System.Diagnostics;
using System.Net.NetworkInformation;



using Microsoft.Extensions.Logging;

using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Represents a <see cref="HidDevice" /> which is a compatible input device.
/// </summary>
public abstract partial class CompatibleHidDevice : ICompatibleHidDevice
{
    private readonly List<DeviceInfo> _deviceInfos;
    private const string SonyWirelessAdapterFriendlyName = "DUALSHOCK®4 USB Wireless Adaptor";

    protected static readonly Guid UsbDeviceClassGuid = Guid.Parse("{88BAE032-5A81-49f0-BC3D-A4FF138216D6}");

    private static readonly Guid UsbCompositeDeviceClassGuid = Guid.Parse("{36fc9e60-c465-11cf-8056-444553540000}");

    private static readonly Guid BluetoothDeviceClassGuid = Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}");

    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

    /// <summary>
    ///     The connection type (wire, wireless).
    /// </summary>
    private ConnectionType? _connection;

    private bool _disposed;

    protected CompatibleHidDevice(ILogger<CompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
    {
        _deviceInfos = deviceInfos;
        Logger = logger;
        KnownDevices = deviceInfos.Where(i => i.DeviceType == InputDeviceType).ToList();
    }
    
    /// <summary>
    ///     Logger instance.
    /// </summary>
    protected ILogger<CompatibleHidDevice> Logger { get; }

    /// <summary>
    ///     The parsed input report. Depends on device type.
    /// </summary>
    public abstract InputSourceReport InputSourceReport { get; }
    public List<DeviceInfo> KnownDevices { get; }
    protected abstract InputDeviceType InputDeviceType { get; }

    /// <inheritdoc />
    public IHidDevice SourceDevice { get; private set; }
    
    /// <inheritdoc />
    public ConnectionType? Connection => _connection ??= GetConnectionType();

    /// <inheritdoc />
    public PhysicalAddress Serial { get; protected set; }

    /// <inheritdoc />
    public string SerialString => Serial?.ToString();

    public string DeviceKey
    {
        get
        {
            return SerialString;
        }
    }
    
    /// <inheritdoc />
    public bool IsFiltered { get; set; }
    
    public InputSourceConfiguration CurrentConfiguration { get; private set; }
    public DeviceInfo CurrentDeviceInfo { get; private set; }
    public int Index { get; set; }

    public void Initialize(IHidDevice hidDevice, DeviceInfo deviceInfo)
    {
        SourceDevice = hidDevice;
        CurrentDeviceInfo = deviceInfo;
        
        if (Connection == ConnectionType.Unknown)
        {
            throw new ArgumentException("Couldn't determine connection type.");
        }

        if (CurrentDeviceInfo.FeatureSet != CompatibleHidDeviceFeatureSet.Default)
        {
            Logger.LogInformation("Controller {Device} is using custom feature set {Feature}",
                this, CurrentDeviceInfo.FeatureSet);
        }

        //
        // Open handle
        // 
        SourceDevice.OpenDevice();

        OnInitialize();
    }

    protected abstract void OnInitialize();

    public virtual int ReadInputReport(Span<byte> buffer)
    {
        return SourceDevice.ReadInputReport(buffer);
    }

    public void SetConfiguration(InputSourceConfiguration configuration)
    {
        InputSourceConfiguration oldConfiguration = CurrentConfiguration;
        CurrentConfiguration = configuration;
        OnConfigurationChanged(oldConfiguration, configuration);
    }

    /// <inheritdoc />
    public event Action<ICompatibleHidDevice> Disconnected;

    public void FireDisconnected()
    {
        Disconnected?.Invoke(this);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    ///     Process the input report read from the device.
    /// </summary>
    /// <param name="input">The raw report buffer.</param>
    public abstract void ProcessInputReport(ReadOnlySpan<byte> input);

    public virtual void OnAfterStartListening()
    {
    }

    protected virtual void OnConfigurationChanged(InputSourceConfiguration oldProfile,
        InputSourceConfiguration newProfile)
    {
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
    ///     Invokes a GET_FEATURE request to query for the device serial (MAC address).
    /// </summary>
    /// <remarks>
    ///     If the request fails in an expected way or isn't supported at all, an auto-generated value based on the device
    ///     path will be calculated and returned.
    /// </remarks>
    /// <param name="featureId">The report ID of the GET_REPORT request.</param>
    /// <returns>The MAC address of the device.</returns>
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
    /// <returns>The calculated <see cref="PhysicalAddress" />.</returns>
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
            }

            _disposed = true;
        }
    }

    public override string ToString()
    {
        return Serial is null
            ? $"{SourceDevice.DisplayName} via {Connection}"
            : $"{SourceDevice.DisplayName} ({Serial}) via {Connection}";
    }
}