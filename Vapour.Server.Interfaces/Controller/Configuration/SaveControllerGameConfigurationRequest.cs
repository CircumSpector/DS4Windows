using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Controller.Configuration;
public class SaveControllerGameConfigurationRequest
{
    public string ControllerKey { get; set; }
    public GameInfo GameInfo { get; set; }
    public ControllerConfiguration ControllerConfiguration { get; set; }
}
