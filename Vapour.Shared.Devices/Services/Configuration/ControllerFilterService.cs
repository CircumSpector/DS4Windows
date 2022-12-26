using System.Diagnostics.CodeAnalysis;
using System.Security;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
using Nefarius.Utilities.DeviceManagement.Exceptions;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

public class ControllerFilterService : IControllerFilterService
{
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private readonly IDeviceSettingsService _deviceSettingsService;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<ControllerFilterService> _logger;

    private FilterDriver _filterDriver;
    private bool _isInitializing = true;

    public ControllerFilterService(ILogger<ControllerFilterService> logger,
        ICurrentControllerDataSource currentControllerDataSource,
        IDeviceSettingsService deviceSettingsService)
    {
        _logger = logger;
        _currentControllerDataSource = currentControllerDataSource;
        _deviceSettingsService = deviceSettingsService;
    }

    public void Initialize()
    {
        try
        {
            if (!FilterDriver.IsDriverInstalled)
            {
                _logger.LogWarning(
                    "The filter driver appears to be missing on this machine, rewrite feature will not be available!");
            }

            _filterDriver = new FilterDriver();
            if (FilterDriver.IsDriverInstalled)
            {
                SetFilterDriverEnabled(_deviceSettingsService.Settings.IsFilteringEnabled ?? true);
            }
            else
            {
                SetFilterDriverEnabled(false);
            }

            _isInitializing = false;
        }
        catch (SecurityException ex)
        {
            _logger.LogError(ex, "To use the rewrite feature, the service must be run as Administrator!");
            throw;
        }
    }

    /// <inheritdoc />
    public bool IsFilterDriverInstalled => FilterDriver.IsDriverInstalled;

    /// <inheritdoc />
    public bool IsFilterDriverEnabled => _filterDriver.IsEnabled;

    /// <inheritdoc />
    public void SetFilterDriverEnabled(bool isEnabled)
    {
        _filterDriver.IsEnabled = isEnabled;
        _deviceSettingsService.Settings.IsFilteringEnabled = isEnabled;
        _deviceSettingsService.SaveSettings();

        if (!_isInitializing)
        {
            foreach (ICompatibleHidDevice device in _currentControllerDataSource.CurrentControllers
                         .Select(activeController => activeController)
                         .ToList())
            {
                if (!isEnabled)
                {
                    UnfilterController(device.SourceDevice.InstanceId);
                }
                else if (FilterUnfilterIfNeeded(device))
                {
                    //dont do anything
                }
                else
                {
                    UsbPnPDevice usbDevice = PnPDevice.GetDeviceByInstanceId(device.SourceDevice.InstanceId)
                        .ToUsbPnPDevice();
                    usbDevice.CyclePort();
                    CyclePort(usbDevice);
                }
            }
        }
    }

    /// <inheritdoc />
    public void FilterController(string instanceId)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(instanceId);

        try
        {
            UsbPnPDevice usbDevice = device.ToUsbPnPDevice();

            using RewriteEntry entry = _filterDriver.AddOrUpdateRewriteEntry(hardwareId);
            entry.IsReplacingEnabled = true;
            entry.CompatibleIds = new[]
            {
                @"USB\MS_COMP_WINUSB", @"USB\Class_FF&SubClass_5D&Prot_01", @"USB\Class_FF&SubClass_5D",
                @"USB\Class_FF"
            };

            CyclePort(usbDevice);
        }
        catch (UsbPnPDeviceConversionException)
        {
            // TODO: handle Bluetooth
            _logger.LogWarning("{InstanceId} is not a USB device", instanceId);
            return;
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    /// <inheritdoc />
    public void UnfilterController(string instanceId)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(instanceId);

        try
        {
            UsbPnPDevice usbDevice = device.ToUsbPnPDevice();

            using RewriteEntry entry = _filterDriver.GetRewriteEntryFor(hardwareId);
            if (entry is not null)
            {
                entry.IsReplacingEnabled = false;
            }

            CyclePort(usbDevice);
        }
        catch (UsbPnPDeviceConversionException)
        {
            // TODO: handle Bluetooth
            _logger.LogWarning("{InstanceId} is not a USB device", instanceId);
            return;
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    /// <inheritdoc />
    public void UnfilterAllControllers()
    {
        foreach (IHidDevice sourceDevice in _currentControllerDataSource.CurrentControllers
                     .Select(activeController => activeController.SourceDevice)
                     .ToList())
        {
            UnfilterController(sourceDevice.InstanceId);
        }
    }

    /// <inheritdoc />
    public bool FilterUnfilterIfNeeded(ICompatibleHidDevice device)
    {
        // TODO: implement properly once wireless filtering is implemented 
        if (device.Connection == ConnectionType.Bluetooth)
        {
            return false;
        }

        if (IsFilterDriverEnabled)
        {
            ControllerConfiguration config = device.CurrentConfiguration;
            if (!device.IsFiltered)
            {
                CompatibleDeviceIdentification supportsWinUsbRewrite =
                    KnownDevices.IsWinUsbRewriteSupported(device.SourceDevice.VendorId, device.SourceDevice.ProductId);
                if (supportsWinUsbRewrite != null && config.OutputDeviceType != OutputDeviceType.None)
                {
                    FilterController(device.SourceDevice.InstanceId);
                    return true;
                }
            }
            else if (device.IsFiltered && config.OutputDeviceType == OutputDeviceType.None)
            {
                UnfilterController(device.SourceDevice.InstanceId);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Power-cycles the port the given device is attached to.
    /// </summary>
    /// <param name="usbDevice">The USB device to restart.</param>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    private void CyclePort(UsbPnPDevice usbDevice)
    {
        ManualResetEvent wait = new(false);
        DeviceNotificationListener listener = new();

        listener.RegisterDeviceArrived(args =>
        {
            wait.Set();
        });
        listener.StartListen(FilterDriver.RewrittenDeviceInterfaceId);
        listener.StartListen(DeviceInterfaceIds.HidDevice);

        usbDevice.CyclePort();

        wait.WaitOne(TimeSpan.FromSeconds(1));

        listener.StopListen();
        listener.Dispose();
        wait.Dispose();
    }

    private static Tuple<PnPDevice, string> GetDeviceToFilter(string instanceId)
    {
        PnPDevice device = PnPDevice.GetDeviceByInstanceId(instanceId);
        string[] hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
        if (hardwareIds[0].StartsWith("HID"))
        {
            string parentInputDeviceId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);
            device = PnPDevice.GetDeviceByInstanceId(parentInputDeviceId);
            hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
        }

        return new Tuple<PnPDevice, string>(device, hardwareIds[0]);
    }
}