using System;

namespace DS4Windows.InputDevices
{
    public enum InputDeviceType : uint
    {
        DualShock4,
        SwitchPro,
        JoyConL,
        JoyConR,
        DualSense
    }

    public interface IInputDeviceFactory
    {
        DS4Device CreateDevice(
            InputDeviceType deviceType,
            HidDevice hidDevice,
            string displayName,
            VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4
        );
    }

    public class InputDeviceFactory : IInputDeviceFactory
    {
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