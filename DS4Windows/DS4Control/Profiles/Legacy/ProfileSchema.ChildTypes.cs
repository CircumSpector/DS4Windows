using System.Xml.Serialization;
using DS4Windows;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "LSOutputSettings")]
    public class LSOutputSettings
    {
        [XmlElement(ElementName = "FlickStickSettings")]
        public FlickStickSettings FlickStickSettings { get; set; } = new();
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "RSOutputSettings")]
    public class RSOutputSettings
    {
        [XmlElement(ElementName = "FlickStickSettings")]
        public FlickStickSettings FlickStickSettings { get; set; } = new();
    }

    [AddINotifyPropertyChangedInterface]
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

    [AddINotifyPropertyChangedInterface]
    public class ControlsCollectionEntity
    {
        public string Value { get; set; }

        public string ShiftTrigger { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public abstract class ControlsCollection
    {
        public ControlsCollectionEntity LXNeg { get; set; } = new();
        public ControlsCollectionEntity LXPos { get; set; } = new();
        public ControlsCollectionEntity LYNeg { get; set; } = new();
        public ControlsCollectionEntity LYPos { get; set; } = new();
        public ControlsCollectionEntity RXNeg { get; set; } = new();
        public ControlsCollectionEntity RXPos { get; set; } = new();
        public ControlsCollectionEntity RYNeg { get; set; } = new();
        public ControlsCollectionEntity RYPos { get; set; } = new();
        public ControlsCollectionEntity L1 { get; set; } = new();
        public ControlsCollectionEntity L2 { get; set; } = new();
        public ControlsCollectionEntity L3 { get; set; } = new();
        public ControlsCollectionEntity R1 { get; set; } = new();
        public ControlsCollectionEntity R2 { get; set; } = new();
        public ControlsCollectionEntity R3 { get; set; } = new();
        public ControlsCollectionEntity Square { get; set; } = new();
        public ControlsCollectionEntity Triangle { get; set; } = new();
        public ControlsCollectionEntity Circle { get; set; } = new();
        public ControlsCollectionEntity Cross { get; set; } = new();
        public ControlsCollectionEntity DpadUp { get; set; } = new();
        public ControlsCollectionEntity DpadRight { get; set; } = new();
        public ControlsCollectionEntity DpadDown { get; set; } = new();
        public ControlsCollectionEntity DpadLeft { get; set; } = new();
        public ControlsCollectionEntity PS { get; set; } = new();
        public ControlsCollectionEntity TouchLeft { get; set; } = new();
        public ControlsCollectionEntity TouchUpper { get; set; } = new();
        public ControlsCollectionEntity TouchMulti { get; set; } = new();
        public ControlsCollectionEntity TouchRight { get; set; } = new();
        public ControlsCollectionEntity Share { get; set; } = new();
        public ControlsCollectionEntity Options { get; set; } = new();
        public ControlsCollectionEntity Mute { get; set; } = new();
        public ControlsCollectionEntity GyroXPos { get; set; } = new();
        public ControlsCollectionEntity GyroXNeg { get; set; } = new();
        public ControlsCollectionEntity GyroZPos { get; set; } = new();
        public ControlsCollectionEntity GyroZNeg { get; set; } = new();
        public ControlsCollectionEntity SwipeLeft { get; set; } = new();
        public ControlsCollectionEntity SwipeRight { get; set; } = new();
        public ControlsCollectionEntity SwipeUp { get; set; } = new();
        public ControlsCollectionEntity SwipeDown { get; set; } = new();
        public ControlsCollectionEntity L2FullPull { get; set; } = new();
        public ControlsCollectionEntity R2FullPull { get; set; } = new();
        public ControlsCollectionEntity GyroSwipeLeft { get; set; } = new();
        public ControlsCollectionEntity GyroSwipeRight { get; set; } = new();
        public ControlsCollectionEntity GyroSwipeUp { get; set; } = new();
        public ControlsCollectionEntity GyroSwipeDown { get; set; } = new();
        public ControlsCollectionEntity Capture { get; set; } = new();
        public ControlsCollectionEntity SideL { get; set; } = new();
        public ControlsCollectionEntity SideR { get; set; } = new();
        public ControlsCollectionEntity LSOuter { get; set; } = new();
        public ControlsCollectionEntity RSOuter { get; set; } = new();
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Key")]
    public class ControlKey : ControlsCollection
    {
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "KeyType")]
    public class ControlKeyType : ControlsCollection
    {
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Button")]
    public class ControlButton : ControlsCollection
    {
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Extras")]
    public class ControlExtras : ControlsCollection
    {
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Macro")]
    public class ControlMacro : ControlsCollection
    {
    }

    [AddINotifyPropertyChangedInterface]
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

        [XmlElement(ElementName = "Macro")] 
        public ControlMacro Macros { get; set; } = new();
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "ShiftControl")]
    public class ShiftControl : Control
    {
    }
}
