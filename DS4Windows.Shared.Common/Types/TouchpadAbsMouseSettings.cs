using System.Xml.Serialization;
using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "TouchpadAbsMouseSettings")]
    public class TouchpadAbsMouseSettings
    {
        [XmlElement(ElementName = "MaxZoneX")] public int MaxZoneX { get; set; }

        [XmlElement(ElementName = "MaxZoneY")] public int MaxZoneY { get; set; }

        [XmlElement(ElementName = "SnapToCenter")]
        public bool SnapToCenter { get; set; }
    }
}