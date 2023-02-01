using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
using Nefarius.Utilities.DeviceManagement.Exceptions;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace Vapour.Shared.Devices.Services.Configuration;

/// <summary>
///     A potential <see cref="FilterService"/> error.
/// </summary>
public sealed class FilterServiceException : Exception
{
    internal FilterServiceException(string message) : base(message) { }
}

public partial class FilterService : IFilterService
{
    private readonly IDeviceSettingsService _deviceSettingsService;

    private readonly FilterDriver _filterDriver;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<FilterService> _logger;

    public FilterService(ILogger<FilterService> logger,
        IDeviceSettingsService deviceSettingsService)
    {
        _logger = logger;
        _deviceSettingsService = deviceSettingsService;

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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task FilterController(string instanceId, CancellationToken ct = default)
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
            await FilterBtController(instanceId, ct);
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UnfilterController(string instanceId, CancellationToken ct = default)
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
            await UnfilterBtController(instanceId, ct);
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    /// <summary>
    ///     Power-cycles the port the given device is attached to.
    /// </summary>
    /// <param name="usbDevice">The USB device to restart.</param>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    private static void CyclePort(UsbPnPDevice usbDevice)
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

    /// <summary>
    ///     Finds the correct parent USB instance for a HID device.
    /// </summary>
    /// <param name="instanceId">The HID (or, if rewritten already, USB) instance ID.</param>
    private static Tuple<PnPDevice, string> GetDeviceToFilter(string instanceId)
    {
        PnPDevice device = PnPDevice.GetDeviceByInstanceId(instanceId);

        string[] hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);

        if (hardwareIds.First().StartsWith("HID"))
        {
            string parentInputDeviceId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);
            device = PnPDevice.GetDeviceByInstanceId(parentInputDeviceId);
            hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
        }

        return new Tuple<PnPDevice, string>(device, hardwareIds.First());
    }
}