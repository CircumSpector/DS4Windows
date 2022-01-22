using System;
using System.Xml.Serialization;
using DS4Windows.Shared.Common.Types.DualSense;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PropertyChanged;

namespace DS4Windows.Shared.Configuration.Application.Schema
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "DeviceOptions")]
    public class DeviceOptions
    {
        [XmlElement(ElementName = "DS4SupportSettings")]
        public DS4DeviceOptions DS4SupportSettings { get; set; } = new();

        [XmlElement(ElementName = "DualSenseSupportSettings")]
        public DualSenseDeviceOptions DualSenseSupportSettings { get; set; } = new();

        [XmlElement(ElementName = "SwitchProSupportSettings")]
        public SwitchProDeviceOptions SwitchProSupportSettings { get; set; } = new();

        [XmlElement(ElementName = "JoyConSupportSettings")]
        public JoyConDeviceOptions JoyConSupportSettings { get; set; } = new();

        /// <summary>
        ///     If enabled then DS4Windows shows additional log messages when a gamepad is connected (may be useful to diagnose
        ///     connection problems).
        ///     This option is not persistent (ie. not saved into config files), so if enabled then it is reset back to FALSE when
        ///     DS4Windows is restarted.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool VerboseLogMessages { get; set; }
    }

    [XmlRoot(ElementName = "Controller")]
    public abstract class ControllerOptionsStore
    {
    }

    [XmlRoot(ElementName = "DS4SupportSettings")]
    [AddINotifyPropertyChangedInterface]
    public class DS4DeviceOptions
    {
        [XmlElement(ElementName = "Enabled")] public bool Enabled { get; set; } = true;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [XmlRoot(ElementName = "DS4SupportSettings")]
    [AddINotifyPropertyChangedInterface]
    public class DS4ControllerOptions : ControllerOptionsStore
    {
        [XmlElement(ElementName = "Copycat")] public bool IsCopyCat { get; set; }

        [UsedImplicitly]
        private void OnIsCopyCatChanged()
        {
            IsCopyCatChanged?.Invoke();
        }

        public event Action IsCopyCatChanged;
    }

    [XmlRoot(ElementName = "DualSenseSupportSettings")]
    [AddINotifyPropertyChangedInterface]
    public class DualSenseDeviceOptions
    {
        [XmlElement(ElementName = "Enabled")] public bool Enabled { get; set; } = true;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [XmlRoot(ElementName = "DualSenseSupportSettings")]
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

        [XmlElement(ElementName = "EnableRumble")]
        public bool EnableRumble { get; set; } = true;

        [XmlElement(ElementName = "RumbleStrength")]
        public HapticIntensity HapticIntensity { get; set; } = HapticIntensity.Medium;

        [XmlElement(ElementName = "LEDBarMode")]
        public LEDBarMode LedMode { get; set; } = LEDBarMode.MultipleControllers;

        [XmlElement(ElementName = "MuteLEDMode")]
        public MuteLEDMode MuteLedMode { get; set; } = MuteLEDMode.Off;

        public bool HasUserConfirmedProblematicFirmware { get; set; }

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
    }

    [XmlRoot(ElementName = "SwitchProSupportSettings")]
    [AddINotifyPropertyChangedInterface]
    public class SwitchProDeviceOptions
    {
        [XmlElement(ElementName = "Enabled")] public bool Enabled { get; set; } = true;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [XmlRoot(ElementName = "SwitchProSupportSettings")]
    [AddINotifyPropertyChangedInterface]
    public class SwitchProControllerOptions : ControllerOptionsStore
    {
        [XmlElement(ElementName = "EnableHomeLED")]
        public bool EnableHomeLED { get; set; } = true;

        [UsedImplicitly]
        private void OnEnableHomeLEDChanged()
        {
            EnableHomeLEDChanged?.Invoke();
        }

        public event Action EnableHomeLEDChanged;
    }

    [XmlRoot(ElementName = "JoyConSupportSettings")]
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

        [XmlElement(ElementName = "LinkMode")] public LinkMode LinkedMode { get; set; } = LinkMode.Joined;

        [XmlElement(ElementName = "JoinedGyroProvider")]
        public JoinedGyroProvider JoinGyroProv { get; set; } = JoinedGyroProvider.JoyConR;

        [UsedImplicitly]
        private void OnEnabledChanged()
        {
            EnabledChanged?.Invoke();
        }

        public event Action EnabledChanged;
    }

    [XmlRoot(ElementName = "JoyConSupportSettings")]
    [AddINotifyPropertyChangedInterface]
    public class JoyConControllerOptions : ControllerOptionsStore
    {
        [XmlElement(ElementName = "EnableHomeLED")]
        public bool EnableHomeLED { get; set; } = true;

        [UsedImplicitly]
        private void OnEnableHomeLEDChanged()
        {
            EnableHomeLEDChanged?.Invoke();
        }

        public event Action EnableHomeLEDChanged;
    }
}