using MessagePipe;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Vapour.Server.System;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services.Configuration.Messages;

namespace Vapour.Server.Host.System;
public class SystemService
{
    private readonly SystemManagerHost _systemManagerHost;
    private readonly IAsyncSubscriber<string, bool> _sourceListReadySubscriber;
    private readonly ISystemMessageForwarder _systemMessageForwarder;

    public SystemService(
        ISystemMessageForwarder systemMessageForwarder,
        SystemManagerHost systemManagerHost,
        IAsyncSubscriber<string, bool> sourceListReadySubscriber)
    {
        _systemMessageForwarder = systemMessageForwarder;
        _systemManagerHost = systemManagerHost;
        _sourceListReadySubscriber = sourceListReadySubscriber;
        _sourceListReadySubscriber.Subscribe(MessageKeys.InputSourceReadyKey, OnSourceListReady);
        _systemManagerHost.RunningChanged += SystemManagerHostRunningChanged;
    }

    public bool IsReady { get; private set; }

    public bool IsSystemHostRunning => _systemManagerHost.IsRunning;

    public static void RegisterRoutes(WebApplication app)
    {
        app.Services.GetService<SystemService>();
    }

    private async void SystemManagerHostRunningChanged(object sender, bool e)
    {
        await _systemMessageForwarder.SendIsHostRunning(e);
    }

    private ValueTask OnSourceListReady(bool isReady, CancellationToken cs)
    {
        IsReady = isReady;
        return ValueTask.CompletedTask;
    }
}
