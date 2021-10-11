using System;
using System.Collections.Generic;
using DS4Windows;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    /// <summary>
    ///     "New" controller profile definition.
    /// </summary>
    public partial class DS4WindowsProfile
    {
        /// <summary>
        ///     Auto-generated unique ID for this profile.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        ///     Friendly name of this profile.
        /// </summary>
        public string DisplayName { get; set; }

        public ButtonMouseInfo ButtonMouseInfo { get; set; } = new();

        public GyroControlsInfo GyroControlsInfo { get; set; } = new();

        public bool EnableTouchToggle = true;

        public int IdleDisconnectTimeout { get; set; } = 0;

        public bool EnableOutputDataToDS4 { get; set; } = true;

        public bool TouchpadJitterCompensation { get; set; } = true;

        public bool LowerRCOn { get; set; } = false;

        public bool TouchClickPassthru { get; set; } = false;

        public byte RumbleBoost { get; set; } = 100;

        public int RumbleAutostopTime { get; set; } = 0;

        public byte TouchSensitivity { get; set; } = 100;

        public StickDeadZoneInfo LSModInfo { get; set; } = new();

        public StickDeadZoneInfo RSModInfo { get; set; } = new();

        public TriggerDeadZoneZInfo L2ModInfo { get; set; } = new();
        
        public TriggerDeadZoneZInfo R2ModInfo { get; set; } = new();

        public double LSRotation { get; set; } = 0.0;

        public double RSRotation { get; set; } = 0.0;

        public double SXDeadzone { get; set; } = 0.25;
        
        public double SZDeadzone { get; set; } = 0.25;

        public double SXMaxzone { get; set; } = 1.0;
        
        public double SZMaxzone { get; set; } = 1.0;

        public double SXAntiDeadzone { get; set; } = 0.0;

        public double SZAntiDeadzone { get; set; } = 0.0;

        public double L2Sens { get; set; } = 1;

        public double R2Sens { get; set; } = 1;

        public double LSSens { get; set; } = 1;
        
        public double RSSens { get; set; } = 1;
        
        public double SXSens { get; set; } = 1;
        
        public double SZSens { get; set; } = 1;

        public byte TapSensitivity { get; set; } = 0;

        public bool DoubleTap { get; set; } = false;

        public int ScrollSensitivity { get; set; } = 0;

        public int TouchPadInvert { get; set; } = 0;

        public int BluetoothPollRate { get; set; } = 4;

        public StickOutputSetting LSOutputSettings { get; set; } = new();
        
        public StickOutputSetting RSOutputSettings { get; set; } = new();
        
        public StickOutputSetting L2OutputSettings { get; set; } = new();
        
        public StickOutputSetting R2OutputSettings { get; set; } = new();

        public string LaunchProgram { get; set; }

        public bool DirectInputOnly { get; set; } = false;

        public bool StartTouchpadOff { get; set; } = false;

        public TouchpadOutMode TouchOutMode { get; set; } = TouchpadOutMode.Mouse;

        public string SATriggers { get; set; } = "-1";

        public bool SATriggerCondition { get; set; } = true;

        public GyroOutMode GyroOutputMode { get; set; } = GyroOutMode.Controls;

        public string SAMouseStickTriggers { get; set; } = "-1";

        public bool SAMouseStickTriggerCond { get; set; } = true;

        public GyroMouseStickInfo GyroMouseStickInfo { get; set; } = new();

        public GyroDirectionalSwipeInfo GyroSwipeInfo { get; set; } = new();

        public bool GyroMouseStickToggle { get; set; } = false;
        
        public bool GyroMouseStickTriggerTurns { get; set; } = true;

        public SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis { get; set; } =
            SASteeringWheelEmulationAxisType.None;

        public int SASteeringWheelEmulationRange { get; set; } = 360;

        public int SAWheelFuzzValues { get; set; } = 0;

        public SteeringWheelSmoothingInfo WheelSmoothInfo { get; set; } = new();

        public IList<int> TouchDisInvertTriggers { get; set; } = new List<int>() { -1 };

        public int GyroSensitivity { get; set; } = 100;

        public int GyroSensVerticalScale { get; set; } = 100;

        public int GyroInvert { get; set; } = 0;

        public bool GyroTriggerTurns { get; set; } = true;

        public GyroMouseInfo GyroMouseInfo { get; set; } = new();

        public int GyroMouseHorizontalAxis { get; set; } = 0;

        public bool GyroMouseToggle { get; set; } = false;

        public SquareStickInfo SquStickInfo { get; set; } = new();

        public StickAntiSnapbackInfo LSAntiSnapbackInfo { get; set; } = new();

        public BezierCurve LSOutCurve { get; set; } = new();
        
        public BezierCurve RSOutCurve { get; set; } = new();
        
        public BezierCurve L2OutCurve { get; set; } = new();
        
        public BezierCurve R2OutCurve { get; set; } = new();
        
        public BezierCurve SXOutCurve { get; set; } = new();
        
        public BezierCurve SZOutCurve { get; set; } = new();

        public bool TrackballMode { get; set; } = false;

        public double TrackballFriction { get; set; } = 10.0;

        public TouchpadAbsMouseSettings TouchPadAbsMouse { get; set; } = new();

        public TouchPadRelMouseSettings TouchPadRelMouse { get; set; } = new();

        public OutContType OutputDeviceType { get; set; } = OutContType.X360;

        public bool Ds4Mapping { get; set; } = false;

        public LightbarSettingInfo LightbarSettingInfo { get; set; } = new();
    }
}