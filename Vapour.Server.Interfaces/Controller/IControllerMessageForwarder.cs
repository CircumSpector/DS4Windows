using Vapour.Shared.Devices.HID;

namespace Vapour.Server.Controller;

/// <summary>
///     Dispatches controller events to clients.
/// </summary>
public interface IControllerMessageForwarder
{
    ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice);

    Task SendIsHostRunning(bool isRunning);
}