using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public interface IDeviceInfo
{
    /// <summary>
    ///     The Vendor ID.
    /// </summary>
    public int Vid { get; }

    /// <summary>
    ///     The Product ID.
    /// </summary>
    public int Pid { get; }

    /// <summary>
    ///     The friendly display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The <see cref="InputDeviceType" />.
    /// </summary>
    InputDeviceType DeviceType { get; }

    /// <summary>
    ///     The <see cref="CompatibleHidDeviceFeatureSet" />.
    /// </summary>
    CompatibleHidDeviceFeatureSet FeatureSet { get; }

    /// <summary>
    ///     The winusb endpoints for devices that support winusb rewrite
    /// </summary>
    HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; }

    /// <summary>
    ///     Gets whether the device is a dongle masquerading a wireless device.
    /// </summary>
    bool IsDongle { get; }
}