using DS4Windows.Shared.Devices.HID.Devices;
using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Shared.Devices.HID;

public abstract partial class CompatibleHidDevice : ICompatibleHidDevice
{
    /// <summary>
    ///     Craft a new specific input device depending on supplied <see cref="InputDeviceType" />.
    /// </summary>
    /// <param name="deviceType">The <see cref="InputDeviceType" /> to base the new device on.</param>
    /// <param name="source">The source <see cref="HidDevice" /> to copy from.</param>
    /// <param name="featureSet">The <see cref="CompatibleHidDeviceFeatureSet" /> flags to use to create this device.</param>
    /// <param name="services">The <see cref="IServiceProvider" />.</param>
    /// <returns>The new <see cref="CompatibleHidDevice" /> instance.</returns>
    public static CompatibleHidDevice CreateFrom(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider services)
    {
        return deviceType switch
        {
            InputDeviceType.DualShock4 => new DualShock4CompatibleHidDevice(deviceType, source, featureSet, services),
            InputDeviceType.DualSense => new DualSenseCompatibleHidDevice(deviceType, source, featureSet, services),
            InputDeviceType.SwitchPro => new SwitchProCompatibleHidDevice(deviceType, source, featureSet, services),
            InputDeviceType.JoyConL or InputDeviceType.JoyConR => new JoyConCompatibleHidDevice(deviceType, source, featureSet, services),
            
            _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
        };
    }
}