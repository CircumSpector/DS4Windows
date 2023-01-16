using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource.Configuration;
public class GameListRequest
{
    public string InputSourceKey { get; set; }
    public GameSource GameSource { get; set; }
}
