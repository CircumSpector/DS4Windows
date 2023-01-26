using Windows.Web.Http;

using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.System.Endpoints;

/// <summary>
///     Filter driver actions accepted by this API.
/// </summary>
public enum SystemFilterDriverAction
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
        SystemFilterDriverAction driverAction = Route<SystemFilterDriverAction>("Action");

        switch (driverAction)
        {
            case SystemFilterDriverAction.Enable:
                _filterService.SetFilterDriverEnabled(true);
                await SendOkAsync(ct);
                return;
            case SystemFilterDriverAction.Disable:
                _filterService.SetFilterDriverEnabled(false);
                await SendOkAsync(ct);
                return;
            case SystemFilterDriverAction.Install:
                if (!_filterService.IsFilterDriverInstalled)
                {
                    Version installedVersion = await _filterService.InstallFilterDriver();
                    await SendOkAsync(new { InstalledVersion = installedVersion }, ct);
                }
                else
                {
                    await SendAsync("Latest filter driver already installed, nothing to do.",
                        (int)HttpStatusCode.Conflict, ct);
                }

                return;
            case SystemFilterDriverAction.Uninstall:
                await _filterService.UninstallFilterDriver();
                await SendOkAsync(ct);
                return;
            default:
                await SendNotFoundAsync(ct);
                return;
        }
    }
}