using Vapour.Server.InputSource.Configuration;

namespace Vapour.Server.InputSource;

/// <summary>
///     Describes input source events exchangeable between client and server.
/// </summary>
public interface IInputSourceMessageClient
{
    Task InputSourceCreated(InputSourceMessage message);

    Task InputSourceRemoved(InputSourceRemovedMessage message);
    
    Task InputSourceConfigurationChanged(InputSourceConfigurationChangedMessage inputSourceConfiguration);
}