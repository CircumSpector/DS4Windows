using System;
using System.Xml;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.Attributes;
using JetBrains.Annotations;
using PropertyChanged;

namespace DS4Windows
{
    public class ControlServiceDeviceOptions
    {
        public DS4DeviceOptions Ds4DeviceOpts { get; set; } = new();

        public DualSenseDeviceOptions DualSenseOpts { get; set; } = new();

        public SwitchProDeviceOptions SwitchProDeviceOpts { get; set; } = new();

        public JoyConDeviceOptions JoyConDeviceOpts { get; set; } = new();

        /// <summary>
        ///     If enabled then DS4Windows shows additional log messages when a gamepad is connected (may be useful to diagnose
        ///     connection problems).
        ///     This option is not persistent (ie. not saved into config files), so if enabled then it is reset back to FALSE when
        ///     DS4Windows is restarted.
        /// </summary>
        public bool VerboseLogMessages { get; set; }
    }

    public abstract class ControllerOptionsStore
    {
        protected ControllerOptionsStore(InputDeviceType deviceType)
        {
            DeviceType = deviceType;
        }

        public InputDeviceType DeviceType { get; }

        [ConfigurationSystemComponent]
        public virtual void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }

        [ConfigurationSystemComponent]
        public virtual void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class DS4DeviceOptions
    {
        public bool Enabled { get; set; } = true;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [AddINotifyPropertyChangedInterface]
    public class DS4ControllerOptions : ControllerOptionsStore
    {
        public DS4ControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public bool IsCopyCat { get; set; }

        [UsedImplicitly]
        private void OnIsCopyCatChanged()
        {
            IsCopyCatChanged?.Invoke();
        }

        public event Action IsCopyCatChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("DS4SupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("DS4SupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempRumbleNode = xmlDoc.CreateElement("Copycat");
            tempRumbleNode.InnerText = IsCopyCat.ToString();
            tempOptsNode.AppendChild(tempRumbleNode);

            node.AppendChild(tempOptsNode);
        }

        [ConfigurationSystemComponent]
        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var baseNode = node.SelectSingleNode("DS4SupportSettings");
            if (baseNode != null)
            {
                var item = baseNode.SelectSingleNode("Copycat");
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) IsCopyCat = temp;
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class DualSenseDeviceOptions
    {
        public bool Enabled { get; set; } = true;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [AddINotifyPropertyChangedInterface]
    public class DualSenseControllerOptions : ControllerOptionsStore
    {
        public enum LEDBarMode : ushort
        {
            Off,
            MultipleControllers,
            BatteryPercentage,
            On
        }

        public enum MuteLEDMode : ushort
        {
            Off,
            On,
            Pulse
        }

        public DualSenseControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public bool EnableRumble { get; set; } = true;

        public DualSenseDevice.HapticIntensity HapticIntensity { get; set; } = DualSenseDevice.HapticIntensity.Medium;

        public LEDBarMode LedMode { get; set; } = LEDBarMode.MultipleControllers;

        public MuteLEDMode MuteLedMode { get; set; } = MuteLEDMode.Off;

        [UsedImplicitly]
        private void OnEnableRumbleChanged()
        {
            EnableRumbleChanged?.Invoke();
        }

        [UsedImplicitly]
        private void OnHapticIntensityChanged()
        {
            HapticIntensityChanged?.Invoke();
        }

        [UsedImplicitly]
        private void OnLedModeChanged()
        {
            LedModeChanged?.Invoke();
        }

        [UsedImplicitly]
        private void OnMuteLedModeChanged()
        {
            MuteLedModeChanged?.Invoke();
        }

        public event Action EnableRumbleChanged;
        public event Action HapticIntensityChanged;
        public event Action LedModeChanged;
        public event Action MuteLedModeChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("DualSenseSupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempRumbleNode = xmlDoc.CreateElement("EnableRumble");
            tempRumbleNode.InnerText = EnableRumble.ToString();
            tempOptsNode.AppendChild(tempRumbleNode);

            XmlNode tempRumbleStrengthNode = xmlDoc.CreateElement("RumbleStrength");
            tempRumbleStrengthNode.InnerText = HapticIntensity.ToString();
            tempOptsNode.AppendChild(tempRumbleStrengthNode);

            XmlNode tempLedMode = xmlDoc.CreateElement("LEDBarMode");
            tempLedMode.InnerText = LedMode.ToString();
            tempOptsNode.AppendChild(tempLedMode);

            XmlNode tempMuteLedMode = xmlDoc.CreateElement("MuteLEDMode");
            tempMuteLedMode.InnerText = MuteLedMode.ToString();
            tempOptsNode.AppendChild(tempMuteLedMode);

            node.AppendChild(tempOptsNode);
        }

        [ConfigurationSystemComponent]
        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var baseNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (baseNode != null)
            {
                var item = baseNode.SelectSingleNode("EnableRumble");
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) EnableRumble = temp;

                var itemStrength = baseNode.SelectSingleNode("RumbleStrength");
                if (Enum.TryParse(itemStrength?.InnerText ?? "",
                    out DualSenseDevice.HapticIntensity tempHap))
                    HapticIntensity = tempHap;

                var itemLedMode = baseNode.SelectSingleNode("LEDBarMode");
                if (Enum.TryParse(itemLedMode?.InnerText ?? "",
                    out LEDBarMode tempLED))
                    LedMode = tempLED;

                var itemMuteLedMode = baseNode.SelectSingleNode("MuteLEDMode");
                if (Enum.TryParse(itemMuteLedMode?.InnerText ?? "",
                    out MuteLEDMode tempMuteLED))
                    MuteLedMode = tempMuteLED;
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class SwitchProDeviceOptions
    {
        public bool Enabled { get; set; } = true;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [AddINotifyPropertyChangedInterface]
    public class SwitchProControllerOptions : ControllerOptionsStore
    {
        public SwitchProControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public bool EnableHomeLED { get; set; } = true;

        [UsedImplicitly]
        private void OnEnableHomeLEDChanged()
        {
            EnableHomeLEDChanged?.Invoke();
        }

        public event Action EnableHomeLEDChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("SwitchProSupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = EnableHomeLED.ToString();
            tempOptsNode.AppendChild(tempElement);

            node.AppendChild(tempOptsNode);
        }

        [ConfigurationSystemComponent]
        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var baseNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (baseNode != null)
            {
                var item = baseNode.SelectSingleNode("EnableHomeLED");
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) EnableHomeLED = temp;
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class JoyConDeviceOptions
    {
        public enum JoinedGyroProvider : ushort
        {
            JoyConL,
            JoyConR
        }

        public enum LinkMode : ushort
        {
            Split,
            Joined
        }

        public bool Enabled { get; set; } = true;

        public LinkMode LinkedMode { get; set; } = LinkMode.Joined;

        public JoinedGyroProvider JoinGyroProv { get; set; } = JoinedGyroProvider.JoyConR;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [AddINotifyPropertyChangedInterface]
    public class JoyConControllerOptions : ControllerOptionsStore
    {
        public JoyConControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public bool EnableHomeLED { get; set; } = true;

        private void OnEnableHomeLEDChanged()
        {
            EnableHomeLEDChanged?.Invoke();
        }

        public event Action EnableHomeLEDChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("JoyConSupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("JoyConSupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = EnableHomeLED.ToString();
            tempOptsNode.AppendChild(tempElement);

            node.AppendChild(tempOptsNode);
        }

        [ConfigurationSystemComponent]
        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var baseNode = node.SelectSingleNode("JoyConSupportSettings");
            if (baseNode != null)
            {
                var item = baseNode.SelectSingleNode("EnableHomeLED");
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) EnableHomeLED = temp;
            }
        }
    }
}