namespace Vapour.Shared.Devices.Services;

public class HidDeviceOverWinUsbEndpoints
{
    /// <summary>
    ///     Gets the address of the Interrupt IN endpoint (where input reports are to be expected).
    /// </summary>
    public byte InterruptInEndpointAddress { get; init; }

    /// <summary>
    ///     Gets the address of the Interrupt OUT endpoint (where output reports are supposed to be sent over).
    /// </summary>
    public byte InterruptOutEndpointAddress { get; init; }
}