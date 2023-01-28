using System.Diagnostics.CodeAnalysis;
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
    private readonly IInputSourceDataSource _inputSourceDataSource;

    private IInputSourceService _inputSourceService;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<FilterService> _logger;

    private FilterDriver _filterDriver;
    private bool _isInitializing = true;

    public FilterService(ILogger<FilterService> logger,
        IInputSourceDataSource inputSourceDataSource,
        IDeviceSettingsService deviceSettingsService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _inputSourceDataSource = inputSourceDataSource;
        _deviceSettingsService = deviceSettingsService;
        _serviceProvider = serviceProvider;
    }

    public async Task Initialize()
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
                await SetFilterDriverEnabled(_deviceSettingsService.Settings.IsFilteringEnabled ?? true);
            }
            else
            {
                await SetFilterDriverEnabled(false);
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
    public async Task SetFilterDriverEnabled(bool isEnabled, bool shouldFixupAfter = true)
    {
        _filterDriver.IsEnabled = isEnabled;
        _deviceSettingsService.Settings.IsFilteringEnabled = isEnabled;
        _deviceSettingsService.SaveSettings();

        if (!_isInitializing)
        {
            DisableAutoFixup();
            foreach (var inputSource in _inputSourceDataSource.InputSources)
            {
                inputSource.Stop();

                foreach (var device in inputSource.GetControllers())
                {
                    if (!isEnabled)
                    {
                        UnfilterController(device.SourceDevice.InstanceId);
                    }
                    else if (FilterUnfilterIfNeeded(device, device.CurrentConfiguration.OutputDeviceType))
                    {
                        //dont do anything
                    }
                    else
                    {
                        UsbPnPDevice usbDevice = PnPDevice
                            .GetDeviceByInstanceId(device.SourceDevice.InstanceId)
                            .ToUsbPnPDevice();

                        CyclePort(usbDevice);
                    }
                }
            }

            if (shouldFixupAfter)
            {
                await Task.Delay(500);
                await EnableAndRunAutoFixup();
            }
        }
    }

    /// <inheritdoc />
    public async Task UnfilterAllControllers(bool shouldFixupAfter = true)
    {
        DisableAutoFixup();
        foreach (var inputSource in _inputSourceDataSource.InputSources)
        {
            inputSource.Stop();

            foreach (var device in inputSource.GetControllers())
            {
                UnfilterController(device.SourceDevice.InstanceId);
            }
        }

        if (shouldFixupAfter)
        {
            await EnableAndRunAutoFixup();
        }
    }

    /// <inheritdoc />
    public bool FilterUnfilterIfNeeded(ICompatibleHidDevice device, OutputDeviceType outputDeviceType, bool shouldRestartBtHost = true)
    { 
        if (device.Connection == ConnectionType.Bluetooth)
        {
            if (!device.CurrentDeviceInfo.IsBtFilterable)
            {
                return false;
            }

            var neededFilterAction = false;
            if (device.IsFiltered && outputDeviceType == OutputDeviceType.None)
            {
                FilterBtController(device, shouldRestartBtHost);
                neededFilterAction = true;
            }
            else if (!device.IsFiltered && outputDeviceType != OutputDeviceType.None)
            {
                UnfilterBtController(device, shouldRestartBtHost);
                neededFilterAction = true;
            }

            return neededFilterAction;
        }
        
        if (IsFilterDriverEnabled)
        {
            if (!device.IsFiltered)
            {
                if (device.CurrentDeviceInfo.WinUsbEndpoints != null && outputDeviceType != OutputDeviceType.None)
                {
                    FilterController(device.SourceDevice.InstanceId);
                    return true;
                }
            }
            else if (device.IsFiltered && outputDeviceType == OutputDeviceType.None)
            {
                UnfilterController(device.SourceDevice.InstanceId);
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<Version> InstallFilterDriver()
    {
        await FilterDriverInstaller.InstallFilterDriverAsync();
        await SetFilterDriverEnabled(true);
        return FilterDriverInstaller.EmbeddedDriverVersion;
    }

    /// <inheritdoc />
    public async Task UninstallFilterDriver()
    {
        await SetFilterDriverEnabled(false, false);
        await FilterDriverInstaller.UninstallFilterDriverAsync();
        await EnableAndRunAutoFixup();
    }

    /// <inheritdoc />
    private void FilterController(string instanceId)
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

    /// <inheritdoc />
    private void UnfilterController(string instanceId)
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

    private bool _existingAutoFixup;

    private void DisableAutoFixup()
    {
        EnsureInputSourceService();
        _existingAutoFixup = _inputSourceService.ShouldAutoFixup;
        _inputSourceService.ShouldAutoFixup = false;
    }

    private async Task EnableAndRunAutoFixup()
    {
        await _inputSourceService.FixupInputSources();
        _inputSourceService.ShouldAutoFixup = _existingAutoFixup;
    }

    private void EnsureInputSourceService()
    {
        if (_inputSourceService == null)
        {
            _inputSourceService = _serviceProvider.GetService<IInputSourceService>();
        }
    }
}