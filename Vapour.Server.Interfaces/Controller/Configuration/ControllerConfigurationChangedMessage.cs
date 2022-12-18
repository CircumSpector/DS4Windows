using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Controller.Configuration;
public class ControllerConfigurationChangedMessage
{
    public string ControllerKey { get; set; }
    public ControllerConfiguration ControllerConfiguration { get; set; }
}
