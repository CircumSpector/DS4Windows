namespace DS4Windows.InputDevices
{
    public enum InputDeviceType : uint
    {
        DS4,
        SwitchPro,
        JoyConL,
        JoyConR,
        DualSense
    }

    public abstract class InputDeviceFactory
    {
        public static DS4Device CreateDevice(
            InputDeviceType deviceType,
            HidDevice hidDevice,
            string disName,
            VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4
        )
        {
            switch (deviceType)
            {
                case InputDeviceType.DS4:
                    return new DS4Device(hidDevice, disName, featureSet);
                case InputDeviceType.SwitchPro:
                    return new SwitchProDevice(hidDevice, disName, featureSet);
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    return new JoyConDevice(hidDevice, disName, featureSet);
                case InputDeviceType.DualSense:
                    return new DualSenseDevice(hidDevice, disName, featureSet);
            }

            return null;
        }
    }
}