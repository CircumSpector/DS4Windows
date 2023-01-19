namespace Vapour.Server.System;

/// <summary>
///     Dispatches system events to clients.
/// </summary>
public interface ISystemMessageForwarder
{
    Task SendIsHostRunning(bool isRunning);
}