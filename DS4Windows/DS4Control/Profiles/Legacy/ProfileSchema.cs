using System;
using System.Xml.Serialization;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    /*
     * POCO definitions auto-generated from current XML file format for Profiles
     */

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
        public int MaxOutputX { get; set; }

        [XmlElement(ElementName = "MaxOutputY")]
        public int MaxOutputY { get; set; }
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
        public int MaxOutputX { get; set; }

        [XmlElement(ElementName = "MaxOutputY")]
        public int MaxOutputY { get; set; }
    }

    [XmlRoot(ElementName = "SASteeringWheelSmoothingOptions")]
    public class SASteeringWheelSmoothingOptions
    {
        [XmlElement(ElementName = "SASteeringWheelUseSmoothing")]
        public bool SASteeringWheelUseSmoothing { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothMinCutoff")]
        public double SASteeringWheelSmoothMinCutoff { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothBeta")]
        public double SASteeringWheelSmoothBeta { get; set; }
    }

    [XmlRoot(ElementName = "GyroControlsSettings")]
    public class GyroControlsSettings
    {
        [XmlElement(ElementName = "Triggers")] 
        public int Triggers { get; set; }

        [XmlElement(ElementName = "TriggerCond")]
        public string TriggerCond { get; set; }

        [XmlElement(ElementName = "TriggerTurns")]
        public bool TriggerTurns { get; set; }

        [XmlElement(ElementName = "Toggle")] 
        public bool Toggle { get; set; }
    }

    [XmlRoot(ElementName = "GyroMouseSmoothingSettings")]
    public class GyroMouseSmoothingSettings
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; }

        [XmlElement(ElementName = "SmoothingMethod")]
        public string SmoothingMethod { get; set; }

        [XmlElement(ElementName = "SmoothingWeight")]
        public int SmoothingWeight { get; set; }

        [XmlElement(ElementName = "SmoothingMinCutoff")]
        public int SmoothingMinCutoff { get; set; }

        [XmlElement(ElementName = "SmoothingBeta")]
        public double SmoothingBeta { get; set; }
    }

    [XmlRoot(ElementName = "GyroMouseStickSmoothingSettings")]
    public class GyroMouseStickSmoothingSettings
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; }

        [XmlElement(ElementName = "SmoothingMethod")]
        public string SmoothingMethod { get; set; }

        [XmlElement(ElementName = "SmoothingWeight")]
        public int SmoothingWeight { get; set; }

        [XmlElement(ElementName = "SmoothingMinCutoff")]
        public double SmoothingMinCutoff { get; set; }

        [XmlElement(ElementName = "SmoothingBeta")]
        public double SmoothingBeta { get; set; }
    }

    [XmlRoot(ElementName = "GyroSwipeSettings")]
    public class GyroSwipeSettings
    {
        [XmlElement(ElementName = "DeadZoneX")]
        public int DeadZoneX { get; set; }

        [XmlElement(ElementName = "DeadZoneY")]
        public int DeadZoneY { get; set; }

        [XmlElement(ElementName = "Triggers")] 
        public int Triggers { get; set; }

        [XmlElement(ElementName = "TriggerCond")]
        public string TriggerCond { get; set; }

        [XmlElement(ElementName = "TriggerTurns")]
        public bool TriggerTurns { get; set; }

        [XmlElement(ElementName = "XAxis")] 
        public string XAxis { get; set; }

        [XmlElement(ElementName = "DelayTime")]
        public int DelayTime { get; set; }
    }

    [XmlRoot(ElementName = "FlickStickSettings")]
    public class FlickStickSettings
    {
        [XmlElement(ElementName = "RealWorldCalibration")]
        public DateTime RealWorldCalibration { get; set; }

        [XmlElement(ElementName = "FlickThreshold")]
        public double FlickThreshold { get; set; }

        [XmlElement(ElementName = "FlickTime")]
        public double FlickTime { get; set; }
    }

    [XmlRoot(ElementName = "LSOutputSettings")]
    public class LSOutputSettings
    {
        [XmlElement(ElementName = "FlickStickSettings")]
        public FlickStickSettings FlickStickSettings { get; set; }
    }

    [XmlRoot(ElementName = "RSOutputSettings")]
    public class RSOutputSettings
    {
        [XmlElement(ElementName = "FlickStickSettings")]
        public FlickStickSettings FlickStickSettings { get; set; }
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

    [XmlRoot(ElementName = "DS4Windows")]
    public class DS4Windows
    {
        [XmlElement(ElementName = "touchToggle")]
        public bool TouchToggle { get; set; }

        [XmlElement(ElementName = "idleDisconnectTimeout")]
        public int IdleDisconnectTimeout { get; set; }

        [XmlElement(ElementName = "outputDataToDS4")]
        public bool OutputDataToDS4 { get; set; }

        [XmlElement(ElementName = "Color")] 
        public double Color { get; set; }

        [XmlElement(ElementName = "RumbleBoost")]
        public int RumbleBoost { get; set; }

        [XmlElement(ElementName = "RumbleAutostopTime")]
        public int RumbleAutostopTime { get; set; }

        [XmlElement(ElementName = "LightbarMode")]
        public string LightbarMode { get; set; }

        [XmlElement(ElementName = "ledAsBatteryIndicator")]
        public bool LedAsBatteryIndicator { get; set; }

        [XmlElement(ElementName = "FlashType")]
        public int FlashType { get; set; }

        [XmlElement(ElementName = "flashBatteryAt")]
        public int FlashBatteryAt { get; set; }

        [XmlElement(ElementName = "touchSensitivity")]
        public int TouchSensitivity { get; set; }

        [XmlElement(ElementName = "LowColor")] 
        public double LowColor { get; set; }

        [XmlElement(ElementName = "ChargingColor")]
        public double ChargingColor { get; set; }

        [XmlElement(ElementName = "FlashColor")]
        public double FlashColor { get; set; }

        [XmlElement(ElementName = "touchpadJitterCompensation")]
        public bool TouchpadJitterCompensation { get; set; }

        [XmlElement(ElementName = "lowerRCOn")]
        public bool LowerRCOn { get; set; }

        [XmlElement(ElementName = "tapSensitivity")]
        public int TapSensitivity { get; set; }

        [XmlElement(ElementName = "doubleTap")]
        public bool DoubleTap { get; set; }

        [XmlElement(ElementName = "scrollSensitivity")]
        public int ScrollSensitivity { get; set; }

        [XmlElement(ElementName = "LeftTriggerMiddle")]
        public int LeftTriggerMiddle { get; set; }

        [XmlElement(ElementName = "RightTriggerMiddle")]
        public int RightTriggerMiddle { get; set; }

        [XmlElement(ElementName = "TouchpadInvert")]
        public int TouchpadInvert { get; set; }

        [XmlElement(ElementName = "TouchpadClickPassthru")]
        public bool TouchpadClickPassthru { get; set; }

        [XmlElement(ElementName = "L2AntiDeadZone")]
        public int L2AntiDeadZone { get; set; }

        [XmlElement(ElementName = "R2AntiDeadZone")]
        public int R2AntiDeadZone { get; set; }

        [XmlElement(ElementName = "L2MaxZone")]
        public int L2MaxZone { get; set; }

        [XmlElement(ElementName = "R2MaxZone")]
        public int R2MaxZone { get; set; }

        [XmlElement(ElementName = "L2MaxOutput")]
        public int L2MaxOutput { get; set; }

        [XmlElement(ElementName = "R2MaxOutput")]
        public int R2MaxOutput { get; set; }

        [XmlElement(ElementName = "ButtonMouseSensitivity")]
        public int ButtonMouseSensitivity { get; set; }

        [XmlElement(ElementName = "ButtonMouseOffset")]
        public double ButtonMouseOffset { get; set; }

        [XmlElement(ElementName = "Rainbow")] 
        public int Rainbow { get; set; }

        [XmlElement(ElementName = "MaxSatRainbow")]
        public int MaxSatRainbow { get; set; }

        [XmlElement(ElementName = "LSDeadZone")]
        public int LSDeadZone { get; set; }

        [XmlElement(ElementName = "RSDeadZone")]
        public int RSDeadZone { get; set; }

        [XmlElement(ElementName = "LSAntiDeadZone")]
        public int LSAntiDeadZone { get; set; }

        [XmlElement(ElementName = "RSAntiDeadZone")]
        public int RSAntiDeadZone { get; set; }

        [XmlElement(ElementName = "LSMaxZone")]
        public int LSMaxZone { get; set; }

        [XmlElement(ElementName = "RSMaxZone")]
        public int RSMaxZone { get; set; }

        [XmlElement(ElementName = "LSVerticalScale")]
        public int LSVerticalScale { get; set; }

        [XmlElement(ElementName = "RSVerticalScale")]
        public int RSVerticalScale { get; set; }

        [XmlElement(ElementName = "LSMaxOutput")]
        public int LSMaxOutput { get; set; }

        [XmlElement(ElementName = "RSMaxOutput")]
        public int RSMaxOutput { get; set; }

        [XmlElement(ElementName = "LSMaxOutputForce")]
        public bool LSMaxOutputForce { get; set; }

        [XmlElement(ElementName = "RSMaxOutputForce")]
        public bool RSMaxOutputForce { get; set; }

        [XmlElement(ElementName = "LSDeadZoneType")]
        public string LSDeadZoneType { get; set; }

        [XmlElement(ElementName = "RSDeadZoneType")]
        public string RSDeadZoneType { get; set; }

        [XmlElement(ElementName = "LSAxialDeadOptions")]
        public LSAxialDeadOptions LSAxialDeadOptions { get; set; }

        [XmlElement(ElementName = "RSAxialDeadOptions")]
        public RSAxialDeadOptions RSAxialDeadOptions { get; set; }

        [XmlElement(ElementName = "LSRotation")]
        public int LSRotation { get; set; }

        [XmlElement(ElementName = "RSRotation")]
        public int RSRotation { get; set; }

        [XmlElement(ElementName = "LSFuzz")] 
        public int LSFuzz { get; set; }

        [XmlElement(ElementName = "RSFuzz")] 
        public int RSFuzz { get; set; }

        [XmlElement(ElementName = "LSOuterBindDead")]
        public int LSOuterBindDead { get; set; }

        [XmlElement(ElementName = "RSOuterBindDead")]
        public int RSOuterBindDead { get; set; }

        [XmlElement(ElementName = "LSOuterBindInvert")]
        public bool LSOuterBindInvert { get; set; }

        [XmlElement(ElementName = "RSOuterBindInvert")]
        public bool RSOuterBindInvert { get; set; }

        [XmlElement(ElementName = "SXDeadZone")]
        public double SXDeadZone { get; set; }

        [XmlElement(ElementName = "SZDeadZone")]
        public double SZDeadZone { get; set; }

        [XmlElement(ElementName = "SXMaxZone")]
        public int SXMaxZone { get; set; }

        [XmlElement(ElementName = "SZMaxZone")]
        public int SZMaxZone { get; set; }

        [XmlElement(ElementName = "SXAntiDeadZone")]
        public int SXAntiDeadZone { get; set; }

        [XmlElement(ElementName = "SZAntiDeadZone")]
        public int SZAntiDeadZone { get; set; }

        [XmlElement(ElementName = "Sensitivity")]
        public string Sensitivity { get; set; }

        [XmlElement(ElementName = "ChargingType")]
        public int ChargingType { get; set; }

        [XmlElement(ElementName = "MouseAcceleration")]
        public bool MouseAcceleration { get; set; }

        [XmlElement(ElementName = "ButtonMouseVerticalScale")]
        public int ButtonMouseVerticalScale { get; set; }

        [XmlElement(ElementName = "LaunchProgram")]
        public object LaunchProgram { get; set; }

        [XmlElement(ElementName = "DinputOnly")]
        public bool DinputOnly { get; set; }

        [XmlElement(ElementName = "StartTouchpadOff")]
        public bool StartTouchpadOff { get; set; }

        [XmlElement(ElementName = "TouchpadOutputMode")]
        public string TouchpadOutputMode { get; set; }

        [XmlElement(ElementName = "SATriggers")]
        public int SATriggers { get; set; }

        [XmlElement(ElementName = "SATriggerCond")]
        public string SATriggerCond { get; set; }

        [XmlElement(ElementName = "SASteeringWheelEmulationAxis")]
        public string SASteeringWheelEmulationAxis { get; set; }

        [XmlElement(ElementName = "SASteeringWheelEmulationRange")]
        public int SASteeringWheelEmulationRange { get; set; }

        [XmlElement(ElementName = "SASteeringWheelFuzz")]
        public int SASteeringWheelFuzz { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothingOptions")]
        public SASteeringWheelSmoothingOptions SASteeringWheelSmoothingOptions { get; set; }

        [XmlElement(ElementName = "TouchDisInvTriggers")]
        public int TouchDisInvTriggers { get; set; }

        [XmlElement(ElementName = "GyroSensitivity")]
        public int GyroSensitivity { get; set; }

        [XmlElement(ElementName = "GyroSensVerticalScale")]
        public int GyroSensVerticalScale { get; set; }

        [XmlElement(ElementName = "GyroInvert")]
        public int GyroInvert { get; set; }

        [XmlElement(ElementName = "GyroTriggerTurns")]
        public bool GyroTriggerTurns { get; set; }

        [XmlElement(ElementName = "GyroControlsSettings")]
        public GyroControlsSettings GyroControlsSettings { get; set; }

        [XmlElement(ElementName = "GyroMouseSmoothingSettings")]
        public GyroMouseSmoothingSettings GyroMouseSmoothingSettings { get; set; }

        [XmlElement(ElementName = "GyroMouseHAxis")]
        public int GyroMouseHAxis { get; set; }

        [XmlElement(ElementName = "GyroMouseDeadZone")]
        public int GyroMouseDeadZone { get; set; }

        [XmlElement(ElementName = "GyroMouseMinThreshold")]
        public int GyroMouseMinThreshold { get; set; }

        [XmlElement(ElementName = "GyroMouseToggle")]
        public bool GyroMouseToggle { get; set; }

        [XmlElement(ElementName = "GyroOutputMode")]
        public string GyroOutputMode { get; set; }

        [XmlElement(ElementName = "GyroMouseStickTriggers")]
        public int GyroMouseStickTriggers { get; set; }

        [XmlElement(ElementName = "GyroMouseStickTriggerCond")]
        public string GyroMouseStickTriggerCond { get; set; }

        [XmlElement(ElementName = "GyroMouseStickTriggerTurns")]
        public bool GyroMouseStickTriggerTurns { get; set; }

        [XmlElement(ElementName = "GyroMouseStickHAxis")]
        public int GyroMouseStickHAxis { get; set; }

        [XmlElement(ElementName = "GyroMouseStickDeadZone")]
        public int GyroMouseStickDeadZone { get; set; }

        [XmlElement(ElementName = "GyroMouseStickMaxZone")]
        public int GyroMouseStickMaxZone { get; set; }

        [XmlElement(ElementName = "GyroMouseStickOutputStick")]
        public string GyroMouseStickOutputStick { get; set; }

        [XmlElement(ElementName = "GyroMouseStickOutputStickAxes")]
        public string GyroMouseStickOutputStickAxes { get; set; }

        [XmlElement(ElementName = "GyroMouseStickAntiDeadX")]
        public double GyroMouseStickAntiDeadX { get; set; }

        [XmlElement(ElementName = "GyroMouseStickAntiDeadY")]
        public double GyroMouseStickAntiDeadY { get; set; }

        [XmlElement(ElementName = "GyroMouseStickInvert")]
        public int GyroMouseStickInvert { get; set; }

        [XmlElement(ElementName = "GyroMouseStickToggle")]
        public bool GyroMouseStickToggle { get; set; }

        [XmlElement(ElementName = "GyroMouseStickMaxOutput")]
        public int GyroMouseStickMaxOutput { get; set; }

        [XmlElement(ElementName = "GyroMouseStickMaxOutputEnabled")]
        public bool GyroMouseStickMaxOutputEnabled { get; set; }

        [XmlElement(ElementName = "GyroMouseStickVerticalScale")]
        public int GyroMouseStickVerticalScale { get; set; }

        [XmlElement(ElementName = "GyroMouseStickSmoothingSettings")]
        public GyroMouseStickSmoothingSettings GyroMouseStickSmoothingSettings { get; set; }

        [XmlElement(ElementName = "GyroSwipeSettings")]
        public GyroSwipeSettings GyroSwipeSettings { get; set; }

        [XmlElement(ElementName = "ProfileActions")]
        public string ProfileActions { get; set; }

        [XmlElement(ElementName = "BTPollRate")]
        public int BTPollRate { get; set; }

        [XmlElement(ElementName = "LSOutputCurveMode")]
        public string LSOutputCurveMode { get; set; }

        [XmlElement(ElementName = "LSOutputCurveCustom")]
        public object LSOutputCurveCustom { get; set; }

        [XmlElement(ElementName = "RSOutputCurveMode")]
        public string RSOutputCurveMode { get; set; }

        [XmlElement(ElementName = "RSOutputCurveCustom")]
        public object RSOutputCurveCustom { get; set; }

        [XmlElement(ElementName = "LSSquareStick")]
        public bool LSSquareStick { get; set; }

        [XmlElement(ElementName = "RSSquareStick")]
        public bool RSSquareStick { get; set; }

        [XmlElement(ElementName = "SquareStickRoundness")]
        public int SquareStickRoundness { get; set; }

        [XmlElement(ElementName = "SquareRStickRoundness")]
        public int SquareRStickRoundness { get; set; }

        [XmlElement(ElementName = "LSAntiSnapback")]
        public bool LSAntiSnapback { get; set; }

        [XmlElement(ElementName = "RSAntiSnapback")]
        public bool RSAntiSnapback { get; set; }

        [XmlElement(ElementName = "LSAntiSnapbackDelta")]
        public int LSAntiSnapbackDelta { get; set; }

        [XmlElement(ElementName = "RSAntiSnapbackDelta")]
        public int RSAntiSnapbackDelta { get; set; }

        [XmlElement(ElementName = "LSAntiSnapbackTimeout")]
        public int LSAntiSnapbackTimeout { get; set; }

        [XmlElement(ElementName = "RSAntiSnapbackTimeout")]
        public int RSAntiSnapbackTimeout { get; set; }

        [XmlElement(ElementName = "LSOutputMode")]
        public string LSOutputMode { get; set; }

        [XmlElement(ElementName = "RSOutputMode")]
        public string RSOutputMode { get; set; }

        [XmlElement(ElementName = "LSOutputSettings")]
        public LSOutputSettings LSOutputSettings { get; set; }

        [XmlElement(ElementName = "RSOutputSettings")]
        public RSOutputSettings RSOutputSettings { get; set; }

        [XmlElement(ElementName = "L2OutputCurveMode")]
        public string L2OutputCurveMode { get; set; }

        [XmlElement(ElementName = "L2OutputCurveCustom")]
        public object L2OutputCurveCustom { get; set; }

        [XmlElement(ElementName = "L2TwoStageMode")]
        public string L2TwoStageMode { get; set; }

        [XmlElement(ElementName = "R2TwoStageMode")]
        public string R2TwoStageMode { get; set; }

        [XmlElement(ElementName = "L2TriggerEffect")]
        public string L2TriggerEffect { get; set; }

        [XmlElement(ElementName = "R2TriggerEffect")]
        public string R2TriggerEffect { get; set; }

        [XmlElement(ElementName = "R2OutputCurveMode")]
        public string R2OutputCurveMode { get; set; }

        [XmlElement(ElementName = "R2OutputCurveCustom")]
        public object R2OutputCurveCustom { get; set; }

        [XmlElement(ElementName = "SXOutputCurveMode")]
        public string SXOutputCurveMode { get; set; }

        [XmlElement(ElementName = "SXOutputCurveCustom")]
        public object SXOutputCurveCustom { get; set; }

        [XmlElement(ElementName = "SZOutputCurveMode")]
        public string SZOutputCurveMode { get; set; }

        [XmlElement(ElementName = "SZOutputCurveCustom")]
        public object SZOutputCurveCustom { get; set; }

        [XmlElement(ElementName = "TrackballMode")]
        public bool TrackballMode { get; set; }

        [XmlElement(ElementName = "TrackballFriction")]
        public int TrackballFriction { get; set; }

        [XmlElement(ElementName = "TouchRelMouseRotation")]
        public int TouchRelMouseRotation { get; set; }

        [XmlElement(ElementName = "TouchRelMouseMinThreshold")]
        public int TouchRelMouseMinThreshold { get; set; }

        [XmlElement(ElementName = "TouchpadAbsMouseSettings")]
        public TouchpadAbsMouseSettings TouchpadAbsMouseSettings { get; set; }

        [XmlElement(ElementName = "OutputContDevice")]
        public string OutputContDevice { get; set; }

        [XmlElement(ElementName = "Control")] 
        public object Control { get; set; }

        [XmlElement(ElementName = "ShiftControl")]
        public object ShiftControl { get; set; }

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }

        [XmlAttribute(AttributeName = "config_version")]
        public int ConfigVersion { get; set; }

        [XmlText] 
        public string Text { get; set; }
    }
}