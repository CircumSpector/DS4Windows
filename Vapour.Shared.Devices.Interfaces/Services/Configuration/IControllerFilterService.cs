using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

public interface IControllerFilterService
{
    /// <summary>
    ///     Filters the passed controller
    /// </summary>
    /// <param name="instanceId">The instanceId of the controller to filter</param>
    void FilterController(string instanceId);

    /// <summary>
    ///     Unfilters the passed controller
    /// </summary>
    /// <param name="instanceId">The instanceId of the controller to unfilter</param>
    void UnfilterController(string instanceId);

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
    ///     Perform one-time checks and tasks on service start.
    /// </summary>
    void Initialize();

    void UnfilterAllControllers();
    bool FilterUnfilterIfNeeded(ICompatibleHidDevice device);
}