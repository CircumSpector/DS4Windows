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
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control.Profiles.Schema;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public partial class ProfileSettingsViewModel
    {
        public int Device { get; }

        public int FuncDevNum { get; }

        public string ProfileName
        {
            get => profileService.CurrentlyEditedProfile.DisplayName;
            set => profileService.CurrentlyEditedProfile.DisplayName = value;
        }

        public int LightbarModeIndex
        {
            get
            {
                var index = 0;
                switch (profileService.CurrentlyEditedProfile.LightbarSettingInfo.Mode)
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

                profileService.CurrentlyEditedProfile.LightbarSettingInfo.Mode = temp;
            }
        }

        public Brush LightbarBrush
        {
            get
            {
                Brush tempBrush;
                var color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led;
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

        public Color MainColor => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.ToColor();

        public string MainColorString
        {
            get
            {
                var color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public int MainColorR
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Red;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Red = (byte)value;
        }

        public string MainColorRString =>
            $"#{profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Red:X2}FF0000";

        public int MainColorG
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Green;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Green = (byte)value;
        }

        public string MainColorGString =>
            $"#{profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Green:X2}00FF00";

        public int MainColorB
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Blue;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Blue = (byte)value;
        }

        public string MainColorBString =>
            $"#{profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led.Blue:X2}0000FF";

        public string LowColor
        {
            get
            {
                var color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public int LowColorR
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Red;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Red = (byte)value;
        }

        public string LowColorRString =>
            $"#{profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Red:X2}FF0000";

        public int LowColorG
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Green;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Green = (byte)value;
        }

        public string LowColorGString =>
            $"#{profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Green:X2}00FF00";

        public int LowColorB
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Blue;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Blue = (byte)value;
        }

        public string LowColorBString =>
            $"#{profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.Blue:X2}0000FF";

        public Color LowColorMedia => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed.ToColor();

        public int FlashTypeIndex
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings
                .FlashType; //Global.Instance.FlashType[device];
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.FlashType = (byte)value;
        }

        public int FlashAt
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings
                .FlashAt; //Global.Instance.FlashAt[device];
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.FlashAt = value;
        }

        public string FlashColor
        {
            get
            {
                var color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.FlashLed;

                if (color.Red == 0 && color.Green == 0 && color.Blue == 0)
                    color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.FlashLed =
                        profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led;

                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public Color FlashColorMedia
        {
            get
            {
                var color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.FlashLed;
                if (color.Red == 0 && color.Green == 0 && color.Blue == 0)
                    color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.FlashLed =
                        profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led;

                return color.ToColor();
            }
        }

        public int ChargingType
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.ChargingType;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.ChargingType = value;
        }

        public bool ColorBatteryPercent
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LedAsBattery;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LedAsBattery = value;
        }

        public string ChargingColor
        {
            get
            {
                var color = profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.ChargingLed;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public Color ChargingColorMedia =>
            profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.ChargingLed.ToColor();

        public Visibility ChargingColorVisible =>
            profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.ChargingType == 3
                ? Visibility.Visible
                : Visibility.Hidden;

        public double Rainbow
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Rainbow;
            set
            {
                profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Rainbow = value;
            }
        }

        public bool RainbowExists => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Rainbow != 0.0;

        public double MaxSatRainbow
        {
            get => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.MaxRainbowSaturation * 100.0;
            set => profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.MaxRainbowSaturation = value / 100.0;
        }

        public int RumbleBoost
        {
            get => profileService.CurrentlyEditedProfile.RumbleBoost;
            set => profileService.CurrentlyEditedProfile.RumbleBoost = (byte)value;
        }

        public int RumbleAutostopTime
        {
            // RumbleAutostopTime value is in milliseconds in XML config file, but GUI uses just seconds
            get => profileService.CurrentlyEditedProfile.RumbleAutostopTime / 1000;
            set => profileService.CurrentlyEditedProfile.RumbleAutostopTime = value * 1000;
        }

        public bool HeavyRumbleActive { get; set; }

        public bool LightRumbleActive { get; set; }

        public bool UseControllerReadout
        {
            get => Global.Instance.Config.Ds4Mapping;
            set => Global.Instance.Config.Ds4Mapping = value;
        }

        public int ButtonMouseSensitivity
        {
            get => profileService.CurrentlyEditedProfile.ButtonMouseInfo.ButtonSensitivity;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.ButtonMouseInfo.ButtonSensitivity;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.ButtonMouseInfo.ButtonSensitivity = value;
            }
        }

        public int ButtonMouseVerticalScale
        {
            get => Convert.ToInt32(profileService.CurrentlyEditedProfile.ButtonMouseInfo
                .ButtonVerticalScale * 100.0);
            set
            {
                var temp = profileService.CurrentlyEditedProfile.ButtonMouseInfo
                    .ButtonVerticalScale;
                var attemptValue = value * 0.01;
                if (temp == attemptValue) return;
                profileService.CurrentlyEditedProfile.ButtonMouseInfo.ButtonVerticalScale =
                    attemptValue;
            }
        }

        private double RawButtonMouseOffset => profileService.CurrentlyEditedProfile.ButtonMouseInfo
            .MouseVelocityOffset;

        public double ButtonMouseOffset
        {
            get => profileService.CurrentlyEditedProfile.ButtonMouseInfo.MouseVelocityOffset *
                   100.0;
            set
            {
                var temp =
                    profileService.CurrentlyEditedProfile.ButtonMouseInfo.MouseVelocityOffset *
                    100.0;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.ButtonMouseInfo.MouseVelocityOffset =
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
            get => profileService.CurrentlyEditedProfile.ButtonMouseInfo.MouseAcceleration;
            set => profileService.CurrentlyEditedProfile.ButtonMouseInfo.MouseAcceleration = value;
        }

        public bool EnableTouchpadToggle
        {
            get => profileService.CurrentlyEditedProfile.EnableTouchToggle;
            set => profileService.CurrentlyEditedProfile.EnableTouchToggle = value;
        }

        public bool EnableOutputDataToDS4
        {
            get => profileService.CurrentlyEditedProfile.EnableOutputDataToDS4;
            set => profileService.CurrentlyEditedProfile.EnableOutputDataToDS4 = value;
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
            get => profileService.CurrentlyEditedProfile.DisableVirtualController;
            set => profileService.CurrentlyEditedProfile.DisableVirtualController = value;
        }

        public bool IdleDisconnectExists
        {
            get => profileService.CurrentlyEditedProfile.IdleDisconnectTimeout != 0;
            set
            {
                // If enabling Idle Disconnect, set default time.
                // Otherwise, set time to 0 to mean disabled
                profileService.CurrentlyEditedProfile.IdleDisconnectTimeout =
                    value ? Global.DEFAULT_ENABLE_IDLE_DISCONN_MINS * 60 : 0;
            }
        }

        public int IdleDisconnect
        {
            get => profileService.CurrentlyEditedProfile.IdleDisconnectTimeout / 60;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.IdleDisconnectTimeout / 60;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.IdleDisconnectTimeout = value * 60;
            }
        }

        public int TempBTPollRateIndex { get; set; }

        public int ControllerTypeIndex
        {
            get
            {
                var type = 0;
                switch (profileService.CurrentlyEditedProfile.OutputDeviceType)
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
                switch (profileService.CurrentlyEditedProfile.GyroOutputMode)
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

                var current = profileService.CurrentlyEditedProfile.GyroOutputMode;
                if (temp == current) return;
                profileService.CurrentlyEditedProfile.GyroOutputMode = temp;
            }
        }

        public OutContType ContType => profileService.CurrentlyEditedProfile.OutputDeviceType;

        public int SASteeringWheelEmulationAxisIndex
        {
            get => (int)profileService.CurrentlyEditedProfile.SASteeringWheelEmulationAxis;
            set
            {
                var temp = (int)profileService.CurrentlyEditedProfile.SASteeringWheelEmulationAxis;
                if (temp == value) return;

                profileService.CurrentlyEditedProfile.SASteeringWheelEmulationAxis =
                    (SASteeringWheelEmulationAxisType)value;
            }
        }

        public int SASteeringWheelEmulationRangeIndex
        {
            get
            {
                var index = 360;
                switch (profileService.CurrentlyEditedProfile.SASteeringWheelEmulationRange)
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
                profileService.CurrentlyEditedProfile.SASteeringWheelEmulationRange = temp;
            }
        }

        public int SASteeringWheelEmulationRange
        {
            get => profileService.CurrentlyEditedProfile.SASteeringWheelEmulationRange;
            set => profileService.CurrentlyEditedProfile.SASteeringWheelEmulationRange = value;
        }

        public int SASteeringWheelFuzz
        {
            get => profileService.CurrentlyEditedProfile.SAWheelFuzzValues;
            set => profileService.CurrentlyEditedProfile.SAWheelFuzzValues = value;
        }

        public bool SASteeringWheelUseSmoothing
        {
            get => profileService.CurrentlyEditedProfile.WheelSmoothInfo.Enabled;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.WheelSmoothInfo.Enabled;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.WheelSmoothInfo.Enabled = value;
            }
        }

        public double SASteeringWheelSmoothMinCutoff
        {
            get => profileService.CurrentlyEditedProfile.WheelSmoothInfo.MinCutoff;
            set => profileService.CurrentlyEditedProfile.WheelSmoothInfo.MinCutoff = value;
        }

        public double SASteeringWheelSmoothBeta
        {
            get => profileService.CurrentlyEditedProfile.WheelSmoothInfo.Beta;
            set => profileService.CurrentlyEditedProfile.WheelSmoothInfo.Beta = value;
        }

        public double LSDeadZone
        {
            get => Math.Round(profileService.CurrentlyEditedProfile.LSModInfo.DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(profileService.CurrentlyEditedProfile.LSModInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.LSModInfo.DeadZone = (int)Math.Round(value * 127d);
            }
        }

        public double RSDeadZone
        {
            get => Math.Round(profileService.CurrentlyEditedProfile.RSModInfo.DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(profileService.CurrentlyEditedProfile.RSModInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.RSModInfo.DeadZone = (int)Math.Round(value * 127d);
            }
        }

        public double LSMaxZone
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.MaxZone / 100.0;
            set => profileService.CurrentlyEditedProfile.LSModInfo.MaxZone = (int)(value * 100.0);
        }

        public double RSMaxZone
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.MaxZone / 100.0;
            set => profileService.CurrentlyEditedProfile.RSModInfo.MaxZone = (int)(value * 100.0);
        }

        public double LSAntiDeadZone
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.AntiDeadZone / 100.0;
            set => profileService.CurrentlyEditedProfile.LSModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double RSAntiDeadZone
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.AntiDeadZone / 100.0;
            set => profileService.CurrentlyEditedProfile.RSModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double LSVerticalScale
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.VerticalScale / 100.0;
            set => profileService.CurrentlyEditedProfile.LSModInfo.VerticalScale = value * 100.0;
        }

        public double LSMaxOutput
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.MaxOutput / 100.0;
            set => profileService.CurrentlyEditedProfile.LSModInfo.MaxOutput = value * 100.0;
        }

        public bool LSMaxOutputForce
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.MaxOutputForce;
            set => profileService.CurrentlyEditedProfile.LSModInfo.MaxOutputForce = value;
        }

        public double RSVerticalScale
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.VerticalScale / 100.0;
            set => profileService.CurrentlyEditedProfile.RSModInfo.VerticalScale = value * 100.0;
        }

        public double RSMaxOutput
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.MaxOutput / 100.0;
            set => profileService.CurrentlyEditedProfile.RSModInfo.MaxOutput = value * 100.0;
        }

        public bool RSMaxOutputForce
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.MaxOutputForce;
            set => profileService.CurrentlyEditedProfile.RSModInfo.MaxOutputForce = value;
        }

        public int LSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (profileService.CurrentlyEditedProfile.LSModInfo.DZType)
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

                var current = profileService.CurrentlyEditedProfile.LSModInfo.DZType;
                if (temp == current) return;
                profileService.CurrentlyEditedProfile.LSModInfo.DZType = temp;
            }
        }

        public int RSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (profileService.CurrentlyEditedProfile.RSModInfo.DZType)
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

                var current = profileService.CurrentlyEditedProfile.RSModInfo.DZType;
                if (temp == current) return;
                profileService.CurrentlyEditedProfile.RSModInfo.DZType = temp;
            }
        }

        public double LSSens
        {
            get => profileService.CurrentlyEditedProfile.LSSens;
            set => profileService.CurrentlyEditedProfile.LSSens = value;
        }

        public double RSSens
        {
            get => profileService.CurrentlyEditedProfile.RSSens;
            set => profileService.CurrentlyEditedProfile.RSSens = value;
        }

        public bool LSSquareStick
        {
            get => profileService.CurrentlyEditedProfile.SquStickInfo.LSMode;
            set => profileService.CurrentlyEditedProfile.SquStickInfo.LSMode = value;
        }

        public bool RSSquareStick
        {
            get => profileService.CurrentlyEditedProfile.SquStickInfo.RSMode;
            set => profileService.CurrentlyEditedProfile.SquStickInfo.RSMode = value;
        }

        public double LSSquareRoundness
        {
            get => profileService.CurrentlyEditedProfile.SquStickInfo.LSRoundness;
            set => profileService.CurrentlyEditedProfile.SquStickInfo.LSRoundness = value;
        }

        public double RSSquareRoundness
        {
            get => profileService.CurrentlyEditedProfile.SquStickInfo.RSRoundness;
            set => profileService.CurrentlyEditedProfile.SquStickInfo.RSRoundness = value;
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
            get => profileService.CurrentlyEditedProfile.LSRotation * 180.0 / Math.PI;
            set => profileService.CurrentlyEditedProfile.LSRotation = value * Math.PI / 180.0;
        }

        public double RSRotation
        {
            get => profileService.CurrentlyEditedProfile.RSRotation * 180.0 / Math.PI;
            set => profileService.CurrentlyEditedProfile.RSRotation = value * Math.PI / 180.0;
        }

        public bool LSCustomCurveSelected => profileService.CurrentlyEditedProfile. LSOutCurveMode == CurveMode.Custom;

        public bool RSCustomCurveSelected => profileService.CurrentlyEditedProfile. RSOutCurveMode == CurveMode.Custom;

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
            get => profileService.CurrentlyEditedProfile.LSModInfo.Fuzz;
            set => profileService.CurrentlyEditedProfile.LSModInfo.Fuzz = value;
        }

        public int RSFuzz
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.Fuzz;
            set => profileService.CurrentlyEditedProfile.RSModInfo.Fuzz = value;
        }

        public bool LSAntiSnapback
        {
            get => profileService.CurrentlyEditedProfile.LSAntiSnapbackInfo.Enabled;
            set => profileService.CurrentlyEditedProfile.LSAntiSnapbackInfo.Enabled = value;
        }

        public bool RSAntiSnapback
        {
            get => profileService.CurrentlyEditedProfile.RSAntiSnapbackInfo.Enabled;
            set => profileService.CurrentlyEditedProfile.RSAntiSnapbackInfo.Enabled = value;
        }

        public double LSAntiSnapbackDelta
        {
            get => profileService.CurrentlyEditedProfile.LSAntiSnapbackInfo.Delta;
            set => profileService.CurrentlyEditedProfile.LSAntiSnapbackInfo.Delta = value;
        }

        public double RSAntiSnapbackDelta
        {
            get => profileService.CurrentlyEditedProfile.RSAntiSnapbackInfo.Delta;
            set => profileService.CurrentlyEditedProfile.RSAntiSnapbackInfo.Delta = value;
        }

        public int LSAntiSnapbackTimeout
        {
            get => profileService.CurrentlyEditedProfile.LSAntiSnapbackInfo.Timeout;
            set => profileService.CurrentlyEditedProfile.LSAntiSnapbackInfo.Timeout = value;
        }

        public int RSAntiSnapbackTimeout
        {
            get => profileService.CurrentlyEditedProfile.RSAntiSnapbackInfo.Timeout;
            set => profileService.CurrentlyEditedProfile.RSAntiSnapbackInfo.Timeout = value;
        }

        public bool LSOuterBindInvert
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.OuterBindInvert;
            set => profileService.CurrentlyEditedProfile.LSModInfo.OuterBindInvert = value;
        }

        public bool RSOuterBindInvert
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.OuterBindInvert;
            set => profileService.CurrentlyEditedProfile.RSModInfo.OuterBindInvert = value;
        }

        public double LSOuterBindDead
        {
            get => profileService.CurrentlyEditedProfile.LSModInfo.OuterBindDeadZone / 100.0;
            set => profileService.CurrentlyEditedProfile.LSModInfo.OuterBindDeadZone = value * 100.0;
        }

        public double RSOuterBindDead
        {
            get => profileService.CurrentlyEditedProfile.RSModInfo.OuterBindDeadZone / 100.0;
            set => profileService.CurrentlyEditedProfile.RSModInfo.OuterBindDeadZone = value * 100.0;
        }

        public int LSOutputIndex
        {
            get
            {
                var index = 0;
                switch (profileService.CurrentlyEditedProfile.LSOutputSettings.Mode)
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

                var current = profileService.CurrentlyEditedProfile.LSOutputSettings.Mode;
                if (temp == current) return;
                profileService.CurrentlyEditedProfile.LSOutputSettings.Mode = temp;
            }
        }

        public double LSFlickRWC
        {
            get => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration;
            set => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration =
                value;
        }

        public double LSFlickThreshold
        {
            get => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold;
            set => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold = value;
        }

        public double LSFlickTime
        {
            get => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickTime;
            set => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickTime = value;
        }

        public double LSMinAngleThreshold
        {
            get => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold;
            set => profileService.CurrentlyEditedProfile.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold =
                value;
        }

        public int RSOutputIndex
        {
            get
            {
                var index = 0;
                switch (profileService.CurrentlyEditedProfile.RSOutputSettings.Mode)
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

                var current = profileService.CurrentlyEditedProfile.RSOutputSettings.Mode;
                if (temp == current) return;
                profileService.CurrentlyEditedProfile.RSOutputSettings.Mode = temp;
            }
        }

        public double RSFlickRWC
        {
            get => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration;
            set => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration =
                value;
        }

        public double RSFlickThreshold
        {
            get => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold;
            set => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold = value;
        }

        public double RSFlickTime
        {
            get => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickTime;
            set => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickTime = value;
        }

        public double RSMinAngleThreshold
        {
            get => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold;
            set => profileService.CurrentlyEditedProfile.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold =
                value;
        }

        public double L2DeadZone
        {
            get => profileService.CurrentlyEditedProfile.L2ModInfo.DeadZone / 255.0;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.L2ModInfo.DeadZone / 255.0;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.L2ModInfo.DeadZone = (byte)(value * 255.0);
            }
        }

        public double R2DeadZone
        {
            get => profileService.CurrentlyEditedProfile.R2ModInfo.DeadZone / 255.0;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.R2ModInfo.DeadZone / 255.0;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.R2ModInfo.DeadZone = (byte)(value * 255.0);
            }
        }

        public double L2MaxZone
        {
            get => profileService.CurrentlyEditedProfile.L2ModInfo.MaxZone / 100.0;
            set => profileService.CurrentlyEditedProfile.L2ModInfo.MaxZone = (int)(value * 100.0);
        }

        public double R2MaxZone
        {
            get => profileService.CurrentlyEditedProfile.R2ModInfo.MaxZone / 100.0;
            set => profileService.CurrentlyEditedProfile.R2ModInfo.MaxZone = (int)(value * 100.0);
        }

        public double L2AntiDeadZone
        {
            get => profileService.CurrentlyEditedProfile.L2ModInfo.AntiDeadZone / 100.0;
            set => profileService.CurrentlyEditedProfile.L2ModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double R2AntiDeadZone
        {
            get => profileService.CurrentlyEditedProfile.R2ModInfo.AntiDeadZone / 100.0;
            set => profileService.CurrentlyEditedProfile.R2ModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double L2MaxOutput
        {
            get => profileService.CurrentlyEditedProfile.L2ModInfo.MaxOutput / 100.0;
            set => profileService.CurrentlyEditedProfile.L2ModInfo.MaxOutput = value * 100.0;
        }

        public double R2MaxOutput
        {
            get => profileService.CurrentlyEditedProfile.R2ModInfo.MaxOutput / 100.0;
            set => profileService.CurrentlyEditedProfile.R2ModInfo.MaxOutput = value * 100.0;
        }

        public double L2Sens
        {
            get => profileService.CurrentlyEditedProfile.L2Sens;
            set => profileService.CurrentlyEditedProfile.L2Sens = value;
        }

        public double R2Sens
        {
            get => profileService.CurrentlyEditedProfile.R2Sens;
            set => profileService.CurrentlyEditedProfile.R2Sens = value;
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
            get => profileService.CurrentlyEditedProfile.L2OutputSettings.TwoStageMode;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.L2OutputSettings.TwoStageMode;
                if (temp == value) return;

                profileService.CurrentlyEditedProfile.L2OutputSettings.TwoStageMode = value;
            }
        }

        public TwoStageTriggerMode R2TriggerMode
        {
            get => profileService.CurrentlyEditedProfile.R2OutputSettings.TwoStageMode;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.R2OutputSettings.TwoStageMode;
                if (temp == value) return;

                profileService.CurrentlyEditedProfile.R2OutputSettings.TwoStageMode = value;
            }
        }

        public int L2HipFireTime
        {
            get => profileService.CurrentlyEditedProfile.L2OutputSettings.HipFireMs;
            set => profileService.CurrentlyEditedProfile.L2OutputSettings.HipFireMs = value;
        }

        public int R2HipFireTime
        {
            get => profileService.CurrentlyEditedProfile.R2OutputSettings.HipFireMs;
            set => profileService.CurrentlyEditedProfile.R2OutputSettings.HipFireMs = value;
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
            get => profileService.CurrentlyEditedProfile.L2OutputSettings.TriggerEffect;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.L2OutputSettings.TriggerEffect;
                if (temp == value) return;

                profileService.CurrentlyEditedProfile.L2OutputSettings.TriggerEffect = value;
            }
        }

        public TriggerEffects R2TriggerEffect
        {
            get => profileService.CurrentlyEditedProfile.R2OutputSettings.TriggerEffect;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.R2OutputSettings.TriggerEffect;
                if (temp == value) return;

                profileService.CurrentlyEditedProfile.R2OutputSettings.TriggerEffect = value;
            }
        }

        public double SXDeadZone
        {
            get => profileService.CurrentlyEditedProfile.SXDeadZone;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.SXDeadZone;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.SXDeadZone = value;
            }
        }

        public double SZDeadZone
        {
            get => profileService.CurrentlyEditedProfile.SZDeadZone;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.SZDeadZone;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.SZDeadZone = value;
            }
        }

        public double SXMaxZone
        {
            get => profileService.CurrentlyEditedProfile.SXMaxZone;
            set => profileService.CurrentlyEditedProfile.SXMaxZone = value;
        }

        public double SZMaxZone
        {
            get => profileService.CurrentlyEditedProfile.SZMaxZone;
            set => profileService.CurrentlyEditedProfile.SZMaxZone = value;
        }

        public double SXAntiDeadZone
        {
            get => profileService.CurrentlyEditedProfile.SXAntiDeadZone;
            set => profileService.CurrentlyEditedProfile.SXAntiDeadZone = value;
        }

        public double SZAntiDeadZone
        {
            get => profileService.CurrentlyEditedProfile.SZAntiDeadZone;
            set => profileService.CurrentlyEditedProfile.SZAntiDeadZone = value;
        }

        public double SXSens
        {
            get => profileService.CurrentlyEditedProfile.SXSens;
            set => profileService.CurrentlyEditedProfile.SXSens = value;
        }

        public double SZSens
        {
            get => profileService.CurrentlyEditedProfile.SZSens;
            set => profileService.CurrentlyEditedProfile.SZSens = value;
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
                switch (profileService.CurrentlyEditedProfile.TouchOutMode)
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

                var current = profileService.CurrentlyEditedProfile.TouchOutMode;
                if (temp == current) return;
                profileService.CurrentlyEditedProfile.TouchOutMode = temp;
            }
        }

        public bool TouchSenExists
        {
            get => profileService.CurrentlyEditedProfile.TouchSensitivity != 0;
            set =>
                profileService.CurrentlyEditedProfile.TouchSensitivity =
                    value ? (byte)100 : (byte)0;
        }

        public int TouchSens
        {
            get => profileService.CurrentlyEditedProfile.TouchSensitivity;
            set
            {
                int temp = profileService.CurrentlyEditedProfile.TouchSensitivity;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.TouchSensitivity = (byte)value;
            }
        }

        public bool TouchScrollExists
        {
            get => profileService.CurrentlyEditedProfile.ScrollSensitivity != 0;
            set => profileService.CurrentlyEditedProfile.ScrollSensitivity = value ? 100 : 0;
        }

        public int TouchScroll
        {
            get => profileService.CurrentlyEditedProfile.ScrollSensitivity;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.ScrollSensitivity;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.ScrollSensitivity = value;
            }
        }

        public bool TouchTapExists
        {
            get => profileService.CurrentlyEditedProfile.TapSensitivity != 0;
            set => profileService.CurrentlyEditedProfile.TapSensitivity = value ? (byte)100 : (byte)0;
        }

        public int TouchTap
        {
            get => profileService.CurrentlyEditedProfile.TapSensitivity;
            set
            {
                int temp = profileService.CurrentlyEditedProfile.TapSensitivity;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.TapSensitivity = (byte)value;
            }
        }

        public bool TouchDoubleTap
        {
            get => profileService.CurrentlyEditedProfile.DoubleTap;
            set => profileService.CurrentlyEditedProfile.DoubleTap = value;
        }

        public bool TouchJitter
        {
            get => profileService.CurrentlyEditedProfile.TouchpadJitterCompensation;
            set => profileService.CurrentlyEditedProfile.TouchpadJitterCompensation = value;
        }

        public int TouchInvertIndex
        {
            get
            {
                var invert = profileService.CurrentlyEditedProfile.TouchPadInvert;
                var index = Array.IndexOf(touchpadInvertToValue, invert);
                return index;
            }
            set
            {
                var invert = touchpadInvertToValue[value];
                profileService.CurrentlyEditedProfile.TouchPadInvert = invert;
            }
        }

        public bool LowerRightTouchRMB
        {
            get => profileService.CurrentlyEditedProfile.LowerRCOn;
            set => profileService.CurrentlyEditedProfile.LowerRCOn = value;
        }

        public bool TouchpadClickPassthru
        {
            get => profileService.CurrentlyEditedProfile.TouchClickPassthru;
            set => profileService.CurrentlyEditedProfile.TouchClickPassthru = value;
        }

        public bool StartTouchpadOff
        {
            get => profileService.CurrentlyEditedProfile.StartTouchpadOff;
            set => profileService.CurrentlyEditedProfile.StartTouchpadOff = value;
        }

        public double TouchRelMouseRotation
        {
            get => profileService.CurrentlyEditedProfile.TouchPadRelMouse.Rotation * 180.0 / Math.PI;
            set => profileService.CurrentlyEditedProfile.TouchPadRelMouse.Rotation = value * Math.PI / 180.0;
        }

        public double TouchRelMouseMinThreshold
        {
            get => profileService.CurrentlyEditedProfile.TouchPadRelMouse.MinThreshold;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.TouchPadRelMouse.MinThreshold;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.TouchPadRelMouse.MinThreshold = value;
            }
        }

        public bool TouchTrackball
        {
            get => profileService.CurrentlyEditedProfile.TrackballMode;
            set => profileService.CurrentlyEditedProfile.TrackballMode = value;
        }

        public double TouchTrackballFriction
        {
            get => profileService.CurrentlyEditedProfile.TrackballFriction;
            set => profileService.CurrentlyEditedProfile.TrackballFriction = value;
        }

        public int TouchAbsMouseMaxZoneX
        {
            get => profileService.CurrentlyEditedProfile.TouchPadAbsMouse.MaxZoneX;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.TouchPadAbsMouse.MaxZoneX;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.TouchPadAbsMouse.MaxZoneX = value;
            }
        }

        public int TouchAbsMouseMaxZoneY
        {
            get => profileService.CurrentlyEditedProfile.TouchPadAbsMouse.MaxZoneY;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.TouchPadAbsMouse.MaxZoneY;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.TouchPadAbsMouse.MaxZoneY = value;
            }
        }

        public bool TouchAbsMouseSnapCenter
        {
            get => profileService.CurrentlyEditedProfile.TouchPadAbsMouse.SnapToCenter;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.TouchPadAbsMouse.SnapToCenter;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.TouchPadAbsMouse.SnapToCenter = value;
            }
        }

        public bool GyroMouseTurns
        {
            get => profileService.CurrentlyEditedProfile.GyroTriggerTurns;
            set => profileService.CurrentlyEditedProfile.GyroTriggerTurns = value;
        }

        public int GyroSensitivity
        {
            get => profileService.CurrentlyEditedProfile.GyroSensitivity;
            set => profileService.CurrentlyEditedProfile.GyroSensitivity = value;
        }

        public int GyroVertScale
        {
            get => profileService.CurrentlyEditedProfile.GyroSensVerticalScale;
            set => profileService.CurrentlyEditedProfile.GyroSensVerticalScale = value;
        }

        public int GyroMouseEvalCondIndex
        {
            get => Global.Instance.Config.GetSATriggerCondition(Device) ? 0 : 1;
            set => Global.Instance.Config.SetSaTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseXAxis
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseHorizontalAxis;
            set => profileService.CurrentlyEditedProfile.GyroMouseHorizontalAxis = value;
        }

        public double GyroMouseMinThreshold
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseInfo.MinThreshold;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.GyroMouseInfo.MinThreshold;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.GyroMouseInfo.MinThreshold = value;
            }
        }

        public bool GyroMouseInvertX
        {
            get => (profileService.CurrentlyEditedProfile.GyroInvert & 2) == 2;
            set
            {
                if (value)
                    profileService.CurrentlyEditedProfile.GyroInvert |= 2;
                else
                    profileService.CurrentlyEditedProfile.GyroInvert &= ~2;
            }
        }

        public bool GyroMouseInvertY
        {
            get => (profileService.CurrentlyEditedProfile.GyroInvert & 1) == 1;
            set
            {
                if (value)
                    profileService.CurrentlyEditedProfile.GyroInvert |= 1;
                else
                    profileService.CurrentlyEditedProfile.GyroInvert &= ~1;
            }
        }

        public bool GyroMouseSmooth
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseInfo.EnableSmoothing;
            set
            {
                var tempInfo = profileService.CurrentlyEditedProfile.GyroMouseInfo;
                if (tempInfo.EnableSmoothing == value) return;

                profileService.CurrentlyEditedProfile.GyroMouseInfo.EnableSmoothing = value;
            }
        }

        public int GyroMouseSmoothMethodIndex
        {
            get => gyroMouseSmoothMethodIndex;
            set
            {
                if (gyroMouseSmoothMethodIndex == value) return;

                var tempInfo = profileService.CurrentlyEditedProfile.GyroMouseInfo;
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
                switch (profileService.CurrentlyEditedProfile.GyroMouseInfo.Smoothing)
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
                switch (profileService.CurrentlyEditedProfile.GyroMouseInfo.Smoothing)
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
            get => profileService.CurrentlyEditedProfile.GyroMouseInfo.SmoothingWeight;
            set => profileService.CurrentlyEditedProfile.GyroMouseInfo.SmoothingWeight = value;
        }

        public double GyroMouseOneEuroMinCutoff
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseInfo.MinCutoff;
            set => profileService.CurrentlyEditedProfile.GyroMouseInfo.MinCutoff = value;
        }

        public double GyroMouseOneEuroBeta
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseInfo.Beta;
            set => profileService.CurrentlyEditedProfile.GyroMouseInfo.Beta = value;
        }

        public int GyroMouseStickSmoothMethodIndex
        {
            get => gyroMouseStickSmoothMethodIndex;
            set
            {
                if (gyroMouseStickSmoothMethodIndex == value) return;

                var tempInfo = profileService.CurrentlyEditedProfile.GyroMouseStickInfo;
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
                switch (profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Smoothing)
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
                switch (profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Smoothing)
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
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.SmoothWeight;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.SmoothWeight = value;
        }

        public double GyroMouseStickOneEuroMinCutoff
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MinCutoff;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MinCutoff = value;
        }

        public double GyroMouseStickOneEuroBeta
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Beta;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Beta = value;
        }


        public int GyroMouseDeadZone
        {
            get => Global.Instance.Config.GyroMouseDeadZone[Device];
            set => Global.Instance.Config.SetGyroMouseDZ(Device, value, rootHub);
        }

        public bool GyroMouseToggle
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseToggle;
            set => Global.Instance.Config.SetGyroMouseToggle(Device, value, rootHub);
        }

        public bool GyroMouseStickTurns
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickTriggerTurns;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickTriggerTurns = value;
        }

        public bool GyroMouseStickToggle
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickToggle;
            set => Global.Instance.Config.SetGyroMouseStickToggle(Device, value, rootHub);
        }

        public int GyroMouseStickDeadZone
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.DeadZone;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.DeadZone = value;
        }

        public int GyroMouseStickMaxZone
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxZone;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxZone = value;
        }

        public int GyroMouseStickOutputStick
        {
            get => (int)profileService.CurrentlyEditedProfile.GyroMouseStickInfo.OutStick;
            set =>
                profileService.CurrentlyEditedProfile.GyroMouseStickInfo.OutStick =
                    (GyroMouseStickInfo.OutputStick)value;
        }

        public int GyroMouseStickOutputAxes
        {
            get => (int)profileService.CurrentlyEditedProfile.GyroMouseStickInfo.OutputStickDir;
            set =>
                profileService.CurrentlyEditedProfile.GyroMouseStickInfo.OutputStickDir =
                    (GyroMouseStickInfo.OutputStickAxes)value;
        }

        public double GyroMouseStickAntiDeadX
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.AntiDeadX * 100.0;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.AntiDeadX = value * 0.01;
        }

        public double GyroMouseStickAntiDeadY
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.AntiDeadY * 100.0;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.AntiDeadY = value * 0.01;
        }

        public int GyroMouseStickVertScale
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.VerticalScale;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.VerticalScale = value;
        }

        public bool GyroMouseStickMaxOutputEnabled
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxOutputEnabled;
            set
            {
                var temp = profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxOutputEnabled;
                if (temp == value) return;
                profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxOutputEnabled = value;
            }
        }

        public double GyroMouseStickMaxOutput
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxOutput;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.MaxOutput = value;
        }

        public int GyroMouseStickEvalCondIndex
        {
            get => profileService.CurrentlyEditedProfile.SAMouseStickTriggerCond ? 0 : 1;
            set => Global.Instance.Config.SetSaMouseStickTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseStickXAxis
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickHorizontalAxis;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickHorizontalAxis = value;
        }

        public bool GyroMouseStickInvertX
        {
            get => (profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted & 1) == 1;
            set
            {
                if (value)
                {
                    profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted |= 1;
                }
                else
                {
                    var temp = profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted;
                    profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted = (uint)(temp & ~1);
                }
            }
        }

        public bool GyroMouseStickInvertY
        {
            get => (profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted & 2) == 2;
            set
            {
                if (value)
                {
                    profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted |= 2;
                }
                else
                {
                    var temp = profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted;
                    profileService.CurrentlyEditedProfile.GyroMouseStickInfo.Inverted = (uint)(temp & ~2);
                }
            }
        }

        public bool GyroMouseStickSmooth
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.UseSmoothing;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.UseSmoothing = value;
        }

        public double GyroMousetickSmoothWeight
        {
            get => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.SmoothWeight;
            set => profileService.CurrentlyEditedProfile.GyroMouseStickInfo.SmoothWeight = value;
        }

        public string TouchDisInvertString { get; set; } = "None";

        public string GyroControlsTrigDisplay { get; set; } = "Always On";

        public bool GyroControlsTurns
        {
            get => profileService.CurrentlyEditedProfile.GyroControlsInfo.TriggerTurns;
            set => profileService.CurrentlyEditedProfile.GyroControlsInfo.TriggerTurns = value;
        }

        public int GyroControlsEvalCondIndex
        {
            get => profileService.CurrentlyEditedProfile.GyroControlsInfo.TriggerCond ? 0 : 1;
            set => profileService.CurrentlyEditedProfile.GyroControlsInfo.TriggerCond =
                value == 0 ? true : false;
        }

        public bool GyroControlsToggle
        {
            get => profileService.CurrentlyEditedProfile.GyroControlsInfo.TriggerToggle;
            set => Global.Instance.Config.SetGyroControlsToggle(Device, value, rootHub);
        }

        public string GyroMouseTrigDisplay { get; set; } = "Always On";

        public string GyroMouseStickTrigDisplay { get; set; } = "Always On";

        public string GyroSwipeTrigDisplay { get; set; } = "Always On";

        public bool GyroSwipeTurns
        {
            get => profileService.CurrentlyEditedProfile.GyroSwipeInfo.TriggerTurns;
            set => profileService.CurrentlyEditedProfile.GyroSwipeInfo.TriggerTurns = value;
        }

        public int GyroSwipeEvalCondIndex
        {
            get => profileService.CurrentlyEditedProfile.GyroSwipeInfo.TriggerCondition ? 0 : 1;
            set => profileService.CurrentlyEditedProfile.GyroSwipeInfo.TriggerCondition = value == 0 ? true : false;
        }

        public int GyroSwipeXAxis
        {
            get => (int)profileService.CurrentlyEditedProfile.GyroSwipeInfo.XAxis;
            set => profileService.CurrentlyEditedProfile.GyroSwipeInfo.XAxis = (GyroDirectionalSwipeInfo.XAxisSwipe)value;
        }

        public int GyroSwipeDeadZoneX
        {
            get => profileService.CurrentlyEditedProfile.GyroSwipeInfo.DeadZoneX;
            set => profileService.CurrentlyEditedProfile.GyroSwipeInfo.DeadZoneX = value;
        }

        public int GyroSwipeDeadZoneY
        {
            get => profileService.CurrentlyEditedProfile.GyroSwipeInfo.DeadZoneY;
            set => profileService.CurrentlyEditedProfile.GyroSwipeInfo.DeadZoneY = value;
        }

        public int GyroSwipeDelayTime
        {
            get => profileService.CurrentlyEditedProfile.GyroSwipeInfo.DelayTime;
            set => profileService.CurrentlyEditedProfile.GyroSwipeInfo.DelayTime = value;
        }
    }
}
