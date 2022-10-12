using System.Windows.Media.Animation;

using FastEndpoints;
using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class IdentinateControllerEndpoint : Endpoint<IdentinateControllerRequest>
{
    private readonly IControllerManagerService _controllerManagerService;

    public IdentinateControllerEndpoint(IControllerManagerService controllerManagerService)
    {
        _controllerManagerService = controllerManagerService;
    }

    public override void Configure()
    {
        Post("/controller/host/identinate/{instanceId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Identinates the controller";
            s.Description = "Identinates the controller";
            s.Responses[200] = "Controller identinated properly";
        });
    }

    public override async Task HandleAsync(IdentinateControllerRequest req, CancellationToken ct)
    {
        _controllerManagerService.IdentinateController(req.InstanceId);

        await SendOkAsync(ct);
    }
}
