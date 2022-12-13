using System.Security;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Identinator;
using Nefarius.Utilities.DeviceManagement.Drivers;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public class ControllerFilterService : IControllerFilterService
{
    /// <summary>
    ///     Regex to strip out version value from INF file.
    /// </summary>
    private static readonly Regex DriverVersionRegex =
        new(@"^DriverVer *=.*,(\d*\.\d*\.\d*\.\d*)", RegexOptions.Multiline);

    private FilterDriver _filterDriver;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<ControllerFilterService> _logger;
    private readonly IControllerManagerService _controllerManagerService;
    private readonly IDeviceSettingsService _deviceSettingsService;
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private bool _isInitializing = true;

    public ControllerFilterService(ILogger<ControllerFilterService> logger,
        IControllerManagerService controllerManagerService,
        IDeviceSettingsService deviceSettingsService,
        IControllerConfigurationService controllerConfigurationService)
    {
        _logger = logger;
        _controllerManagerService = controllerManagerService;
        _deviceSettingsService = deviceSettingsService;
        _controllerConfigurationService = controllerConfigurationService;
    }

    public void Initialize()
    {
        try
        {
            _filterDriver = new FilterDriver();
            if (GetFilterDriverInstalled())
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
    public bool GetFilterDriverInstalled()
    {
        return !string.IsNullOrEmpty(GetLocalDriverVersion());
    }

    /// <inheritdoc />
    public bool GetFilterDriverEnabled()
    {
        return _filterDriver.IsEnabled;
    }

    /// <inheritdoc />
    public void SetFilterDriverEnabled(bool isEnabled)
    {
        _filterDriver.IsEnabled = isEnabled;
        _deviceSettingsService.Settings.IsFilteringEnabled = isEnabled;
        _deviceSettingsService.SaveSettings();

        if (!_isInitializing)
        {
            foreach (var device in _controllerManagerService.ActiveControllers
                         .Where(c => c.Device != null)
                         .Select(activeController => activeController.Device)
                         .ToList())
            {
                if (!isEnabled)
                {
                    UnfilterController(device.SourceDevice.InstanceId);
                }
                else if (KnownDevices.IsWinUsbRewriteSupported(device.SourceDevice.VendorId, device.SourceDevice.ProductId) != null)
                {
                    var config = _controllerConfigurationService.GetActiveControllerConfiguration(device.SerialString);
                    if (config.OutputDeviceType != OutputDeviceType.None)
                    {
                        FilterController(device.SourceDevice.InstanceId);
                    }
                }
                else
                {
                    UsbPnPDevice usbDevice = PnPDevice.GetDeviceByInstanceId(device.SourceDevice.InstanceId)
                        .ToUsbPnPDevice();
                    usbDevice.CyclePort();
                }

                Thread.Sleep(250);
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

        usbDevice.CyclePort();
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

        usbDevice.CyclePort();
    }

    public void UnfilterAllControllers()
    {
        foreach (IHidDevice sourceDevice in _controllerManagerService.ActiveControllers
                     .Where(c => c.Device != null)
                     .Select(activeController => activeController.Device.SourceDevice)
                     .ToList())
        {
            UnfilterController(sourceDevice.InstanceId);
            Thread.Sleep(250);
        }
    }

    private static string? GetLocalDriverVersion()
    {
        return DriverStore.ExistingDrivers
            .Where(s => s.Contains("nssidswap", StringComparison.OrdinalIgnoreCase))
            .Select(d => GetInfDriverVersion(File.ReadAllText(d)))
            .MaxBy(k => k)?.ToString();
    }

    /// <summary>
    ///     Extracts the driver version from an INF file.
    /// </summary>
    /// <param name="infContent">The string content of the INF file.</param>
    /// <returns>The detected <see cref="Version" />.</returns>
    private static Version GetInfDriverVersion(string infContent)
    {
        Match match = DriverVersionRegex.Match(infContent);

        return Version.Parse(match.Groups[1].Value);
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