using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource;

public sealed class InputSourceMessage
{ 
    public List<InputSourceController> Controllers { get; set; }
    public InputSourceConfiguration CurrentConfiguration { get; set; }
    public string InputSourceKey { get; set; }
}