using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource.Configuration;

public sealed class InputSourceSetConfigRequest
{
    public string InputSourceKey { get; init; }

    public InputSourceConfiguration InputSourceConfiguration { get; init; }
}