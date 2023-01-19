using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource;

public sealed class InputSourceMessage
{ 
    public InputSourceController Controller1 { get; set; }
    public InputSourceController Controller2 { get; set; }
    public InputSourceConfiguration CurrentConfiguration { get; set; }
    public string InputSourceKey { get; set; }
}