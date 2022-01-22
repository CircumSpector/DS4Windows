using System;
using DS4Windows.Shared.Devices.HID.Devices;

namespace DS4Windows.Shared.Devices.HID
{
    public abstract partial class CompatibleHidDevice
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
            switch (deviceType)
            {
                case InputDeviceType.DualShock4:
                    return new DualShock4CompatibleHidDevice(deviceType, source, featureSet, services);
                case InputDeviceType.DualSense:
                    return new DualSenseCompatibleHidDevice(deviceType, source, featureSet, services);
                case InputDeviceType.SwitchPro:
                    return new SwitchProCompatibleHidDevice(deviceType, source, featureSet, services);
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    return new JoyConCompatibleHidDevice(deviceType, source, featureSet, services);
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }
        }
    }
}