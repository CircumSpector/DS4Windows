using Vapour.Server.Controller;

namespace Vapour.Server.Profile;

public interface IProfileMessageClient
{
    Task ProfileChanged(ProfileChangedMessage message);
}