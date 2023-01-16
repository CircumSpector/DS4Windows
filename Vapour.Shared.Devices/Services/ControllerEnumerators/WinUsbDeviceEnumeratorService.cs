using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
using Nefarius.Drivers.WinUSB;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Single point of truth of states for all connected and handled WinUSB devices.
/// </summary>
internal class WinUsbDeviceEnumeratorService : IHidDeviceEnumeratorService<HidDeviceOverWinUsb>
{
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDeviceNotificationListener _deviceNotificationListener;

    private readonly ILogger<WinUsbDeviceEnumeratorService> _logger;
    private readonly IDeviceFactory _deviceFactory;

    public WinUsbDeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
        ILogger<WinUsbDeviceEnumeratorService> logger,
        IDeviceFactory deviceFactory)
    {
        _deviceNotificationListener = deviceNotificationListener;
        _logger = logger;
        _deviceFactory = deviceFactory;

        _deviceNotificationListener.RegisterDeviceArrived(DeviceNotificationListenerOnDeviceArrived,
            FilterDriver.RewrittenDeviceInterfaceId);
        _deviceNotificationListener.RegisterDeviceRemoved(DeviceNotificationListenerOnDeviceRemoved,
            FilterDriver.RewrittenDeviceInterfaceId);
    }

    /// <inheritdoc />
    public event Action<HidDeviceOverWinUsb> DeviceArrived;

    /// <inheritdoc />
    public event Action<string> DeviceRemoved;
    
    /// <inheritdoc />
    public void Start()
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(Start)}");

        int deviceIndex = 0;
        
        while (Devcon.FindByInterfaceGuid(FilterDriver.RewrittenDeviceInterfaceId, out string path, out string _, deviceIndex++))
        {
            try
            {
                CreateNewHidDeviceOverWinUsb(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to create Filtered USB device for {path}");
            }
        }

        _deviceNotificationListener.StartListen(FilterDriver.RewrittenDeviceInterfaceId);
    }

    public void Stop()
    {
        _deviceNotificationListener.StopListen(FilterDriver.RewrittenDeviceInterfaceId);
    }

    private void DeviceNotificationListenerOnDeviceArrived(DeviceEventArgs args)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceArrived)}");

        string symLink = args.SymLink;
        activity?.SetTag("Path", symLink);

        _logger.LogInformation("Filtered Device ({Path}) arrived", symLink);
        CreateNewHidDeviceOverWinUsb(symLink);
    }

    private void DeviceNotificationListenerOnDeviceRemoved(DeviceEventArgs args)
    {
        RemoveDevice(args.SymLink);
    }

    private void CreateNewHidDeviceOverWinUsb(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(CreateNewHidDeviceOverWinUsb)}");

        activity?.SetTag("Path", path);

        try
        {
            using USBDevice winUsbDevice = USBDevice.GetSingleDeviceByPath(path);
            var supportedDevice =
                _deviceFactory.IsKnownDevice(winUsbDevice.Descriptor.VID, winUsbDevice.Descriptor.PID);
            

            // Filter out devices we don't know about
            if (supportedDevice == null)
            {
                return;
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

            var hidDevice = new HidDeviceOverWinUsb(path, identification)
            {
                InstanceId = device.InstanceId.ToUpper(),
                Description = device.GetProperty<string>(DevicePropertyKey.Device_DeviceDesc),
                DisplayName = friendlyName,
                ParentInstance = parentId
            };

            DeviceArrived?.Invoke(hidDevice);
        }
        catch (USBException ex)
        {
            _logger.LogWarning(ex, "Couldn't access WinUSB device ({Path})", path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to create Filtered USB device for {path}");
        }
    }

    private void RemoveDevice(string symLink)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(WinUsbDeviceEnumeratorService)}:{nameof(RemoveDevice)}");

        activity?.SetTag("Path", symLink);

        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(symLink, DeviceLocationFlags.Phantom);

        _logger.LogInformation("WinUSB Device {Instance} ({Path}) removed",
            device.InstanceId, symLink);

        if (!device.IsVirtual())
        {
            DeviceRemoved?.Invoke(device.InstanceId);
        }
    }
}