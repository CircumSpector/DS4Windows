using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

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
    Task SetFilterDriverEnabled(bool isEnabled, bool shouldFixupAfter = true);

    /// <summary>
    ///     Perform one-time checks and tasks on service start.
    /// </summary>
    Task Initialize();

    /// <summary>
    ///     Revers all controllers to their original state.
    /// </summary>
    Task UnfilterAllControllers(bool shouldFixupAfter = true);

    /// <summary>
    ///     Check if <see cref="ICompatibleHidDevice"/> needs unfiltering and performs it, if necessary.
    /// </summary>
    /// <param name="device">The device to unfilter.</param>
    /// <param name="outputDeviceType">The output device type of the configuration</param>
    /// <returns>True on success, false otherwise.</returns>
    bool FilterUnfilterIfNeeded(ICompatibleHidDevice device, OutputDeviceType outputDeviceType, bool autoRestartBtHost = true);

    /// <summary>
    ///     Installs the class filter driver.
    /// </summary>
    Task<Version> InstallFilterDriver();
    
    /// <summary>
    ///     Uninstalls the class filter driver.
    /// </summary>
    Task UninstallFilterDriver();
}