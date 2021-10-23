using System;
using System.Collections.Generic;
using System.Linq;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using DS4WinWPF.DS4Control.Profiles.Schema.Migrations;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    public partial class DS4WindowsProfileV3 : XmlSerializable<DS4WindowsProfileV3>
    {
        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .EnableReferences()
                .EnableMemberExceptionHandling()
                .EnableImplicitTyping(typeof(DS4WindowsProfileV3))
                .Type<DS4Color>().Register().Converter().Using(DS4ColorConverter.Default)
                .Type<SensitivityProxyType>().Register().Converter().Using(SensitivityConverter.Default)
                .Type<List<int>>().Register().Converter().Using(IntegerListConverterConverter.Default)
                .Type<bool>().Register().Converter().Using(BooleanConverter.Default)
                .Type<BezierCurve>().Register().Converter().Using(BezierCurveConverter.Default)
                .Type<double>().Register().Converter().Using(DoubleConverter.Default)
                .Type<ControlsCollectionEntity>().AddMigration(new ControlsCollectionEntityMigration())
                .Create();
        }

        /// <summary>
        ///     Converts properties from <see cref="IBackingStore" /> for a specified device index to this
        ///     <see cref="DS4WindowsProfileV3" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore" />.</param>
        /// <param name="device">The zero-based device index to copy.</param>
        [IntermediateSolution]
        public void CopyFrom(IAppSettingsService appSettings, IBackingStore store, int device)
        {
            var light = appSettings.Settings.LightbarSettingInfo[device];

            IdleDisconnectTimeout = store.IdleDisconnectTimeout[device];
            Color = light.Ds4WinSettings.Led;
            LightbarMode = appSettings.Settings.LightbarSettingInfo[device].Mode;
            LedAsBatteryIndicator = light.Ds4WinSettings.LedAsBattery;
            FlashType = light.Ds4WinSettings.FlashType;
            FlashBatteryAt = light.Ds4WinSettings.FlashAt;

            LowColor = light.Ds4WinSettings.LowLed;
            ChargingColor = light.Ds4WinSettings.ChargingLed;
            FlashColor = light.Ds4WinSettings.FlashLed;

            TapSensitivity = store.TapSensitivity[device];
            ScrollSensitivity = store.ScrollSensitivity[device];

            LeftTriggerMiddle = store.L2ModInfo[device].deadZone;
            RightTriggerMiddle = store.R2ModInfo[device].deadZone;

            TouchpadInvert = store.TouchPadInvert[device];

            L2AntiDeadZone = store.L2ModInfo[device].AntiDeadZone;
            R2AntiDeadZone = store.R2ModInfo[device].AntiDeadZone;

            L2MaxZone = store.L2ModInfo[device].maxZone;
            R2MaxZone = store.R2ModInfo[device].maxZone;

            L2MaxOutput = store.L2ModInfo[device].maxOutput;
            R2MaxOutput = store.R2ModInfo[device].maxOutput;

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

            //LSRotation = Convert.ToInt32(store.LSRotation[device] * 180.0 / Math.PI);
            //RSRotation = Convert.ToInt32(store.RSRotation[device] * 180.0 / Math.PI);

            LSFuzz = store.LSModInfo[device].Fuzz;
            RSFuzz = store.RSModInfo[device].Fuzz;

            LSOuterBindDead = Convert.ToInt32(store.LSModInfo[device].OuterBindDeadZone);
            RSOuterBindDead = Convert.ToInt32(store.RSModInfo[device].OuterBindDeadZone);

            LSOuterBindInvert = store.LSModInfo[device].OuterBindInvert;
            RSOuterBindInvert = store.RSModInfo[device].OuterBindInvert;

            /*
             TODO: migrate
            SXDeadZone = store.SXDeadzone[device];
            SZDeadZone = store.SZDeadzone[device];

            SXMaxZone = Convert.ToInt32(store.SXMaxzone[device] * 100.0);
            SZMaxZone = Convert.ToInt32(store.SZMaxzone[device] * 100.0);

            SXAntiDeadZone = Convert.ToInt32(store.SXAntiDeadzone[device] * 100.0);
            SZAntiDeadZone = Convert.ToInt32(store.SZAntiDeadzone[device] * 100.0);

            Sensitivity = new SensitivityProxyType
            {
                LSSens = store.LSSens[device],
                RSSens = store.RSSens[device],
                L2Sens = store.L2Sens[device],
                R2Sens = store.R2Sens[device],
                SXSens = store.SXSens[device],
                SZSens = store.SZSens[device]
            };
            */

            ChargingType = light.Ds4WinSettings.ChargingType;

            /*
             TODO: port
            MouseAcceleration = store.ButtonMouseInfos[device].mouseAccel;
            ButtonMouseVerticalScale = Convert.ToInt32(store.ButtonMouseInfos[device].buttonVerticalScale * 100);
            */

            LaunchProgram = store.LaunchProgram[device];
            //StartTouchpadOff = store.StartTouchpadOff[device];
            TouchpadOutputMode = store.TouchOutMode[device];
            SATriggers = store.SATriggers[device];
            SATriggerCond = store.SaTriggerCondString(store.SATriggerCondition[device]);
            SASteeringWheelEmulationAxis = store.SASteeringWheelEmulationAxis[device];
            //SASteeringWheelEmulationRange = store.SASteeringWheelEmulationRange[device];
            SASteeringWheelFuzz = store.SAWheelFuzzValues[device];

            SASteeringWheelSmoothingOptions.SASteeringWheelUseSmoothing = store.WheelSmoothInfo[device].Enabled;
            SASteeringWheelSmoothingOptions.SASteeringWheelSmoothMinCutoff = store.WheelSmoothInfo[device].MinCutoff;
            SASteeringWheelSmoothingOptions.SASteeringWheelSmoothBeta = store.WheelSmoothInfo[device].Beta;

            TouchDisInvTriggers = store.TouchDisInvertTriggers[device].ToList();

            //GyroSensitivity = store.GyroSensitivity[device];
            //GyroSensVerticalScale = store.GyroSensVerticalScale[device];
            //GyroInvert = store.GyroInvert[device];

            GyroMouseSmoothingSettings.UseSmoothing = store.GyroMouseInfo[device].enableSmoothing;
            GyroMouseSmoothingSettings.SmoothingMethod = store.GyroMouseInfo[device].SmoothMethodIdentifier();
            GyroMouseSmoothingSettings.SmoothingWeight =
                Convert.ToInt32(store.GyroMouseInfo[device].smoothingWeight * 100);
            GyroMouseSmoothingSettings.SmoothingMinCutoff = store.GyroMouseInfo[device].minCutoff;
            GyroMouseSmoothingSettings.SmoothingBeta = store.GyroMouseInfo[device].beta;

            GyroMouseHAxis = store.GyroMouseHorizontalAxis[device];
            GyroMouseDeadZone = store.GyroMouseDeadZone[device];
            GyroMouseMinThreshold = store.GyroMouseInfo[device].minThreshold;
            GyroOutputMode = store.GyroOutputMode[device];
            GyroMouseStickTriggers = store.SAMouseStickTriggers[device];
            //GyroMouseStickTriggerCond = store.SaTriggerCondString(store.SAMouseStickTriggerCond[device]);
            GyroMouseStickHAxis = store.GyroMouseStickHorizontalAxis[device];
            GyroMouseStickDeadZone = store.GyroMouseStickInfo[device].DeadZone;
            GyroMouseStickMaxZone = store.GyroMouseStickInfo[device].MaxZone;
            GyroMouseStickOutputStick = store.GyroMouseStickInfo[device].outputStick;
            GyroMouseStickOutputStickAxes = store.GyroMouseStickInfo[device].outputStickDir;
            GyroMouseStickAntiDeadX = store.GyroMouseStickInfo[device].AntiDeadX;
            GyroMouseStickAntiDeadY = store.GyroMouseStickInfo[device].AntiDeadY;
            GyroMouseStickInvert = store.GyroMouseStickInfo[device].Inverted;
            GyroMouseStickMaxOutput = store.GyroMouseStickInfo[device].MaxOutput;
            GyroMouseStickMaxOutputEnabled = store.GyroMouseStickInfo[device].MaxOutputEnabled;
            GyroMouseStickVerticalScale = store.GyroMouseStickInfo[device].VertScale;

            GyroMouseStickSmoothingSettings.UseSmoothing = store.GyroMouseStickInfo[device].UseSmoothing;
            GyroMouseStickSmoothingSettings.SmoothingMethod = store.GyroMouseStickInfo[device].SmoothMethodIdentifier();
            GyroMouseStickSmoothingSettings.SmoothingWeight =
                Convert.ToInt32(store.GyroMouseStickInfo[device].SmoothWeight * 100);
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

            LSOutputCurveMode = store.StickOutputCurveString(store.GetLsOutCurveMode(device));
            LSOutputCurveCustom = store.LSOutBezierCurveObj[device];

            RSOutputCurveMode = store.StickOutputCurveString(store.GetRsOutCurveMode(device));
            RSOutputCurveCustom = store.RSOutBezierCurveObj[device];

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
            LSOutputSettings.FlickStickSettings.MinAngleThreshold = store
                .LSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .minAngleThreshold;

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
            RSOutputSettings.FlickStickSettings.MinAngleThreshold = store
                .RSOutputSettings[device]
                .OutputSettings
                .flickSettings
                .minAngleThreshold;

            L2OutputCurveMode = store.AxisOutputCurveString(store.GetL2OutCurveMode(device));
            L2OutputCurveCustom = store.L2OutBezierCurveObj[device];

            L2TwoStageMode = store.L2OutputSettings[device].twoStageMode;
            R2TwoStageMode = store.R2OutputSettings[device].twoStageMode;

            L2HipFireTime = store.L2OutputSettings[device].hipFireMS;
            R2HipFireTime = store.R2OutputSettings[device].hipFireMS;

            L2TriggerEffect = store.L2OutputSettings[device].triggerEffect;
            R2TriggerEffect = store.R2OutputSettings[device].triggerEffect;

            R2OutputCurveMode = store.AxisOutputCurveString(store.GetR2OutCurveMode(device));
            R2OutputCurveCustom = store.R2OutBezierCurveObj[device];

            SXOutputCurveMode = store.AxisOutputCurveString(store.GetSXOutCurveMode(device));
            SXOutputCurveCustom = store.SXOutBezierCurveObj[device];

            SZOutputCurveMode = store.AxisOutputCurveString(store.GetSZOutCurveMode(device));
            SZOutputCurveCustom = store.SZOutBezierCurveObj[device];

            TrackballFriction = store.TrackballFriction[device];

            TouchRelMouseRotation = Convert.ToInt32(store.TouchPadRelMouse[device].Rotation * 180.0 / Math.PI);
            TouchRelMouseMinThreshold = store.TouchPadRelMouse[device].MinThreshold;

            TouchpadAbsMouseSettings.MaxZoneX = store.TouchPadAbsMouse[device].MaxZoneX;
            TouchpadAbsMouseSettings.MaxZoneY = store.TouchPadAbsMouse[device].MaxZoneY;
            TouchpadAbsMouseSettings.SnapToCenter = store.TouchPadAbsMouse[device].SnapToCenter;
        }

        /// <summary>
        ///     Injects properties from <see cref="DS4WindowsProfileV3" /> for a specified device index into
        ///     <see cref="IBackingStore" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore" />.</param>
        /// <param name="device">The zero-based device index to copy.</param>
        [IntermediateSolution]
        public void CopyTo(IAppSettingsService appSettings, IBackingStore store, int device)
        {
            var lightbarSettings = appSettings.Settings.LightbarSettingInfo[device];
            var lightInfo = lightbarSettings.Ds4WinSettings;

            store.IdleDisconnectTimeout[device] = IdleDisconnectTimeout;
            lightbarSettings.Mode = LightbarMode;
            lightInfo.Led = Color;
            lightInfo.LedAsBattery = LedAsBatteryIndicator;
            lightInfo.FlashType = FlashType;
            lightInfo.FlashAt = FlashBatteryAt;
            lightInfo.LowLed = LowColor;
            lightInfo.ChargingLed = ChargingColor;
            lightInfo.FlashLed = FlashColor;
            store.TapSensitivity[device] = TapSensitivity;
            store.ScrollSensitivity[device] = ScrollSensitivity;
            store.TouchPadInvert[device] = Math.Min(Math.Max(TouchpadInvert, 0), 3);
            store.L2ModInfo[device].deadZone = LeftTriggerMiddle;
            store.R2ModInfo[device].deadZone = RightTriggerMiddle;
            store.L2ModInfo[device].AntiDeadZone = L2AntiDeadZone;
            store.R2ModInfo[device].AntiDeadZone = R2AntiDeadZone;
            store.L2ModInfo[device].maxZone = Math.Min(Math.Max(L2MaxZone, 0), 100);
            store.R2ModInfo[device].maxZone = Math.Min(Math.Max(R2MaxZone, 0), 100);
            store.L2ModInfo[device].maxOutput = Math.Min(Math.Max(L2MaxOutput, 0.0), 100.0);
            store.R2ModInfo[device].maxOutput = Math.Min(Math.Max(R2MaxOutput, 0.0), 100.0);
            //store.LSRotation[device] = Math.Min(Math.Max(LSRotation, -180), 180) * Math.PI / 180.0;
            //store.RSRotation[device] = Math.Min(Math.Max(RSRotation, -180), 180) * Math.PI / 180.0;
            store.LSModInfo[device].Fuzz = Math.Min(Math.Max(LSFuzz, 0), 100);
            store.RSModInfo[device].Fuzz = Math.Min(Math.Max(RSFuzz, 0), 100);
            /*
             TODO: port math
            store.ButtonMouseInfos[device].buttonSensitivity = ButtonMouseSensitivity;
            store.ButtonMouseInfos[device].mouseVelocityOffset = ButtonMouseOffset;
            store.ButtonMouseInfos[device].buttonVerticalScale =
                Math.Min(Math.Max(ButtonMouseVerticalScale, 0), 500) * 0.01;
            */
            lightInfo.Rainbow = Rainbow;
            lightInfo.MaxRainbowSaturation = Math.Max(0, Math.Min(100, MaxSatRainbow)) / 100.0;
            store.LSModInfo[device].DeadZone = Math.Min(Math.Max(LSDeadZone, 0), 127);
            store.RSModInfo[device].DeadZone = Math.Min(Math.Max(RSDeadZone, 0), 127);
            store.LSModInfo[device].AntiDeadZone = LSAntiDeadZone;
            store.RSModInfo[device].AntiDeadZone = RSAntiDeadZone;
            store.LSModInfo[device].MaxZone = Math.Min(Math.Max(LSMaxZone, 0), 100);
            store.RSModInfo[device].MaxZone = Math.Min(Math.Max(RSMaxZone, 0), 100);
            store.LSModInfo[device].VerticalScale = Math.Min(Math.Max(LSVerticalScale, 0.0), 200.0);
            store.RSModInfo[device].VerticalScale = Math.Min(Math.Max(RSVerticalScale, 0.0), 200.0);
            store.LSModInfo[device].MaxOutput = Math.Min(Math.Max(LSMaxOutput, 0.0), 100.0);
            store.RSModInfo[device].MaxOutput = Math.Min(Math.Max(RSMaxOutput, 0.0), 100.0);
            store.LSModInfo[device].MaxOutputForce = LSMaxOutputForce;
            store.RSModInfo[device].MaxOutputForce = RSMaxOutputForce;
            store.LSModInfo[device].OuterBindDeadZone = Math.Min(Math.Max(LSOuterBindDead, 0), 100);
            store.RSModInfo[device].OuterBindDeadZone = Math.Min(Math.Max(RSOuterBindDead, 0), 100);
            store.LSModInfo[device].OuterBindInvert = LSOuterBindInvert;
            store.RSModInfo[device].OuterBindInvert = RSOuterBindInvert;
            store.LSModInfo[device].DZType = LSDeadZoneType;
            store.RSModInfo[device].DZType = RSDeadZoneType;
            store.LSModInfo[device].XAxisDeadInfo.DeadZone = Math.Min(Math.Max(LSAxialDeadOptions.DeadZoneX, 0), 127);
            store.LSModInfo[device].YAxisDeadInfo.DeadZone = Math.Min(Math.Max(LSAxialDeadOptions.DeadZoneY, 0), 127);
            store.LSModInfo[device].XAxisDeadInfo.MaxZone = Math.Min(Math.Max(LSAxialDeadOptions.MaxZoneX, 0), 100);
            store.LSModInfo[device].YAxisDeadInfo.MaxZone = Math.Min(Math.Max(LSAxialDeadOptions.MaxZoneY, 0), 100);
            store.LSModInfo[device].XAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(LSAxialDeadOptions.AntiDeadZoneX, 0), 100);
            store.LSModInfo[device].YAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(LSAxialDeadOptions.AntiDeadZoneY, 0), 100);
            store.LSModInfo[device].XAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(LSAxialDeadOptions.MaxOutputX, 0.0), 100.0);
            store.LSModInfo[device].YAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(LSAxialDeadOptions.MaxOutputY, 0.0), 100.0);
            store.RSModInfo[device].XAxisDeadInfo.DeadZone = Math.Min(Math.Max(RSAxialDeadOptions.DeadZoneX, 0), 127);
            store.RSModInfo[device].YAxisDeadInfo.DeadZone = Math.Min(Math.Max(RSAxialDeadOptions.DeadZoneY, 0), 127);
            store.RSModInfo[device].XAxisDeadInfo.MaxZone = Math.Min(Math.Max(RSAxialDeadOptions.MaxZoneX, 0), 100);
            store.RSModInfo[device].YAxisDeadInfo.MaxZone = Math.Min(Math.Max(RSAxialDeadOptions.MaxZoneY, 0), 100);
            store.RSModInfo[device].XAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(RSAxialDeadOptions.AntiDeadZoneX, 0), 100);
            store.RSModInfo[device].YAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(RSAxialDeadOptions.AntiDeadZoneY, 0), 100);
            store.RSModInfo[device].XAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(RSAxialDeadOptions.MaxOutputX, 0.0), 100.0);
            store.RSModInfo[device].YAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(RSAxialDeadOptions.MaxOutputY, 0.0), 100.0);
            /*
             * TODO: migrate
             
            store.SXDeadzone[device] = SXDeadZone;
            store.SZDeadzone[device] = SZDeadZone;
            store.SXMaxzone[device] = Math.Min(Math.Max(SXMaxZone * 0.01, 0.0), 1.0);
            store.SZMaxzone[device] = Math.Min(Math.Max(SZMaxZone * 0.01, 0.0), 1.0);
            store.SXAntiDeadzone[device] = Math.Min(Math.Max(SXAntiDeadZone * 0.01, 0.0), 1.0);
            store.SZAntiDeadzone[device] = Math.Min(Math.Max(SZAntiDeadZone * 0.01, 0.0), 1.0);

            store.LSSens[device] = Sensitivity.LSSens;
            store.RSSens[device] = Sensitivity.RSSens;
            store.L2Sens[device] = Sensitivity.L2Sens;
            store.R2Sens[device] = Sensitivity.R2Sens;
            store.SXSens[device] = Sensitivity.SXSens;
            store.SZSens[device] = Sensitivity.SZSens;
            */

            lightInfo.ChargingType = ChargingType;
            //store.ButtonMouseInfos[device].mouseAccel = MouseAcceleration;
            //ShiftModifier
            store.LaunchProgram[device] = LaunchProgram;

            store.SATriggers[device] = SATriggers;
            store.SATriggerCondition[device] = store.SaTriggerCondValue(SATriggerCond);
            store.SASteeringWheelEmulationAxis[device] = SASteeringWheelEmulationAxis;
            //store.SASteeringWheelEmulationRange[device] = SASteeringWheelEmulationRange;

            store.WheelSmoothInfo[device].Enabled = SASteeringWheelSmoothingOptions.SASteeringWheelUseSmoothing;
            store.WheelSmoothInfo[device].MinCutoff = SASteeringWheelSmoothingOptions.SASteeringWheelSmoothMinCutoff;
            store.WheelSmoothInfo[device].Beta = SASteeringWheelSmoothingOptions.SASteeringWheelSmoothBeta;

            store.SAWheelFuzzValues[device] = SASteeringWheelFuzz is >= 0 and <= 100 ? SASteeringWheelFuzz : 0;

            store.GyroOutputMode[device] = GyroOutputMode;

            store.SAMouseStickTriggers[device] = GyroMouseStickTriggers;
            //store.SAMouseStickTriggerCond[device] = store.SaTriggerCondValue(GyroMouseStickTriggerCond);
            store.GyroMouseStickHorizontalAxis[device] = Math.Min(Math.Max(0, GyroMouseStickHAxis), 1);
            store.GyroMouseStickInfo[device].DeadZone = GyroMouseStickDeadZone;
            store.GyroMouseStickInfo[device].MaxZone = Math.Max(GyroMouseStickMaxZone, 1);
            store.GyroMouseStickInfo[device].outputStick = GyroMouseStickOutputStick;
            store.GyroMouseStickInfo[device].outputStickDir = GyroMouseStickOutputStickAxes;
            store.GyroMouseStickInfo[device].AntiDeadX = GyroMouseStickAntiDeadX;
            store.GyroMouseStickInfo[device].AntiDeadY = GyroMouseStickAntiDeadY;
            store.GyroMouseStickInfo[device].Inverted = GyroMouseStickInvert;
            //store.SetGyroMouseStickToggle(device, GyroMouseStickToggle, control)
            store.GyroMouseStickInfo[device].MaxOutput = Math.Min(Math.Max(GyroMouseStickMaxOutput, 0.0), 100.0);
            store.GyroMouseStickInfo[device].MaxOutputEnabled = GyroMouseStickMaxOutputEnabled;
            store.GyroMouseStickInfo[device].VertScale = GyroMouseStickVerticalScale;
            store.GyroMouseStickInfo[device].UseSmoothing = GyroMouseStickSmoothingSettings.UseSmoothing;
            store.GyroMouseStickInfo[device].DetermineSmoothMethod(GyroMouseStickSmoothingSettings.SmoothingMethod);
            store.GyroMouseStickInfo[device].SmoothWeight = Math.Min(
                Math.Max(0.0, Convert.ToDouble(GyroMouseStickSmoothingSettings.SmoothingWeight * 0.01)), 1.0);
            store.GyroMouseStickInfo[device].minCutoff =
                Math.Min(Math.Max(0.0, GyroMouseStickSmoothingSettings.SmoothingMinCutoff), 100.0);
            store.GyroMouseStickInfo[device].beta =
                Math.Min(Math.Max(0.0, GyroMouseStickSmoothingSettings.SmoothingBeta), 1.0);

            store.GyroSwipeInfo[device].deadzoneX = GyroSwipeSettings.DeadZoneX;
            store.GyroSwipeInfo[device].deadzoneY = GyroSwipeSettings.DeadZoneY;
            store.GyroSwipeInfo[device].triggers = GyroSwipeSettings.Triggers;
            store.GyroSwipeInfo[device].triggerCond = store.SaTriggerCondValue(GyroSwipeSettings.TriggerCond);
            store.GyroSwipeInfo[device].triggerTurns = GyroSwipeSettings.TriggerTurns;
            store.GyroSwipeInfo[device].xAxis = GyroSwipeSettings.XAxis;
            store.GyroSwipeInfo[device].delayTime = GyroSwipeSettings.DelayTime;

            store.TouchOutMode[device] = TouchpadOutputMode;
            store.TouchDisInvertTriggers[device] = TouchDisInvTriggers;
            //store.GyroSensitivity[device] = GyroSensitivity;
            //store.GyroSensVerticalScale[device] = GyroSensVerticalScale;
            //store.GyroInvert[device] = GyroInvert;

            store.GyroMouseInfo[device].enableSmoothing = GyroMouseSmoothingSettings.UseSmoothing;
            store.GyroMouseInfo[device].DetermineSmoothMethod(GyroMouseSmoothingSettings.SmoothingMethod);
            store.GyroMouseInfo[device].smoothingWeight =
                Math.Min(Math.Max(0.0, Convert.ToDouble(GyroMouseSmoothingSettings.SmoothingWeight * 0.01)), 1.0);
            store.GyroMouseInfo[device].minCutoff =
                Math.Min(Math.Max(0.0, GyroMouseSmoothingSettings.SmoothingMinCutoff), 100.0);
            store.GyroMouseInfo[device].beta = Math.Min(Math.Max(0.0, GyroMouseSmoothingSettings.SmoothingBeta), 1.0);

            store.GyroMouseHorizontalAxis[device] = Math.Min(Math.Max(0, GyroMouseHAxis), 1);
            //store.SetGyroMouseDZ(device, temp, control);
            store.GyroMouseInfo[device].minThreshold = Math.Min(Math.Max(GyroMouseMinThreshold, 1.0), 40.0);
            //SetGyroMouseToggle(device, temp, control);
            store.BluetoothPollRate[device] = BTPollRate is >= 0 and <= 16 ? BTPollRate : 4;

            store.LSOutBezierCurveObj[device] = LSOutputCurveCustom;
            store.SetLsOutCurveMode(device, store.StickOutputCurveId(LSOutputCurveMode));
            store.RSOutBezierCurveObj[device] = RSOutputCurveCustom;
            store.SetRsOutCurveMode(device, store.StickOutputCurveId(RSOutputCurveMode));

            store.SquStickInfo[device].LSMode = LSSquareStick;
            store.SquStickInfo[device].LSRoundness = SquareStickRoundness;
            store.SquStickInfo[device].RSRoundness = SquareRStickRoundness;
            store.SquStickInfo[device].RSMode = RSSquareStick;
            store.LSAntiSnapbackInfo[device].Enabled = LSAntiSnapback;
            store.RSAntiSnapbackInfo[device].Enabled = RSAntiSnapback;
            store.LSAntiSnapbackInfo[device].Delta = LSAntiSnapbackDelta;
            store.RSAntiSnapbackInfo[device].Delta = RSAntiSnapbackDelta;
            store.LSAntiSnapbackInfo[device].Timeout = LSAntiSnapbackTimeout;
            store.RSAntiSnapbackInfo[device].Timeout = RSAntiSnapbackTimeout;
            store.LSOutputSettings[device].Mode = LSOutputMode;
            store.RSOutputSettings[device].Mode = RSOutputMode;

            store.LSOutputSettings[device].OutputSettings.flickSettings.realWorldCalibration =
                LSOutputSettings.FlickStickSettings.RealWorldCalibration;
            store.LSOutputSettings[device].OutputSettings.flickSettings.flickThreshold =
                LSOutputSettings.FlickStickSettings.FlickThreshold;
            store.LSOutputSettings[device].OutputSettings.flickSettings.flickTime =
                LSOutputSettings.FlickStickSettings.FlickTime;
            store.LSOutputSettings[device].OutputSettings.flickSettings.minAngleThreshold =
                LSOutputSettings.FlickStickSettings.MinAngleThreshold;
            store.RSOutputSettings[device].OutputSettings.flickSettings.realWorldCalibration =
                RSOutputSettings.FlickStickSettings.RealWorldCalibration;
            store.RSOutputSettings[device].OutputSettings.flickSettings.flickThreshold =
                RSOutputSettings.FlickStickSettings.FlickThreshold;
            store.RSOutputSettings[device].OutputSettings.flickSettings.flickTime =
                RSOutputSettings.FlickStickSettings.FlickTime;
            store.RSOutputSettings[device].OutputSettings.flickSettings.minAngleThreshold =
                RSOutputSettings.FlickStickSettings.MinAngleThreshold;

            store.L2OutBezierCurveObj[device] = L2OutputCurveCustom;
            store.SetL2OutCurveMode(device, store.StickOutputCurveId(L2OutputCurveMode));
            store.L2OutputSettings[device].TwoStageMode = L2TwoStageMode;
            store.L2OutputSettings[device].hipFireMS = Math.Min(Math.Max(0, L2HipFireTime), 5000);
            store.L2OutputSettings[device].TriggerEffect = L2TriggerEffect;

            store.R2OutBezierCurveObj[device] = R2OutputCurveCustom;
            store.SetR2OutCurveMode(device, store.StickOutputCurveId(R2OutputCurveMode));
            store.R2OutputSettings[device].TwoStageMode = R2TwoStageMode;
            store.R2OutputSettings[device].TriggerEffect = R2TriggerEffect;
            store.R2OutputSettings[device].hipFireMS = Math.Min(Math.Max(0, R2HipFireTime), 5000);

            store.SXOutBezierCurveObj[device] = SXOutputCurveCustom;
            store.SetSXOutCurveMode(device, store.StickOutputCurveId(SXOutputCurveMode));
            store.SZOutBezierCurveObj[device] = SZOutputCurveCustom;
            store.SetSZOutCurveMode(device, store.StickOutputCurveId(SZOutputCurveMode));
            store.TrackballFriction[device] = TrackballFriction;

            store.TouchPadRelMouse[device].Rotation =
                Math.Min(Math.Max(TouchRelMouseRotation, -180), 180) * Math.PI / 180.0;
            store.TouchPadRelMouse[device].MinThreshold = Math.Min(Math.Max(TouchRelMouseMinThreshold, 1.0), 40.0);

            store.TouchPadAbsMouse[device].MaxZoneX = TouchpadAbsMouseSettings.MaxZoneX;
            store.TouchPadAbsMouse[device].MaxZoneY = TouchpadAbsMouseSettings.MaxZoneY;
            store.TouchPadAbsMouse[device].SnapToCenter = TouchpadAbsMouseSettings.SnapToCenter;
        }

        /// <summary>
        ///     Converts a <see cref="DS4WindowsProfileV3"/> to <see cref="DS4WindowsProfile"/>.
        /// </summary>
        /// <param name="profile">The receiving <see cref="DS4WindowsProfile"/>.</param>
        public void ConvertTo(DS4WindowsProfile profile)
        {
            var lightbarSettings = profile.LightbarSettingInfo;
            var lightInfo = lightbarSettings.Ds4WinSettings;

            profile.EnableTouchToggle = TouchToggle;
            profile.IdleDisconnectTimeout = IdleDisconnectTimeout;
            profile.EnableOutputDataToDS4 = OutputDataToDS4;
            lightbarSettings.Mode = LightbarMode;
            lightInfo.Led = Color;
            profile.RumbleBoost = RumbleBoost;
            profile.RumbleAutostopTime = RumbleAutostopTime;
            lightInfo.LedAsBattery = LedAsBatteryIndicator;
            lightInfo.FlashType = FlashType;
            lightInfo.FlashAt = FlashBatteryAt;
            profile.TouchSensitivity = TouchSensitivity;
            lightInfo.LowLed = LowColor;
            lightInfo.ChargingLed = ChargingColor;
            lightInfo.FlashLed = FlashColor;
            profile.TouchpadJitterCompensation = TouchpadJitterCompensation;
            profile.LowerRCOn = LowerRCOn;
            profile.TapSensitivity = TapSensitivity;
            profile.DoubleTap = DoubleTap;
            profile.ScrollSensitivity = ScrollSensitivity;
            profile.TouchPadInvert = Math.Min(Math.Max(TouchpadInvert, 0), 3);
            profile.TouchClickPassthru = TouchpadClickPassthru;
            profile.L2ModInfo.deadZone = LeftTriggerMiddle;
            profile.R2ModInfo.deadZone = RightTriggerMiddle;
            profile.L2ModInfo.AntiDeadZone = L2AntiDeadZone;
            profile.R2ModInfo.AntiDeadZone = R2AntiDeadZone;
            profile.L2ModInfo.maxZone = Math.Min(Math.Max(L2MaxZone, 0), 100);
            profile.R2ModInfo.maxZone = Math.Min(Math.Max(R2MaxZone, 0), 100);
            profile.L2ModInfo.maxOutput = Math.Min(Math.Max(L2MaxOutput, 0.0), 100.0);
            profile.R2ModInfo.maxOutput = Math.Min(Math.Max(R2MaxOutput, 0.0), 100.0);
            profile.LSRotation = Math.Min(Math.Max(LSRotation, -180), 180) * Math.PI / 180.0;
            profile.RSRotation = Math.Min(Math.Max(RSRotation, -180), 180) * Math.PI / 180.0;
            profile.LSModInfo.Fuzz = Math.Min(Math.Max(LSFuzz, 0), 100);
            profile.RSModInfo.Fuzz = Math.Min(Math.Max(RSFuzz, 0), 100);
            profile.ButtonMouseInfo.buttonSensitivity = ButtonMouseSensitivity;
            profile.ButtonMouseInfo.mouseVelocityOffset = ButtonMouseOffset;
            profile.ButtonMouseInfo.buttonVerticalScale =
                Math.Min(Math.Max(ButtonMouseVerticalScale, 0), 500) * 0.01;
            lightInfo.Rainbow = Rainbow;
            lightInfo.MaxRainbowSaturation = Math.Max(0, Math.Min(100, MaxSatRainbow)) / 100.0;
            profile.LSModInfo.DeadZone = Math.Min(Math.Max(LSDeadZone, 0), 127);
            profile.RSModInfo.DeadZone = Math.Min(Math.Max(RSDeadZone, 0), 127);
            profile.LSModInfo.AntiDeadZone = LSAntiDeadZone;
            profile.RSModInfo.AntiDeadZone = RSAntiDeadZone;
            profile.LSModInfo.MaxZone = Math.Min(Math.Max(LSMaxZone, 0), 100);
            profile.RSModInfo.MaxZone = Math.Min(Math.Max(RSMaxZone, 0), 100);
            profile.LSModInfo.VerticalScale = Math.Min(Math.Max(LSVerticalScale, 0.0), 200.0);
            profile.RSModInfo.VerticalScale = Math.Min(Math.Max(RSVerticalScale, 0.0), 200.0);
            profile.LSModInfo.MaxOutput = Math.Min(Math.Max(LSMaxOutput, 0.0), 100.0);
            profile.RSModInfo.MaxOutput = Math.Min(Math.Max(RSMaxOutput, 0.0), 100.0);
            profile.LSModInfo.MaxOutputForce = LSMaxOutputForce;
            profile.RSModInfo.MaxOutputForce = RSMaxOutputForce;
            profile.LSModInfo.OuterBindDeadZone = Math.Min(Math.Max(LSOuterBindDead, 0), 100);
            profile.RSModInfo.OuterBindDeadZone = Math.Min(Math.Max(RSOuterBindDead, 0), 100);
            profile.LSModInfo.OuterBindInvert = LSOuterBindInvert;
            profile.RSModInfo.OuterBindInvert = RSOuterBindInvert;
            profile.LSModInfo.DZType = LSDeadZoneType;
            profile.RSModInfo.DZType = RSDeadZoneType;
            profile.LSModInfo.XAxisDeadInfo.DeadZone = Math.Min(Math.Max(LSAxialDeadOptions.DeadZoneX, 0), 127);
            profile.LSModInfo.YAxisDeadInfo.DeadZone = Math.Min(Math.Max(LSAxialDeadOptions.DeadZoneY, 0), 127);
            profile.LSModInfo.XAxisDeadInfo.MaxZone = Math.Min(Math.Max(LSAxialDeadOptions.MaxZoneX, 0), 100);
            profile.LSModInfo.YAxisDeadInfo.MaxZone = Math.Min(Math.Max(LSAxialDeadOptions.MaxZoneY, 0), 100);
            profile.LSModInfo.XAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(LSAxialDeadOptions.AntiDeadZoneX, 0), 100);
            profile.LSModInfo.YAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(LSAxialDeadOptions.AntiDeadZoneY, 0), 100);
            profile.LSModInfo.XAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(LSAxialDeadOptions.MaxOutputX, 0.0), 100.0);
            profile.LSModInfo.YAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(LSAxialDeadOptions.MaxOutputY, 0.0), 100.0);
            profile.RSModInfo.XAxisDeadInfo.DeadZone = Math.Min(Math.Max(RSAxialDeadOptions.DeadZoneX, 0), 127);
            profile.RSModInfo.YAxisDeadInfo.DeadZone = Math.Min(Math.Max(RSAxialDeadOptions.DeadZoneY, 0), 127);
            profile.RSModInfo.XAxisDeadInfo.MaxZone = Math.Min(Math.Max(RSAxialDeadOptions.MaxZoneX, 0), 100);
            profile.RSModInfo.YAxisDeadInfo.MaxZone = Math.Min(Math.Max(RSAxialDeadOptions.MaxZoneY, 0), 100);
            profile.RSModInfo.XAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(RSAxialDeadOptions.AntiDeadZoneX, 0), 100);
            profile.RSModInfo.YAxisDeadInfo.AntiDeadZone =
                Math.Min(Math.Max(RSAxialDeadOptions.AntiDeadZoneY, 0), 100);
            profile.RSModInfo.XAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(RSAxialDeadOptions.MaxOutputX, 0.0), 100.0);
            profile.RSModInfo.YAxisDeadInfo.MaxOutput =
                Math.Min(Math.Max(RSAxialDeadOptions.MaxOutputY, 0.0), 100.0);
            profile.SXDeadzone = SXDeadZone;
            profile.SZDeadzone = SZDeadZone;
            profile.SXMaxzone = Math.Min(Math.Max(SXMaxZone * 0.01, 0.0), 1.0);
            profile.SZMaxzone = Math.Min(Math.Max(SZMaxZone * 0.01, 0.0), 1.0);
            profile.SXAntiDeadzone = Math.Min(Math.Max(SXAntiDeadZone * 0.01, 0.0), 1.0);
            profile.SZAntiDeadzone = Math.Min(Math.Max(SZAntiDeadZone * 0.01, 0.0), 1.0);

            profile.LSSens = Sensitivity.LSSens;
            profile.RSSens = Sensitivity.RSSens;
            profile.L2Sens = Sensitivity.L2Sens;
            profile.R2Sens = Sensitivity.R2Sens;
            profile.SXSens = Sensitivity.SXSens;
            profile.SZSens = Sensitivity.SZSens;

            lightInfo.ChargingType = ChargingType;
            profile.ButtonMouseInfo.mouseAccel = MouseAcceleration;
            //ShiftModifier
            profile.LaunchProgram = LaunchProgram;
            profile.DisableVirtualController = DisableVirtualController;
            profile.StartTouchpadOff = StartTouchpadOff;

            profile.SATriggers = SATriggers;
            //store.SATriggerCondition = store.SaTriggerCondValue(SATriggerCond);
            profile.SASteeringWheelEmulationAxis = SASteeringWheelEmulationAxis;
            profile.SASteeringWheelEmulationRange = SASteeringWheelEmulationRange;

            profile.WheelSmoothInfo.Enabled = SASteeringWheelSmoothingOptions.SASteeringWheelUseSmoothing;
            profile.WheelSmoothInfo.MinCutoff = SASteeringWheelSmoothingOptions.SASteeringWheelSmoothMinCutoff;
            profile.WheelSmoothInfo.Beta = SASteeringWheelSmoothingOptions.SASteeringWheelSmoothBeta;

            profile.SAWheelFuzzValues = SASteeringWheelFuzz is >= 0 and <= 100 ? SASteeringWheelFuzz : 0;

            profile.GyroOutputMode = GyroOutputMode;

            profile.GyroControlsInfo.Triggers = GyroControlsSettings.Triggers;
            //store.GyroControlsInfo.TriggerCond = store.SaTriggerCondValue(GyroControlsSettings.TriggerCond);
            profile.GyroControlsInfo.TriggerTurns = GyroControlsSettings.TriggerTurns;
            profile.GyroControlsInfo.TriggerToggle = GyroControlsSettings.Toggle;

            profile.SAMouseStickTriggers = GyroMouseStickTriggers;
            //store.SAMouseStickTriggerCond = store.SaTriggerCondValue(GyroMouseStickTriggerCond);
            profile.GyroMouseStickTriggerTurns = GyroMouseStickTriggerTurns;
            profile.GyroMouseStickHorizontalAxis = Math.Min(Math.Max(0, GyroMouseStickHAxis), 1);
            profile.GyroMouseStickInfo.DeadZone = GyroMouseStickDeadZone;
            profile.GyroMouseStickInfo.MaxZone = Math.Max(GyroMouseStickMaxZone, 1);
            profile.GyroMouseStickInfo.outputStick = GyroMouseStickOutputStick;
            profile.GyroMouseStickInfo.outputStickDir = GyroMouseStickOutputStickAxes;
            profile.GyroMouseStickInfo.AntiDeadX = GyroMouseStickAntiDeadX;
            profile.GyroMouseStickInfo.AntiDeadY = GyroMouseStickAntiDeadY;
            profile.GyroMouseStickInfo.Inverted = GyroMouseStickInvert;
            //store.SetGyroMouseStickToggle(device, GyroMouseStickToggle, control)
            profile.GyroMouseStickInfo.MaxOutput = Math.Min(Math.Max(GyroMouseStickMaxOutput, 0.0), 100.0);
            profile.GyroMouseStickInfo.MaxOutputEnabled = GyroMouseStickMaxOutputEnabled;
            profile.GyroMouseStickInfo.VertScale = GyroMouseStickVerticalScale;
            profile.GyroMouseStickInfo.UseSmoothing = GyroMouseStickSmoothingSettings.UseSmoothing;
            profile.GyroMouseStickInfo.DetermineSmoothMethod(GyroMouseStickSmoothingSettings.SmoothingMethod);
            profile.GyroMouseStickInfo.SmoothWeight = Math.Min(
                Math.Max(0.0, Convert.ToDouble(GyroMouseStickSmoothingSettings.SmoothingWeight * 0.01)), 1.0);
            profile.GyroMouseStickInfo.minCutoff =
                Math.Min(Math.Max(0.0, GyroMouseStickSmoothingSettings.SmoothingMinCutoff), 100.0);
            profile.GyroMouseStickInfo.beta =
                Math.Min(Math.Max(0.0, GyroMouseStickSmoothingSettings.SmoothingBeta), 1.0);

            profile.GyroSwipeInfo.deadzoneX = GyroSwipeSettings.DeadZoneX;
            profile.GyroSwipeInfo.deadzoneY = GyroSwipeSettings.DeadZoneY;
            profile.GyroSwipeInfo.triggers = GyroSwipeSettings.Triggers;
            //store.GyroSwipeInfo.triggerCond = store.SaTriggerCondValue(GyroSwipeSettings.TriggerCond);
            profile.GyroSwipeInfo.triggerTurns = GyroSwipeSettings.TriggerTurns;
            profile.GyroSwipeInfo.xAxis = GyroSwipeSettings.XAxis;
            profile.GyroSwipeInfo.delayTime = GyroSwipeSettings.DelayTime;

            profile.TouchOutMode = TouchpadOutputMode;
            profile.TouchDisInvertTriggers = TouchDisInvTriggers;
            profile.GyroSensitivity = GyroSensitivity;
            profile.GyroSensVerticalScale = GyroSensVerticalScale;
            profile.GyroInvert = GyroInvert;
            profile.GyroTriggerTurns = GyroTriggerTurns;

            profile.GyroMouseInfo.enableSmoothing = GyroMouseSmoothingSettings.UseSmoothing;
            profile.GyroMouseInfo.DetermineSmoothMethod(GyroMouseSmoothingSettings.SmoothingMethod);
            profile.GyroMouseInfo.smoothingWeight =
                Math.Min(Math.Max(0.0, Convert.ToDouble(GyroMouseSmoothingSettings.SmoothingWeight * 0.01)), 1.0);
            profile.GyroMouseInfo.minCutoff =
                Math.Min(Math.Max(0.0, GyroMouseSmoothingSettings.SmoothingMinCutoff), 100.0);
            profile.GyroMouseInfo.beta = Math.Min(Math.Max(0.0, GyroMouseSmoothingSettings.SmoothingBeta), 1.0);

            profile.GyroMouseHorizontalAxis = Math.Min(Math.Max(0, GyroMouseHAxis), 1);
            //store.SetGyroMouseDZ(device, temp, control);
            profile.GyroMouseInfo.minThreshold = Math.Min(Math.Max(GyroMouseMinThreshold, 1.0), 40.0);
            //SetGyroMouseToggle(device, temp, control);
            profile.BluetoothPollRate = BTPollRate is >= 0 and <= 16 ? BTPollRate : 4;

            //store.LSOutCurve = LSOutputCurveCustom;
            //store.SetLsOutCurveMode(device, store.StickOutputCurveId(LSOutputCurveMode));
            //store.RSOutCurve = RSOutputCurveCustom;
            //store.SetRsOutCurveMode(device, store.StickOutputCurveId(RSOutputCurveMode));

            profile.SquStickInfo.LSMode = LSSquareStick;
            profile.SquStickInfo.LSRoundness = SquareStickRoundness;
            profile.SquStickInfo.RSRoundness = SquareRStickRoundness;
            profile.SquStickInfo.RSMode = RSSquareStick;
            profile.LSAntiSnapbackInfo.Enabled = LSAntiSnapback;
            profile.RSAntiSnapbackInfo.Enabled = RSAntiSnapback;
            profile.LSAntiSnapbackInfo.Delta = LSAntiSnapbackDelta;
            profile.RSAntiSnapbackInfo.Delta = RSAntiSnapbackDelta;
            profile.LSAntiSnapbackInfo.Timeout = LSAntiSnapbackTimeout;
            profile.RSAntiSnapbackInfo.Timeout = RSAntiSnapbackTimeout;
            profile.LSOutputSettings.Mode = LSOutputMode;
            profile.RSOutputSettings.Mode = RSOutputMode;

            profile.LSOutputSettings.OutputSettings.flickSettings.realWorldCalibration =
                LSOutputSettings.FlickStickSettings.RealWorldCalibration;
            profile.LSOutputSettings.OutputSettings.flickSettings.flickThreshold =
                LSOutputSettings.FlickStickSettings.FlickThreshold;
            profile.LSOutputSettings.OutputSettings.flickSettings.flickTime =
                LSOutputSettings.FlickStickSettings.FlickTime;
            profile.LSOutputSettings.OutputSettings.flickSettings.minAngleThreshold =
                LSOutputSettings.FlickStickSettings.MinAngleThreshold;
            profile.RSOutputSettings.OutputSettings.flickSettings.realWorldCalibration =
                RSOutputSettings.FlickStickSettings.RealWorldCalibration;
            profile.RSOutputSettings.OutputSettings.flickSettings.flickThreshold =
                RSOutputSettings.FlickStickSettings.FlickThreshold;
            profile.RSOutputSettings.OutputSettings.flickSettings.flickTime =
                RSOutputSettings.FlickStickSettings.FlickTime;
            profile.RSOutputSettings.OutputSettings.flickSettings.minAngleThreshold =
                RSOutputSettings.FlickStickSettings.MinAngleThreshold;

            profile.L2OutCurve = L2OutputCurveCustom;
            //store.SetL2OutCurveMode(device, store.StickOutputCurveId(L2OutputCurveMode));
            profile.L2OutputSettings.TwoStageMode = L2TwoStageMode;
            profile.L2OutputSettings.hipFireMS = Math.Min(Math.Max(0, L2HipFireTime), 5000);
            profile.L2OutputSettings.TriggerEffect = L2TriggerEffect;

            profile.R2OutCurve = R2OutputCurveCustom;
            //store.SetR2OutCurveMode(device, store.StickOutputCurveId(R2OutputCurveMode));
            profile.R2OutputSettings.TwoStageMode = R2TwoStageMode;
            profile.R2OutputSettings.TriggerEffect = R2TriggerEffect;
            profile.R2OutputSettings.hipFireMS = Math.Min(Math.Max(0, R2HipFireTime), 5000);

            profile.SXOutCurve = SXOutputCurveCustom;
            //store.SetSXOutCurveMode(device, store.StickOutputCurveId(SXOutputCurveMode));
            profile.SZOutCurve = SZOutputCurveCustom;
            //store.SetSZOutCurveMode(device, store.StickOutputCurveId(SZOutputCurveMode));
            profile.TrackballMode = TrackballMode;
            profile.TrackballFriction = TrackballFriction;

            profile.TouchPadRelMouse.Rotation =
                Math.Min(Math.Max(TouchRelMouseRotation, -180), 180) * Math.PI / 180.0;
            profile.TouchPadRelMouse.MinThreshold = Math.Min(Math.Max(TouchRelMouseMinThreshold, 1.0), 40.0);

            profile.TouchPadAbsMouse.MaxZoneX = TouchpadAbsMouseSettings.MaxZoneX;
            profile.TouchPadAbsMouse.MaxZoneY = TouchpadAbsMouseSettings.MaxZoneY;
            profile.TouchPadAbsMouse.SnapToCenter = TouchpadAbsMouseSettings.SnapToCenter;

            profile.OutputDeviceType = OutputContDevice;
        }
    }
}