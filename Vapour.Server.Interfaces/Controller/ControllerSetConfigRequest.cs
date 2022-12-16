using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Controller;

public sealed class ControllerSetConfigRequest
{
    public string ControllerKey { get; init; }

    public ControllerConfiguration ControllerConfiguration { get; init; }
}