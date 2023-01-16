using Vapour.Shared.Devices.Services;

namespace Vapour.Server.InputSource;

/// <summary>
///     Dispatches input source events to clients.
/// </summary>
public interface IInputSourceMessageForwarder
{
    InputSourceCreatedMessage MapInputSourceCreated(IInputSource inputSource);
}