using FastEndpoints;

using Vapour.Server.System;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.System.Endpoints;

public class SystemFilterDriverStatusEndpoint : EndpointWithoutRequest<SystemFilterDriverStatusResponse>
{
    private readonly IFilterService _filterService;

    public SystemFilterDriverStatusEndpoint(IFilterService filterService)
    {
        _filterService = filterService;
    }

    public override void Configure()
    {
        Get("/system/filterdriver/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Queries the system filter driver status";
            s.Description = "Returns whether or not the filter driver is installed and if it is globally enabled";
            s.Responses[200] = "Status response was delivered";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(
            new SystemFilterDriverStatusResponse
            {
                IsDriverInstalled = _filterService.IsFilterDriverInstalled,
                IsFilteringEnabled = _filterService.IsFilterDriverEnabled
            },
            ct);
    }
}