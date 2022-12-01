using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Controller;
public class ControllerConfigurationChangedMessage
{
    public string ControllerKey { get; set; }
    public ControllerConfiguration ControllerConfiguration { get; set; }
}
