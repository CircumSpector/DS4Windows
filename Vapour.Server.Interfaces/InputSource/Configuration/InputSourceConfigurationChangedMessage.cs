using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource.Configuration;
public class InputSourceConfigurationChangedMessage
{
    public string InputSourceKey { get; set; }
    public InputSourceConfiguration InputSourceConfiguration { get; set; }
}
