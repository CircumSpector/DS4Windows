namespace Vapour.Shared.Devices.Services.Configuration;

public interface IFilterService
{
    /// <summary>
    ///     Gets whether or not the filter driver is installed
    /// </summary>
    /// <returns>A bool representing whether or not the filter driver is installed</returns>
    bool IsFilterDriverInstalled { get; }

    /// <summary>
    ///     Gets whether or not the filter driver is enabled
    /// </summary>
    /// <returns>A bool representing whether or not the filter driver is enabled</returns>
    bool IsFilterDriverEnabled { get; }

    /// <summary>
    ///     Sets whether or not to enable the filter driver
    /// </summary>
    /// <param name="isEnabled">A bool representing whether or not to enable the filter driver</param>
    void SetFilterDriverEnabled(bool isEnabled);

    /// <summary>
    ///     Installs the class filter driver.
    /// </summary>
    Task<Version> InstallFilterDriver();

    /// <summary>
    ///     Uninstalls the class filter driver.
    /// </summary>
    Task UninstallFilterDriver();

    /// <summary>
    ///     Applies USB or Bluetooth filtering/patching for the given Instance ID.
    /// </summary>
    /// <param name="instanceId">The HID device Instance ID.</param>
    Task FilterController(string instanceId, CancellationToken ct = default);

    /// <summary>
    ///     Reverts USB or Bluetooth filtering/patching for the given Instance ID.
    /// </summary>
    /// <param name="instanceId">The HID device Instance ID.</param>
    Task UnfilterController(string instanceId, CancellationToken ct = default);

    /// <summary>
    ///     Gets invoked when the filter enabled state has changed.
    /// </summary>
    event Action<bool> FilterDriverEnabledChanged;
}