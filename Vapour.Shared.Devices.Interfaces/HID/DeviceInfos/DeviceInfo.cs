using System.Diagnostics.CodeAnalysis;

using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

/// <summary>
///     Describes identification characteristics of an input device.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
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
    ///     The available features of the device.
    /// </summary>
    public virtual CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.Default;

    /// <summary>
    ///     WinUSB-specific configuration.
    /// </summary>
    public virtual HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } = null;

    /// <summary>
    ///     Gets whether this is a wireless device but connected via USB dongle.
    /// </summary>
    public virtual bool IsDongle => false;

    /// <summary>
    ///     If true, identifies the left piece of hardware in case of a split handheld device.
    /// </summary>
    public virtual bool IsLeftDevice => false;

    /// <summary>
    ///     If true, identifies the right piece of hardware in case of a split handheld device.
    /// </summary>
    public virtual bool IsRightDevice => false;

    /// <summary>
    ///     Gets whether this device can be filtered when connected via Bluetooth.
    /// </summary>
    public virtual bool IsBtFilterable => false;
}
