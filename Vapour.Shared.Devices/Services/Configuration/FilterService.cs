﻿using System.Diagnostics.CodeAnalysis;
using System.Security;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
using Nefarius.Utilities.DeviceManagement.Exceptions;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

public class FilterService : IFilterService
{
    private readonly IDeviceSettingsService _deviceSettingsService;
    private readonly IServiceProvider _serviceProvider;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<FilterService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private readonly FilterDriver _filterDriver;

    public FilterService(ILogger<FilterService> logger,
        IDeviceSettingsService deviceSettingsService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _deviceSettingsService = deviceSettingsService;
        _serviceProvider = serviceProvider;

        _deviceSettingsService.LoadSettings();

        _filterDriver = new FilterDriver();
        if (FilterDriver.IsDriverInstalled)
        {
            SetFilterDriverEnabled(_deviceSettingsService.Settings.IsFilteringEnabled ?? true);
        }
        else
        {
            SetFilterDriverEnabled(false);
        }
    }

    public event Action<bool> FilterDriverEnabledChanged;

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

        FilterDriverEnabledChanged?.Invoke(isEnabled);
    }

    /// <inheritdoc />
    public async Task<Version> InstallFilterDriver()
    {
        await FilterDriverInstaller.InstallFilterDriverAsync();
        SetFilterDriverEnabled(true);
        return FilterDriverInstaller.EmbeddedDriverVersion;
    }

    /// <inheritdoc />
    public async Task UninstallFilterDriver()
    {
        await FilterDriverInstaller.UninstallFilterDriverAsync();
        SetFilterDriverEnabled(false);
    }

    /// <summary>
    ///     Filters a particular USB device instance.
    /// </summary>
    /// <param name="instanceId">The instance ID of the device to filter.</param>
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
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    /// <summary>
    ///     Reverts filtering a particular USB device instance.
    /// </summary>
    /// <param name="instanceId">The instance ID of the device to revert.</param>
    public void UnfilterController(string instanceId)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(instanceId);

        try
        {
            UsbPnPDevice usbDevice = device.ToUsbPnPDevice();

            using RewriteEntry entry = _filterDriver.GetRewriteEntryFor(hardwareId);

            if (entry is null)
            {
                return;
            }

            entry.IsReplacingEnabled = false;

            CyclePort(usbDevice);
        }
        catch (UsbPnPDeviceConversionException)
        {
            // TODO: handle Bluetooth
            _logger.LogWarning("{InstanceId} is not a USB device", instanceId);
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    private void FilterBtController(ICompatibleHidDevice device, bool shouldRestartBtHost)
    {
        //do sdp stuff
        if (shouldRestartBtHost)
        {
            //restart bt host
        }
    }

    private void UnfilterBtController(ICompatibleHidDevice device, bool shouldRestartBtHost)
    {
        //do sdp stuff
        if (shouldRestartBtHost)
        {
            //restart bt host
        }
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