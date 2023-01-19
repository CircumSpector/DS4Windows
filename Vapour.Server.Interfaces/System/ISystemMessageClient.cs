namespace Vapour.Server.System;

/// <summary>
///     Describes system events exchangeable between client and server.
/// </summary>
public interface ISystemMessageClient
{
    Task IsHostRunningChanged(IsHostRunningChangedMessage message);
}