using System.Collections.ObjectModel;
using System.Diagnostics;

using Windows.Win32.Devices.HumanInterfaceDevice;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.WinUSB;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public interface IWinUsbDeviceEnumeratorService
{
    /// <summary>
    ///     List of currently available (connected) HID devices.
    /// </summary>
    ReadOnlyObservableCollection<HidDeviceOverWinUsb> ConnectedDevices { get; }

    /// <summary>
    ///     Gets fired when a new HID device has been detected.
    /// </summary>
    event Action<HidDeviceOverWinUsb> DeviceArrived;

    /// <summary>
    ///     Gets fired when an existing HID device has been removed.
    /// </summary>
    event Action<HidDeviceOverWinUsb> DeviceRemoved;

    /// <summary>
    ///     Refreshes <see cref="ConnectedDevices" />. This clears out the list and repopulates is.
    /// </summary>
    void EnumerateDevices();

    /// <summary>
    ///     Drops all devices from <see cref="ConnectedDevices" />.
    /// </summary>
    void ClearDevices();
}

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public class WinUsbDeviceEnumeratorService : IWinUsbDeviceEnumeratorService
{
    private static readonly Guid DeviceInterfaceGuid = Guid.Parse("{F72FE0D4-CBCB-407d-8814-9ED673D0DD6B}");

    private readonly ObservableCollection<HidDeviceOverWinUsb> _connectedDevices;
    private readonly ActivitySource _coreActivity = new(TracingSources.DevicesAssemblyActivitySourceName);

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDeviceNotificationListener _deviceNotificationListener;

    private readonly ILogger<WinUsbDeviceEnumeratorService> _logger;

    public WinUsbDeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
        ILogger<WinUsbDeviceEnumeratorService> logger)
    {
        _deviceNotificationListener = deviceNotificationListener;
        _logger = logger;

        _deviceNotificationListener.RegisterDeviceArrived(DeviceNotificationListenerOnDeviceArrived,
            DeviceInterfaceGuid);
        _deviceNotificationListener.RegisterDeviceRemoved(DeviceNotificationListenerOnDeviceRemoved,
            DeviceInterfaceGuid);

        _connectedDevices = new ObservableCollection<HidDeviceOverWinUsb>();

        ConnectedDevices = new ReadOnlyObservableCollection<HidDeviceOverWinUsb>(_connectedDevices);
    }

    /// <inheritdoc />
    public event Action<HidDeviceOverWinUsb> DeviceArrived;

    /// <inheritdoc />
    public event Action<HidDeviceOverWinUsb> DeviceRemoved;

    /// <inheritdoc />
    public ReadOnlyObservableCollection<HidDeviceOverWinUsb> ConnectedDevices { get; }

    /// <inheritdoc />
    public void EnumerateDevices()
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(EnumerateDevices)}");

        int deviceIndex = 0;

        _connectedDevices.Clear();

        while (Devcon.FindByInterfaceGuid(DeviceInterfaceGuid, out string path, out _, deviceIndex++))
        {
            HidDeviceOverWinUsb entry = CreateNewHidDeviceOverWinUsb(path);

            _logger.LogInformation("Discovered WinUSB device {Device}", entry);

            _connectedDevices.Add(entry);
        }
    }

    /// <inheritdoc />
    public void ClearDevices()
    {
        foreach (HidDeviceOverWinUsb connectedDevice in ConnectedDevices.ToList())
        {
            RemoveDevice(connectedDevice.Path);
        }
    }

    /// <summary>
    ///     Create new <see cref="HidDeviceOverWinUsb" /> and initialize basic properties.
    /// </summary>
    /// <param name="path">The symbolic link path of the device instance.</param>
    /// <returns>The new <see cref="HidDeviceOverWinUsb" />.</returns>
    private HidDeviceOverWinUsb CreateNewHidDeviceOverWinUsb(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(CreateNewHidDeviceOverWinUsb)}");

        activity?.SetTag("Path", path);

        USBDevice winUsbDevice = USBDevice.GetSingleDeviceByPath(path);
        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(path);

        //
        // Try to get friendly display name (not always there)
        // 
        string friendlyName = device.GetProperty<string>(DevicePropertyKey.Device_FriendlyName);
        string parentId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);

        //
        // Grab product string from device if property is missing
        // 
        if (string.IsNullOrEmpty(friendlyName))
        {
            friendlyName = winUsbDevice.Descriptor.PathName;
        }

        return new HidDeviceOverWinUsb
        {
            Path = path,
            InstanceId = device.InstanceId.ToUpper(),
            Description = device.GetProperty<string>(DevicePropertyKey.Device_DeviceDesc),
            DisplayName = friendlyName,
            ParentInstance = parentId,
            ManufacturerString = winUsbDevice.Descriptor.Manufacturer,
            ProductString = winUsbDevice.Descriptor.Product,
            SerialNumberString = winUsbDevice.Descriptor.SerialNumber,
            Attributes = new HIDD_ATTRIBUTES()
            {
                VendorID = (ushort)winUsbDevice.Descriptor.VID,
                ProductID =  (ushort)winUsbDevice.Descriptor.PID
            },
            Capabilities = new HIDP_CAPS()
            {
                Usage = HidDevice.HidUsageGamepad,
                // TODO: finish me!
            }
        };
    }

    private void DeviceNotificationListenerOnDeviceArrived(DeviceEventArgs args)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceArrived)}");

        string symLink = args.SymLink;
        activity?.SetTag("Path", symLink);

        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(symLink);

        _logger.LogInformation("WinUSB Device {Instance} ({Path}) arrived",
            device.InstanceId, symLink);

        HidDeviceOverWinUsb entry = CreateNewHidDeviceOverWinUsb(symLink);

        if (!_connectedDevices.Contains(entry))
        {
            _connectedDevices.Add(entry);
        }

        DeviceArrived?.Invoke(entry);
    }

    private void DeviceNotificationListenerOnDeviceRemoved(DeviceEventArgs args)
    {
        RemoveDevice(args.SymLink);
    }

    private void RemoveDevice(string symLink)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(RemoveDevice)}");

        activity?.SetTag("Path", symLink);

        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(symLink, DeviceLocationFlags.Phantom);

        _logger.LogInformation("HID Device {Instance} ({Path}) removed",
            device.InstanceId, symLink);

        HidDeviceOverWinUsb entry = new()
        {
            Path = symLink, InstanceId = device.InstanceId.ToUpper()
        };

        if (_connectedDevices.Contains(entry))
        {
            _connectedDevices.Remove(entry);
        }

        DeviceRemoved?.Invoke(entry);
    }
}