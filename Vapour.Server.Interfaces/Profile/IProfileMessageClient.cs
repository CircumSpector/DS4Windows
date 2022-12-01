using Vapour.Server.Controller;

namespace Vapour.Server.Profile;

/// <summary>
///     Describes profile events exchangeable between client and server.
/// </summary>
public interface IProfileMessageClient
{
    Task ProfileChanged(ProfileChangedMessage message);
}