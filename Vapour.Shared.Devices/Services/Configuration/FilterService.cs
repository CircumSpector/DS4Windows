using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

/// <summary>
///     A potential <see cref="FilterService" /> error.
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
    public void FilterController(ICompatibleHidDevice deviceToFilter, CancellationToken ct = default)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(deviceToFilter.SourceDevice.InstanceId);

        if (deviceToFilter.Connection == ConnectionType.Bluetooth)
        {
            FilterBtController(deviceToFilter, ct);
        }
        else
        {
            FilterUsbDevice(deviceToFilter);
        }
    }

    /// <inheritdoc />
    public void UnfilterController(ICompatibleHidDevice deviceToUnfilter, CancellationToken ct = default)
    {
        if (deviceToUnfilter.Connection == ConnectionType.Bluetooth)
        {
            UnfilterBtController(deviceToUnfilter, ct);
        }
        else
        {
            UnfilterUsbDevice(deviceToUnfilter);
        }
    }
}