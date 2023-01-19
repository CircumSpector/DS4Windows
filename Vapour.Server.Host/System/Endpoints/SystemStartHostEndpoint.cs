using FastEndpoints;

using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.System.Endpoints;

public sealed class SystemStartHostEndpoint : EndpointWithoutRequest
{
    private readonly SystemManagerHost _systemManagerHost;

    public SystemStartHostEndpoint(SystemManagerHost systemManagerHost)
    {
        _systemManagerHost = systemManagerHost;
    }

    public override void Configure()
    {
        Post("/system/host/start");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Requests the system host to start, if stopped";
            s.Description = "Requests the system host to start, if stopped";
            s.Responses[200] = "System host has been started successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!_systemManagerHost.IsRunning)
        {
            await _systemManagerHost.StartAsync();
        }

        await SendOkAsync(ct);
    }
}