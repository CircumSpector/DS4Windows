using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.IoC.Services;
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
                LightbarModeIndexChanged?.Invoke(this, EventArgs.Empty);
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
                MainColorRChanged?.Invoke(this, EventArgs.Empty);
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
                MainColorGChanged?.Invoke(this, EventArgs.Empty);
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
                MainColorBChanged?.Invoke(this, EventArgs.Empty);
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
                LowColorRChanged?.Invoke(this, EventArgs.Empty);
                LowColorRStringChanged?.Invoke(this, EventArgs.Empty);
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
                LowColorGChanged?.Invoke(this, EventArgs.Empty);
                LowColorGStringChanged?.Invoke(this, EventArgs.Empty);
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
                LowColorBChanged?.Invoke(this, EventArgs.Empty);
                LowColorBStringChanged?.Invoke(this, EventArgs.Empty);
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
                ChargingColorVisibleChanged?.Invoke(this, EventArgs.Empty);
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
                RainbowChanged?.Invoke(this, EventArgs.Empty);
                RainbowExistsChanged?.Invoke(this, EventArgs.Empty);
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
            get => currentProfile.RumbleBoost;
            set => currentProfile.RumbleBoost = (byte)value;
        }

        public int RumbleAutostopTime
        {
            // RumbleAutostopTime value is in milliseconds in XML config file, but GUI uses just seconds
            get => currentProfile.RumbleAutostopTime / 1000;
            set => currentProfile.RumbleAutostopTime = value * 1000;
        }

        public bool HeavyRumbleActive
        {
            get => heavyRumbleActive;
            set
            {
                heavyRumbleActive = value;
                HeavyRumbleActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool LightRumbleActive
        {
            get => lightRumbleActive;
            set
            {
                lightRumbleActive = value;
                LightRumbleActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool UseControllerReadout
        {
            get => Global.Instance.Config.Ds4Mapping;
            set => Global.Instance.Config.Ds4Mapping = value;
        }

        public int ButtonMouseSensitivity
        {
            get => currentProfile.ButtonMouseInfo.ButtonSensitivity;
            set
            {
                var temp = currentProfile.ButtonMouseInfo.ButtonSensitivity;
                if (temp == value) return;
                currentProfile.ButtonMouseInfo.ButtonSensitivity = value;
                ButtonMouseSensitivityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ButtonMouseVerticalScale
        {
            get => Convert.ToInt32(currentProfile.ButtonMouseInfo
                .ButtonVerticalScale * 100.0);
            set
            {
                var temp = currentProfile.ButtonMouseInfo
                    .ButtonVerticalScale;
                var attemptValue = value * 0.01;
                if (temp == attemptValue) return;
                currentProfile.ButtonMouseInfo.ButtonVerticalScale =
                    attemptValue;
                ButtonMouseVerticalScaleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private double RawButtonMouseOffset => currentProfile.ButtonMouseInfo
            .MouseVelocityOffset;

        public double ButtonMouseOffset
        {
            get => currentProfile.ButtonMouseInfo.MouseVelocityOffset *
                   100.0;
            set
            {
                var temp =
                    currentProfile.ButtonMouseInfo.MouseVelocityOffset *
                    100.0;
                if (temp == value) return;
                currentProfile.ButtonMouseInfo.MouseVelocityOffset =
                    value * 0.01;
                ButtonMouseOffsetChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int OutputMouseSpeed
        {
            get => outputMouseSpeed;
            set
            {
                if (value == outputMouseSpeed) return;
                outputMouseSpeed = value;
                OutputMouseSpeedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double MouseOffsetSpeed
        {
            get => mouseOffsetSpeed;
            set
            {
                if (mouseOffsetSpeed == value) return;
                mouseOffsetSpeed = value;
                MouseOffsetSpeedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool MouseAcceleration
        {
            get => currentProfile.ButtonMouseInfo.MouseAcceleration;
            set => currentProfile.ButtonMouseInfo.MouseAcceleration = value;
        }

        public bool EnableTouchpadToggle
        {
            get => currentProfile.EnableTouchToggle;
            set => currentProfile.EnableTouchToggle = value;
        }

        public bool EnableOutputDataToDS4
        {
            get => currentProfile.EnableOutputDataToDS4;
            set => currentProfile.EnableOutputDataToDS4 = value;
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
            get => currentProfile.DisableVirtualController;
            set => currentProfile.DisableVirtualController = value;
        }

        public bool IdleDisconnectExists
        {
            get => currentProfile.IdleDisconnectTimeout != 0;
            set
            {
                // If enabling Idle Disconnect, set default time.
                // Otherwise, set time to 0 to mean disabled
                currentProfile.IdleDisconnectTimeout =
                    value ? Global.DEFAULT_ENABLE_IDLE_DISCONN_MINS * 60 : 0;

                IdleDisconnectChanged?.Invoke(this, EventArgs.Empty);
                IdleDisconnectExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int IdleDisconnect
        {
            get => currentProfile.IdleDisconnectTimeout / 60;
            set
            {
                var temp = currentProfile.IdleDisconnectTimeout / 60;
                if (temp == value) return;
                currentProfile.IdleDisconnectTimeout = value * 60;
                IdleDisconnectChanged?.Invoke(this, EventArgs.Empty);
                IdleDisconnectExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TempBTPollRateIndex { get; set; }

        public int ControllerTypeIndex
        {
            get
            {
                var type = 0;
                switch (currentProfile.OutputDeviceType)
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
                switch (currentProfile.GyroOutputMode)
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

                var current = currentProfile.GyroOutputMode;
                if (temp == current) return;
                currentProfile.GyroOutputMode = temp;
                GyroOutModeIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public OutContType ContType => currentProfile.OutputDeviceType;

        public int SASteeringWheelEmulationAxisIndex
        {
            get => (int)currentProfile.SASteeringWheelEmulationAxis;
            set
            {
                var temp = (int)currentProfile.SASteeringWheelEmulationAxis;
                if (temp == value) return;

                currentProfile.SASteeringWheelEmulationAxis =
                    (SASteeringWheelEmulationAxisType)value;
                SASteeringWheelEmulationAxisIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SASteeringWheelEmulationRangeIndex
        {
            get
            {
                var index = 360;
                switch (currentProfile.SASteeringWheelEmulationRange)
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
                currentProfile.SASteeringWheelEmulationRange = temp;
            }
        }

        public int SASteeringWheelEmulationRange
        {
            get => currentProfile.SASteeringWheelEmulationRange;
            set => currentProfile.SASteeringWheelEmulationRange = value;
        }

        public int SASteeringWheelFuzz
        {
            get => currentProfile.SAWheelFuzzValues;
            set => currentProfile.SAWheelFuzzValues = value;
        }

        public bool SASteeringWheelUseSmoothing
        {
            get => currentProfile.WheelSmoothInfo.Enabled;
            set
            {
                var temp = currentProfile.WheelSmoothInfo.Enabled;
                if (temp == value) return;
                currentProfile.WheelSmoothInfo.Enabled = value;
                SASteeringWheelUseSmoothingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SASteeringWheelSmoothMinCutoff
        {
            get => currentProfile.WheelSmoothInfo.MinCutoff;
            set => currentProfile.WheelSmoothInfo.MinCutoff = value;
        }

        public double SASteeringWheelSmoothBeta
        {
            get => currentProfile.WheelSmoothInfo.Beta;
            set => currentProfile.WheelSmoothInfo.Beta = value;
        }

        public double LSDeadZone
        {
            get => Math.Round(currentProfile.LSModInfo.DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(currentProfile.LSModInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                currentProfile.LSModInfo.DeadZone = (int)Math.Round(value * 127d);
                LSDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double RSDeadZone
        {
            get => Math.Round(currentProfile.RSModInfo.DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(currentProfile.RSModInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                currentProfile.RSModInfo.DeadZone = (int)Math.Round(value * 127d);
                RSDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double LSMaxZone
        {
            get => currentProfile.LSModInfo.MaxZone / 100.0;
            set => currentProfile.LSModInfo.MaxZone = (int)(value * 100.0);
        }

        public double RSMaxZone
        {
            get => currentProfile.RSModInfo.MaxZone / 100.0;
            set => currentProfile.RSModInfo.MaxZone = (int)(value * 100.0);
        }

        public double LSAntiDeadZone
        {
            get => currentProfile.LSModInfo.AntiDeadZone / 100.0;
            set => currentProfile.LSModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double RSAntiDeadZone
        {
            get => currentProfile.RSModInfo.AntiDeadZone / 100.0;
            set => currentProfile.RSModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double LSVerticalScale
        {
            get => currentProfile.LSModInfo.VerticalScale / 100.0;
            set => currentProfile.LSModInfo.VerticalScale = value * 100.0;
        }

        public double LSMaxOutput
        {
            get => currentProfile.LSModInfo.MaxOutput / 100.0;
            set => currentProfile.LSModInfo.MaxOutput = value * 100.0;
        }

        public bool LSMaxOutputForce
        {
            get => currentProfile.LSModInfo.MaxOutputForce;
            set => currentProfile.LSModInfo.MaxOutputForce = value;
        }

        public double RSVerticalScale
        {
            get => currentProfile.RSModInfo.VerticalScale / 100.0;
            set => currentProfile.RSModInfo.VerticalScale = value * 100.0;
        }

        public double RSMaxOutput
        {
            get => currentProfile.RSModInfo.MaxOutput / 100.0;
            set => currentProfile.RSModInfo.MaxOutput = value * 100.0;
        }

        public bool RSMaxOutputForce
        {
            get => currentProfile.RSModInfo.MaxOutputForce;
            set => currentProfile.RSModInfo.MaxOutputForce = value;
        }

        public int LSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (currentProfile.LSModInfo.DZType)
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

                var current = currentProfile.LSModInfo.DZType;
                if (temp == current) return;
                currentProfile.LSModInfo.DZType = temp;
            }
        }

        public int RSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (currentProfile.RSModInfo.DZType)
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

                var current = currentProfile.RSModInfo.DZType;
                if (temp == current) return;
                currentProfile.RSModInfo.DZType = temp;
            }
        }

        public double LSSens
        {
            get => currentProfile.LSSens;
            set => currentProfile.LSSens = value;
        }

        public double RSSens
        {
            get => currentProfile.RSSens;
            set => currentProfile.RSSens = value;
        }

        public bool LSSquareStick
        {
            get => currentProfile.SquStickInfo.LSMode;
            set => currentProfile.SquStickInfo.LSMode = value;
        }

        public bool RSSquareStick
        {
            get => currentProfile.SquStickInfo.RSMode;
            set => currentProfile.SquStickInfo.RSMode = value;
        }

        public double LSSquareRoundness
        {
            get => currentProfile.SquStickInfo.LSRoundness;
            set => currentProfile.SquStickInfo.LSRoundness = value;
        }

        public double RSSquareRoundness
        {
            get => currentProfile.SquStickInfo.RSRoundness;
            set => currentProfile.SquStickInfo.RSRoundness = value;
        }

        public int LSOutputCurveIndex
        {
            get => Global.Instance.Config.GetLsOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetLsOutCurveMode(Device, value);
                LSCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int RSOutputCurveIndex
        {
            get => Global.Instance.Config.GetRsOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetRsOutCurveMode(Device, value);
                RSCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double LSRotation
        {
            get => currentProfile.LSRotation * 180.0 / Math.PI;
            set => currentProfile.LSRotation = value * Math.PI / 180.0;
        }

        public double RSRotation
        {
            get => currentProfile.RSRotation * 180.0 / Math.PI;
            set => currentProfile.RSRotation = value * Math.PI / 180.0;
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
            get => currentProfile.LSModInfo.Fuzz;
            set => currentProfile.LSModInfo.Fuzz = value;
        }

        public int RSFuzz
        {
            get => currentProfile.RSModInfo.Fuzz;
            set => currentProfile.RSModInfo.Fuzz = value;
        }

        public bool LSAntiSnapback
        {
            get => currentProfile.LSAntiSnapbackInfo.Enabled;
            set => currentProfile.LSAntiSnapbackInfo.Enabled = value;
        }

        public bool RSAntiSnapback
        {
            get => currentProfile.RSAntiSnapbackInfo.Enabled;
            set => currentProfile.RSAntiSnapbackInfo.Enabled = value;
        }

        public double LSAntiSnapbackDelta
        {
            get => currentProfile.LSAntiSnapbackInfo.Delta;
            set => currentProfile.LSAntiSnapbackInfo.Delta = value;
        }

        public double RSAntiSnapbackDelta
        {
            get => currentProfile.RSAntiSnapbackInfo.Delta;
            set => currentProfile.RSAntiSnapbackInfo.Delta = value;
        }

        public int LSAntiSnapbackTimeout
        {
            get => currentProfile.LSAntiSnapbackInfo.Timeout;
            set => currentProfile.LSAntiSnapbackInfo.Timeout = value;
        }

        public int RSAntiSnapbackTimeout
        {
            get => currentProfile.RSAntiSnapbackInfo.Timeout;
            set => currentProfile.RSAntiSnapbackInfo.Timeout = value;
        }

        public bool LSOuterBindInvert
        {
            get => currentProfile.LSModInfo.OuterBindInvert;
            set => currentProfile.LSModInfo.OuterBindInvert = value;
        }

        public bool RSOuterBindInvert
        {
            get => currentProfile.RSModInfo.OuterBindInvert;
            set => currentProfile.RSModInfo.OuterBindInvert = value;
        }

        public double LSOuterBindDead
        {
            get => currentProfile.LSModInfo.OuterBindDeadZone / 100.0;
            set => currentProfile.LSModInfo.OuterBindDeadZone = value * 100.0;
        }

        public double RSOuterBindDead
        {
            get => currentProfile.RSModInfo.OuterBindDeadZone / 100.0;
            set => currentProfile.RSModInfo.OuterBindDeadZone = value * 100.0;
        }

        public int LSOutputIndex
        {
            get
            {
                var index = 0;
                switch (currentProfile.LSOutputSettings.Mode)
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

                var current = currentProfile.LSOutputSettings.Mode;
                if (temp == current) return;
                currentProfile.LSOutputSettings.Mode = temp;
                LSOutputIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double LSFlickRWC
        {
            get => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration;
            set => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration =
                value;
        }

        public double LSFlickThreshold
        {
            get => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold;
            set => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold = value;
        }

        public double LSFlickTime
        {
            get => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickTime;
            set => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.FlickTime = value;
        }

        public double LSMinAngleThreshold
        {
            get => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold;
            set => currentProfile.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold =
                value;
        }

        public int RSOutputIndex
        {
            get
            {
                var index = 0;
                switch (currentProfile.RSOutputSettings.Mode)
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

                var current = currentProfile.RSOutputSettings.Mode;
                if (temp == current) return;
                currentProfile.RSOutputSettings.Mode = temp;
                RSOutputIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double RSFlickRWC
        {
            get => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration;
            set => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration =
                value;
        }

        public double RSFlickThreshold
        {
            get => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold;
            set => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold = value;
        }

        public double RSFlickTime
        {
            get => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickTime;
            set => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.FlickTime = value;
        }

        public double RSMinAngleThreshold
        {
            get => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold;
            set => currentProfile.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold =
                value;
        }

        public double L2DeadZone
        {
            get => currentProfile.L2ModInfo.DeadZone / 255.0;
            set
            {
                var temp = currentProfile.L2ModInfo.DeadZone / 255.0;
                if (temp == value) return;
                currentProfile.L2ModInfo.DeadZone = (byte)(value * 255.0);
                L2DeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double R2DeadZone
        {
            get => currentProfile.R2ModInfo.DeadZone / 255.0;
            set
            {
                var temp = currentProfile.R2ModInfo.DeadZone / 255.0;
                if (temp == value) return;
                currentProfile.R2ModInfo.DeadZone = (byte)(value * 255.0);
                R2DeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double L2MaxZone
        {
            get => currentProfile.L2ModInfo.MaxZone / 100.0;
            set => currentProfile.L2ModInfo.MaxZone = (int)(value * 100.0);
        }

        public double R2MaxZone
        {
            get => currentProfile.R2ModInfo.MaxZone / 100.0;
            set => currentProfile.R2ModInfo.MaxZone = (int)(value * 100.0);
        }

        public double L2AntiDeadZone
        {
            get => currentProfile.L2ModInfo.AntiDeadZone / 100.0;
            set => currentProfile.L2ModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double R2AntiDeadZone
        {
            get => currentProfile.R2ModInfo.AntiDeadZone / 100.0;
            set => currentProfile.R2ModInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double L2MaxOutput
        {
            get => currentProfile.L2ModInfo.MaxOutput / 100.0;
            set => currentProfile.L2ModInfo.MaxOutput = value * 100.0;
        }

        public double R2MaxOutput
        {
            get => currentProfile.R2ModInfo.MaxOutput / 100.0;
            set => currentProfile.R2ModInfo.MaxOutput = value * 100.0;
        }

        public double L2Sens
        {
            get => currentProfile.L2Sens;
            set => currentProfile.L2Sens = value;
        }

        public double R2Sens
        {
            get => currentProfile.R2Sens;
            set => currentProfile.R2Sens = value;
        }

        public int L2OutputCurveIndex
        {
            get => Global.Instance.Config.GetL2OutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetL2OutCurveMode(Device, value);
                L2CustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int R2OutputCurveIndex
        {
            get => Global.Instance.Config.GetR2OutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetR2OutCurveMode(Device, value);
                R2CustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
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
            get => currentProfile.L2OutputSettings.TwoStageMode;
            set
            {
                var temp = currentProfile.L2OutputSettings.TwoStageMode;
                if (temp == value) return;

                currentProfile.L2OutputSettings.TwoStageMode = value;
                L2TriggerModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TwoStageTriggerMode R2TriggerMode
        {
            get => currentProfile.R2OutputSettings.TwoStageMode;
            set
            {
                var temp = currentProfile.R2OutputSettings.TwoStageMode;
                if (temp == value) return;

                currentProfile.R2OutputSettings.TwoStageMode = value;
                R2TriggerModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int L2HipFireTime
        {
            get => currentProfile.L2OutputSettings.HipFireMs;
            set => currentProfile.L2OutputSettings.HipFireMs = value;
        }

        public int R2HipFireTime
        {
            get => currentProfile.R2OutputSettings.HipFireMs;
            set => currentProfile.R2OutputSettings.HipFireMs = value;
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
            get => currentProfile.L2OutputSettings.TriggerEffect;
            set
            {
                var temp = currentProfile.L2OutputSettings.TriggerEffect;
                if (temp == value) return;

                currentProfile.L2OutputSettings.TriggerEffect = value;
            }
        }

        public TriggerEffects R2TriggerEffect
        {
            get => currentProfile.R2OutputSettings.TriggerEffect;
            set
            {
                var temp = currentProfile.R2OutputSettings.TriggerEffect;
                if (temp == value) return;

                currentProfile.R2OutputSettings.TriggerEffect = value;
            }
        }

        public double SXDeadZone
        {
            get => currentProfile.SXDeadZone;
            set
            {
                var temp = currentProfile.SXDeadZone;
                if (temp == value) return;
                currentProfile.SXDeadZone = value;
                SXDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SZDeadZone
        {
            get => currentProfile.SZDeadZone;
            set
            {
                var temp = currentProfile.SZDeadZone;
                if (temp == value) return;
                currentProfile.SZDeadZone = value;
                SZDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SXMaxZone
        {
            get => currentProfile.SXMaxZone;
            set => currentProfile.SXMaxZone = value;
        }

        public double SZMaxZone
        {
            get => currentProfile.SZMaxZone;
            set => currentProfile.SZMaxZone = value;
        }

        public double SXAntiDeadZone
        {
            get => currentProfile.SXAntiDeadZone;
            set => currentProfile.SXAntiDeadZone = value;
        }

        public double SZAntiDeadZone
        {
            get => currentProfile.SZAntiDeadZone;
            set => currentProfile.SZAntiDeadZone = value;
        }

        public double SXSens
        {
            get => currentProfile.SXSens;
            set => currentProfile.SXSens = value;
        }

        public double SZSens
        {
            get => currentProfile.SZSens;
            set => currentProfile.SZSens = value;
        }

        public int SXOutputCurveIndex
        {
            get => Global.Instance.Config.GetSXOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetSXOutCurveMode(Device, value);
                SXCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SZOutputCurveIndex
        {
            get => Global.Instance.Config.GetSZOutCurveMode(Device);
            set
            {
                Global.Instance.Config.SetSZOutCurveMode(Device, value);
                SZCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
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
                switch (currentProfile.TouchOutMode)
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

                var current = currentProfile.TouchOutMode;
                if (temp == current) return;
                currentProfile.TouchOutMode = temp;
                TouchpadOutputIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchSenExists
        {
            get => currentProfile.TouchSensitivity != 0;
            set
            {
                currentProfile.TouchSensitivity =
                    value ? (byte)100 : (byte)0;
                TouchSenExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TouchSens
        {
            get => currentProfile.TouchSensitivity;
            set
            {
                int temp = currentProfile.TouchSensitivity;
                if (temp == value) return;
                currentProfile.TouchSensitivity = (byte)value;
                if (value == 0) TouchSenExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchScrollExists
        {
            get => currentProfile.ScrollSensitivity != 0;
            set
            {
                currentProfile.ScrollSensitivity = value ? 100 : 0;
                TouchScrollExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchScrollChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TouchScroll
        {
            get => currentProfile.ScrollSensitivity;
            set
            {
                var temp = currentProfile.ScrollSensitivity;
                if (temp == value) return;
                currentProfile.ScrollSensitivity = value;
                if (value == 0) TouchScrollExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchScrollChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchTapExists
        {
            get => currentProfile.TapSensitivity != 0;
            set
            {
                currentProfile.TapSensitivity = value ? (byte)100 : (byte)0;
                TouchTapExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchTapChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TouchTap
        {
            get => currentProfile.TapSensitivity;
            set
            {
                int temp = currentProfile.TapSensitivity;
                if (temp == value) return;
                currentProfile.TapSensitivity = (byte)value;
                if (value == 0) TouchTapExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchTapChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchDoubleTap
        {
            get => currentProfile.DoubleTap;
            set => currentProfile.DoubleTap = value;
        }

        public bool TouchJitter
        {
            get => currentProfile.TouchpadJitterCompensation;
            set => currentProfile.TouchpadJitterCompensation = value;
        }

        public int TouchInvertIndex
        {
            get
            {
                var invert = currentProfile.TouchPadInvert;
                var index = Array.IndexOf(touchpadInvertToValue, invert);
                return index;
            }
            set
            {
                var invert = touchpadInvertToValue[value];
                currentProfile.TouchPadInvert = invert;
            }
        }

        public bool LowerRightTouchRMB
        {
            get => currentProfile.LowerRCOn;
            set => currentProfile.LowerRCOn = value;
        }

        public bool TouchpadClickPassthru
        {
            get => currentProfile.TouchClickPassthru;
            set => currentProfile.TouchClickPassthru = value;
        }

        public bool StartTouchpadOff
        {
            get => currentProfile.StartTouchpadOff;
            set => currentProfile.StartTouchpadOff = value;
        }

        public double TouchRelMouseRotation
        {
            get => currentProfile.TouchPadRelMouse.Rotation * 180.0 / Math.PI;
            set => currentProfile.TouchPadRelMouse.Rotation = value * Math.PI / 180.0;
        }

        public double TouchRelMouseMinThreshold
        {
            get => currentProfile.TouchPadRelMouse.MinThreshold;
            set
            {
                var temp = currentProfile.TouchPadRelMouse.MinThreshold;
                if (temp == value) return;
                currentProfile.TouchPadRelMouse.MinThreshold = value;
            }
        }

        public bool TouchTrackball
        {
            get => currentProfile.TrackballMode;
            set => currentProfile.TrackballMode = value;
        }

        public double TouchTrackballFriction
        {
            get => currentProfile.TrackballFriction;
            set => currentProfile.TrackballFriction = value;
        }

        public int TouchAbsMouseMaxZoneX
        {
            get => currentProfile.TouchPadAbsMouse.MaxZoneX;
            set
            {
                var temp = currentProfile.TouchPadAbsMouse.MaxZoneX;
                if (temp == value) return;
                currentProfile.TouchPadAbsMouse.MaxZoneX = value;
            }
        }

        public int TouchAbsMouseMaxZoneY
        {
            get => currentProfile.TouchPadAbsMouse.MaxZoneY;
            set
            {
                var temp = currentProfile.TouchPadAbsMouse.MaxZoneY;
                if (temp == value) return;
                currentProfile.TouchPadAbsMouse.MaxZoneY = value;
            }
        }

        public bool TouchAbsMouseSnapCenter
        {
            get => currentProfile.TouchPadAbsMouse.SnapToCenter;
            set
            {
                var temp = currentProfile.TouchPadAbsMouse.SnapToCenter;
                if (temp == value) return;
                currentProfile.TouchPadAbsMouse.SnapToCenter = value;
            }
        }

        public bool GyroMouseTurns
        {
            get => currentProfile.GyroTriggerTurns;
            set => currentProfile.GyroTriggerTurns = value;
        }

        public int GyroSensitivity
        {
            get => currentProfile.GyroSensitivity;
            set => currentProfile.GyroSensitivity = value;
        }

        public int GyroVertScale
        {
            get => currentProfile.GyroSensVerticalScale;
            set => currentProfile.GyroSensVerticalScale = value;
        }

        public int GyroMouseEvalCondIndex
        {
            get => Global.Instance.Config.GetSATriggerCondition(Device) ? 0 : 1;
            set => Global.Instance.Config.SetSaTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseXAxis
        {
            get => currentProfile.GyroMouseHorizontalAxis;
            set => currentProfile.GyroMouseHorizontalAxis = value;
        }

        public double GyroMouseMinThreshold
        {
            get => currentProfile.GyroMouseInfo.MinThreshold;
            set
            {
                var temp = currentProfile.GyroMouseInfo.MinThreshold;
                if (temp == value) return;
                currentProfile.GyroMouseInfo.MinThreshold = value;
            }
        }

        public bool GyroMouseInvertX
        {
            get => (currentProfile.GyroInvert & 2) == 2;
            set
            {
                if (value)
                    currentProfile.GyroInvert |= 2;
                else
                    currentProfile.GyroInvert &= ~2;
            }
        }

        public bool GyroMouseInvertY
        {
            get => (currentProfile.GyroInvert & 1) == 1;
            set
            {
                if (value)
                    currentProfile.GyroInvert |= 1;
                else
                    currentProfile.GyroInvert &= ~1;
            }
        }

        public bool GyroMouseSmooth
        {
            get => currentProfile.GyroMouseInfo.EnableSmoothing;
            set
            {
                var tempInfo = currentProfile.GyroMouseInfo;
                if (tempInfo.EnableSmoothing == value) return;

                currentProfile.GyroMouseInfo.EnableSmoothing = value;
                GyroMouseSmoothChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int GyroMouseSmoothMethodIndex
        {
            get => gyroMouseSmoothMethodIndex;
            set
            {
                if (gyroMouseSmoothMethodIndex == value) return;

                var tempInfo = currentProfile.GyroMouseInfo;
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
                GyroMouseSmoothMethodIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Visibility GyroMouseWeightAvgPanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                switch (currentProfile.GyroMouseInfo.Smoothing)
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
                switch (currentProfile.GyroMouseInfo.Smoothing)
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
            get => currentProfile.GyroMouseInfo.SmoothingWeight;
            set => currentProfile.GyroMouseInfo.SmoothingWeight = value;
        }

        public double GyroMouseOneEuroMinCutoff
        {
            get => currentProfile.GyroMouseInfo.MinCutoff;
            set => currentProfile.GyroMouseInfo.MinCutoff = value;
        }

        public double GyroMouseOneEuroBeta
        {
            get => currentProfile.GyroMouseInfo.Beta;
            set => currentProfile.GyroMouseInfo.Beta = value;
        }

        public int GyroMouseStickSmoothMethodIndex
        {
            get => gyroMouseStickSmoothMethodIndex;
            set
            {
                if (gyroMouseStickSmoothMethodIndex == value) return;

                var tempInfo = currentProfile.GyroMouseStickInfo;
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
                GyroMouseStickSmoothMethodIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Visibility GyroMouseStickWeightAvgPanelVisibility
        {
            get
            {
                var result = Visibility.Collapsed;
                switch (currentProfile.GyroMouseStickInfo.Smoothing)
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
                switch (currentProfile.GyroMouseStickInfo.Smoothing)
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
            get => currentProfile.GyroMouseStickInfo.SmoothWeight;
            set => currentProfile.GyroMouseStickInfo.SmoothWeight = value;
        }

        public double GyroMouseStickOneEuroMinCutoff
        {
            get => currentProfile.GyroMouseStickInfo.MinCutoff;
            set => currentProfile.GyroMouseStickInfo.MinCutoff = value;
        }

        public double GyroMouseStickOneEuroBeta
        {
            get => currentProfile.GyroMouseStickInfo.Beta;
            set => currentProfile.GyroMouseStickInfo.Beta = value;
        }


        public int GyroMouseDeadZone
        {
            get => Global.Instance.Config.GyroMouseDeadZone[Device];
            set => Global.Instance.Config.SetGyroMouseDZ(Device, value, rootHub);
        }

        public bool GyroMouseToggle
        {
            get => currentProfile.GyroMouseToggle;
            set => Global.Instance.Config.SetGyroMouseToggle(Device, value, rootHub);
        }

        public bool GyroMouseStickTurns
        {
            get => currentProfile.GyroMouseStickTriggerTurns;
            set => currentProfile.GyroMouseStickTriggerTurns = value;
        }

        public bool GyroMouseStickToggle
        {
            get => currentProfile.GyroMouseStickToggle;
            set => Global.Instance.Config.SetGyroMouseStickToggle(Device, value, rootHub);
        }

        public int GyroMouseStickDeadZone
        {
            get => currentProfile.GyroMouseStickInfo.DeadZone;
            set => currentProfile.GyroMouseStickInfo.DeadZone = value;
        }

        public int GyroMouseStickMaxZone
        {
            get => currentProfile.GyroMouseStickInfo.MaxZone;
            set => currentProfile.GyroMouseStickInfo.MaxZone = value;
        }

        public int GyroMouseStickOutputStick
        {
            get => (int)currentProfile.GyroMouseStickInfo.OutStick;
            set =>
                currentProfile.GyroMouseStickInfo.OutStick =
                    (GyroMouseStickInfo.OutputStick)value;
        }

        public int GyroMouseStickOutputAxes
        {
            get => (int)currentProfile.GyroMouseStickInfo.OutputStickDir;
            set =>
                currentProfile.GyroMouseStickInfo.OutputStickDir =
                    (GyroMouseStickInfo.OutputStickAxes)value;
        }

        public double GyroMouseStickAntiDeadX
        {
            get => currentProfile.GyroMouseStickInfo.AntiDeadX * 100.0;
            set => currentProfile.GyroMouseStickInfo.AntiDeadX = value * 0.01;
        }

        public double GyroMouseStickAntiDeadY
        {
            get => currentProfile.GyroMouseStickInfo.AntiDeadY * 100.0;
            set => currentProfile.GyroMouseStickInfo.AntiDeadY = value * 0.01;
        }

        public int GyroMouseStickVertScale
        {
            get => currentProfile.GyroMouseStickInfo.VerticalScale;
            set => currentProfile.GyroMouseStickInfo.VerticalScale = value;
        }

        public bool GyroMouseStickMaxOutputEnabled
        {
            get => currentProfile.GyroMouseStickInfo.MaxOutputEnabled;
            set
            {
                var temp = currentProfile.GyroMouseStickInfo.MaxOutputEnabled;
                if (temp == value) return;
                currentProfile.GyroMouseStickInfo.MaxOutputEnabled = value;
                GyroMouseStickMaxOutputChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double GyroMouseStickMaxOutput
        {
            get => currentProfile.GyroMouseStickInfo.MaxOutput;
            set => currentProfile.GyroMouseStickInfo.MaxOutput = value;
        }

        public int GyroMouseStickEvalCondIndex
        {
            get => currentProfile.SAMouseStickTriggerCond ? 0 : 1;
            set => Global.Instance.Config.SetSaMouseStickTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseStickXAxis
        {
            get => currentProfile.GyroMouseStickHorizontalAxis;
            set => currentProfile.GyroMouseStickHorizontalAxis = value;
        }

        public bool GyroMouseStickInvertX
        {
            get => (currentProfile.GyroMouseStickInfo.Inverted & 1) == 1;
            set
            {
                if (value)
                {
                    currentProfile.GyroMouseStickInfo.Inverted |= 1;
                }
                else
                {
                    var temp = currentProfile.GyroMouseStickInfo.Inverted;
                    currentProfile.GyroMouseStickInfo.Inverted = (uint)(temp & ~1);
                }
            }
        }

        public bool GyroMouseStickInvertY
        {
            get => (currentProfile.GyroMouseStickInfo.Inverted & 2) == 2;
            set
            {
                if (value)
                {
                    currentProfile.GyroMouseStickInfo.Inverted |= 2;
                }
                else
                {
                    var temp = currentProfile.GyroMouseStickInfo.Inverted;
                    currentProfile.GyroMouseStickInfo.Inverted = (uint)(temp & ~2);
                }
            }
        }

        public bool GyroMouseStickSmooth
        {
            get => currentProfile.GyroMouseStickInfo.UseSmoothing;
            set => currentProfile.GyroMouseStickInfo.UseSmoothing = value;
        }

        public double GyroMousetickSmoothWeight
        {
            get => currentProfile.GyroMouseStickInfo.SmoothWeight;
            set => currentProfile.GyroMouseStickInfo.SmoothWeight = value;
        }

        public string TouchDisInvertString
        {
            get => touchDisInvertString;
            set
            {
                touchDisInvertString = value;
                TouchDisInvertStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GyroControlsTrigDisplay
        {
            get => gyroControlsTrigDisplay;
            set
            {
                gyroControlsTrigDisplay = value;
                GyroControlsTrigDisplayChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool GyroControlsTurns
        {
            get => currentProfile.GyroControlsInfo.TriggerTurns;
            set => currentProfile.GyroControlsInfo.TriggerTurns = value;
        }

        public int GyroControlsEvalCondIndex
        {
            get => currentProfile.GyroControlsInfo.TriggerCond ? 0 : 1;
            set => currentProfile.GyroControlsInfo.TriggerCond =
                value == 0 ? true : false;
        }

        public bool GyroControlsToggle
        {
            get => currentProfile.GyroControlsInfo.TriggerToggle;
            set => Global.Instance.Config.SetGyroControlsToggle(Device, value, rootHub);
        }

        public string GyroMouseTrigDisplay
        {
            get => gyroMouseTrigDisplay;
            set
            {
                gyroMouseTrigDisplay = value;
                GyroMouseTrigDisplayChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GyroMouseStickTrigDisplay
        {
            get => gyroMouseStickTrigDisplay;
            set
            {
                gyroMouseStickTrigDisplay = value;
                GyroMouseStickTrigDisplayChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GyroSwipeTrigDisplay
        {
            get => gyroSwipeTrigDisplay;
            set
            {
                gyroSwipeTrigDisplay = value;
                GyroSwipeTrigDisplayChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool GyroSwipeTurns
        {
            get => currentProfile.GyroSwipeInfo.TriggerTurns;
            set => currentProfile.GyroSwipeInfo.TriggerTurns = value;
        }

        public int GyroSwipeEvalCondIndex
        {
            get => currentProfile.GyroSwipeInfo.TriggerCondition ? 0 : 1;
            set => currentProfile.GyroSwipeInfo.TriggerCondition = value == 0 ? true : false;
        }

        public int GyroSwipeXAxis
        {
            get => (int)currentProfile.GyroSwipeInfo.XAxis;
            set => currentProfile.GyroSwipeInfo.XAxis = (GyroDirectionalSwipeInfo.XAxisSwipe)value;
        }

        public int GyroSwipeDeadZoneX
        {
            get => currentProfile.GyroSwipeInfo.DeadZoneX;
            set => currentProfile.GyroSwipeInfo.DeadZoneX = value;
        }

        public int GyroSwipeDeadZoneY
        {
            get => currentProfile.GyroSwipeInfo.DeadZoneY;
            set => currentProfile.GyroSwipeInfo.DeadZoneY = value;
        }

        public int GyroSwipeDelayTime
        {
            get => currentProfile.GyroSwipeInfo.DelayTime;
            set => currentProfile.GyroSwipeInfo.DelayTime = value;
        }
    }
}
