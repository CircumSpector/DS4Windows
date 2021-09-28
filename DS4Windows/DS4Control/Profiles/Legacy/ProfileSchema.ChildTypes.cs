using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "LSAxialDeadOptions")]
    public class LSAxialDeadOptions
    {
        [XmlElement(ElementName = "DeadZoneX")]
        public int DeadZoneX { get; set; }

        [XmlElement(ElementName = "DeadZoneY")]
        public int DeadZoneY { get; set; }

        [XmlElement(ElementName = "MaxZoneX")]
        public int MaxZoneX { get; set; }

        [XmlElement(ElementName = "MaxZoneY")]
        public int MaxZoneY { get; set; }

        [XmlElement(ElementName = "AntiDeadZoneX")]
        public int AntiDeadZoneX { get; set; }

        [XmlElement(ElementName = "AntiDeadZoneY")]
        public int AntiDeadZoneY { get; set; }

        [XmlElement(ElementName = "MaxOutputX")]
        public double MaxOutputX { get; set; }

        [XmlElement(ElementName = "MaxOutputY")]
        public double MaxOutputY { get; set; }
    }

    [XmlRoot(ElementName = "RSAxialDeadOptions")]
    public class RSAxialDeadOptions
    {
        [XmlElement(ElementName = "DeadZoneX")]
        public int DeadZoneX { get; set; }

        [XmlElement(ElementName = "DeadZoneY")]
        public int DeadZoneY { get; set; }

        [XmlElement(ElementName = "MaxZoneX")]
        public int MaxZoneX { get; set; }

        [XmlElement(ElementName = "MaxZoneY")]
        public int MaxZoneY { get; set; }

        [XmlElement(ElementName = "AntiDeadZoneX")]
        public int AntiDeadZoneX { get; set; }

        [XmlElement(ElementName = "AntiDeadZoneY")]
        public int AntiDeadZoneY { get; set; }

        [XmlElement(ElementName = "MaxOutputX")]
        public double MaxOutputX { get; set; }

        [XmlElement(ElementName = "MaxOutputY")]
        public double MaxOutputY { get; set; }
    }

    [XmlRoot(ElementName = "SASteeringWheelSmoothingOptions")]
    public class SASteeringWheelSmoothingOptions
    {
        [XmlElement(ElementName = "SASteeringWheelUseSmoothing")]
        public bool SASteeringWheelUseSmoothing { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothMinCutoff")]
        public double SASteeringWheelSmoothMinCutoff { get; set; } = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;

        [XmlElement(ElementName = "SASteeringWheelSmoothBeta")]
        public double SASteeringWheelSmoothBeta { get; set; } = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
    }

    [XmlRoot(ElementName = "GyroControlsSettings")]
    public class GyroControlsSettings
    {
        [XmlElement(ElementName = "Triggers")]
        public string Triggers { get; set; } = "-1";

        [XmlElement(ElementName = "TriggerCond")]
        public string TriggerCond { get; set; } = "and";

        [XmlElement(ElementName = "TriggerTurns")]
        public bool TriggerTurns { get; set; } = true;

        [XmlElement(ElementName = "Toggle")] 
        public bool Toggle { get; set; } = false;
    }

    [XmlRoot(ElementName = "GyroMouseSmoothingSettings")]
    public class GyroMouseSmoothingSettings
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; }

        [XmlElement(ElementName = "SmoothingMethod")]
        public string SmoothingMethod { get; set; }

        [XmlElement(ElementName = "SmoothingWeight")]
        public double SmoothingWeight { get; set; } = 0.5;

        [XmlElement(ElementName = "SmoothingMinCutoff")]
        public double SmoothingMinCutoff { get; set; } = GyroMouseStickInfo.DEFAULT_MINCUTOFF;

        [XmlElement(ElementName = "SmoothingBeta")]
        public double SmoothingBeta { get; set; } = GyroMouseStickInfo.DEFAULT_BETA;
    }

    [XmlRoot(ElementName = "GyroMouseStickSmoothingSettings")]
    public class GyroMouseStickSmoothingSettings
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; }

        [XmlElement(ElementName = "SmoothingMethod")]
        public string SmoothingMethod { get; set; }

        [XmlElement(ElementName = "SmoothingWeight")]
        public double SmoothingWeight { get; set; } = 0.5;

        [XmlElement(ElementName = "SmoothingMinCutoff")]
        public double SmoothingMinCutoff { get; set; } = GyroMouseStickInfo.DEFAULT_MINCUTOFF;

        [XmlElement(ElementName = "SmoothingBeta")]
        public double SmoothingBeta { get; set; } = GyroMouseStickInfo.DEFAULT_BETA;
    }

    [XmlRoot(ElementName = "GyroSwipeSettings")]
    public class GyroSwipeSettings
    {
        [XmlElement(ElementName = "DeadZoneX")]
        public int DeadZoneX { get; set; } = 80;

        [XmlElement(ElementName = "DeadZoneY")]
        public int DeadZoneY { get; set; } = 80;

        [XmlElement(ElementName = "Triggers")]
        public string Triggers { get; set; } = "-1";

        [XmlElement(ElementName = "TriggerCond")]
        public string TriggerCond { get; set; } = "and";

        [XmlElement(ElementName = "TriggerTurns")]
        public bool TriggerTurns { get; set; } = true;

        [XmlElement(ElementName = "XAxis")]
        public GyroDirectionalSwipeInfo.XAxisSwipe XAxis { get; set; } = GyroDirectionalSwipeInfo.XAxisSwipe.Yaw;

        [XmlElement(ElementName = "DelayTime")]
        public int DelayTime { get; set; } = 0;
    }

    [XmlRoot(ElementName = "FlickStickSettings")]
    public class FlickStickSettings
    {
        [XmlElement(ElementName = "RealWorldCalibration")]
        public double RealWorldCalibration { get; set; }

        [XmlElement(ElementName = "FlickThreshold")]
        public double FlickThreshold { get; set; }

        [XmlElement(ElementName = "FlickTime")]
        public double FlickTime { get; set; }

        [XmlElement(ElementName = "MinAngleThreshold")]
        public double MinAngleThreshold { get; set; }
    }

    [XmlRoot(ElementName = "LSOutputSettings")]
    public class LSOutputSettings
    {
        [XmlElement(ElementName = "FlickStickSettings")]
        public FlickStickSettings FlickStickSettings { get; set; } = new();
    }

    [XmlRoot(ElementName = "RSOutputSettings")]
    public class RSOutputSettings
    {
        [XmlElement(ElementName = "FlickStickSettings")]
        public FlickStickSettings FlickStickSettings { get; set; } = new();
    }

    [XmlRoot(ElementName = "TouchpadAbsMouseSettings")]
    public class TouchpadAbsMouseSettings
    {
        [XmlElement(ElementName = "MaxZoneX")]
        public int MaxZoneX { get; set; }

        [XmlElement(ElementName = "MaxZoneY")]
        public int MaxZoneY { get; set; }

        [XmlElement(ElementName = "SnapToCenter")]
        public bool SnapToCenter { get; set; }
    }

    public abstract class ControlsCollection
    {
        public string LXNeg { get; set; }
        public string LXPos { get; set; }
        public string LYNeg { get; set; }
        public string LYPos { get; set; }
        public string RXNeg { get; set; }
        public string RXPos { get; set; }
        public string RYNeg { get; set; }
        public string RYPos { get; set; }
        public string L1 { get; set; }
        public string L2 { get; set; }
        public string L3 { get; set; }
        public string R1 { get; set; }
        public string R2 { get; set; }
        public string R3 { get; set; }
        public string Square { get; set; }
        public string Triangle { get; set; }
        public string Circle { get; set; }
        public string Cross { get; set; }
        public string DpadUp { get; set; }
        public string DpadRight { get; set; }
        public string DpadDown { get; set; }
        public string DpadLeft { get; set; }
        public string PS { get; set; }
        public string TouchLeft { get; set; }
        public string TouchUpper { get; set; }
        public string TouchMulti { get; set; }
        public string TouchRight { get; set; }
        public string Share { get; set; }
        public string Options { get; set; }
        public string Mute { get; set; }
        public string GyroXPos { get; set; }
        public string GyroXNeg { get; set; }
        public string GyroZPos { get; set; }
        public string GyroZNeg { get; set; }
        public string SwipeLeft { get; set; }
        public string SwipeRight { get; set; }
        public string SwipeUp { get; set; }
        public string SwipeDown { get; set; }
        public string L2FullPull { get; set; }
        public string R2FullPull { get; set; }
        public string GyroSwipeLeft { get; set; }
        public string GyroSwipeRight { get; set; }
        public string GyroSwipeUp { get; set; }
        public string GyroSwipeDown { get; set; }
        public string Capture { get; set; }
        public string SideL { get; set; }
        public string SideR { get; set; }
        public string LSOuter { get; set; }
        public string RSOuter { get; set; }
    }

    [XmlRoot(ElementName = "Key")]
    public class ControlKey : ControlsCollection
    {
    }

    [XmlRoot(ElementName = "KeyType")]
    public class ControlKeyType : ControlsCollection
    {
    }

    [XmlRoot(ElementName = "Button")]
    public class ControlButton : ControlsCollection
    {
    }

    [XmlRoot(ElementName = "Extras")]
    public class ControlExtras : ControlsCollection
    {
    }

    [XmlRoot(ElementName = "Control")]
    public class Control
    {
        [XmlElement(ElementName = "Key")] 
        public ControlKey Keys { get; set; } = new();

        [XmlElement(ElementName = "KeyType")] 
        public ControlKeyType KeyTypes { get; set; } = new();

        [XmlElement(ElementName = "Button")] 
        public ControlButton Buttons { get; set; } = new();

        [XmlElement(ElementName = "Extras")] 
        public ControlExtras Extras { get; set; } = new();
    }
}
