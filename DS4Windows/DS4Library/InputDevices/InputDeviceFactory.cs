using System;
using DS4Windows;
using DS4Windows.InputDevices;

namespace DS4WinWPF.DS4Library.InputDevices
{
    public enum InputDeviceType : uint
    {
        /// <summary>
        ///     DualShock 4 variants
        /// </summary>
        DualShock4,

        /// <summary>
        ///     Switch Pro Controller
        /// </summary>
        SwitchPro,

        /// <summary>
        ///     Left JoyCons Controller
        /// </summary>
        JoyConL,

        /// <summary>
        ///      Right JoyCons Controller
        /// </summary>
        JoyConR,

        /// <summary>
        ///     DualSense Controller
        /// </summary>
        DualSense
    }

    public interface IInputDeviceFactory
    {
        /// <summary>
        ///     Creates a new <see cref="DS4Device"/> based on <see cref="HidDevice"/>.
        /// </summary>
        /// <param name="deviceType">The <see cref="InputDeviceType"/> to instantiate.</param>
        /// <param name="hidDevice">The underlying <see cref="HidDevice"/> this device is based on.</param>
        /// <param name="displayName">The display name to use.</param>
        /// <param name="featureSet">Features altering device behaviour.</param>
        /// <returns>The new <see cref="DS4Device"/>.</returns>
        DS4Device CreateDevice(
            InputDeviceType deviceType,
            HidDevice hidDevice,
            string displayName,
            VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4
        );
    }

    /// <summary>
    ///     Provides factory method to create new controller object.
    /// </summary>
    public class InputDeviceFactory : IInputDeviceFactory
    {
        /// <summary>
        ///     Creates a new <see cref="DS4Device"/> based on <see cref="HidDevice"/>.
        /// </summary>
        /// <param name="deviceType">The <see cref="InputDeviceType"/> to instantiate.</param>
        /// <param name="hidDevice">The underlying <see cref="HidDevice"/> this device is based on.</param>
        /// <param name="displayName">The display name to use.</param>
        /// <param name="featureSet">Features altering device behaviour.</param>
        /// <returns>The new <see cref="DS4Device"/>.</returns>
        public DS4Device CreateDevice(
            InputDeviceType deviceType,
            HidDevice hidDevice,
            string displayName,
            VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4
        )
        {
            switch (deviceType)
            {
                case InputDeviceType.DualShock4:
                    return new DS4Device(hidDevice, displayName, featureSet);
                case InputDeviceType.SwitchPro:
                    return new SwitchProDevice(hidDevice, displayName, featureSet);
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    return new JoyConDevice(hidDevice, displayName, featureSet);
                case InputDeviceType.DualSense:
                    return new DualSenseDevice(hidDevice, displayName, featureSet);
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }
        }
    }
}