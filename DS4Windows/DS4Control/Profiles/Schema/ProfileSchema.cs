using System.Collections.Generic;
using System.Xml.Serialization;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "DS4Windows")]
    public partial class DS4WindowsProfile
    {
        [XmlElement(ElementName = "touchToggle")]
        public bool TouchToggle { get; set; } = true;

        [XmlElement(ElementName = "idleDisconnectTimeout")]
        public int IdleDisconnectTimeout { get; set; } = 0;

        [XmlElement(ElementName = "outputDataToDS4")]
        public bool OutputDataToDS4 { get; set; } = true;

        [XmlElement(ElementName = "Color")]
        public DS4Color Color { get; set; } = new(System.Drawing.Color.Blue);

        [XmlElement(ElementName = "RumbleBoost")]
        public byte RumbleBoost { get; set; } = 100;

        [XmlElement(ElementName = "RumbleAutostopTime")]
        public int RumbleAutostopTime { get; set; }

        [XmlElement(ElementName = "LightbarMode")]
        public LightbarMode LightbarMode { get; set; }

        [XmlElement(ElementName = "ledAsBatteryIndicator")]
        public bool LedAsBatteryIndicator { get; set; }

        [XmlElement(ElementName = "FlashType")]
        public byte FlashType { get; set; }

        [XmlElement(ElementName = "flashBatteryAt")]
        public int FlashBatteryAt { get; set; }

        [XmlElement(ElementName = "touchSensitivity")]
        public byte TouchSensitivity { get; set; } = 100;

        [XmlElement(ElementName = "LowColor")]
        public DS4Color LowColor { get; set; } = new(System.Drawing.Color.Red);

        [XmlElement(ElementName = "ChargingColor")]
        public DS4Color ChargingColor { get; set; } = new(System.Drawing.Color.Orange);

        [XmlElement(ElementName = "FlashColor")]
        public DS4Color FlashColor { get; set; } = new(System.Drawing.Color.Blue);

        [XmlElement(ElementName = "touchpadJitterCompensation")]
        public bool TouchpadJitterCompensation { get; set; } = true;

        [XmlElement(ElementName = "lowerRCOn")]
        public bool LowerRCOn { get; set; }

        [XmlElement(ElementName = "tapSensitivity")]
        public byte TapSensitivity { get; set; }

        [XmlElement(ElementName = "doubleTap")]
        public bool DoubleTap { get; set; }

        [XmlElement(ElementName = "scrollSensitivity")]
        public int ScrollSensitivity { get; set; }

        [XmlElement(ElementName = "LeftTriggerMiddle")]
        public byte LeftTriggerMiddle { get; set; }

        [XmlElement(ElementName = "RightTriggerMiddle")]
        public byte RightTriggerMiddle { get; set; }

        [XmlElement(ElementName = "TouchpadInvert")]
        public int TouchpadInvert { get; set; }

        [XmlElement(ElementName = "TouchpadClickPassthru")]
        public bool TouchpadClickPassthru { get; set; }

        [XmlElement(ElementName = "L2AntiDeadZone")]
        public int L2AntiDeadZone { get; set; }

        [XmlElement(ElementName = "R2AntiDeadZone")]
        public int R2AntiDeadZone { get; set; }

        [XmlElement(ElementName = "L2MaxZone")]
        public int L2MaxZone { get; set; } = 100;

        [XmlElement(ElementName = "R2MaxZone")]
        public int R2MaxZone { get; set; } = 100;

        [XmlElement(ElementName = "L2MaxOutput")]
        public double L2MaxOutput { get; set; } = 100;

        [XmlElement(ElementName = "R2MaxOutput")]
        public double R2MaxOutput { get; set; } = 100;

        [XmlElement(ElementName = "ButtonMouseSensitivity")]
        public int ButtonMouseSensitivity { get; set; }

        [XmlElement(ElementName = "ButtonMouseOffset")]
        public double ButtonMouseOffset { get; set; }

        [XmlElement(ElementName = "Rainbow")]
        public double Rainbow { get; set; }

        [XmlElement(ElementName = "MaxSatRainbow")]
        public double MaxSatRainbow { get; set; } = 1.0;

        [XmlElement(ElementName = "LSDeadZone")]
        public int LSDeadZone { get; set; }

        [XmlElement(ElementName = "RSDeadZone")]
        public int RSDeadZone { get; set; }

        [XmlElement(ElementName = "LSAntiDeadZone")]
        public int LSAntiDeadZone { get; set; }

        [XmlElement(ElementName = "RSAntiDeadZone")]
        public int RSAntiDeadZone { get; set; }

        [XmlElement(ElementName = "LSMaxZone")]
        public int LSMaxZone { get; set; } = 100;

        [XmlElement(ElementName = "RSMaxZone")]
        public int RSMaxZone { get; set; } = 100;

        [XmlElement(ElementName = "LSVerticalScale")]
        public double LSVerticalScale { get; set; }

        [XmlElement(ElementName = "RSVerticalScale")]
        public double RSVerticalScale { get; set; }

        [XmlElement(ElementName = "LSMaxOutput")]
        public double LSMaxOutput { get; set; } = 100;

        [XmlElement(ElementName = "RSMaxOutput")]
        public double RSMaxOutput { get; set; } = 100;

        [XmlElement(ElementName = "LSMaxOutputForce")]
        public bool LSMaxOutputForce { get; set; }

        [XmlElement(ElementName = "RSMaxOutputForce")]
        public bool RSMaxOutputForce { get; set; }

        [XmlElement(ElementName = "LSDeadZoneType")]
        public StickDeadZoneInfo.DeadZoneType LSDeadZoneType { get; set; }

        [XmlElement(ElementName = "RSDeadZoneType")]
        public StickDeadZoneInfo.DeadZoneType RSDeadZoneType { get; set; }

        [XmlElement(ElementName = "LSAxialDeadOptions")]
        public LSAxialDeadOptions LSAxialDeadOptions { get; set; } = new();

        [XmlElement(ElementName = "RSAxialDeadOptions")]
        public RSAxialDeadOptions RSAxialDeadOptions { get; set; } = new();

        [XmlElement(ElementName = "LSRotation")]
        public int LSRotation { get; set; }

        [XmlElement(ElementName = "RSRotation")]
        public int RSRotation { get; set; }

        [XmlElement(ElementName = "LSFuzz")]
        public int LSFuzz { get; set; }

        [XmlElement(ElementName = "RSFuzz")]
        public int RSFuzz { get; set; }

        [XmlElement(ElementName = "LSOuterBindDead")]
        public double LSOuterBindDead { get; set; } = StickDeadZoneInfo.DEFAULT_OUTER_BIND_DEAD;

        [XmlElement(ElementName = "RSOuterBindDead")]
        public double RSOuterBindDead { get; set; } = StickDeadZoneInfo.DEFAULT_OUTER_BIND_DEAD;

        [XmlElement(ElementName = "LSOuterBindInvert")]
        public bool LSOuterBindInvert { get; set; }

        [XmlElement(ElementName = "RSOuterBindInvert")]
        public bool RSOuterBindInvert { get; set; }

        [XmlElement(ElementName = "SXDeadZone")]
        public double SXDeadZone { get; set; } = 0.02;

        [XmlElement(ElementName = "SZDeadZone")]
        public double SZDeadZone { get; set; } = 0.02;

        [XmlElement(ElementName = "SXMaxZone")]
        public double SXMaxZone { get; set; } = 1.0f;

        [XmlElement(ElementName = "SZMaxZone")]
        public double SZMaxZone { get; set; } = 1.0f;

        [XmlElement(ElementName = "SXAntiDeadZone")]
        public double SXAntiDeadZone { get; set; }

        [XmlElement(ElementName = "SZAntiDeadZone")]
        public double SZAntiDeadZone { get; set; }

        [XmlElement(ElementName = "Sensitivity")]
        public SensitivityProxyType Sensitivity { get; set; } = new();

        [XmlElement(ElementName = "ChargingType")]
        public int ChargingType { get; set; }

        [XmlElement(ElementName = "MouseAcceleration")]
        public bool MouseAcceleration { get; set; }

        [XmlElement(ElementName = "ButtonMouseVerticalScale")]
        public int ButtonMouseVerticalScale { get; set; }

        [XmlElement(ElementName = "LaunchProgram")]
        public string LaunchProgram { get; set; } = string.Empty;

        [XmlElement(ElementName = "DinputOnly")]
        public bool DinputOnly { get; set; }

        [XmlElement(ElementName = "StartTouchpadOff")]
        public bool StartTouchpadOff { get; set; }

        [XmlElement(ElementName = "TouchpadOutputMode")]
        public TouchpadOutMode TouchpadOutputMode { get; set; } = TouchpadOutMode.Mouse;

        [XmlElement(ElementName = "SATriggers")]
        public string SATriggers { get; set; } = "-1";

        [XmlElement(ElementName = "SATriggerCond")]
        public string SATriggerCond { get; set; }

        [XmlElement(ElementName = "SASteeringWheelEmulationAxis")]
        public SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis { get; set; } = SASteeringWheelEmulationAxisType.None;

        [XmlElement(ElementName = "SASteeringWheelEmulationRange")]
        public int SASteeringWheelEmulationRange { get; set; } = 360;

        [XmlElement(ElementName = "SASteeringWheelFuzz")]
        public int SASteeringWheelFuzz { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothingOptions")]
        public SASteeringWheelSmoothingOptions SASteeringWheelSmoothingOptions { get; set; } = new();

        [XmlElement(ElementName = "TouchDisInvTriggers")]
        public List<int> TouchDisInvTriggers { get; set; } = new() { -1 };

        [XmlElement(ElementName = "GyroSensitivity")]
        public int GyroSensitivity { get; set; } = 100;

        [XmlElement(ElementName = "GyroSensVerticalScale")]
        public int GyroSensVerticalScale { get; set; } = 100;

        [XmlElement(ElementName = "GyroInvert")]
        public int GyroInvert { get; set; }

        [XmlElement(ElementName = "GyroTriggerTurns")]
        public bool GyroTriggerTurns { get; set; } = true;

        [XmlElement(ElementName = "GyroControlsSettings")]
        public GyroControlsSettings GyroControlsSettings { get; set; } = new();

        [XmlElement(ElementName = "GyroMouseSmoothingSettings")]
        public GyroMouseSmoothingSettings GyroMouseSmoothingSettings { get; set; } = new();

        [XmlElement(ElementName = "GyroMouseHAxis")]
        public int GyroMouseHAxis { get; set; }

        [XmlElement(ElementName = "GyroMouseDeadZone")]
        public int GyroMouseDeadZone { get; set; }

        [XmlElement(ElementName = "GyroMouseMinThreshold")]
        public double GyroMouseMinThreshold { get; set; }

        [XmlElement(ElementName = "GyroMouseToggle")]
        public bool GyroMouseToggle { get; set; }

        [XmlElement(ElementName = "GyroOutputMode")]
        public GyroOutMode GyroOutputMode { get; set; }

        [XmlElement(ElementName = "GyroMouseStickTriggers")]
        public string GyroMouseStickTriggers { get; set; } = "-1";

        [XmlElement(ElementName = "GyroMouseStickTriggerCond")]
        public string GyroMouseStickTriggerCond { get; set; }

        [XmlElement(ElementName = "GyroMouseStickTriggerTurns")]
        public bool GyroMouseStickTriggerTurns { get; set; }

        [XmlElement(ElementName = "GyroMouseStickHAxis")]
        public int GyroMouseStickHAxis { get; set; }

        [XmlElement(ElementName = "GyroMouseStickDeadZone")]
        public int GyroMouseStickDeadZone { get; set; } = 30;

        [XmlElement(ElementName = "GyroMouseStickMaxZone")]
        public int GyroMouseStickMaxZone { get; set; } = 830;

        [XmlElement(ElementName = "GyroMouseStickOutputStick")]
        public GyroMouseStickInfo.OutputStick GyroMouseStickOutputStick { get; set; }

        [XmlElement(ElementName = "GyroMouseStickOutputStickAxes")]
        public GyroMouseStickInfo.OutputStickAxes GyroMouseStickOutputStickAxes { get; set; }

        [XmlElement(ElementName = "GyroMouseStickAntiDeadX")]
        public double GyroMouseStickAntiDeadX { get; set; } = 0.4;

        [XmlElement(ElementName = "GyroMouseStickAntiDeadY")]
        public double GyroMouseStickAntiDeadY { get; set; } = 0.4;

        [XmlElement(ElementName = "GyroMouseStickInvert")]
        public uint GyroMouseStickInvert { get; set; }

        [XmlElement(ElementName = "GyroMouseStickToggle")]
        public bool GyroMouseStickToggle { get; set; }

        [XmlElement(ElementName = "GyroMouseStickMaxOutput")]
        public double GyroMouseStickMaxOutput { get; set; } = 100.0;

        [XmlElement(ElementName = "GyroMouseStickMaxOutputEnabled")]
        public bool GyroMouseStickMaxOutputEnabled { get; set; }

        [XmlElement(ElementName = "GyroMouseStickVerticalScale")]
        public int GyroMouseStickVerticalScale { get; set; } = 100;

        [XmlElement(ElementName = "GyroMouseStickSmoothingSettings")]
        public GyroMouseStickSmoothingSettings GyroMouseStickSmoothingSettings { get; set; } = new();

        [XmlElement(ElementName = "GyroSwipeSettings")]
        public GyroSwipeSettings GyroSwipeSettings { get; set; } = new();

        [XmlElement(ElementName = "ProfileActions")]
        public string ProfileActions { get; set; }

        [XmlElement(ElementName = "BTPollRate")]
        public int BTPollRate { get; set; } = 4;

        [XmlElement(ElementName = "LSOutputCurveMode")]
        public string LSOutputCurveMode { get; set; }

        [XmlElement(ElementName = "LSOutputCurveCustom")]
        public BezierCurve LSOutputCurveCustom { get; set; } = new();

        [XmlElement(ElementName = "RSOutputCurveMode")]
        public string RSOutputCurveMode { get; set; }

        [XmlElement(ElementName = "RSOutputCurveCustom")]
        public BezierCurve RSOutputCurveCustom { get; set; } = new();

        [XmlElement(ElementName = "LSSquareStick")]
        public bool LSSquareStick { get; set; }

        [XmlElement(ElementName = "RSSquareStick")]
        public bool RSSquareStick { get; set; }

        [XmlElement(ElementName = "SquareStickRoundness")]
        public double SquareStickRoundness { get; set; } = 5.0;

        [XmlElement(ElementName = "SquareRStickRoundness")]
        public double SquareRStickRoundness { get; set; } = 5.0;

        [XmlElement(ElementName = "LSAntiSnapback")]
        public bool LSAntiSnapback { get; set; }

        [XmlElement(ElementName = "RSAntiSnapback")]
        public bool RSAntiSnapback { get; set; }

        [XmlElement(ElementName = "LSAntiSnapbackDelta")]
        public double LSAntiSnapbackDelta { get; set; } = StickAntiSnapbackInfo.DEFAULT_DELTA;

        [XmlElement(ElementName = "RSAntiSnapbackDelta")]
        public double RSAntiSnapbackDelta { get; set; }

        [XmlElement(ElementName = "LSAntiSnapbackTimeout")]
        public int LSAntiSnapbackTimeout { get; set; }

        [XmlElement(ElementName = "RSAntiSnapbackTimeout")]
        public int RSAntiSnapbackTimeout { get; set; }

        [XmlElement(ElementName = "LSOutputMode")]
        public StickMode LSOutputMode { get; set; }

        [XmlElement(ElementName = "RSOutputMode")]
        public StickMode RSOutputMode { get; set; }

        [XmlElement(ElementName = "LSOutputSettings")]
        public LSOutputSettings LSOutputSettings { get; set; } = new();

        [XmlElement(ElementName = "RSOutputSettings")]
        public RSOutputSettings RSOutputSettings { get; set; } = new();

        [XmlElement(ElementName = "L2OutputCurveMode")]
        public string L2OutputCurveMode { get; set; }

        [XmlElement(ElementName = "L2OutputCurveCustom")]
        public BezierCurve L2OutputCurveCustom { get; set; } = new();

        [XmlElement(ElementName = "L2TwoStageMode")]
        public TwoStageTriggerMode L2TwoStageMode { get; set; }

        [XmlElement(ElementName = "R2TwoStageMode")]
        public TwoStageTriggerMode R2TwoStageMode { get; set; }

        [XmlElement(ElementName = "L2HipFireTime")]
        public int L2HipFireTime { get; set; }

        [XmlElement(ElementName = "R2HipFireTime")]
        public int R2HipFireTime { get; set; }

        [XmlElement(ElementName = "L2TriggerEffect")]
        public TriggerEffects L2TriggerEffect { get; set; }

        [XmlElement(ElementName = "R2TriggerEffect")]
        public TriggerEffects R2TriggerEffect { get; set; }

        [XmlElement(ElementName = "R2OutputCurveMode")]
        public string R2OutputCurveMode { get; set; }

        [XmlElement(ElementName = "R2OutputCurveCustom")]
        public BezierCurve R2OutputCurveCustom { get; set; } = new();

        [XmlElement(ElementName = "SXOutputCurveMode")]
        public string SXOutputCurveMode { get; set; }

        [XmlElement(ElementName = "SXOutputCurveCustom")]
        public BezierCurve SXOutputCurveCustom { get; set; } = new();

        [XmlElement(ElementName = "SZOutputCurveMode")]
        public string SZOutputCurveMode { get; set; }

        [XmlElement(ElementName = "SZOutputCurveCustom")]
        public BezierCurve SZOutputCurveCustom { get; set; } = new();

        [XmlElement(ElementName = "TrackballMode")]
        public bool TrackballMode { get; set; }

        [XmlElement(ElementName = "TrackballFriction")]
        public double TrackballFriction { get; set; }

        [XmlElement(ElementName = "TouchRelMouseRotation")]
        public int TouchRelMouseRotation { get; set; }

        [XmlElement(ElementName = "TouchRelMouseMinThreshold")]
        public double TouchRelMouseMinThreshold { get; set; }

        [XmlElement(ElementName = "TouchpadAbsMouseSettings")]
        public TouchpadAbsMouseSettings TouchpadAbsMouseSettings { get; set; } = new();

        [XmlElement(ElementName = "OutputContDevice")]
        public OutContType OutputContDevice { get; set; } = OutContType.X360;
        
        [XmlElement(ElementName = "Control")] 
        public Control Controls { get; set; } = new();

        [XmlElement(ElementName = "ShiftControl")]
        public ShiftControl ShiftControls { get; set; } = new();

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }

        [XmlAttribute(AttributeName = "config_version")]
        public int ConfigVersion { get; set; }

        public DS4WindowsProfile()
        {
        }

        public DS4WindowsProfile(IBackingStore store, int device, string appVersion, int configVersion)
        {
            AppVersion = appVersion;
            ConfigVersion = configVersion;

            CopyFrom(store, device);
        }
    }
}