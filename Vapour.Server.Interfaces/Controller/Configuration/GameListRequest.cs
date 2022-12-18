using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Controller.Configuration;
public class GameListRequest
{
    public string ControllerKey { get; set; }
    public GameSource GameSource { get; set; }
}
