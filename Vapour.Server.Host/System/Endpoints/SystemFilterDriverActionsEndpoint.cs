using Windows.Web.Http;

using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.System.Endpoints;

/// <summary>
///     Filter driver actions accepted by this API.
/// </summary>
public enum SystemFilterDriverAction
{
    /// <summary>
    ///     Globally enable filtering feature.
    /// </summary>
    Enable,

    /// <summary>
    ///     Globally disable filtering feature.
    /// </summary>
    Disable,

    /// <summary>
    ///     Invoke filter driver installation.
    /// </summary>
    Install,

    /// <summary>
    ///     Invoke filter driver removal.
    /// </summary>
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
                await _filterService.SetFilterDriverEnabled(true);
                await SendOkAsync(ct);
                return;
            case SystemFilterDriverAction.Disable:
                await _filterService.SetFilterDriverEnabled(false);
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
                if (!_filterService.IsFilterDriverInstalled)
                {
                    await SendAsync("Filter driver not installed, nothing to do.",
                        (int)HttpStatusCode.NotAcceptable, ct);
                }
                else
                {
                    await _filterService.UninstallFilterDriver();
                    await SendOkAsync(ct);
                }

                return;
            default:
                await SendNotFoundAsync(ct);
                return;
        }
    }
}