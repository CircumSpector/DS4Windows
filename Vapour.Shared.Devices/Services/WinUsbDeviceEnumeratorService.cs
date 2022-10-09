using System.Collections.ObjectModel;
using System.Diagnostics;

using JetBrains.Annotations;

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

    /// <summary>
    ///     Maps VID/PID pair to endpoint addresses where input and output reports are expected.
    /// </summary>
    private static readonly Dictionary<HidDeviceOverWinUsbIdentification, HidDeviceOverWinUsbEndpoints>
        DeviceOverWinUsbEndpointsMap =
            new()
            {
                {
                    new HidDeviceOverWinUsbIdentification { VendorId = 0x054C, ProductId = 0x05C5 },
                    new HidDeviceOverWinUsbEndpoints
                    {
                        InterruptInEndpointAddress = 0x81, InterruptOutEndpointAddress = 0x01
                    }
                }
            };

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
    [CanBeNull]
    private HidDeviceOverWinUsb CreateNewHidDeviceOverWinUsb(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(CreateNewHidDeviceOverWinUsb)}");

        activity?.SetTag("Path", path);

        using USBDevice winUsbDevice = USBDevice.GetSingleDeviceByPath(path);

        HidDeviceOverWinUsbIdentification key =
            HidDeviceOverWinUsbIdentification.FromDescriptor(winUsbDevice.Descriptor);

        if (!DeviceOverWinUsbEndpointsMap.ContainsKey(key))
        {
            return null;
        }

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

        HidDeviceOverWinUsbEndpoints identification = DeviceOverWinUsbEndpointsMap[key];

        return new HidDeviceOverWinUsb(
            path,
            identification.InterruptInEndpointAddress,
            identification.InterruptOutEndpointAddress
        )
        {
            InstanceId = device.InstanceId.ToUpper(),
            Description = device.GetProperty<string>(DevicePropertyKey.Device_DeviceDesc),
            DisplayName = friendlyName,
            ParentInstance = parentId
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

        _logger.LogInformation("WinUSB Device {Instance} ({Path}) removed",
            device.InstanceId, symLink);

        HidDeviceOverWinUsb entry = _connectedDevices.FirstOrDefault(entry => entry.InstanceId == device.InstanceId);

        if (entry is null)
        {
            return;
        }

        DeviceRemoved?.Invoke(entry);
        _connectedDevices.Remove(entry);
    }

    private class HidDeviceOverWinUsbIdentification : IEquatable<HidDeviceOverWinUsbIdentification>
    {
        public ushort VendorId { get; init; }

        public ushort ProductId { get; init; }

        public bool Equals(HidDeviceOverWinUsbIdentification other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return VendorId == other.VendorId && ProductId == other.ProductId;
        }

        public static HidDeviceOverWinUsbIdentification FromDescriptor(USBDeviceDescriptor descriptor)
        {
            return new HidDeviceOverWinUsbIdentification
            {
                VendorId = (ushort)descriptor.VID, ProductId = (ushort)descriptor.PID
            };
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((HidDeviceOverWinUsbIdentification)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VendorId, ProductId);
        }

        public static bool operator ==(HidDeviceOverWinUsbIdentification left, HidDeviceOverWinUsbIdentification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HidDeviceOverWinUsbIdentification left, HidDeviceOverWinUsbIdentification right)
        {
            return !Equals(left, right);
        }
    }

    private class HidDeviceOverWinUsbEndpoints
    {
        public byte InterruptInEndpointAddress { get; init; }

        public byte InterruptOutEndpointAddress { get; init; }
    }
}