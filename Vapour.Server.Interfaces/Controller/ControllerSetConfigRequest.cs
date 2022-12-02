using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Controller;
public class ControllerSetConfigRequest
{
    public string ControllerKey { get; set; }
    public ControllerConfiguration ControllerConfiguration { get; set; }
}
