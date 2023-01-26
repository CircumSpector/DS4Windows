using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.System.Endpoints;

[Obsolete]
public class SystemFilterSetDriverEnabledEndpoint : EndpointWithoutRequest
{
    private readonly IFilterService _filterService;

    public SystemFilterSetDriverEnabledEndpoint(IFilterService filterService)
    {
        _filterService = filterService;
    }

    public override void Configure()
    {
        Post("/system/filterdriver/setenable/{isEnabled}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Enables or disables the filter driver";
            s.Description = "Enables or disables the filter driver";
            s.Responses[200] = "Filter driver enable set correctly";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var isEnabled = Route<bool>("isEnabled");
        _filterService.SetFilterDriverEnabled(isEnabled);
        await SendOkAsync(ct);
    }
}