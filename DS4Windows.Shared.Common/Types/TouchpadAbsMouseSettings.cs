using System.Xml.Serialization;
using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "TouchpadAbsMouseSettings")]
    public class TouchpadAbsMouseSettings
    {
        public const int DefaultMaxZoneX = 90;
        public const int DefaultMaxZoneY = 90;
        public const bool DefaultSnapCenter = false;

        [XmlElement(ElementName = "MaxZoneX")] public int MaxZoneX { get; set; }

        [XmlElement(ElementName = "MaxZoneY")] public int MaxZoneY { get; set; }

        [XmlElement(ElementName = "SnapToCenter")]
        public bool SnapToCenter { get; set; }
    }
}