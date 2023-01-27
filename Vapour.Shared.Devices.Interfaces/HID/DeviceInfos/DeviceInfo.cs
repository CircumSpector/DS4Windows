using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

/// <summary>
///     Describes identification characteristics of an input device.
/// </summary>
public abstract class DeviceInfo
{
    /// <summary>
    ///     The Vendor ID.
    /// </summary>
    public abstract int VendorId { get; }

    /// <summary>
    ///     The Product ID.
    /// </summary>
    public abstract int ProductId { get; }

    /// <summary>
    ///     The display name of the device within the app environment.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     The input device type this device is compatible with.
    /// </summary>
    public abstract InputDeviceType DeviceType { get; }

    /// <summary>
    ///     The available features of the device.
    /// </summary>
    public virtual CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.Default;

    /// <summary>
    ///     WinUSB-specific configuration.
    /// </summary>
    public virtual HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } = null;

    public virtual bool IsDongle => false;

    public virtual bool IsLeftDevice => false;

    public virtual bool IsRightDevice => false;
}
