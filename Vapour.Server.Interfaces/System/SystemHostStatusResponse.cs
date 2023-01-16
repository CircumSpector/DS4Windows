namespace Vapour.Server.System;

public sealed class SystemHostStatusResponse
{
    /// <summary>
    ///     Gets if the host is running.
    /// </summary>
    public bool IsRunning { get; init; }
}