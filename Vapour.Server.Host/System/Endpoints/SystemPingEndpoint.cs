using FastEndpoints;

namespace Vapour.Server.Host.System.Endpoints;

public sealed class SystemPingEndpoint : EndpointWithoutRequest
{
    private readonly SystemService _systemService;

    public SystemPingEndpoint(SystemService systemService)
    {
        _systemService = systemService;
    }

    public override void Configure()
    {
        Get("/system/ping");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Queries the system host for ready status";
            s.Description = "Returns Success when ready, otherwise Not Found";
            s.Responses[200] = "System host is ready";
            s.Responses[404] = "System host not ready";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_systemService.IsReady)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}