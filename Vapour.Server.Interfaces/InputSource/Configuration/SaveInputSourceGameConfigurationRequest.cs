using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource.Configuration;
public class SaveInputSourceGameConfigurationRequest
{
    public string InputSourceKey { get; set; }
    public GameInfo GameInfo { get; set; }
    public InputSourceConfiguration InputSourceConfiguration { get; set; }
}
