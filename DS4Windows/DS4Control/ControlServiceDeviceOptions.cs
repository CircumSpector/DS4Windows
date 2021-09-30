using System;
using System.Xml;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.Attributes;

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
        protected InputDeviceType deviceType;

        public ControllerOptionsStore(InputDeviceType deviceType)
        {
            this.deviceType = deviceType;
        }

        public InputDeviceType DeviceType => deviceType;

        [ConfigurationSystemComponent]
        public virtual void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }

        [ConfigurationSystemComponent]
        public virtual void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }
    }

    public class DS4DeviceOptions
    {
        private bool enabled = true;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EnabledChanged;
    }

    public class DS4ControllerOptions : ControllerOptionsStore
    {
        private bool copyCatController;

        public DS4ControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public bool IsCopyCat
        {
            get => copyCatController;
            set
            {
                if (copyCatController == value) return;
                copyCatController = value;
                IsCopyCatChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler IsCopyCatChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("DS4SupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("DS4SupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempRumbleNode = xmlDoc.CreateElement("Copycat");
            tempRumbleNode.InnerText = copyCatController.ToString();
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
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) copyCatController = temp;
            }
        }
    }

    public class DualSenseDeviceOptions
    {
        private bool enabled = true;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EnabledChanged;
    }

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

        private bool enableRumble = true;

        private DualSenseDevice.HapticIntensity hapticIntensity = DualSenseDevice.HapticIntensity.Medium;

        private LEDBarMode ledMode = LEDBarMode.MultipleControllers;

        private MuteLEDMode muteLedMode = MuteLEDMode.Off;

        public DualSenseControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public bool EnableRumble
        {
            get => enableRumble;
            set
            {
                if (enableRumble == value) return;
                enableRumble = value;
                EnableRumbleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DualSenseDevice.HapticIntensity HapticIntensity
        {
            get => hapticIntensity;
            set
            {
                if (hapticIntensity == value) return;
                hapticIntensity = value;
                HapticIntensityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public LEDBarMode LedMode
        {
            get => ledMode;
            set
            {
                if (ledMode == value) return;
                ledMode = value;
                LedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public MuteLEDMode MuteLedMode
        {
            get => muteLedMode;
            set
            {
                if (muteLedMode == value) return;
                muteLedMode = value;
                MuteLedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EnableRumbleChanged;
        public event EventHandler HapticIntensityChanged;
        public event EventHandler LedModeChanged;
        public event EventHandler MuteLedModeChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("DualSenseSupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempRumbleNode = xmlDoc.CreateElement("EnableRumble");
            tempRumbleNode.InnerText = enableRumble.ToString();
            tempOptsNode.AppendChild(tempRumbleNode);

            XmlNode tempRumbleStrengthNode = xmlDoc.CreateElement("RumbleStrength");
            tempRumbleStrengthNode.InnerText = hapticIntensity.ToString();
            tempOptsNode.AppendChild(tempRumbleStrengthNode);

            XmlNode tempLedMode = xmlDoc.CreateElement("LEDBarMode");
            tempLedMode.InnerText = ledMode.ToString();
            tempOptsNode.AppendChild(tempLedMode);

            XmlNode tempMuteLedMode = xmlDoc.CreateElement("MuteLEDMode");
            tempMuteLedMode.InnerText = muteLedMode.ToString();
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
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) enableRumble = temp;

                var itemStrength = baseNode.SelectSingleNode("RumbleStrength");
                if (Enum.TryParse(itemStrength?.InnerText ?? "",
                    out DualSenseDevice.HapticIntensity tempHap))
                    hapticIntensity = tempHap;

                var itemLedMode = baseNode.SelectSingleNode("LEDBarMode");
                if (Enum.TryParse(itemLedMode?.InnerText ?? "",
                    out LEDBarMode tempLED))
                    ledMode = tempLED;

                var itemMuteLedMode = baseNode.SelectSingleNode("MuteLEDMode");
                if (Enum.TryParse(itemMuteLedMode?.InnerText ?? "",
                    out MuteLEDMode tempMuteLED))
                    muteLedMode = tempMuteLED;
            }
        }
    }

    public class SwitchProDeviceOptions
    {
        private bool enabled = true;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EnabledChanged;
    }

    public class SwitchProControllerOptions : ControllerOptionsStore
    {
        private bool enableHomeLED = true;

        public SwitchProControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EnableHomeLEDChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("SwitchProSupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = enableHomeLED.ToString();
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
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) enableHomeLED = temp;
            }
        }
    }

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

        private bool enabled = true;

        private JoinedGyroProvider joinGyroProv = JoinedGyroProvider.JoyConR;

        private LinkMode linkedMode = LinkMode.Joined;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public LinkMode LinkedMode
        {
            get => linkedMode;
            set
            {
                if (linkedMode == value) return;
                linkedMode = value;
            }
        }

        public JoinedGyroProvider JoinGyroProv
        {
            get => joinGyroProv;
            set
            {
                if (joinGyroProv == value) return;
                joinGyroProv = value;
            }
        }

        public event EventHandler EnabledChanged;
    }

    public class JoyConControllerOptions : ControllerOptionsStore
    {
        private bool enableHomeLED = true;

        public JoyConControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EnableHomeLEDChanged;

        [ConfigurationSystemComponent]
        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            var tempOptsNode = node.SelectSingleNode("JoyConSupportSettings");
            if (tempOptsNode == null)
                tempOptsNode = xmlDoc.CreateElement("JoyConSupportSettings");
            else
                tempOptsNode.RemoveAll();

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = enableHomeLED.ToString();
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
                if (bool.TryParse(item?.InnerText ?? "", out var temp)) enableHomeLED = temp;
            }
        }
    }
}