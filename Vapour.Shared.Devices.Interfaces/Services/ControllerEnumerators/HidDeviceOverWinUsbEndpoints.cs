namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Represents meta-data for WinUSB endpoints (pipes) and behaviour.
/// </summary>
public sealed class HidDeviceOverWinUsbEndpoints
{
    /// <summary>
    ///     Gets the address of the Interrupt IN endpoint (where input reports are to be expected).
    /// </summary>
    public byte InterruptInEndpointAddress { get; init; }

    /// <summary>
    ///     Gets the address of the Interrupt OUT endpoint (where output reports are supposed to be sent over).
    /// </summary>
    public byte InterruptOutEndpointAddress { get; init; }

    /// <summary>
    ///     Gets whether the first found endpoint endpoint can be used if <see cref="InterruptInEndpointAddress" /> or
    ///     <see cref="InterruptOutEndpointAddress" /> is not found in the device descriptor.
    /// </summary>
    /// <remarks>
    ///     This can be useful on devices that spoof hardware IDs so auto-detection can be used as a fallback mechanism to
    ///     get those devices to work.
    /// </remarks>
    public bool AllowAutoDetection { get; init; } = true;
}