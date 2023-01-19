using FastEndpoints;

using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.System.Endpoints;

public sealed class SystemStopHostEndpoint : EndpointWithoutRequest
{
    private readonly SystemManagerHost _systemManagerHost;

    public SystemStopHostEndpoint(SystemManagerHost systemManagerHost)
    {
        _systemManagerHost = systemManagerHost;
    }

    public override void Configure()
    {
        Post("/system/host/stop");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Requests the system host to stop, if started";
            s.Description = "Requests the system host to stop, if started";
            s.Responses[200] = "System host has been stopped successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_systemManagerHost.IsRunning)
        {
            await _systemManagerHost.StopAsync();
        }

        await SendOkAsync(ct);
    }
}