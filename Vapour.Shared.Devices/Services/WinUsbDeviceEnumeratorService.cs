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
///     Single point of truth of states for all connected and handled WinUSB devices.
/// </summary>
internal class WinUsbDeviceEnumeratorService : IHidDeviceEnumeratorService<HidDeviceOverWinUsb>
{
    private readonly ObservableCollection<HidDeviceOverWinUsb> _connectedDevices;
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

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

        while (Devcon.FindByInterfaceGuid(FilterDriver.FilteredDeviceInterfaceId, out string path,
                   out string instanceId,
                   deviceIndex++))
        {
            string service = PnPDevice.GetDeviceByInterfaceId(path)
                .GetProperty<string>(DevicePropertyKey.Device_Service);

            // skip those with unexpected service
            if (service is null || !service.ToUpper().Equals("WINUSB"))
            {
                continue;
            }

            // skip already discovered ones
            if (_connectedDevices.Any(d =>
                    String.Equals(d.InstanceId, instanceId, StringComparison.CurrentCultureIgnoreCase)))
            {
                _logger.LogWarning("Skipping duplicate for WinUSB {InstanceId}", instanceId);
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

            CompatibleDeviceIdentification supportedDevice =
                KnownDevices.IsWinUsbRewriteSupported(winUsbDevice.Descriptor.VID, winUsbDevice.Descriptor.PID);

            // Filter out devices we don't know about
            if (supportedDevice == null)
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

            HidDeviceOverWinUsbEndpoints identification = supportedDevice.WinUsbEndpoints;

            winUsbDevice.Dispose();

            return new HidDeviceOverWinUsb(path, identification)
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

        // skip those with unexpected service
        if (service is null || !service.ToUpper().Equals("WINUSB"))
        {
            return;
        }

        // skip already discovered ones
        if (_connectedDevices.Any(d =>
                String.Equals(d.InstanceId, device.InstanceId, StringComparison.CurrentCultureIgnoreCase)))
        {
            _logger.LogWarning("Skipping duplicate for WinUSB {InstanceId}", device.InstanceId);
            return;
        }

        _logger.LogInformation("WinUSB Device {Instance} ({Path}) arrived",
            device.InstanceId, symLink);

        HidDeviceOverWinUsb entry = CreateNewHidDeviceOverWinUsb(symLink);

        if (entry is null)
        {
            return;
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

        HidDeviceOverWinUsb entry =
            _connectedDevices.FirstOrDefault(entry =>
                String.Equals(entry.InstanceId, device.InstanceId, StringComparison.CurrentCultureIgnoreCase));

        if (entry is null)
        {
            return;
        }

        DeviceRemoved?.Invoke(entry);
        _connectedDevices.Remove(entry);
    }
}