using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Handles reading input reports from a compatible input device and dispatches them.
/// </summary>
public interface IControllerInputReportProcessor
{
    ICompatibleHidDevice HidDevice { get; }

    bool IsInputReportAvailableInvoked { get; set; }

    bool IsProcessing { get; }

    event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

    /// <summary>
    ///     Start the input report reader.
    /// </summary>
    void StartInputReportReader();

    /// <summary>
    ///     Stops the input report reader.
    /// </summary>
    void StopInputReportReader();

    void SetDevice(ICompatibleHidDevice hidDevice);
}