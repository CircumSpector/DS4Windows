using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.System.Endpoints;

/// <summary>
///     Filter driver actions accepted by this API.
/// </summary>
public enum SystemFilterAction
{
    Enable,
    Disable,
    Install,
    Uninstall
}

/// <summary>
///     Endpoint exposing certain actions of the filter driver service.
/// </summary>
public class SystemFilterDriverActionsEndpoint : EndpointWithoutRequest
{
    private readonly IFilterService _filterService;

    public SystemFilterDriverActionsEndpoint(IFilterService filterService)
    {
        _filterService = filterService;
    }

    public override void Configure()
    {
        Post("/system/filterdriver/action/{Action}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        SystemFilterAction action = Route<SystemFilterAction>("Action");

        switch (action)
        {
            case SystemFilterAction.Enable:
                _filterService.SetFilterDriverEnabled(true);
                await SendOkAsync(ct);
                return;
            case SystemFilterAction.Disable:
                _filterService.SetFilterDriverEnabled(false);
                await SendOkAsync(ct);
                return;
            case SystemFilterAction.Install:
                Version installedVersion = await _filterService.InstallFilterDriver();
                await SendOkAsync(new { InstalledVersion = installedVersion }, ct);
                return;
            case SystemFilterAction.Uninstall:
                await _filterService.UninstallFilterDriver();
                await SendOkAsync(ct);
                return;
            default:
                await SendNotFoundAsync(ct);
                return;
        }
    }
}