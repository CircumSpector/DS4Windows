using System.Collections.ObjectModel;
using System.Diagnostics;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Identinator;
using Nefarius.Drivers.WinUSB;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public class WinUsbDeviceEnumeratorService : IHidDeviceEnumeratorService<HidDeviceOverWinUsb>
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
                    // Collective Minds Strike Pack Eliminator Mod Pack - PlayStation 4
                    new HidDeviceOverWinUsbIdentification { VendorId = 0x054C, ProductId = 0x05C5 },
                    new HidDeviceOverWinUsbEndpoints
                    {
                        InterruptInEndpointAddress = 0x81, InterruptOutEndpointAddress = 0x01
                    }
                },
                {
                    // Sony DualShock 4 Rev1
                    new HidDeviceOverWinUsbIdentification { VendorId = 0x054C, ProductId = 0x05C4 },
                    new HidDeviceOverWinUsbEndpoints
                    {
                        InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
                    }
                },
                {
                    // Sony DualSense
                    new HidDeviceOverWinUsbIdentification { VendorId = 0x054C, ProductId = 0x0CE6 },
                    new HidDeviceOverWinUsbEndpoints
                    {
                        InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
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
            FilterDriver.FilteredDeviceInterfaceId);
        _deviceNotificationListener.RegisterDeviceRemoved(DeviceNotificationListenerOnDeviceRemoved,
            FilterDriver.FilteredDeviceInterfaceId);

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

        while (Devcon.FindByInterfaceGuid(FilterDriver.FilteredDeviceInterfaceId, out string path, out _,
                   deviceIndex++))
        {
            string service = PnPDevice.GetDeviceByInterfaceId(path)
                .GetProperty<string>(DevicePropertyKey.Device_Service);

            if (service is null || !service.ToUpper().Equals("WINUSB"))
            {
                continue;
            }

            HidDeviceOverWinUsb entry = CreateNewHidDeviceOverWinUsb(path);

            if (entry is null)
            {
                continue;
            }

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

        try
        {
            using USBDevice winUsbDevice = USBDevice.GetSingleDeviceByPath(path);

            HidDeviceOverWinUsbIdentification key =
                HidDeviceOverWinUsbIdentification.FromDescriptor(winUsbDevice.Descriptor);

            // Filter out devices we don't know about
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

            winUsbDevice.Dispose();

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
        catch (USBException ex)
        {
            _logger.LogWarning(ex, "Couldn't access WinUSB device ({Path})", path);
            return null;
        }
    }

    private void DeviceNotificationListenerOnDeviceArrived(DeviceEventArgs args)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceArrived)}");

        string symLink = args.SymLink;
        activity?.SetTag("Path", symLink);

        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(symLink);

        string service = device.GetProperty<string>(DevicePropertyKey.Device_Service);

        if (service is null || !service.ToUpper().Equals("WINUSB"))
        {
            return;
        }

        _logger.LogInformation("WinUSB Device {Instance} ({Path}) arrived",
            device.InstanceId, symLink);

        HidDeviceOverWinUsb entry = CreateNewHidDeviceOverWinUsb(symLink);

        if (entry is null)
        {
            return;
        }

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

        HidDeviceOverWinUsb entry = _connectedDevices.FirstOrDefault(entry => entry.InstanceId.ToUpper() == device.InstanceId.ToUpper());

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

        public override string ToString()
        {
            return $"VID: 0x{VendorId:X4}, PID: 0x{ProductId:X4}";
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