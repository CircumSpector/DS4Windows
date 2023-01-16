using FastEndpoints;

using Vapour.Server.System;

namespace Vapour.Server.Host.System.Endpoints;

public sealed class SystemHostStatusEndpoint : EndpointWithoutRequest<SystemHostStatusResponse>
{
    private readonly SystemService _systemService;

    public SystemHostStatusEndpoint(SystemService systemService)
    {
        _systemService = systemService;
    }

    public override void Configure()
    {
        Get("/system/host/status");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Queries the system host status";
            s.Description = "Returns various runtime status values about the system host service";
            s.Responses[200] = "Status response was delivered";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(new SystemHostStatusResponse
            {
                IsRunning = _systemService.IsSystemHostRunning
            },
            ct);
    }
}