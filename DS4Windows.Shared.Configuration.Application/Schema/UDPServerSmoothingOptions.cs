using System;
using System.Xml.Serialization;
using PropertyChanged;

namespace DS4Windows.Shared.Configuration.Application.Schema
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "UDPServerSmoothingOptions")]
    public class UDPServerSmoothingOptions
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; }

        [XmlElement(ElementName = "UdpSmoothMinCutoff")]
        public double MinCutoff { get; set; } = 0.4f;

        [XmlElement(ElementName = "UdpSmoothBeta")]
        public double Beta { get; set; } = 0.2f;

        private void OnMinCutoffChanged()
        {
            MinCutoffChanged?.Invoke();
        }

        private void OnBetaChanged()
        {
            BetaChanged?.Invoke();
        }

        public event Action MinCutoffChanged;

        public event Action BetaChanged;
    }
}