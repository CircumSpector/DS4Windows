using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.Profiles.Legacy.Converters;

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
        public double SASteeringWheelSmoothMinCutoff { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothBeta")]
        public double SASteeringWheelSmoothBeta { get; set; }
    }

    [XmlRoot(ElementName = "GyroControlsSettings")]
    public class GyroControlsSettings
    {
        [XmlElement(ElementName = "Triggers")] 
        public string Triggers { get; set; }

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
        public double SmoothingMinCutoff { get; set; }

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
        public string Triggers { get; set; }

        [XmlElement(ElementName = "TriggerCond")]
        public string TriggerCond { get; set; }

        [XmlElement(ElementName = "TriggerTurns")]
        public bool TriggerTurns { get; set; }

        [XmlElement(ElementName = "XAxis")] 
        public GyroDirectionalSwipeInfo.XAxisSwipe XAxis { get; set; }

        [XmlElement(ElementName = "DelayTime")]
        public int DelayTime { get; set; }
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
        public DS4Color Color { get; set; }

        [XmlElement(ElementName = "RumbleBoost")]
        public int RumbleBoost { get; set; }

        [XmlElement(ElementName = "RumbleAutostopTime")]
        public int RumbleAutostopTime { get; set; }

        [XmlElement(ElementName = "LightbarMode")]
        public LightbarMode LightbarMode { get; set; }

        [XmlElement(ElementName = "ledAsBatteryIndicator")]
        public bool LedAsBatteryIndicator { get; set; }

        [XmlElement(ElementName = "FlashType")]
        public int FlashType { get; set; }

        [XmlElement(ElementName = "flashBatteryAt")]
        public int FlashBatteryAt { get; set; }

        [XmlElement(ElementName = "touchSensitivity")]
        public int TouchSensitivity { get; set; }

        [XmlElement(ElementName = "LowColor")] 
        public DS4Color LowColor { get; set; }

        [XmlElement(ElementName = "ChargingColor")]
        public DS4Color ChargingColor { get; set; }

        [XmlElement(ElementName = "FlashColor")]
        public DS4Color FlashColor { get; set; }

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
        public double L2MaxOutput { get; set; }

        [XmlElement(ElementName = "R2MaxOutput")]
        public double R2MaxOutput { get; set; }

        [XmlElement(ElementName = "ButtonMouseSensitivity")]
        public int ButtonMouseSensitivity { get; set; }

        [XmlElement(ElementName = "ButtonMouseOffset")]
        public double ButtonMouseOffset { get; set; }

        [XmlElement(ElementName = "Rainbow")] 
        public double Rainbow { get; set; }

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
        public double LSVerticalScale { get; set; }

        [XmlElement(ElementName = "RSVerticalScale")]
        public double RSVerticalScale { get; set; }

        [XmlElement(ElementName = "LSMaxOutput")]
        public double LSMaxOutput { get; set; }

        [XmlElement(ElementName = "RSMaxOutput")]
        public double RSMaxOutput { get; set; }

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
        public SensitivityProxyType Sensitivity { get; set; }

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
        public TouchpadOutMode TouchpadOutputMode { get; set; }

        [XmlElement(ElementName = "SATriggers")]
        public string SATriggers { get; set; }

        [XmlElement(ElementName = "SATriggerCond")]
        public string SATriggerCond { get; set; }

        [XmlElement(ElementName = "SASteeringWheelEmulationAxis")]
        public SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis { get; set; }

        [XmlElement(ElementName = "SASteeringWheelEmulationRange")]
        public int SASteeringWheelEmulationRange { get; set; }

        [XmlElement(ElementName = "SASteeringWheelFuzz")]
        public int SASteeringWheelFuzz { get; set; }

        [XmlElement(ElementName = "SASteeringWheelSmoothingOptions")]
        public SASteeringWheelSmoothingOptions SASteeringWheelSmoothingOptions { get; set; } = new();

        [XmlElement(ElementName = "TouchDisInvTriggers")]
        public List<int> TouchDisInvTriggers { get; set; }

        [XmlElement(ElementName = "GyroSensitivity")]
        public int GyroSensitivity { get; set; }

        [XmlElement(ElementName = "GyroSensVerticalScale")]
        public int GyroSensVerticalScale { get; set; }

        [XmlElement(ElementName = "GyroInvert")]
        public int GyroInvert { get; set; }

        [XmlElement(ElementName = "GyroTriggerTurns")]
        public bool GyroTriggerTurns { get; set; }

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
        public string GyroMouseStickTriggers { get; set; }

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
        public GyroMouseStickInfo.OutputStick GyroMouseStickOutputStick { get; set; }

        [XmlElement(ElementName = "GyroMouseStickOutputStickAxes")]
        public GyroMouseStickInfo.OutputStickAxes GyroMouseStickOutputStickAxes { get; set; }

        [XmlElement(ElementName = "GyroMouseStickAntiDeadX")]
        public double GyroMouseStickAntiDeadX { get; set; }

        [XmlElement(ElementName = "GyroMouseStickAntiDeadY")]
        public double GyroMouseStickAntiDeadY { get; set; }

        [XmlElement(ElementName = "GyroMouseStickInvert")]
        public uint GyroMouseStickInvert { get; set; }

        [XmlElement(ElementName = "GyroMouseStickToggle")]
        public bool GyroMouseStickToggle { get; set; }

        [XmlElement(ElementName = "GyroMouseStickMaxOutput")]
        public double GyroMouseStickMaxOutput { get; set; }

        [XmlElement(ElementName = "GyroMouseStickMaxOutputEnabled")]
        public bool GyroMouseStickMaxOutputEnabled { get; set; }

        [XmlElement(ElementName = "GyroMouseStickVerticalScale")]
        public int GyroMouseStickVerticalScale { get; set; }

        [XmlElement(ElementName = "GyroMouseStickSmoothingSettings")]
        public GyroMouseStickSmoothingSettings GyroMouseStickSmoothingSettings { get; set; } = new();

        [XmlElement(ElementName = "GyroSwipeSettings")]
        public GyroSwipeSettings GyroSwipeSettings { get; set; } = new();

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
        public double SquareStickRoundness { get; set; }

        [XmlElement(ElementName = "SquareRStickRoundness")]
        public double SquareRStickRoundness { get; set; }

        [XmlElement(ElementName = "LSAntiSnapback")]
        public bool LSAntiSnapback { get; set; }

        [XmlElement(ElementName = "RSAntiSnapback")]
        public bool RSAntiSnapback { get; set; }

        [XmlElement(ElementName = "LSAntiSnapbackDelta")]
        public double LSAntiSnapbackDelta { get; set; }

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
        public object L2OutputCurveCustom { get; set; }

        [XmlElement(ElementName = "L2TwoStageMode")]
        public TwoStageTriggerMode L2TwoStageMode { get; set; }

        [XmlElement(ElementName = "R2TwoStageMode")]
        public TwoStageTriggerMode R2TwoStageMode { get; set; }

        [XmlElement(ElementName = "L2TriggerEffect")]
        public TriggerEffects L2TriggerEffect { get; set; }

        [XmlElement(ElementName = "R2TriggerEffect")]
        public TriggerEffects R2TriggerEffect { get; set; }

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
        public double TrackballFriction { get; set; }

        [XmlElement(ElementName = "TouchRelMouseRotation")]
        public int TouchRelMouseRotation { get; set; }

        [XmlElement(ElementName = "TouchRelMouseMinThreshold")]
        public double TouchRelMouseMinThreshold { get; set; }

        [XmlElement(ElementName = "TouchpadAbsMouseSettings")]
        public TouchpadAbsMouseSettings TouchpadAbsMouseSettings { get; set; } = new();

        [XmlElement(ElementName = "OutputContDevice")]
        public OutContType OutputContDevice { get; set; }

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

        public DS4Windows()
        {
        }

        public DS4Windows(IBackingStore store, int device, string appVersion, int configVersion)
        {
            AppVersion = appVersion;
            ConfigVersion = configVersion;

            CopyFrom(store, device);
        }

        /// <summary>
        ///     Converts properties from <see cref="IBackingStore" /> for a specified device index to this
        ///     <see cref="DS4Windows" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore"/>.</param>
        /// <param name="device">The zero-based device index to copy.</param>
        public void CopyFrom(IBackingStore store, int device)
        {
            var light = store.LightbarSettingInfo[device];

            TouchToggle = store.EnableTouchToggle[device];
            IdleDisconnectTimeout = store.IdleDisconnectTimeout[device];
            OutputDataToDS4 = store.EnableOutputDataToDS4[device];
            Color = light.Ds4WinSettings.Led;
            RumbleBoost = store.RumbleBoost[device];
            RumbleAutostopTime = store.RumbleAutostopTime[device];
            LightbarMode = store.LightbarSettingInfo[device].Mode;
            LedAsBatteryIndicator = light.Ds4WinSettings.LedAsBattery;
            FlashType = light.Ds4WinSettings.FlashType;
            FlashBatteryAt = light.Ds4WinSettings.FlashAt;
            TouchSensitivity = store.TouchSensitivity[device];
            
            LowColor = light.Ds4WinSettings.LowLed;
            ChargingColor = light.Ds4WinSettings.ChargingLed;
            FlashColor = light.Ds4WinSettings.FlashLed;
            
            TouchpadJitterCompensation = store.TouchpadJitterCompensation[device];
            LowerRCOn = store.LowerRCOn[device];
            TapSensitivity = store.TapSensitivity[device];
            DoubleTap = store.DoubleTap[device];
            ScrollSensitivity = store.ScrollSensitivity[device];
            
            LeftTriggerMiddle = store.L2ModInfo[device].deadZone;
            RightTriggerMiddle = store.R2ModInfo[device].deadZone;
            
            TouchpadInvert = store.TouchPadInvert[device];
            TouchpadClickPassthru = store.TouchClickPassthru[device];
            
            L2AntiDeadZone = store.L2ModInfo[device].AntiDeadZone;
            R2AntiDeadZone = store.R2ModInfo[device].AntiDeadZone;
            
            L2MaxZone = store.L2ModInfo[device].maxZone;
            R2MaxZone = store.R2ModInfo[device].maxZone;
            
            L2MaxOutput = store.L2ModInfo[device].maxOutput;
            R2MaxOutput = store.R2ModInfo[device].maxOutput;
            
            ButtonMouseSensitivity = store.ButtonMouseInfos[device].buttonSensitivity;
            ButtonMouseOffset = store.ButtonMouseInfos[device].mouseVelocityOffset;
            
            Rainbow = light.Ds4WinSettings.Rainbow;
            MaxSatRainbow = Convert.ToInt32(light.Ds4WinSettings.MaxRainbowSaturation * 100.0);
            
            LSDeadZone = store.LSModInfo[device].DeadZone;
            RSDeadZone = store.RSModInfo[device].DeadZone;
            
            LSAntiDeadZone = store.LSModInfo[device].AntiDeadZone;
            RSAntiDeadZone = store.RSModInfo[device].AntiDeadZone;

            LSMaxZone = store.LSModInfo[device].MaxZone;
            RSMaxZone = store.RSModInfo[device].MaxZone;

            LSVerticalScale = store.LSModInfo[device].VerticalScale;
            RSVerticalScale = store.RSModInfo[device].VerticalScale;

            LSMaxOutput = store.LSModInfo[device].MaxOutput;
            RSMaxOutput = store.RSModInfo[device].MaxOutput;

            LSMaxOutputForce = store.LSModInfo[device].MaxOutputForce;
            RSMaxOutputForce = store.RSModInfo[device].MaxOutputForce;

            LSDeadZoneType = store.LSModInfo[device].DZType;
            RSDeadZoneType = store.RSModInfo[device].DZType;

            LSAxialDeadOptions.DeadZoneX = store.LSModInfo[device].XAxisDeadInfo.DeadZone;
            LSAxialDeadOptions.DeadZoneY = store.LSModInfo[device].YAxisDeadInfo.DeadZone;
            LSAxialDeadOptions.MaxZoneX = store.LSModInfo[device].XAxisDeadInfo.MaxZone;
            LSAxialDeadOptions.MaxZoneY = store.LSModInfo[device].YAxisDeadInfo.MaxZone;
            LSAxialDeadOptions.AntiDeadZoneX = store.LSModInfo[device].XAxisDeadInfo.AntiDeadZone;
            LSAxialDeadOptions.AntiDeadZoneY = store.LSModInfo[device].YAxisDeadInfo.AntiDeadZone;
            LSAxialDeadOptions.MaxOutputX = store.LSModInfo[device].XAxisDeadInfo.MaxOutput;
            LSAxialDeadOptions.MaxOutputY = store.LSModInfo[device].YAxisDeadInfo.MaxOutput;

            RSAxialDeadOptions.DeadZoneX = store.RSModInfo[device].XAxisDeadInfo.DeadZone;
            RSAxialDeadOptions.DeadZoneY = store.RSModInfo[device].YAxisDeadInfo.DeadZone;
            RSAxialDeadOptions.MaxZoneX = store.RSModInfo[device].XAxisDeadInfo.MaxZone;
            RSAxialDeadOptions.MaxZoneY = store.RSModInfo[device].YAxisDeadInfo.MaxZone;
            RSAxialDeadOptions.AntiDeadZoneX = store.RSModInfo[device].XAxisDeadInfo.AntiDeadZone;
            RSAxialDeadOptions.AntiDeadZoneY = store.RSModInfo[device].YAxisDeadInfo.AntiDeadZone;
            RSAxialDeadOptions.MaxOutputX = store.RSModInfo[device].XAxisDeadInfo.MaxOutput;
            RSAxialDeadOptions.MaxOutputY = store.RSModInfo[device].YAxisDeadInfo.MaxOutput;

            LSRotation = Convert.ToInt32(store.LSRotation[device] * 180.0 / Math.PI);
            RSRotation = Convert.ToInt32(store.RSRotation[device] * 180.0 / Math.PI);

            LSFuzz = store.LSModInfo[device].Fuzz;
            RSFuzz = store.RSModInfo[device].Fuzz;

            LSOuterBindDead = Convert.ToInt32(store.LSModInfo[device].OuterBindDeadZone);
            RSOuterBindDead = Convert.ToInt32(store.RSModInfo[device].OuterBindDeadZone);

            LSOuterBindInvert = store.LSModInfo[device].OuterBindInvert;
            RSOuterBindInvert = store.RSModInfo[device].OuterBindInvert;

            SXDeadZone = store.SXDeadzone[device];
            SZDeadZone = store.SZDeadzone[device];

            SXMaxZone = Convert.ToInt32(store.SXMaxzone[device] * 100.0);
            SZMaxZone = Convert.ToInt32(store.SZMaxzone[device] * 100.0);

            SXAntiDeadZone = Convert.ToInt32(store.SXAntiDeadzone[device] * 100.0);
            SZAntiDeadZone = Convert.ToInt32(store.SZAntiDeadzone[device] * 100.0);

            Sensitivity = new SensitivityProxyType()
            {
                LSSens = store.LSSens[device],
                RSSens = store.RSSens[device],
                L2Sens = store.L2Sens[device],
                R2Sens = store.R2Sens[device],
                SXSens = store.SXSens[device],
                SZSens = store.SZSens[device]
            };

            ChargingType = light.Ds4WinSettings.ChargingType;

            MouseAcceleration = store.ButtonMouseInfos[device].mouseAccel;
            ButtonMouseVerticalScale = Convert.ToInt32(store.ButtonMouseInfos[device].buttonVerticalScale * 100);

            LaunchProgram = store.LaunchProgram[device];
            DinputOnly = store.DirectInputOnly[device];
            StartTouchpadOff = store.StartTouchpadOff[device];
            TouchpadOutputMode = store.TouchOutMode[device];
            SATriggers = store.SATriggers[device];
            SATriggerCond = store.SaTriggerCondString(store.SATriggerCondition[device]);
            SASteeringWheelEmulationAxis = store.SASteeringWheelEmulationAxis[device];
            SASteeringWheelEmulationRange = store.SASteeringWheelEmulationRange[device];
            SASteeringWheelFuzz = store.SAWheelFuzzValues[device];
            
            SASteeringWheelSmoothingOptions.SASteeringWheelUseSmoothing = store.WheelSmoothInfo[device].Enabled;
            SASteeringWheelSmoothingOptions.SASteeringWheelSmoothMinCutoff = store.WheelSmoothInfo[device].MinCutoff;
            SASteeringWheelSmoothingOptions.SASteeringWheelSmoothBeta = store.WheelSmoothInfo[device].Beta;

            TouchDisInvTriggers = store.TouchDisInvertTriggers[device].ToList();

            GyroSensitivity = store.GyroSensitivity[device];
            GyroSensVerticalScale = store.GyroSensVerticalScale[device];
            GyroInvert = store.GyroInvert[device];
            GyroTriggerTurns = store.GyroTriggerTurns[device];

            GyroControlsSettings.Triggers = store.GyroControlsInfo[device].Triggers;
            GyroControlsSettings.TriggerCond = store.SaTriggerCondString(store.GyroControlsInfo[device].TriggerCond);
            GyroControlsSettings.TriggerTurns = store.GyroControlsInfo[device].TriggerTurns;
            GyroControlsSettings.Toggle = store.GyroControlsInfo[device].TriggerToggle;

            GyroMouseSmoothingSettings.UseSmoothing = store.GyroMouseInfo[device].enableSmoothing;
            GyroMouseSmoothingSettings.SmoothingMethod = store.GyroMouseInfo[device].SmoothMethodIdentifier();
            GyroMouseSmoothingSettings.SmoothingWeight =
                Convert.ToInt32(store.GyroMouseInfo[device].smoothingWeight * 100);
            GyroMouseSmoothingSettings.SmoothingMinCutoff = store.GyroMouseInfo[device].minCutoff;
            GyroMouseSmoothingSettings.SmoothingBeta = store.GyroMouseInfo[device].beta;

            GyroMouseHAxis = store.GyroMouseHorizontalAxis[device];
            GyroMouseDeadZone = store.GyroMouseDeadZone[device];
            GyroMouseMinThreshold = store.GyroMouseInfo[device].minThreshold;
            GyroMouseToggle = store.GyroMouseToggle[device];
            GyroOutputMode = store.GyroOutputMode[device];
            GyroMouseStickTriggers = store.SAMouseStickTriggers[device];
            GyroMouseStickTriggerCond = store.SaTriggerCondString(store.SAMouseStickTriggerCond[device]);
            GyroMouseStickTriggerTurns = store.GyroMouseStickTriggerTurns[device];
            GyroMouseStickHAxis = store.GyroMouseStickHorizontalAxis[device];
            GyroMouseStickDeadZone = store.GyroMouseStickInfo[device].DeadZone;
            GyroMouseStickMaxZone = store.GyroMouseStickInfo[device].MaxZone;
            GyroMouseStickOutputStick = store.GyroMouseStickInfo[device].outputStick;
            GyroMouseStickOutputStickAxes = store.GyroMouseStickInfo[device].outputStickDir;
            GyroMouseStickAntiDeadX = store.GyroMouseStickInfo[device].AntiDeadX;
            GyroMouseStickAntiDeadY = store.GyroMouseStickInfo[device].AntiDeadY;
            GyroMouseStickInvert = store.GyroMouseStickInfo[device].Inverted;
            GyroMouseStickToggle = store.GyroMouseStickToggle[device];
            GyroMouseStickMaxOutput = store.GyroMouseStickInfo[device].MaxOutput;
            GyroMouseStickMaxOutputEnabled = store.GyroMouseStickInfo[device].MaxOutputEnabled;
            GyroMouseStickVerticalScale = store.GyroMouseStickInfo[device].VertScale;

            GyroMouseStickSmoothingSettings.UseSmoothing = store.GyroMouseStickInfo[device].UseSmoothing;
            GyroMouseStickSmoothingSettings.SmoothingMethod = store.GyroMouseStickInfo[device].SmoothMethodIdentifier();
            GyroMouseStickSmoothingSettings.SmoothingWeight = Convert.ToInt32(store.GyroMouseStickInfo[device].SmoothWeight * 100);
            GyroMouseStickSmoothingSettings.SmoothingMinCutoff = store.GyroMouseStickInfo[device].minCutoff;
            GyroMouseStickSmoothingSettings.SmoothingBeta = store.GyroMouseStickInfo[device].beta;

            GyroSwipeSettings.DeadZoneX = store.GyroSwipeInfo[device].deadzoneX;
            GyroSwipeSettings.DeadZoneY = store.GyroSwipeInfo[device].deadzoneY;
            GyroSwipeSettings.Triggers = store.GyroSwipeInfo[device].triggers;
            GyroSwipeSettings.TriggerCond = store.SaTriggerCondString(store.GyroSwipeInfo[device].triggerCond);
            GyroSwipeSettings.TriggerTurns = store.GyroSwipeInfo[device].triggerTurns;
            GyroSwipeSettings.XAxis = store.GyroSwipeInfo[device].xAxis;
            GyroSwipeSettings.DelayTime = store.GyroSwipeInfo[device].delayTime;

            ProfileActions = string.Join("/", store.ProfileActions[device]);
            BTPollRate = store.BluetoothPollRate[device];

            // TODO: missing stuff

            LSSquareStick = store.SquStickInfo[device].LSMode;
            RSSquareStick = store.SquStickInfo[device].RSMode;

            SquareStickRoundness = store.SquStickInfo[device].LSRoundness;
            SquareRStickRoundness = store.SquStickInfo[device].RSRoundness;

            LSAntiSnapback = store.LSAntiSnapbackInfo[device].Enabled;
            RSAntiSnapback = store.RSAntiSnapbackInfo[device].Enabled;

            LSAntiSnapbackDelta = store.LSAntiSnapbackInfo[device].Delta;
            RSAntiSnapbackDelta = store.RSAntiSnapbackInfo[device].Delta;

            LSAntiSnapbackTimeout = store.LSAntiSnapbackInfo[device].Timeout;
            RSAntiSnapbackTimeout = store.RSAntiSnapbackInfo[device].Timeout;

            LSOutputMode = store.LSOutputSettings[device].Mode;
            RSOutputMode = store.RSOutputSettings[device].Mode;

            LSOutputSettings.FlickStickSettings.RealWorldCalibration = store
                .LSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .realWorldCalibration;
            LSOutputSettings.FlickStickSettings.FlickThreshold = store
                .LSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .flickThreshold;
            LSOutputSettings.FlickStickSettings.FlickTime = store
                .LSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .flickTime;

            RSOutputSettings.FlickStickSettings.RealWorldCalibration = store
                .RSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .realWorldCalibration;
            RSOutputSettings.FlickStickSettings.FlickThreshold = store
                .RSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .flickThreshold;
            RSOutputSettings.FlickStickSettings.FlickTime = store
                .RSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .flickTime;

            L2OutputCurveMode = store.AxisOutputCurveString(store.GetL2OutCurveMode(device));
            L2OutputCurveCustom = store.L2OutBezierCurveObj[device];

            //
            // TODO: missing
            // 

            L2TwoStageMode = store.L2OutputSettings[device].twoStageMode;
            R2TwoStageMode = store.R2OutputSettings[device].twoStageMode;

            L2TriggerEffect = store.L2OutputSettings[device].triggerEffect;
            R2TriggerEffect = store.R2OutputSettings[device].triggerEffect;

            

            R2OutputCurveMode = store.AxisOutputCurveString(store.GetR2OutCurveMode(device));
            R2OutputCurveCustom = store.R2OutBezierCurveObj[device];

            SXOutputCurveMode = store.AxisOutputCurveString(store.GetSXOutCurveMode(device));
            SXOutputCurveCustom = store.SXOutBezierCurveObj[device];

            SZOutputCurveMode = store.AxisOutputCurveString(store.GetSZOutCurveMode(device));
            SZOutputCurveCustom = store.SZOutBezierCurveObj[device];

            TrackballMode = store.TrackballMode[device];
            TrackballFriction = store.TrackballFriction[device];

            TouchRelMouseRotation = Convert.ToInt32(store.TouchPadRelMouse[device].Rotation * 180.0 / Math.PI);
            TouchRelMouseMinThreshold = store.TouchPadRelMouse[device].MinThreshold;

            TouchpadAbsMouseSettings.MaxZoneX = store.TouchPadAbsMouse[device].MaxZoneX;
            TouchpadAbsMouseSettings.MaxZoneY = store.TouchPadAbsMouse[device].MaxZoneY;
            TouchpadAbsMouseSettings.SnapToCenter = store.TouchPadAbsMouse[device].SnapToCenter;

            OutputContDevice = store.OutputDeviceType[device];
        }
    }
}