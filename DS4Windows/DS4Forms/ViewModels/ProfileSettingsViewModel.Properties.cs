using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4Windows.InputDevices;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public partial class ProfileSettingsViewModel
    {
        public int Device { get; }

        public int FuncDevNum { get; }

        public int LightbarModeIndex
        {
            get
            {
                var index = 0;
                switch (appSettings.Settings.LightbarSettingInfo[Device].Mode)
                {
                    case LightbarMode.DS4Win:
                        index = 0;
                        break;
                    case LightbarMode.Passthru:
                        index = 1;
                        break;
                }

                return index;
            }
            set
            {
                var temp = LightbarMode.DS4Win;
                switch (value)
                {
                    case 0:
                        temp = LightbarMode.DS4Win;
                        break;
                    case 1:
                        temp = LightbarMode.Passthru;
                        break;
                }

                appSettings.Settings.LightbarSettingInfo[Device].Mode = temp;
            }
        }

        public Brush LightbarBrush
        {
            get
            {
                Brush tempBrush;
                var color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led;
                if (!RainbowExists)
                {
                    lightbarColBrush.Color = new Color
                    {
                        A = 255,
                        R = color.Red,
                        G = color.Green,
                        B = color.Blue
                    };
                    tempBrush = lightbarColBrush;
                }
                else
                {
                    tempBrush = lightbarImgBrush;
                }

                return tempBrush;
            }
        }

        public Color MainColor => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.ToColor();

        public string MainColorString
        {
            get
            {
                var color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public int MainColorR
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Red;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Red = (byte)value;
            }
        }

        public string MainColorRString =>
            $"#{appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Red.ToString("X2")}FF0000";

        public int MainColorG
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Green;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Green = (byte)value;
            }
        }

        public string MainColorGString =>
            $"#{appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Green.ToString("X2")}00FF00";

        public int MainColorB
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Blue;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Blue = (byte)value;
            }
        }

        public string MainColorBString =>
            $"#{appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led.Blue.ToString("X2")}0000FF";

        public string LowColor
        {
            get
            {
                var color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public int LowColorR
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Red;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Red = (byte)value;
            }
        }

        public string LowColorRString =>
            $"#{appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Red.ToString("X2")}FF0000";

        public int LowColorG
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Green;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Green = (byte)value;
            }
        }

        public string LowColorGString =>
            $"#{appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Green.ToString("X2")}00FF00";

        public int LowColorB
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Blue;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Blue = (byte)value;
            }
        }

        public string LowColorBString =>
            $"#{appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.Blue.ToString("X2")}0000FF";

        public Color LowColorMedia => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LowLed.ToColor();

        public int FlashTypeIndex
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings
                .FlashType; //Global.Instance.FlashType[device];
            set => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashType = (byte)value;
        }

        public int FlashAt
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings
                .FlashAt; //Global.Instance.FlashAt[device];
            set => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashAt = value;
        }

        public string FlashColor
        {
            get
            {
                var color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashLed;

                if (color.Red == 0 && color.Green == 0 && color.Blue == 0)
                    color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashLed =
                        appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led;

                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public Color FlashColorMedia
        {
            get
            {
                var color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashLed;
                if (color.Red == 0 && color.Green == 0 && color.Blue == 0)
                    color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashLed =
                        appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Led;

                return color.ToColor();
            }
        }

        public int ChargingType
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingType;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingType = value;
            }
        }

        public bool ColorBatteryPercent
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LedAsBattery;
            set => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.LedAsBattery = value;
        }

        public string ChargingColor
        {
            get
            {
                var color = appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingLed;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public Color ChargingColorMedia =>
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingLed.ToColor();

        public Visibility ChargingColorVisible =>
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingType == 3
                ? Visibility.Visible
                : Visibility.Hidden;

        public double Rainbow
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Rainbow;
            set
            {
                appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Rainbow = value;
            }
        }

        public bool RainbowExists => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.Rainbow != 0.0;

        public double MaxSatRainbow
        {
            get => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.MaxRainbowSaturation * 100.0;
            set => appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.MaxRainbowSaturation = value / 100.0;
        }

        public int RumbleBoost
        {
            get => CurrentProfile.RumbleBoost;
            set => CurrentProfile.RumbleBoost = (byte)value;
        }

        public int RumbleAutostopTime
        {
            // RumbleAutostopTime value is in milliseconds in XML config file, but GUI uses just seconds
            get => CurrentProfile.RumbleAutostopTime / 1000;
            set => CurrentProfile.RumbleAutostopTime = value * 1000;
        }

        public bool HeavyRumbleActive
        {
            get => heavyRumbleActive;
            set
            {
                heavyRumbleActive = value;
            }
        }

        public bool LightRumbleActive
        {
            get => lightRumbleActive;
            set
            {
                lightRumbleActive = value;
            }
        }

        public bool UseControllerReadout
        {
            get => Global.Instance.Config.Ds4Mapping;
            set => Global.Instance.Config.Ds4Mapping = value;
        }

        public int ButtonMouseSensitivity
        {
            get => CurrentProfile.ButtonMouseInfo.ButtonSensitivity;
            set
            {
                var temp = CurrentProfile.ButtonMouseInfo.ButtonSensitivity;
                if (temp == value) return;
                CurrentProfile.ButtonMouseInfo.ButtonSensitivity = value;
            }
        }

        public int ButtonMouseVerticalScale
        {
            get => Convert.ToInt32(CurrentProfile.ButtonMouseInfo
                .ButtonVerticalScale * 100.0);
            set
            {
                var temp = CurrentProfile.ButtonMouseInfo
                    .ButtonVerticalScale;
                var attemptValue = value * 0.01;
                if (temp == attemptValue) return;
                CurrentProfile.ButtonMouseInfo.ButtonVerticalScale =
                    attemptValue;
            }
        }

        private double RawButtonMouseOffset => CurrentProfile.ButtonMouseInfo
            .MouseVelocityOffset;

        public double ButtonMouseOffset
        {
            get => CurrentProfile.ButtonMouseInfo.MouseVelocityOffset *
                   100.0;
            set
            {
                var temp =
                    CurrentProfile.ButtonMouseInfo.MouseVelocityOffset *
                    100.0;
                if (temp == value) return;
                CurrentProfile.ButtonMouseInfo.MouseVelocityOffset =
                    value * 0.01;
            }
        }

        public int OutputMouseSpeed
        {
            get => outputMouseSpeed;
            set
            {
                if (value == outputMouseSpeed) return;
                outputMouseSpeed = value;
            }
        }

        public double MouseOffsetSpeed
        {
            get => mouseOffsetSpeed;
            set
            {
                if (mouseOffsetSpeed == value) return;
                mouseOffsetSpeed = value;
            }
        }

        public bool MouseAcceleration
        {
            get => CurrentProfile.ButtonMouseInfo.MouseAcceleration;
            set => CurrentProfile.ButtonMouseInfo.MouseAcceleration = value;
        }

        public bool EnableTouchpadToggle
        {
            get => CurrentProfile.EnableTouchToggle;
            set => CurrentProfile.EnableTouchToggle = value;
        }

        public bool EnableOutputDataToDS4
        {
            get => CurrentProfile.EnableOutputDataToDS4;
            set => CurrentProfile.EnableOutputDataToDS4 = value;
        }

        public bool LaunchProgramExists
        {
            get => !string.IsNullOrEmpty(Global.Instance.Config.LaunchProgram[Device]);
            set
            {
                if (!value) ResetLauchProgram();
            }
        }

        public string LaunchProgram => Global.Instance.Config.LaunchProgram[Device];

        public string LaunchProgramName
        {
            get
            {
                var temp = Global.Instance.Config.LaunchProgram[Device];
                if (!string.IsNullOrEmpty(temp))
                    temp = Path.GetFileNameWithoutExtension(temp);
                else
                    temp = "Browse";

                return temp;
            }
        }

        public ImageSource LaunchProgramIcon
        {
            get
            {
                ImageSource exeicon = null;
                var path = Global.Instance.Config.LaunchProgram[Device];
                if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".exe")
                    using (var ico = Icon.ExtractAssociatedIcon(path))
                    {
                        exeicon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        exeicon.Freeze();
                    }

                return exeicon;
            }
        }

        public bool DInputOnly
        {
            get => CurrentProfile.DisableVirtualController;
            set => CurrentProfile.DisableVirtualController = value;
        }

        public bool IdleDisconnectExists
        {
            get => CurrentProfile.IdleDisconnectTimeout != 0;
            set
            {
                // If enabling Idle Disconnect, set default time.
                // Otherwise, set time to 0 to mean disabled
                CurrentProfile.IdleDisconnectTimeout =
                    value ? Global.DEFAULT_ENABLE_IDLE_DISCONN_MINS * 60 : 0;
            }
        }

        public int IdleDisconnect
        {
            get => CurrentProfile.IdleDisconnectTimeout / 60;
            set
            {
                var temp = CurrentProfile.IdleDisconnectTimeout / 60;
                if (temp == value) return;
                CurrentProfile.IdleDisconnectTimeout = value * 60;
            }
        }

        public int TempBTPollRateIndex { get; set; }

        public int ControllerTypeIndex
        {
            get
            {
                var type = 0;
                switch (CurrentProfile.OutputDeviceType)
                {
                    case OutContType.X360:
                        type = 0;
                        break;

                    case OutContType.DS4:
                        type = 1;
                        break;
                }

                return type;
            }
        }

        public int TempControllerIndex
        {
            get => tempControllerIndex;
            set
            {
                tempControllerIndex = value;
                Global.OutDevTypeTemp[Device] = TempConType;
            }
        }

        public OutContType TempConType
        {
            get
            {
                var result = OutContType.None;
                switch (tempControllerIndex)
                {
                    case 0:
                        result = OutContType.X360;
                        break;
                    case 1:
                        result = OutContType.DS4;
                        break;
                    default:
                        result = OutContType.X360;
                        break;
                }

                return result;
            }
        }

        public int GyroOutModeIndex
        {
            get
            {
                var index = 0;
                switch (CurrentProfile.GyroOutputMode)
                {
                    case GyroOutMode.Controls:
                        index = 0;
                        break;
                    case GyroOutMode.Mouse:
                        index = 1;
                        break;
                    case GyroOutMode.MouseJoystick:
                        index = 2;
                        break;
                    case GyroOutMode.DirectionalSwipe:
                        index = 3;
                        break;
                    case GyroOutMode.Passthru:
                        index = 4;
                        break;
                }

                return index;
            }
            set
            {
                var temp = GyroOutMode.Controls;
                switch (value)
                {
                    case 0: break;
                    case 1:
                        temp = GyroOutMode.Mouse;
                        break;
                    case 2:
                        temp = GyroOutMode.MouseJoystick;
                        break;
                    case 3:
                        temp = GyroOutMode.DirectionalSwipe;
                        break;
                    case 4:
                        temp = GyroOutMode.Passthru;
                        break;
                }

                var current = CurrentProfile.GyroOutputMode;
                if (temp == current) return;
                CurrentProfile.GyroOutputMode = temp;
            }
        }

        public OutContType ContType => CurrentProfile.OutputDeviceType;

        public int SASteeringWheelEmulationAxisIndex
        {
            get => (int)CurrentProfile.SASteeringWheelEmulationAxis;
            set
            {
                var temp = (int)CurrentProfile.SASteeringWheelEmulationAxis;
                if (temp == value) return;

                CurrentProfile.SASteeringWheelEmulationAxis =
                    (SASteeringWheelEmulationAxisType)value;
            }
        }

        public int SASteeringWheelEmulationRangeIndex
        {
            get
            {
                var index = 360;
                switch (CurrentProfile.SASteeringWheelEmulationRange)
                {
                    case 90:
                        index = 0;
                        break;
                    case 180:
                        index = 1;
                        break;
                    case 270:
                        index = 2;
                        break;
                    case 360:
                        index = 3;
                        break;
                    case 450:
                        index = 4;
                        break;
                    case 720:
                        index = 5;
                        break;
                    case 900:
                        index = 6;
                        break;
                    case 1080:
                        index = 7;
                        break;
                    case 1440:
                        index = 8;
                        break;
                }

                return index;
            }
            set
            {
                var temp = saSteeringRangeValues[value];
                CurrentProfile.SASteeringWheelEmulationRange = temp;
            }
        }

        public int SASteeringWheelEmulationRange
        {
            get => CurrentProfile.SASteeringWheelEmulationRange;
            set => CurrentProfile.SASteeringWheelEmulationRange = value;
        }

        public int SASteeringWheelFuzz
        {
            get => CurrentProfile.SAWheelFuzzValues;
            set => CurrentProfile.SAWheelFuzzValues = value;
        }

        public bool SASteeringWheelUseSmoothing
        {
            get => CurrentProfile.WheelSmoothInfo.Enabled;
            set
            {
                var temp = CurrentProfile.WheelSmoothInfo.Enabled;
                if (temp == value) return;
                CurrentProfile.WheelSmoothInfo.Enabled = value;
            }
        }

        public double SASteeringWheelSmoothMinCutoff
        {
            get => CurrentProfile.WheelSmoothInfo.MinCutoff;
            set => CurrentProfile.WheelSmoothInfo.MinCutoff = value;
        }

        public double SASteeringWheelSmoothBeta
        {
            get => CurrentProfile.WheelSmoothInfo.Beta;
            set => CurrentProfile.WheelSmoothInfo.Beta = value;
        }

        public double LSDeadZone
        {
            get => Math.Round(CurrentProfile.LSModInfo.DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(CurrentProfile.LSModInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                CurrentProfile.LSModInfo.DeadZone = (int)Math.Round(value * 127d);
            }
        }

        public double RSDeadZone
        {
            get => Math.Round(CurrentProfile.RSModInfo.DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(CurrentProfile.RSModInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                CurrentProfile.RSModInfo.DeadZone = (int)Math.Round(value * 127d);
            }
        }

        public double LSMaxZone
        {
            get => CurrentProfile.LSModInfo.MaxZone / 100.0;
            set => CurrentProfile.LSModInfo.MaxZone = (int)(value * 100.0);
        }

        public double RSMaxZone
        {
            get => CurrentProfile.RSModInfo.MaxZone / 100.0;
            set => CurrentProfile.RSModInfo.MaxZone = (int)(value * 100.0);
        }

        public double LSAntiDeadZone
        {
            get => CurrentProfile.LSModInfo.AntiDeadZone / 100.0;
            set => CurrentProfile.LSModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double RSAntiDeadZone
        {
            get => CurrentProfile.RSModInfo.AntiDeadZone / 100.0;
            set => CurrentProfile.RSModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double LSVerticalScale
        {
            get => CurrentProfile.LSModInfo.VerticalScale / 100.0;
            set => CurrentProfile.LSModInfo.VerticalScale = value * 100.0;
        }

        public double LSMaxOutput
        {
            get => CurrentProfile.LSModInfo.MaxOutput / 100.0;
            set => CurrentProfile.LSModInfo.MaxOutput = value * 100.0;
        }

        public bool LSMaxOutputForce
        {
            get => CurrentProfile.LSModInfo.MaxOutputForce;
            set => CurrentProfile.LSModInfo.MaxOutputForce = value;
        }

        public double RSVerticalScale
        {
            get => CurrentProfile.RSModInfo.VerticalScale / 100.0;
            set => CurrentProfile.RSModInfo.VerticalScale = value * 100.0;
        }

        public double RSMaxOutput
        {
            get => CurrentProfile.RSModInfo.MaxOutput / 100.0;
            set => CurrentProfile.RSModInfo.MaxOutput = value * 100.0;
        }

        public bool RSMaxOutputForce
        {
            get => CurrentProfile.RSModInfo.MaxOutputForce;
            set => CurrentProfile.RSModInfo.MaxOutputForce = value;
        }

        public int LSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (CurrentProfile.LSModInfo.DZType)
                {
                    case StickDeadZoneInfo.DeadZoneType.Radial:
                        break;
                    case StickDeadZoneInfo.DeadZoneType.Axial:
                        index = 1;
                        break;
                }

                return index;
            }
            set
            {
                var temp = StickDeadZoneInfo.DeadZoneType.Radial;
                switch (value)
                {
                    case 0: break;
                    case 1:
                        temp = StickDeadZoneInfo.DeadZoneType.Axial;
                        break;
                }

                var current = CurrentProfile.LSModInfo.DZType;
                if (temp == current) return;
                CurrentProfile.LSModInfo.DZType = temp;
            }
        }

        public int RSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (CurrentProfile.RSModInfo.DZType)
                {
                    case StickDeadZoneInfo.DeadZoneType.Radial:
                        break;
                    case StickDeadZoneInfo.DeadZoneType.Axial:
                        index = 1;
                        break;
                }

                return index;
            }
            set
            {
                var temp = StickDeadZoneInfo.DeadZoneType.Radial;
                switch (value)
                {
                    case 0: break;
                    case 1:
                        temp = StickDeadZoneInfo.DeadZoneType.Axial;
                        break;
                }

                var current = CurrentProfile.RSModInfo.DZType;
                if (temp == current) return;
                CurrentProfile.RSModInfo.DZType = temp;
            }
        }

        public double LSSens
        {
            get => CurrentProfile.LSSens;
            set => CurrentProfile.LSSens = value;
        }

        public double RSSens
        {
            get => CurrentProfile.RSSens;
            set => CurrentProfile.RSSens = value;
        }

        public bool LSSquareStick
        {
            get => CurrentProfile.SquStickInfo.LSMode;
            set => CurrentProfile.SquStickInfo.LSMode = value;
        }

        public bool RSSquareStick
        {
            get => CurrentProfile.SquStickInfo.RSMode;
            set => CurrentProfile.SquStickInfo.RSMode = value;
        }

        public double LSSquareRoundness
        {
            get => CurrentProfile.SquStickInfo.LSRoundness;
            set => CurrentProfile.SquStickInfo.LSRoundness = value;
        }

        public double RSSquareRoundness
        {
            get => CurrentProfile.SquStickInfo.RSRoundness;
            set => CurrentProfile.SquStickInfo.RSRoundness = value;
        }

        public int LSOutputCurveIndex
        {
            get => Global.Instance.Config.GetLsOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetLsOutCurveMode(Device, value);
            }
        }

        public int RSOutputCurveIndex
        {
            get => Global.Instance.Config.GetRsOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetRsOutCurveMode(Device, value);
            }
        }

        public double LSRotation
        {
            get => CurrentProfile.LSRotation * 180.0 / Math.PI;
            set => CurrentProfile.LSRotation = value * Math.PI / 180.0;
        }

        public double RSRotation
        {
            get => CurrentProfile.RSRotation * 180.0 / Math.PI;
            set => CurrentProfile.RSRotation = value * Math.PI / 180.0;
        }

        public bool LSCustomCurveSelected => Global.Instance.Config.GetLsOutCurveMode(Device) == 6;

        public bool RSCustomCurveSelected => Global.Instance.Config.GetRsOutCurveMode(Device) == 6;

        public string LSCustomCurve
        {
            get => Global.Instance.Config.LSOutCurve[Device].CustomDefinition;
            set => Global.Instance.Config.LSOutCurve[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.LSRS, true);
        }

        public string RSCustomCurve
        {
            get => Global.Instance.Config.RSOutCurve[Device].CustomDefinition;
            set => Global.Instance.Config.RSOutCurve[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.LSRS, true);
        }

        public int LSFuzz
        {
            get => CurrentProfile.LSModInfo.Fuzz;
            set => CurrentProfile.LSModInfo.Fuzz = value;
        }

        public int RSFuzz
        {
            get => CurrentProfile.RSModInfo.Fuzz;
            set => CurrentProfile.RSModInfo.Fuzz = value;
        }

        public bool LSAntiSnapback
        {
            get => CurrentProfile.LSAntiSnapbackInfo.Enabled;
            set => CurrentProfile.LSAntiSnapbackInfo.Enabled = value;
        }

        public bool RSAntiSnapback
        {
            get => CurrentProfile.RSAntiSnapbackInfo.Enabled;
            set => CurrentProfile.RSAntiSnapbackInfo.Enabled = value;
        }

        public double LSAntiSnapbackDelta
        {
            get => CurrentProfile.LSAntiSnapbackInfo.Delta;
            set => CurrentProfile.LSAntiSnapbackInfo.Delta = value;
        }

        public double RSAntiSnapbackDelta
        {
            get => CurrentProfile.RSAntiSnapbackInfo.Delta;
            set => CurrentProfile.RSAntiSnapbackInfo.Delta = value;
        }

        public int LSAntiSnapbackTimeout
        {
            get => CurrentProfile.LSAntiSnapbackInfo.Timeout;
            set => CurrentProfile.LSAntiSnapbackInfo.Timeout = value;
        }

        public int RSAntiSnapbackTimeout
        {
            get => CurrentProfile.RSAntiSnapbackInfo.Timeout;
            set => CurrentProfile.RSAntiSnapbackInfo.Timeout = value;
        }

        public bool LSOuterBindInvert
        {
            get => CurrentProfile.LSModInfo.OuterBindInvert;
            set => CurrentProfile.LSModInfo.OuterBindInvert = value;
        }

        public bool RSOuterBindInvert
        {
            get => CurrentProfile.RSModInfo.OuterBindInvert;
            set => CurrentProfile.RSModInfo.OuterBindInvert = value;
        }

        public double LSOuterBindDead
        {
            get => CurrentProfile.LSModInfo.OuterBindDeadZone / 100.0;
            set => CurrentProfile.LSModInfo.OuterBindDeadZone = value * 100.0;
        }

        public double RSOuterBindDead
        {
            get => CurrentProfile.RSModInfo.OuterBindDeadZone / 100.0;
            set => CurrentProfile.RSModInfo.OuterBindDeadZone = value * 100.0;
        }

        public int LSOutputIndex
        {
            get
            {
                var index = 0;
                switch (CurrentProfile.LSOutputSettings.Mode)
                {
                    case StickMode.None:
                        index = 0;
                        break;
                    case StickMode.Controls:
                        index = 1;
                        break;
                    case StickMode.FlickStick:
                        index = 2;
                        break;
                }

                return index;
            }
            set
            {
                var temp = StickMode.None;
                switch (value)
                {
                    case 0:
                        temp = StickMode.None;
                        break;
                    case 1:
                        temp = StickMode.Controls;
                        break;
                    case 2:
                        temp = StickMode.FlickStick;
                        break;
                }

                var current = CurrentProfile.LSOutputSettings.Mode;
                if (temp == current) return;
                CurrentProfile.LSOutputSettings.Mode = temp;
            }
        }

        public double LSFlickRWC
        {
            get => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration;
            set => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration =
                value;
        }

        public double LSFlickThreshold
        {
            get => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold;
            set => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold = value;
        }

        public double LSFlickTime
        {
            get => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickTime;
            set => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickTime = value;
        }

        public double LSMinAngleThreshold
        {
            get => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold;
            set => CurrentProfile.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold =
                value;
        }

        public int RSOutputIndex
        {
            get
            {
                var index = 0;
                switch (CurrentProfile.RSOutputSettings.Mode)
                {
                    case StickMode.None:
                        break;
                    case StickMode.Controls:
                        index = 1;
                        break;
                    case StickMode.FlickStick:
                        index = 2;
                        break;
                }

                return index;
            }
            set
            {
                var temp = StickMode.None;
                switch (value)
                {
                    case 0:
                        temp = StickMode.None;
                        break;
                    case 1:
                        temp = StickMode.Controls;
                        break;
                    case 2:
                        temp = StickMode.FlickStick;
                        break;
                }

                var current = CurrentProfile.RSOutputSettings.Mode;
                if (temp == current) return;
                CurrentProfile.RSOutputSettings.Mode = temp;
            }
        }

        public double RSFlickRWC
        {
            get => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration;
            set => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration =
                value;
        }

        public double RSFlickThreshold
        {
            get => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold;
            set => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold = value;
        }

        public double RSFlickTime
        {
            get => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickTime;
            set => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickTime = value;
        }

        public double RSMinAngleThreshold
        {
            get => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold;
            set => CurrentProfile.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold =
                value;
        }

        public double L2DeadZone
        {
            get => CurrentProfile.L2ModInfo.DeadZone / 255.0;
            set
            {
                var temp = CurrentProfile.L2ModInfo.DeadZone / 255.0;
                if (temp == value) return;
                CurrentProfile.L2ModInfo.DeadZone = (byte)(value * 255.0);
            }
        }

        public double R2DeadZone
        {
            get => CurrentProfile.R2ModInfo.DeadZone / 255.0;
            set
            {
                var temp = CurrentProfile.R2ModInfo.DeadZone / 255.0;
                if (temp == value) return;
                CurrentProfile.R2ModInfo.DeadZone = (byte)(value * 255.0);
            }
        }

        public double L2MaxZone
        {
            get => CurrentProfile.L2ModInfo.MaxZone / 100.0;
            set => CurrentProfile.L2ModInfo.MaxZone = (int)(value * 100.0);
        }

        public double R2MaxZone
        {
            get => CurrentProfile.R2ModInfo.MaxZone / 100.0;
            set => CurrentProfile.R2ModInfo.MaxZone = (int)(value * 100.0);
        }

        public double L2AntiDeadZone
        {
            get => CurrentProfile.L2ModInfo.AntiDeadZone / 100.0;
            set => CurrentProfile.L2ModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double R2AntiDeadZone
        {
            get => CurrentProfile.R2ModInfo.AntiDeadZone / 100.0;
            set => CurrentProfile.R2ModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double L2MaxOutput
        {
            get => CurrentProfile.L2ModInfo.MaxOutput / 100.0;
            set => CurrentProfile.L2ModInfo.MaxOutput = value * 100.0;
        }

        public double R2MaxOutput
        {
            get => CurrentProfile.R2ModInfo.MaxOutput / 100.0;
            set => CurrentProfile.R2ModInfo.MaxOutput = value * 100.0;
        }

        public double L2Sens
        {
            get => CurrentProfile.L2Sens;
            set => CurrentProfile.L2Sens = value;
        }

        public double R2Sens
        {
            get => CurrentProfile.R2Sens;
            set => CurrentProfile.R2Sens = value;
        }

        public int L2OutputCurveIndex
        {
            get => Global.Instance.Config.GetL2OutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetL2OutCurveMode(Device, value);
            }
        }

        public int R2OutputCurveIndex
        {
            get => Global.Instance.Config.GetR2OutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetR2OutCurveMode(Device, value);
            }
        }

        public bool L2CustomCurveSelected => Global.Instance.Config.GetL2OutCurveMode(Device) == 6;

        public bool R2CustomCurveSelected => Global.Instance.Config.GetR2OutCurveMode(Device) == 6;

        public string L2CustomCurve
        {
            get => Global.Instance.Config.L2OutCurve[Device].CustomDefinition;
            set => Global.Instance.Config.L2OutCurve[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.L2R2, true);
        }

        public string R2CustomCurve
        {
            get => Global.Instance.Config.R2OutCurve[Device].CustomDefinition;
            set => Global.Instance.Config.R2OutCurve[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.L2R2, true);
        }

        public List<TwoStageChoice> TwoStageModeChoices { get; } = new()
        {
            new TwoStageChoice("Disabled", TwoStageTriggerMode.Disabled),
            new TwoStageChoice("Normal", TwoStageTriggerMode.Normal),
            new TwoStageChoice("Exclusive", TwoStageTriggerMode.ExclusiveButtons),
            new TwoStageChoice("Hair Trigger", TwoStageTriggerMode.HairTrigger),
            new TwoStageChoice("Hip Fire", TwoStageTriggerMode.HipFire),
            new TwoStageChoice("Hip Fire Exclusive", TwoStageTriggerMode.HipFireExclusiveButtons)
        };

        public TwoStageTriggerMode L2TriggerMode
        {
            get => CurrentProfile.L2OutputSettings.TwoStageMode;
            set
            {
                var temp = CurrentProfile.L2OutputSettings.TwoStageMode;
                if (temp == value) return;

                CurrentProfile.L2OutputSettings.TwoStageMode = value;
            }
        }

        public TwoStageTriggerMode R2TriggerMode
        {
            get => CurrentProfile.R2OutputSettings.TwoStageMode;
            set
            {
                var temp = CurrentProfile.R2OutputSettings.TwoStageMode;
                if (temp == value) return;

                CurrentProfile.R2OutputSettings.TwoStageMode = value;
            }
        }

        public int L2HipFireTime
        {
            get => CurrentProfile.L2OutputSettings.HipFireMs;
            set => CurrentProfile.L2OutputSettings.HipFireMs = value;
        }

        public int R2HipFireTime
        {
            get => CurrentProfile.R2OutputSettings.HipFireMs;
            set => CurrentProfile.R2OutputSettings.HipFireMs = value;
        }

        public List<TriggerEffectChoice> TriggerEffectChoices { get; } = new()
        {
            new TriggerEffectChoice("None", TriggerEffects.None),
            new TriggerEffectChoice("Full Click", TriggerEffects.FullClick),
            new TriggerEffectChoice("Rigid", TriggerEffects.Rigid),
            new TriggerEffectChoice("Pulse", TriggerEffects.Pulse)
        };

        public TriggerEffects L2TriggerEffect
        {
            get => CurrentProfile.L2OutputSettings.TriggerEffect;
            set
            {
                var temp = CurrentProfile.L2OutputSettings.TriggerEffect;
                if (temp == value) return;

                CurrentProfile.L2OutputSettings.TriggerEffect = value;
            }
        }

        public TriggerEffects R2TriggerEffect
        {
            get => CurrentProfile.R2OutputSettings.TriggerEffect;
            set
            {
                var temp = CurrentProfile.R2OutputSettings.TriggerEffect;
                if (temp == value) return;

                CurrentProfile.R2OutputSettings.TriggerEffect = value;
            }
        }

        public double SXDeadZone
        {
            get => CurrentProfile.SXDeadZone;
            set
            {
                var temp = CurrentProfile.SXDeadZone;
                if (temp == value) return;
                CurrentProfile.SXDeadZone = value;
            }
        }

        public double SZDeadZone
        {
            get => CurrentProfile.SZDeadZone;
            set
            {
                var temp = CurrentProfile.SZDeadZone;
                if (temp == value) return;
                CurrentProfile.SZDeadZone = value;
            }
        }

        public double SXMaxZone
        {
            get => CurrentProfile.SXMaxZone;
            set => CurrentProfile.SXMaxZone = value;
        }

        public double SZMaxZone
        {
            get => CurrentProfile.SZMaxZone;
            set => CurrentProfile.SZMaxZone = value;
        }

        public double SXAntiDeadZone
        {
            get => CurrentProfile.SXAntiDeadZone;
            set => CurrentProfile.SXAntiDeadZone = value;
        }

        public double SZAntiDeadZone
        {
            get => CurrentProfile.SZAntiDeadZone;
            set => CurrentProfile.SZAntiDeadZone = value;
        }

        public double SXSens
        {
            get => CurrentProfile.SXSens;
            set => CurrentProfile.SXSens = value;
        }

        public double SZSens
        {
            get => CurrentProfile.SZSens;
            set => CurrentProfile.SZSens = value;
        }

        public int SXOutputCurveIndex
        {
            get => Global.Instance.Config.GetSXOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetSXOutCurveMode(Device, value);
            }
        }

        public int SZOutputCurveIndex
        {
            get => Global.Instance.Config.GetSZOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetSZOutCurveMode(Device, value);
            }
        }

        public bool SXCustomCurveSelected => Global.Instance.Config.GetSXOutCurveMode(Device) == 6;

        public bool SZCustomCurveSelected => Global.Instance.Config.GetSZOutCurveMode(Device) == 6;

        public string SXCustomCurve
        {
            get => Global.Instance.Config.SXOutCurve[Device].CustomDefinition;
            set => Global.Instance.Config.SXOutCurve[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.SA, true);
        }

        public string SZCustomCurve
        {
            get => Global.Instance.Config.SZOutCurve[Device].CustomDefinition;
            set => Global.Instance.Config.SZOutCurve[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.SA, true);
        }

        public int TouchpadOutputIndex
        {
            get
            {
                var index = 0;
                switch (CurrentProfile.TouchOutMode)
                {
                    case TouchpadOutMode.Mouse:
                        index = 0;
                        break;
                    case TouchpadOutMode.Controls:
                        index = 1;
                        break;
                    case TouchpadOutMode.AbsoluteMouse:
                        index = 2;
                        break;
                    case TouchpadOutMode.Passthru:
                        index = 3;
                        break;
                }

                return index;
            }
            set
            {
                var temp = TouchpadOutMode.Mouse;
                switch (value)
                {
                    case 0: break;
                    case 1:
                        temp = TouchpadOutMode.Controls;
                        break;
                    case 2:
                        temp = TouchpadOutMode.AbsoluteMouse;
                        break;
                    case 3:
                        temp = TouchpadOutMode.Passthru;
                        break;
                }

                var current = CurrentProfile.TouchOutMode;
                if (temp == current) return;
                CurrentProfile.TouchOutMode = temp;
            }
        }

        public bool TouchSenExists
        {
            get => CurrentProfile.TouchSensitivity != 0;
            set
            {
                CurrentProfile.TouchSensitivity =
                    value ? (byte)100 : (byte)0;
            }
        }

        public int TouchSens
        {
            get => CurrentProfile.TouchSensitivity;
            set
            {
                int temp = CurrentProfile.TouchSensitivity;
                if (temp == value) return;
                CurrentProfile.TouchSensitivity = (byte)value;
            }
        }

        public bool TouchScrollExists
        {
            get => CurrentProfile.ScrollSensitivity != 0;
            set
            {
                CurrentProfile.ScrollSensitivity = value ? 100 : 0;
            }
        }

        public int TouchScroll
        {
            get => CurrentProfile.ScrollSensitivity;
            set
            {
                var temp = CurrentProfile.ScrollSensitivity;
                if (temp == value) return;
                CurrentProfile.ScrollSensitivity = value;
            }
        }

        public bool TouchTapExists
        {
            get => CurrentProfile.TapSensitivity != 0;
            set
            {
                CurrentProfile.TapSensitivity = value ? (byte)100 : (byte)0;
            }
        }

        public int TouchTap
        {
            get => CurrentProfile.TapSensitivity;
            set
            {
                int temp = CurrentProfile.TapSensitivity;
                if (temp == value) return;
                CurrentProfile.TapSensitivity = (byte)value;
            }
        }

        public bool TouchDoubleTap
        {
            get => CurrentProfile.DoubleTap;
            set => CurrentProfile.DoubleTap = value;
        }

        public bool TouchJitter
        {
            get => CurrentProfile.TouchpadJitterCompensation;
            set => CurrentProfile.TouchpadJitterCompensation = value;
        }

        public int TouchInvertIndex
        {
            get
            {
                var invert = CurrentProfile.TouchPadInvert;
                var index = Array.IndexOf(touchpadInvertToValue, invert);
                return index;
            }
            set
            {
                var invert = touchpadInvertToValue[value];
                CurrentProfile.TouchPadInvert = invert;
            }
        }

        public bool LowerRightTouchRMB
        {
            get => CurrentProfile.LowerRCOn;
            set => CurrentProfile.LowerRCOn = value;
        }

        public bool TouchpadClickPassthru
        {
            get => CurrentProfile.TouchClickPassthru;
            set => CurrentProfile.TouchClickPassthru = value;
        }

        public bool StartTouchpadOff
        {
            get => CurrentProfile.StartTouchpadOff;
            set => CurrentProfile.StartTouchpadOff = value;
        }

        public double TouchRelMouseRotation
        {
            get => CurrentProfile.TouchPadRelMouse.Rotation * 180.0 / Math.PI;
            set => CurrentProfile.TouchPadRelMouse.Rotation = value * Math.PI / 180.0;
        }

        public double TouchRelMouseMinThreshold
        {
            get => CurrentProfile.TouchPadRelMouse.MinThreshold;
            set
            {
                var temp = CurrentProfile.TouchPadRelMouse.MinThreshold;
                if (temp == value) return;
                CurrentProfile.TouchPadRelMouse.MinThreshold = value;
            }
        }

        public bool TouchTrackball
        {
            get => CurrentProfile.TrackballMode;
            set => CurrentProfile.TrackballMode = value;
        }

        public double TouchTrackballFriction
        {
            get => CurrentProfile.TrackballFriction;
            set => CurrentProfile.TrackballFriction = value;
        }

        public int TouchAbsMouseMaxZoneX
        {
            get => CurrentProfile.TouchPadAbsMouse.MaxZoneX;
            set
            {
                var temp = CurrentProfile.TouchPadAbsMouse.MaxZoneX;
                if (temp == value) return;
                CurrentProfile.TouchPadAbsMouse.MaxZoneX = value;
            }
        }

        public int TouchAbsMouseMaxZoneY
        {
            get => CurrentProfile.TouchPadAbsMouse.MaxZoneY;
            set
            {
                var temp = CurrentProfile.TouchPadAbsMouse.MaxZoneY;
                if (temp == value) return;
                CurrentProfile.TouchPadAbsMouse.MaxZoneY = value;
            }
        }

        public bool TouchAbsMouseSnapCenter
        {
            get => CurrentProfile.TouchPadAbsMouse.SnapToCenter;
            set
            {
                var temp = CurrentProfile.TouchPadAbsMouse.SnapToCenter;
                if (temp == value) return;
                CurrentProfile.TouchPadAbsMouse.SnapToCenter = value;
            }
        }

        public bool GyroMouseTurns
        {
            get => CurrentProfile.GyroTriggerTurns;
            set => CurrentProfile.GyroTriggerTurns = value;
        }

        public int GyroSensitivity
        {
            get => CurrentProfile.GyroSensitivity;
            set => CurrentProfile.GyroSensitivity = value;
        }

        public int GyroVertScale
        {
            get => CurrentProfile.GyroSensVerticalScale;
            set => CurrentProfile.GyroSensVerticalScale = value;
        }

        public int GyroMouseEvalCondIndex
        {
            get => Global.Instance.Config.GetSATriggerCondition(Device) ? 0 : 1;
            set => Global.Instance.Config.SetSaTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseXAxis
        {
            get => CurrentProfile.GyroMouseHorizontalAxis;
            set => CurrentProfile.GyroMouseHorizontalAxis = value;
        }

        public double GyroMouseMinThreshold
        {
            get => CurrentProfile.GyroMouseInfo.MinThreshold;
            set
            {
                var temp = CurrentProfile.GyroMouseInfo.MinThreshold;
                if (temp == value) return;
                CurrentProfile.GyroMouseInfo.MinThreshold = value;
            }
        }

        public bool GyroMouseInvertX
        {
            get => (CurrentProfile.GyroInvert & 2) == 2;
            set
            {
                if (value)
                    CurrentProfile.GyroInvert |= 2;
                else
                    CurrentProfile.GyroInvert &= ~2;
            }
        }

        public bool GyroMouseInvertY
        {
            get => (CurrentProfile.GyroInvert & 1) == 1;
            set
            {
                if (value)
                    CurrentProfile.GyroInvert |= 1;
                else
                    CurrentProfile.GyroInvert &= ~1;
            }
        }

        public bool GyroMouseSmooth
        {
            get => CurrentProfile.GyroMouseInfo.EnableSmoothing;
            set
            {
                var tempInfo = CurrentProfile.GyroMouseInfo;
                if (tempInfo.EnableSmoothing == value) return;

                CurrentProfile.GyroMouseInfo.EnableSmoothing = value;
            }
        }

        public int GyroMouseSmoothMethodIndex
        {
            get => gyroMouseSmoothMethodIndex;
            set
            {
                if (gyroMouseSmoothMethodIndex == value) return;

                var tempInfo = CurrentProfile.GyroMouseInfo;
                switch (value)
                {
                    case 0:
                        tempInfo.ResetSmoothingMethods();
                        tempInfo.Smoothing = GyroMouseInfo.SmoothingMethod.OneEuro;
                        break;
                    case 1:
                        tempInfo.ResetSmoothingMethods();
                        tempInfo.Smoothing = GyroMouseInfo.SmoothingMethod.WeightedAverage;
                        break;
                }

                gyroMouseSmoothMethodIndex = value;
            }
        }

        public Visibility GyroMouseWeightAvgPanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                switch (CurrentProfile.GyroMouseInfo.Smoothing)
                {
                    case GyroMouseInfo.SmoothingMethod.WeightedAverage:
                        result = Visibility.Visible;
                        break;
                }

                return result;
            }
        }

        public Visibility GyroMouseOneEuroPanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                switch (CurrentProfile.GyroMouseInfo.Smoothing)
                {
                    case GyroMouseInfo.SmoothingMethod.OneEuro:
                    case GyroMouseInfo.SmoothingMethod.None:
                        result = Visibility.Visible;
                        break;
                }

                return result;
            }
        }

        public double GyroMouseSmoothWeight
        {
            get => CurrentProfile.GyroMouseInfo.SmoothingWeight;
            set => CurrentProfile.GyroMouseInfo.SmoothingWeight = value;
        }

        public double GyroMouseOneEuroMinCutoff
        {
            get => CurrentProfile.GyroMouseInfo.MinCutoff;
            set => CurrentProfile.GyroMouseInfo.MinCutoff = value;
        }

        public double GyroMouseOneEuroBeta
        {
            get => CurrentProfile.GyroMouseInfo.Beta;
            set => CurrentProfile.GyroMouseInfo.Beta = value;
        }

        public int GyroMouseStickSmoothMethodIndex
        {
            get => gyroMouseStickSmoothMethodIndex;
            set
            {
                if (gyroMouseStickSmoothMethodIndex == value) return;

                var tempInfo = CurrentProfile.GyroMouseStickInfo;
                switch (value)
                {
                    case 0:
                        tempInfo.ResetSmoothingMethods();
                        tempInfo.Smoothing = GyroMouseStickInfo.SmoothingMethod.OneEuro;
                        break;
                    case 1:
                        tempInfo.ResetSmoothingMethods();
                        tempInfo.Smoothing = GyroMouseStickInfo.SmoothingMethod.WeightedAverage;
                        break;
                }

                gyroMouseStickSmoothMethodIndex = value;
            }
        }

        public Visibility GyroMouseStickWeightAvgPanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                switch (CurrentProfile.GyroMouseStickInfo.Smoothing)
                {
                    case GyroMouseStickInfo.SmoothingMethod.WeightedAverage:
                        result = Visibility.Visible;
                        break;
                }

                return result;
            }
        }

        public Visibility GyroMouseStickOneEuroPanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                switch (CurrentProfile.GyroMouseStickInfo.Smoothing)
                {
                    case GyroMouseStickInfo.SmoothingMethod.OneEuro:
                    case GyroMouseStickInfo.SmoothingMethod.None:
                        result = Visibility.Visible;
                        break;
                }

                return result;
            }
        }

        public double GyroMouseStickSmoothWeight
        {
            get => CurrentProfile.GyroMouseStickInfo.SmoothWeight;
            set => CurrentProfile.GyroMouseStickInfo.SmoothWeight = value;
        }

        public double GyroMouseStickOneEuroMinCutoff
        {
            get => CurrentProfile.GyroMouseStickInfo.MinCutoff;
            set => CurrentProfile.GyroMouseStickInfo.MinCutoff = value;
        }

        public double GyroMouseStickOneEuroBeta
        {
            get => CurrentProfile.GyroMouseStickInfo.Beta;
            set => CurrentProfile.GyroMouseStickInfo.Beta = value;
        }


        public int GyroMouseDeadZone
        {
            get => Global.Instance.Config.GyroMouseDeadZone[Device];
            set => Global.Instance.Config.SetGyroMouseDZ(Device, value, rootHub);
        }

        public bool GyroMouseToggle
        {
            get => CurrentProfile.GyroMouseToggle;
            set => Global.Instance.Config.SetGyroMouseToggle(Device, value, rootHub);
        }

        public bool GyroMouseStickTurns
        {
            get => CurrentProfile.GyroMouseStickTriggerTurns;
            set => CurrentProfile.GyroMouseStickTriggerTurns = value;
        }

        public bool GyroMouseStickToggle
        {
            get => CurrentProfile.GyroMouseStickToggle;
            set => Global.Instance.Config.SetGyroMouseStickToggle(Device, value, rootHub);
        }

        public int GyroMouseStickDeadZone
        {
            get => CurrentProfile.GyroMouseStickInfo.DeadZone;
            set => CurrentProfile.GyroMouseStickInfo.DeadZone = value;
        }

        public int GyroMouseStickMaxZone
        {
            get => CurrentProfile.GyroMouseStickInfo.MaxZone;
            set => CurrentProfile.GyroMouseStickInfo.MaxZone = value;
        }

        public int GyroMouseStickOutputStick
        {
            get => (int)CurrentProfile.GyroMouseStickInfo.OutStick;
            set =>
                CurrentProfile.GyroMouseStickInfo.OutStick =
                    (GyroMouseStickInfo.OutputStick)value;
        }

        public int GyroMouseStickOutputAxes
        {
            get => (int)CurrentProfile.GyroMouseStickInfo.OutputStickDir;
            set =>
                CurrentProfile.GyroMouseStickInfo.OutputStickDir =
                    (GyroMouseStickInfo.OutputStickAxes)value;
        }

        public double GyroMouseStickAntiDeadX
        {
            get => CurrentProfile.GyroMouseStickInfo.AntiDeadX * 100.0;
            set => CurrentProfile.GyroMouseStickInfo.AntiDeadX = value * 0.01;
        }

        public double GyroMouseStickAntiDeadY
        {
            get => CurrentProfile.GyroMouseStickInfo.AntiDeadY * 100.0;
            set => CurrentProfile.GyroMouseStickInfo.AntiDeadY = value * 0.01;
        }

        public int GyroMouseStickVertScale
        {
            get => CurrentProfile.GyroMouseStickInfo.VerticalScale;
            set => CurrentProfile.GyroMouseStickInfo.VerticalScale = value;
        }

        public bool GyroMouseStickMaxOutputEnabled
        {
            get => CurrentProfile.GyroMouseStickInfo.MaxOutputEnabled;
            set
            {
                var temp = CurrentProfile.GyroMouseStickInfo.MaxOutputEnabled;
                if (temp == value) return;
                CurrentProfile.GyroMouseStickInfo.MaxOutputEnabled = value;
            }
        }

        public double GyroMouseStickMaxOutput
        {
            get => CurrentProfile.GyroMouseStickInfo.MaxOutput;
            set => CurrentProfile.GyroMouseStickInfo.MaxOutput = value;
        }

        public int GyroMouseStickEvalCondIndex
        {
            get => CurrentProfile.SAMouseStickTriggerCond ? 0 : 1;
            set => Global.Instance.Config.SetSaMouseStickTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseStickXAxis
        {
            get => CurrentProfile.GyroMouseStickHorizontalAxis;
            set => CurrentProfile.GyroMouseStickHorizontalAxis = value;
        }

        public bool GyroMouseStickInvertX
        {
            get => (CurrentProfile.GyroMouseStickInfo.Inverted & 1) == 1;
            set
            {
                if (value)
                {
                    CurrentProfile.GyroMouseStickInfo.Inverted |= 1;
                }
                else
                {
                    var temp = CurrentProfile.GyroMouseStickInfo.Inverted;
                    CurrentProfile.GyroMouseStickInfo.Inverted = (uint)(temp & ~1);
                }
            }
        }

        public bool GyroMouseStickInvertY
        {
            get => (CurrentProfile.GyroMouseStickInfo.Inverted & 2) == 2;
            set
            {
                if (value)
                {
                    CurrentProfile.GyroMouseStickInfo.Inverted |= 2;
                }
                else
                {
                    var temp = CurrentProfile.GyroMouseStickInfo.Inverted;
                    CurrentProfile.GyroMouseStickInfo.Inverted = (uint)(temp & ~2);
                }
            }
        }

        public bool GyroMouseStickSmooth
        {
            get => CurrentProfile.GyroMouseStickInfo.UseSmoothing;
            set => CurrentProfile.GyroMouseStickInfo.UseSmoothing = value;
        }

        public double GyroMousetickSmoothWeight
        {
            get => CurrentProfile.GyroMouseStickInfo.SmoothWeight;
            set => CurrentProfile.GyroMouseStickInfo.SmoothWeight = value;
        }

        public string TouchDisInvertString
        {
            get => touchDisInvertString;
            set
            {
                touchDisInvertString = value;
            }
        }

        public string GyroControlsTrigDisplay
        {
            get => gyroControlsTrigDisplay;
            set
            {
                gyroControlsTrigDisplay = value;
            }
        }

        public bool GyroControlsTurns
        {
            get => CurrentProfile.GyroControlsInfo.TriggerTurns;
            set => CurrentProfile.GyroControlsInfo.TriggerTurns = value;
        }

        public int GyroControlsEvalCondIndex
        {
            get => CurrentProfile.GyroControlsInfo.TriggerCond ? 0 : 1;
            set => CurrentProfile.GyroControlsInfo.TriggerCond =
                value == 0 ? true : false;
        }

        public bool GyroControlsToggle
        {
            get => CurrentProfile.GyroControlsInfo.TriggerToggle;
            set => Global.Instance.Config.SetGyroControlsToggle(Device, value, rootHub);
        }

        public string GyroMouseTrigDisplay
        {
            get => gyroMouseTrigDisplay;
            set
            {
                gyroMouseTrigDisplay = value;
            }
        }

        public string GyroMouseStickTrigDisplay
        {
            get => gyroMouseStickTrigDisplay;
            set
            {
                gyroMouseStickTrigDisplay = value;
            }
        }

        public string GyroSwipeTrigDisplay
        {
            get => gyroSwipeTrigDisplay;
            set
            {
                gyroSwipeTrigDisplay = value;
            }
        }

        public bool GyroSwipeTurns
        {
            get => CurrentProfile.GyroSwipeInfo.TriggerTurns;
            set => CurrentProfile.GyroSwipeInfo.TriggerTurns = value;
        }

        public int GyroSwipeEvalCondIndex
        {
            get => CurrentProfile.GyroSwipeInfo.TriggerCondition ? 0 : 1;
            set => CurrentProfile.GyroSwipeInfo.TriggerCondition = value == 0 ? true : false;
        }

        public int GyroSwipeXAxis
        {
            get => (int)CurrentProfile.GyroSwipeInfo.XAxis;
            set => CurrentProfile.GyroSwipeInfo.XAxis = (GyroDirectionalSwipeInfo.XAxisSwipe)value;
        }

        public int GyroSwipeDeadZoneX
        {
            get => CurrentProfile.GyroSwipeInfo.DeadZoneX;
            set => CurrentProfile.GyroSwipeInfo.DeadZoneX = value;
        }

        public int GyroSwipeDeadZoneY
        {
            get => CurrentProfile.GyroSwipeInfo.DeadZoneY;
            set => CurrentProfile.GyroSwipeInfo.DeadZoneY = value;
        }

        public int GyroSwipeDelayTime
        {
            get => CurrentProfile.GyroSwipeInfo.DelayTime;
            set => CurrentProfile.GyroSwipeInfo.DelayTime = value;
        }
    }
}
