using System.Diagnostics.CodeAnalysis;

using Vapour.Server.System;

namespace Vapour.Client.ServiceClients;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface ISystemServiceClient
{
    /// <summary>
    ///     Starts listening for incoming messages (host to client).
    /// </summary>
    void StartListening(CancellationToken ct = default);

    /// <summary>
    ///     Blocks until the host is reachable.
    /// </summary>
    Task WaitForService(CancellationToken ct = default);

    /// <summary>
    ///     Queries the host status.
    /// </summary>
    Task<bool> IsHostRunning();

    /// <summary>
    ///     Starts the host.
    /// </summary>
    Task StartHost();

    /// <summary>
    ///     Stops the host.
    /// </summary>
    Task StopHost();

    /// <summary>
    ///     Queries rewrite filter driver status.
    /// </summary>
    Task<SystemFilterDriverStatusResponse> GetFilterDriverStatus();

    /// <summary>
    ///     Enables or disables rewrite filter driver globally.
    /// </summary>
    Task SystemFilterSetDriverEnabled(bool isEnabled);

    /// <summary>
    ///     Gets invoked when the host status has changed.
    /// </summary>
    event Action<IsHostRunningChangedMessage> OnIsHostRunningChanged;

    /// <summary>
    ///     Invokes rewrite filter driver installation.
    /// </summary>
    Task SystemFilterInstallDriver();

    /// <summary>
    ///     Invokes rewrite filter driver removal.
    /// </summary>
    Task SystemFilterUninstallDriver();
}