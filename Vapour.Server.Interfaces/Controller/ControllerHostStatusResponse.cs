namespace Vapour.Server.Controller;

public sealed class ControllerHostStatusResponse
{
    /// <summary>
    ///     Gets if the host is running.
    /// </summary>
    public bool IsRunning { get; init; }
}