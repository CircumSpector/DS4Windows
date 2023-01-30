using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services.Reporting;

/// <summary>
///     Handles reading input reports from a compatible input device and dispatches them.
/// </summary>
public interface IInputReportProcessor
{
    IInputSource InputSource { get; }

    bool IsInputReportAvailableInvoked { get; set; }

    bool IsProcessing { get; }

    event Action<IInputSource, InputSourceFinalReport> InputReportAvailable;

    /// <summary>
    ///     Start the input report reader.
    /// </summary>
    void StartInputReportReader();

    /// <summary>
    ///     Stops the input report reader.
    /// </summary>
    void StopInputReportReader();

    void SetInputSource(IInputSource inputSource);
    event Action<ICustomAction> OnCustomActionDetected;
}