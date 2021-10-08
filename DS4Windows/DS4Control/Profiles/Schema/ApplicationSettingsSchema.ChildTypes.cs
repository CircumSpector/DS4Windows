using System;
using System.Xml.Serialization;
using DS4Windows;
using JetBrains.Annotations;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "UDPServerSmoothingOptions")]
    public class UDPServerSmoothingOptions
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; } = false;

        [XmlElement(ElementName = "UdpSmoothMinCutoff")]
        public double MinCutoff { get; set; } = 0.4f;

        [XmlElement(ElementName = "UdpSmoothBeta")]
        public double Beta { get; set; } = 0.2f;

        public event Action UdpSmoothMinCutoffChanged;

        public event Action UdpSmoothBetaChanged;

        [UsedImplicitly]
        private void OnMinCutoffChanged()
        {
            UdpSmoothMinCutoffChanged?.Invoke();
        }

        [UsedImplicitly]
        private void OnBetaChanged()
        {
            UdpSmoothBetaChanged?.Invoke();
        }
    }

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
        public bool VerboseLogMessages { get; set; }
    }
}
