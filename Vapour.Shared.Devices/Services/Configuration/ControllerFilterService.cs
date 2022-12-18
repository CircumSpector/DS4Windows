﻿using System.Security;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
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
        UsbPnPDevice usbDevice = device.ToUsbPnPDevice();
        //TODO: filter the controller and cycle the port

        using RewriteEntry entry = _filterDriver.AddOrUpdateRewriteEntry(hardwareId);
        entry.IsReplacingEnabled = true;
        entry.CompatibleIds = new[]
        {
            @"USB\MS_COMP_WINUSB", @"USB\Class_FF&SubClass_5D&Prot_01", @"USB\Class_FF&SubClass_5D", @"USB\Class_FF"
        };

        CyclePort(usbDevice);
    }

    /// <inheritdoc />
    public void UnfilterController(string instanceId)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(instanceId);
        UsbPnPDevice usbDevice = device.ToUsbPnPDevice();
        //TODO: fill in the unfilter

        using RewriteEntry entry = _filterDriver.GetRewriteEntryFor(hardwareId);
        if (entry is not null)
        {
            entry.IsReplacingEnabled = false;
        }

        CyclePort(usbDevice);
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

    private void CyclePort(UsbPnPDevice usbDevice)
    {
        usbDevice.CyclePort();

        //give it time to cycle
        Thread.Sleep(250);
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