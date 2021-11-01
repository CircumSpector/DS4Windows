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
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RumbleBoost;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RumbleBoost = (byte)value;
        }

        public int RumbleAutostopTime
        {
            // RumbleAutostopTime value is in milliseconds in XML config file, but GUI uses just seconds
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RumbleAutostopTime / 1000;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RumbleAutostopTime = value * 1000;
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
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.buttonSensitivity;
            set
            {
                var temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.buttonSensitivity;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.ButtonSensitivity = value;
                ButtonMouseSensitivityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ButtonMouseVerticalScale
        {
            get => Convert.ToInt32(ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo
                .buttonVerticalScale * 100.0);
            set
            {
                var temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo
                    .buttonVerticalScale;
                var attemptValue = value * 0.01;
                if (temp == attemptValue) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.buttonVerticalScale =
                    attemptValue;
                ButtonMouseVerticalScaleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private double RawButtonMouseOffset => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo
            .mouseVelocityOffset;

        public double ButtonMouseOffset
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.mouseVelocityOffset *
                   100.0;
            set
            {
                var temp =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.mouseVelocityOffset *
                    100.0;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.mouseVelocityOffset =
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
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.mouseAccel;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ButtonMouseInfo.mouseAccel = value;
        }

        public bool EnableTouchpadToggle
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).EnableTouchToggle;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).EnableTouchToggle = value;
        }

        public bool EnableOutputDataToDS4
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).EnableOutputDataToDS4;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).EnableOutputDataToDS4 = value;
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
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).DisableVirtualController;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).DisableVirtualController = value;
        }

        public bool IdleDisconnectExists
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).IdleDisconnectTimeout != 0;
            set
            {
                // If enabling Idle Disconnect, set default time.
                // Otherwise, set time to 0 to mean disabled
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).IdleDisconnectTimeout =
                    value ? Global.DEFAULT_ENABLE_IDLE_DISCONN_MINS * 60 : 0;

                IdleDisconnectChanged?.Invoke(this, EventArgs.Empty);
                IdleDisconnectExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int IdleDisconnect
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).IdleDisconnectTimeout / 60;
            set
            {
                var temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).IdleDisconnectTimeout / 60;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).IdleDisconnectTimeout = value * 60;
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
                switch (ProfilesService.Instance.ActiveProfiles.ElementAt(Device).OutputDeviceType)
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
                switch (ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroOutputMode)
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

                var current = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroOutputMode;
                if (temp == current) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroOutputMode = temp;
                GyroOutModeIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public OutContType ContType => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).OutputDeviceType;

        public int SASteeringWheelEmulationAxisIndex
        {
            get => (int)ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationAxis;
            set
            {
                var temp = (int)ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationAxis;
                if (temp == value) return;

                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationAxis =
                    (SASteeringWheelEmulationAxisType)value;
                SASteeringWheelEmulationAxisIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SASteeringWheelEmulationRangeIndex
        {
            get
            {
                var index = 360;
                switch (ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationRange)
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
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationRange = temp;
            }
        }

        public int SASteeringWheelEmulationRange
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationRange;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SASteeringWheelEmulationRange = value;
        }

        public int SASteeringWheelFuzz
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SAWheelFuzzValues;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SAWheelFuzzValues = value;
        }

        public bool SASteeringWheelUseSmoothing
        {
            get => Global.Instance.Config.WheelSmoothInfo[Device].Enabled;
            set
            {
                var temp = Global.Instance.Config.WheelSmoothInfo[Device].Enabled;
                if (temp == value) return;
                Global.Instance.Config.WheelSmoothInfo[Device].Enabled = value;
                SASteeringWheelUseSmoothingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SASteeringWheelSmoothMinCutoff
        {
            get => Global.Instance.Config.WheelSmoothInfo[Device].MinCutoff;
            set => Global.Instance.Config.WheelSmoothInfo[Device].MinCutoff = value;
        }

        public double SASteeringWheelSmoothBeta
        {
            get => Global.Instance.Config.WheelSmoothInfo[Device].Beta;
            set => Global.Instance.Config.WheelSmoothInfo[Device].Beta = value;
        }

        public double LSDeadZone
        {
            get => Math.Round(Global.Instance.Config.LSModInfo[Device].DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(Global.Instance.Config.LSModInfo[Device].DeadZone / 127d, 2);
                if (temp == value) return;
                Global.Instance.Config.LSModInfo[Device].DeadZone = (int)Math.Round(value * 127d);
                LSDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double RSDeadZone
        {
            get => Math.Round(Global.Instance.Config.RSModInfo[Device].DeadZone / 127d, 2);
            set
            {
                var temp = Math.Round(Global.Instance.Config.RSModInfo[Device].DeadZone / 127d, 2);
                if (temp == value) return;
                Global.Instance.Config.RSModInfo[Device].DeadZone = (int)Math.Round(value * 127d);
                RSDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double LSMaxZone
        {
            get => Global.Instance.Config.LSModInfo[Device].MaxZone / 100.0;
            set => Global.Instance.Config.LSModInfo[Device].MaxZone = (int)(value * 100.0);
        }

        public double RSMaxZone
        {
            get => Global.Instance.Config.RSModInfo[Device].MaxZone / 100.0;
            set => Global.Instance.Config.RSModInfo[Device].MaxZone = (int)(value * 100.0);
        }

        public double LSAntiDeadZone
        {
            get => Global.Instance.Config.LSModInfo[Device].AntiDeadZone / 100.0;
            set => Global.Instance.Config.LSModInfo[Device].AntiDeadZone = (int)(value * 100.0);
        }

        public double RSAntiDeadZone
        {
            get => Global.Instance.Config.RSModInfo[Device].AntiDeadZone / 100.0;
            set => Global.Instance.Config.RSModInfo[Device].AntiDeadZone = (int)(value * 100.0);
        }

        public double LSVerticalScale
        {
            get => Global.Instance.Config.LSModInfo[Device].VerticalScale / 100.0;
            set => Global.Instance.Config.LSModInfo[Device].VerticalScale = value * 100.0;
        }

        public double LSMaxOutput
        {
            get => Global.Instance.Config.LSModInfo[Device].MaxOutput / 100.0;
            set => Global.Instance.Config.LSModInfo[Device].MaxOutput = value * 100.0;
        }

        public bool LSMaxOutputForce
        {
            get => Global.Instance.Config.LSModInfo[Device].MaxOutputForce;
            set => Global.Instance.Config.LSModInfo[Device].MaxOutputForce = value;
        }

        public double RSVerticalScale
        {
            get => Global.Instance.Config.RSModInfo[Device].VerticalScale / 100.0;
            set => Global.Instance.Config.RSModInfo[Device].VerticalScale = value * 100.0;
        }

        public double RSMaxOutput
        {
            get => Global.Instance.Config.RSModInfo[Device].MaxOutput / 100.0;
            set => Global.Instance.Config.RSModInfo[Device].MaxOutput = value * 100.0;
        }

        public bool RSMaxOutputForce
        {
            get => Global.Instance.Config.RSModInfo[Device].MaxOutputForce;
            set => Global.Instance.Config.RSModInfo[Device].MaxOutputForce = value;
        }

        public int LSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (Global.Instance.Config.LSModInfo[Device].DZType)
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

                var current = Global.Instance.Config.LSModInfo[Device].DZType;
                if (temp == current) return;
                Global.Instance.Config.LSModInfo[Device].DZType = temp;
            }
        }

        public int RSDeadTypeIndex
        {
            get
            {
                var index = 0;
                switch (Global.Instance.Config.RSModInfo[Device].DZType)
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

                var current = Global.Instance.Config.RSModInfo[Device].DZType;
                if (temp == current) return;
                Global.Instance.Config.RSModInfo[Device].DZType = temp;
            }
        }

        public double LSSens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).LSSens;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).LSSens = value;
        }

        public double RSSens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RSSens;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RSSens = value;
        }

        public bool LSSquareStick
        {
            get => Global.Instance.Config.SquStickInfo[Device].LSMode;
            set => Global.Instance.Config.SquStickInfo[Device].LSMode = value;
        }

        public bool RSSquareStick
        {
            get => Global.Instance.Config.SquStickInfo[Device].RSMode;
            set => Global.Instance.Config.SquStickInfo[Device].RSMode = value;
        }

        public double LSSquareRoundness
        {
            get => Global.Instance.Config.SquStickInfo[Device].LSRoundness;
            set => Global.Instance.Config.SquStickInfo[Device].LSRoundness = value;
        }

        public double RSSquareRoundness
        {
            get => Global.Instance.Config.SquStickInfo[Device].RSRoundness;
            set => Global.Instance.Config.SquStickInfo[Device].RSRoundness = value;
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
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).LSRotation * 180.0 / Math.PI;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).LSRotation = value * Math.PI / 180.0;
        }

        public double RSRotation
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RSRotation * 180.0 / Math.PI;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).RSRotation = value * Math.PI / 180.0;
        }

        public bool LSCustomCurveSelected => Global.Instance.Config.GetLsOutCurveMode(Device) == 6;

        public bool RSCustomCurveSelected => Global.Instance.Config.GetRsOutCurveMode(Device) == 6;

        public string LSCustomCurve
        {
            get => Global.Instance.Config.LSOutBezierCurveObj[Device].CustomDefinition;
            set => Global.Instance.Config.LSOutBezierCurveObj[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.LSRS, true);
        }

        public string RSCustomCurve
        {
            get => Global.Instance.Config.RSOutBezierCurveObj[Device].CustomDefinition;
            set => Global.Instance.Config.RSOutBezierCurveObj[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.LSRS, true);
        }

        public int LSFuzz
        {
            get => Global.Instance.Config.LSModInfo[Device].Fuzz;
            set => Global.Instance.Config.LSModInfo[Device].Fuzz = value;
        }

        public int RSFuzz
        {
            get => Global.Instance.Config.RSModInfo[Device].Fuzz;
            set => Global.Instance.Config.RSModInfo[Device].Fuzz = value;
        }

        public bool LSAntiSnapback
        {
            get => Global.Instance.Config.LSAntiSnapbackInfo[Device].Enabled;
            set => Global.Instance.Config.LSAntiSnapbackInfo[Device].Enabled = value;
        }

        public bool RSAntiSnapback
        {
            get => Global.Instance.Config.RSAntiSnapbackInfo[Device].Enabled;
            set => Global.Instance.Config.RSAntiSnapbackInfo[Device].Enabled = value;
        }

        public double LSAntiSnapbackDelta
        {
            get => Global.Instance.Config.LSAntiSnapbackInfo[Device].Delta;
            set => Global.Instance.Config.LSAntiSnapbackInfo[Device].Delta = value;
        }

        public double RSAntiSnapbackDelta
        {
            get => Global.Instance.Config.RSAntiSnapbackInfo[Device].Delta;
            set => Global.Instance.Config.RSAntiSnapbackInfo[Device].Delta = value;
        }

        public int LSAntiSnapbackTimeout
        {
            get => Global.Instance.Config.LSAntiSnapbackInfo[Device].Timeout;
            set => Global.Instance.Config.LSAntiSnapbackInfo[Device].Timeout = value;
        }

        public int RSAntiSnapbackTimeout
        {
            get => Global.Instance.Config.RSAntiSnapbackInfo[Device].Timeout;
            set => Global.Instance.Config.RSAntiSnapbackInfo[Device].Timeout = value;
        }

        public bool LSOuterBindInvert
        {
            get => Global.Instance.Config.LSModInfo[Device].OuterBindInvert;
            set => Global.Instance.Config.LSModInfo[Device].OuterBindInvert = value;
        }

        public bool RSOuterBindInvert
        {
            get => Global.Instance.Config.RSModInfo[Device].OuterBindInvert;
            set => Global.Instance.Config.RSModInfo[Device].OuterBindInvert = value;
        }

        public double LSOuterBindDead
        {
            get => Global.Instance.Config.LSModInfo[Device].OuterBindDeadZone / 100.0;
            set => Global.Instance.Config.LSModInfo[Device].OuterBindDeadZone = value * 100.0;
        }

        public double RSOuterBindDead
        {
            get => Global.Instance.Config.RSModInfo[Device].OuterBindDeadZone / 100.0;
            set => Global.Instance.Config.RSModInfo[Device].OuterBindDeadZone = value * 100.0;
        }

        public int LSOutputIndex
        {
            get
            {
                var index = 0;
                switch (Global.Instance.Config.LSOutputSettings[Device].Mode)
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

                var current = Global.Instance.Config.LSOutputSettings[Device].Mode;
                if (temp == current) return;
                Global.Instance.Config.LSOutputSettings[Device].Mode = temp;
                LSOutputIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double LSFlickRWC
        {
            get => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.realWorldCalibration;
            set => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.realWorldCalibration =
                value;
        }

        public double LSFlickThreshold
        {
            get => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.flickThreshold;
            set => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.flickThreshold = value;
        }

        public double LSFlickTime
        {
            get => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.flickTime;
            set => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.flickTime = value;
        }

        public double LSMinAngleThreshold
        {
            get => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.minAngleThreshold;
            set => Global.Instance.Config.LSOutputSettings[Device].OutputSettings.flickSettings.minAngleThreshold =
                value;
        }

        public int RSOutputIndex
        {
            get
            {
                var index = 0;
                switch (Global.Instance.Config.RSOutputSettings[Device].Mode)
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

                var current = Global.Instance.Config.RSOutputSettings[Device].Mode;
                if (temp == current) return;
                Global.Instance.Config.RSOutputSettings[Device].Mode = temp;
                RSOutputIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double RSFlickRWC
        {
            get => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.realWorldCalibration;
            set => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.realWorldCalibration =
                value;
        }

        public double RSFlickThreshold
        {
            get => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.flickThreshold;
            set => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.flickThreshold = value;
        }

        public double RSFlickTime
        {
            get => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.flickTime;
            set => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.flickTime = value;
        }

        public double RSMinAngleThreshold
        {
            get => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.minAngleThreshold;
            set => Global.Instance.Config.RSOutputSettings[Device].OutputSettings.flickSettings.minAngleThreshold =
                value;
        }

        public double L2DeadZone
        {
            get => Global.Instance.Config.L2ModInfo[Device].deadZone / 255.0;
            set
            {
                var temp = Global.Instance.Config.L2ModInfo[Device].deadZone / 255.0;
                if (temp == value) return;
                Global.Instance.Config.L2ModInfo[Device].deadZone = (byte)(value * 255.0);
                L2DeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double R2DeadZone
        {
            get => Global.Instance.Config.R2ModInfo[Device].deadZone / 255.0;
            set
            {
                var temp = Global.Instance.Config.R2ModInfo[Device].deadZone / 255.0;
                if (temp == value) return;
                Global.Instance.Config.R2ModInfo[Device].deadZone = (byte)(value * 255.0);
                R2DeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double L2MaxZone
        {
            get => Global.Instance.Config.L2ModInfo[Device].MaxZone / 100.0;
            set => Global.Instance.Config.L2ModInfo[Device].MaxZone = (int)(value * 100.0);
        }

        public double R2MaxZone
        {
            get => Global.Instance.Config.R2ModInfo[Device].MaxZone / 100.0;
            set => Global.Instance.Config.R2ModInfo[Device].MaxZone = (int)(value * 100.0);
        }

        public double L2AntiDeadZone
        {
            get => Global.Instance.Config.L2ModInfo[Device].AntiDeadZone / 100.0;
            set => Global.Instance.Config.L2ModInfo[Device].AntiDeadZone = (int)(value * 100.0);
        }

        public double R2AntiDeadZone
        {
            get => Global.Instance.Config.R2ModInfo[Device].AntiDeadZone / 100.0;
            set => Global.Instance.Config.R2ModInfo[Device].AntiDeadZone = (int)(value * 100.0);
        }

        public double L2MaxOutput
        {
            get => Global.Instance.Config.L2ModInfo[Device].MaxOutput / 100.0;
            set => Global.Instance.Config.L2ModInfo[Device].MaxOutput = value * 100.0;
        }

        public double R2MaxOutput
        {
            get => Global.Instance.Config.R2ModInfo[Device].MaxOutput / 100.0;
            set => Global.Instance.Config.R2ModInfo[Device].MaxOutput = value * 100.0;
        }

        public double L2Sens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).L2Sens;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).L2Sens = value;
        }

        public double R2Sens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).R2Sens;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).R2Sens = value;
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
            get => Global.Instance.Config.L2OutBezierCurveObj[Device].CustomDefinition;
            set => Global.Instance.Config.L2OutBezierCurveObj[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.L2R2, true);
        }

        public string R2CustomCurve
        {
            get => Global.Instance.Config.R2OutBezierCurveObj[Device].CustomDefinition;
            set => Global.Instance.Config.R2OutBezierCurveObj[Device]
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
            get => Global.Instance.Config.L2OutputSettings[Device].twoStageMode;
            set
            {
                var temp = Global.Instance.Config.L2OutputSettings[Device].TwoStageMode;
                if (temp == value) return;

                Global.Instance.Config.L2OutputSettings[Device].TwoStageMode = value;
                L2TriggerModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TwoStageTriggerMode R2TriggerMode
        {
            get => Global.Instance.Config.R2OutputSettings[Device].TwoStageMode;
            set
            {
                var temp = Global.Instance.Config.R2OutputSettings[Device].TwoStageMode;
                if (temp == value) return;

                Global.Instance.Config.R2OutputSettings[Device].twoStageMode = value;
                R2TriggerModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int L2HipFireTime
        {
            get => Global.Instance.Config.L2OutputSettings[Device].hipFireMS;
            set => Global.Instance.Config.L2OutputSettings[Device].hipFireMS = value;
        }

        public int R2HipFireTime
        {
            get => Global.Instance.Config.R2OutputSettings[Device].hipFireMS;
            set => Global.Instance.Config.R2OutputSettings[Device].hipFireMS = value;
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
            get => Global.Instance.Config.L2OutputSettings[Device].triggerEffect;
            set
            {
                var temp = Global.Instance.Config.L2OutputSettings[Device].TriggerEffect;
                if (temp == value) return;

                Global.Instance.Config.L2OutputSettings[Device].TriggerEffect = value;
            }
        }

        public TriggerEffects R2TriggerEffect
        {
            get => Global.Instance.Config.R2OutputSettings[Device].triggerEffect;
            set
            {
                var temp = Global.Instance.Config.R2OutputSettings[Device].TriggerEffect;
                if (temp == value) return;

                Global.Instance.Config.R2OutputSettings[Device].TriggerEffect = value;
            }
        }

        public double SXDeadZone
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXDeadZone;
            set
            {
                var temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXDeadZone;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXDeadZone = value;
                SXDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SZDeadZone
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZDeadZone;
            set
            {
                var temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZDeadZone;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZDeadZone = value;
                SZDeadZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SXMaxZone
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXMaxZone;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXMaxZone = value;
        }

        public double SZMaxZone
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZMaxZone;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZMaxZone = value;
        }

        public double SXAntiDeadZone
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXAntiDeadZone;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXAntiDeadZone = value;
        }

        public double SZAntiDeadZone
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZAntiDeadZone;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZAntiDeadZone = value;
        }

        public double SXSens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXSens;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SXSens = value;
        }

        public double SZSens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZSens;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SZSens = value;
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
            get => Global.Instance.Config.SXOutBezierCurveObj[Device].CustomDefinition;
            set => Global.Instance.Config.SXOutBezierCurveObj[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.SA, true);
        }

        public string SZCustomCurve
        {
            get => Global.Instance.Config.SZOutBezierCurveObj[Device].CustomDefinition;
            set => Global.Instance.Config.SZOutBezierCurveObj[Device]
                .InitBezierCurve(value, BezierCurve.AxisType.SA, true);
        }

        public int TouchpadOutputIndex
        {
            get
            {
                var index = 0;
                switch (ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchOutMode)
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

                var current = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchOutMode;
                if (temp == current) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchOutMode = temp;
                TouchpadOutputIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchSenExists
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchSensitivity != 0;
            set
            {
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchSensitivity =
                    value ? (byte)100 : (byte)0;
                TouchSenExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TouchSens
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchSensitivity;
            set
            {
                int temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchSensitivity;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchSensitivity = (byte)value;
                if (value == 0) TouchSenExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchScrollExists
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ScrollSensitivity != 0;
            set
            {
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ScrollSensitivity = value ? 100 : 0;
                TouchScrollExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchScrollChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TouchScroll
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ScrollSensitivity;
            set
            {
                var temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ScrollSensitivity;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).ScrollSensitivity = value;
                if (value == 0) TouchScrollExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchScrollChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchTapExists
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TapSensitivity != 0;
            set
            {
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TapSensitivity = value ? (byte)100 : (byte)0;
                TouchTapExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchTapChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TouchTap
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TapSensitivity;
            set
            {
                int temp = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TapSensitivity;
                if (temp == value) return;
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TapSensitivity = (byte)value;
                if (value == 0) TouchTapExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchTapChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TouchDoubleTap
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).DoubleTap;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).DoubleTap = value;
        }

        public bool TouchJitter
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchpadJitterCompensation;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchpadJitterCompensation = value;
        }

        public int TouchInvertIndex
        {
            get
            {
                var invert = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchPadInvert;
                var index = Array.IndexOf(touchpadInvertToValue, invert);
                return index;
            }
            set
            {
                var invert = touchpadInvertToValue[value];
                ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchPadInvert = invert;
            }
        }

        public bool LowerRightTouchRMB
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).LowerRCOn;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).LowerRCOn = value;
        }

        public bool TouchpadClickPassthru
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchClickPassthru;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TouchClickPassthru = value;
        }

        public bool StartTouchpadOff
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).StartTouchpadOff;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).StartTouchpadOff = value;
        }

        public double TouchRelMouseRotation
        {
            get => Global.Instance.Config.TouchPadRelMouse[Device].Rotation * 180.0 / Math.PI;
            set => Global.Instance.Config.TouchPadRelMouse[Device].Rotation = value * Math.PI / 180.0;
        }

        public double TouchRelMouseMinThreshold
        {
            get => Global.Instance.Config.TouchPadRelMouse[Device].MinThreshold;
            set
            {
                var temp = Global.Instance.Config.TouchPadRelMouse[Device].MinThreshold;
                if (temp == value) return;
                Global.Instance.Config.TouchPadRelMouse[Device].MinThreshold = value;
            }
        }

        public bool TouchTrackball
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TrackballMode;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TrackballMode = value;
        }

        public double TouchTrackballFriction
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TrackballFriction;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).TrackballFriction = value;
        }

        public int TouchAbsMouseMaxZoneX
        {
            get => Global.Instance.Config.TouchPadAbsMouse[Device].MaxZoneX;
            set
            {
                var temp = Global.Instance.Config.TouchPadAbsMouse[Device].MaxZoneX;
                if (temp == value) return;
                Global.Instance.Config.TouchPadAbsMouse[Device].MaxZoneX = value;
            }
        }

        public int TouchAbsMouseMaxZoneY
        {
            get => Global.Instance.Config.TouchPadAbsMouse[Device].MaxZoneY;
            set
            {
                var temp = Global.Instance.Config.TouchPadAbsMouse[Device].MaxZoneY;
                if (temp == value) return;
                Global.Instance.Config.TouchPadAbsMouse[Device].MaxZoneY = value;
            }
        }

        public bool TouchAbsMouseSnapCenter
        {
            get => Global.Instance.Config.TouchPadAbsMouse[Device].SnapToCenter;
            set
            {
                var temp = Global.Instance.Config.TouchPadAbsMouse[Device].SnapToCenter;
                if (temp == value) return;
                Global.Instance.Config.TouchPadAbsMouse[Device].SnapToCenter = value;
            }
        }

        public bool GyroMouseTurns
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroTriggerTurns;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroTriggerTurns = value;
        }

        public int GyroSensitivity
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSensitivity;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSensitivity = value;
        }

        public int GyroVertScale
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSensVerticalScale;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSensVerticalScale = value;
        }

        public int GyroMouseEvalCondIndex
        {
            get => Global.Instance.Config.GetSATriggerCondition(Device) ? 0 : 1;
            set => Global.Instance.Config.SetSaTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseXAxis
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseHorizontalAxis;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseHorizontalAxis = value;
        }

        public double GyroMouseMinThreshold
        {
            get => Global.Instance.Config.GyroMouseInfo[Device].minThreshold;
            set
            {
                var temp = Global.Instance.Config.GyroMouseInfo[Device].minThreshold;
                if (temp == value) return;
                Global.Instance.Config.GyroMouseInfo[Device].minThreshold = value;
            }
        }

        public bool GyroMouseInvertX
        {
            get => (ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroInvert & 2) == 2;
            set
            {
                if (value)
                    ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroInvert |= 2;
                else
                    ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroInvert &= ~2;
            }
        }

        public bool GyroMouseInvertY
        {
            get => (ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroInvert & 1) == 1;
            set
            {
                if (value)
                    ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroInvert |= 1;
                else
                    ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroInvert &= ~1;
            }
        }

        public bool GyroMouseSmooth
        {
            get => Global.Instance.Config.GyroMouseInfo[Device].enableSmoothing;
            set
            {
                var tempInfo = Global.Instance.Config.GyroMouseInfo[Device];
                if (tempInfo.enableSmoothing == value) return;

                Global.Instance.Config.GyroMouseInfo[Device].enableSmoothing = value;
                GyroMouseSmoothChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int GyroMouseSmoothMethodIndex
        {
            get => gyroMouseSmoothMethodIndex;
            set
            {
                if (gyroMouseSmoothMethodIndex == value) return;

                var tempInfo = Global.Instance.Config.GyroMouseInfo[Device];
                switch (value)
                {
                    case 0:
                        tempInfo.ResetSmoothingMethods();
                        tempInfo.smoothingMethod = GyroMouseInfo.SmoothingMethod.OneEuro;
                        break;
                    case 1:
                        tempInfo.ResetSmoothingMethods();
                        tempInfo.smoothingMethod = GyroMouseInfo.SmoothingMethod.WeightedAverage;
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
                switch (Global.Instance.Config.GyroMouseInfo[Device].smoothingMethod)
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
                switch (Global.Instance.Config.GyroMouseInfo[Device].smoothingMethod)
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
            get => Global.Instance.Config.GyroMouseInfo[Device].smoothingWeight;
            set => Global.Instance.Config.GyroMouseInfo[Device].smoothingWeight = value;
        }

        public double GyroMouseOneEuroMinCutoff
        {
            get => Global.Instance.Config.GyroMouseInfo[Device].MinCutoff;
            set => Global.Instance.Config.GyroMouseInfo[Device].MinCutoff = value;
        }

        public double GyroMouseOneEuroBeta
        {
            get => Global.Instance.Config.GyroMouseInfo[Device].Beta;
            set => Global.Instance.Config.GyroMouseInfo[Device].Beta = value;
        }

        public int GyroMouseStickSmoothMethodIndex
        {
            get => gyroMouseStickSmoothMethodIndex;
            set
            {
                if (gyroMouseStickSmoothMethodIndex == value) return;

                var tempInfo = Global.Instance.Config.GyroMouseStickInfo[Device];
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
                switch (Global.Instance.Config.GyroMouseStickInfo[Device].Smoothing)
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
                switch (Global.Instance.Config.GyroMouseStickInfo[Device].Smoothing)
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
            get => Global.Instance.Config.GyroMouseStickInfo[Device].SmoothWeight;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].SmoothWeight = value;
        }

        public double GyroMouseStickOneEuroMinCutoff
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].MinCutoff;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].MinCutoff = value;
        }

        public double GyroMouseStickOneEuroBeta
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].Beta;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].Beta = value;
        }


        public int GyroMouseDeadZone
        {
            get => Global.Instance.Config.GyroMouseDeadZone[Device];
            set => Global.Instance.Config.SetGyroMouseDZ(Device, value, rootHub);
        }

        public bool GyroMouseToggle
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseToggle;
            set => Global.Instance.Config.SetGyroMouseToggle(Device, value, rootHub);
        }

        public bool GyroMouseStickTurns
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseStickTriggerTurns;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseStickTriggerTurns = value;
        }

        public bool GyroMouseStickToggle
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseStickToggle;
            set => Global.Instance.Config.SetGyroMouseStickToggle(Device, value, rootHub);
        }

        public int GyroMouseStickDeadZone
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].DeadZone;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].DeadZone = value;
        }

        public int GyroMouseStickMaxZone
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].MaxZone;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].MaxZone = value;
        }

        public int GyroMouseStickOutputStick
        {
            get => (int)Global.Instance.Config.GyroMouseStickInfo[Device].outputStick;
            set =>
                Global.Instance.Config.GyroMouseStickInfo[Device].outputStick =
                    (GyroMouseStickInfo.OutputStick)value;
        }

        public int GyroMouseStickOutputAxes
        {
            get => (int)Global.Instance.Config.GyroMouseStickInfo[Device].outputStickDir;
            set =>
                Global.Instance.Config.GyroMouseStickInfo[Device].outputStickDir =
                    (GyroMouseStickInfo.OutputStickAxes)value;
        }

        public double GyroMouseStickAntiDeadX
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].AntiDeadX * 100.0;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].AntiDeadX = value * 0.01;
        }

        public double GyroMouseStickAntiDeadY
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].AntiDeadY * 100.0;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].AntiDeadY = value * 0.01;
        }

        public int GyroMouseStickVertScale
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].VertScale;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].VertScale = value;
        }

        public bool GyroMouseStickMaxOutputEnabled
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].MaxOutputEnabled;
            set
            {
                var temp = Global.Instance.Config.GyroMouseStickInfo[Device].MaxOutputEnabled;
                if (temp == value) return;
                Global.Instance.Config.GyroMouseStickInfo[Device].MaxOutputEnabled = value;
                GyroMouseStickMaxOutputChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double GyroMouseStickMaxOutput
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].MaxOutput;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].MaxOutput = value;
        }

        public int GyroMouseStickEvalCondIndex
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SAMouseStickTriggerCond ? 0 : 1;
            set => Global.Instance.Config.SetSaMouseStickTriggerCond(Device, value == 0 ? "and" : "or");
        }

        public int GyroMouseStickXAxis
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseStickHorizontalAxis;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseStickHorizontalAxis = value;
        }

        public bool GyroMouseStickInvertX
        {
            get => (Global.Instance.Config.GyroMouseStickInfo[Device].Inverted & 1) == 1;
            set
            {
                if (value)
                {
                    Global.Instance.Config.GyroMouseStickInfo[Device].Inverted |= 1;
                }
                else
                {
                    var temp = Global.Instance.Config.GyroMouseStickInfo[Device].Inverted;
                    Global.Instance.Config.GyroMouseStickInfo[Device].Inverted = (uint)(temp & ~1);
                }
            }
        }

        public bool GyroMouseStickInvertY
        {
            get => (Global.Instance.Config.GyroMouseStickInfo[Device].Inverted & 2) == 2;
            set
            {
                if (value)
                {
                    Global.Instance.Config.GyroMouseStickInfo[Device].Inverted |= 2;
                }
                else
                {
                    var temp = Global.Instance.Config.GyroMouseStickInfo[Device].Inverted;
                    Global.Instance.Config.GyroMouseStickInfo[Device].Inverted = (uint)(temp & ~2);
                }
            }
        }

        public bool GyroMouseStickSmooth
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].UseSmoothing;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].UseSmoothing = value;
        }

        public double GyroMousetickSmoothWeight
        {
            get => Global.Instance.Config.GyroMouseStickInfo[Device].SmoothWeight;
            set => Global.Instance.Config.GyroMouseStickInfo[Device].SmoothWeight = value;
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
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.TriggerTurns;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.TriggerTurns = value;
        }

        public int GyroControlsEvalCondIndex
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.TriggerCond ? 0 : 1;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.TriggerCond =
                value == 0 ? true : false;
        }

        public bool GyroControlsToggle
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.TriggerToggle;
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
            get => Global.Instance.Config.GyroSwipeInfo[Device].TriggerTurns;
            set => Global.Instance.Config.GyroSwipeInfo[Device].TriggerTurns = value;
        }

        public int GyroSwipeEvalCondIndex
        {
            get => Global.Instance.Config.GyroSwipeInfo[Device].TriggerCondition ? 0 : 1;
            set => Global.Instance.Config.GyroSwipeInfo[Device].TriggerCondition = value == 0 ? true : false;
        }

        public int GyroSwipeXAxis
        {
            get => (int)Global.Instance.Config.GyroSwipeInfo[Device].XAxis;
            set => Global.Instance.Config.GyroSwipeInfo[Device].XAxis = (GyroDirectionalSwipeInfo.XAxisSwipe)value;
        }

        public int GyroSwipeDeadZoneX
        {
            get => Global.Instance.Config.GyroSwipeInfo[Device].DeadZoneX;
            set => Global.Instance.Config.GyroSwipeInfo[Device].DeadZoneX = value;
        }

        public int GyroSwipeDeadZoneY
        {
            get => Global.Instance.Config.GyroSwipeInfo[Device].DeadZoneY;
            set => Global.Instance.Config.GyroSwipeInfo[Device].DeadZoneY = value;
        }

        public int GyroSwipeDelayTime
        {
            get => Global.Instance.Config.GyroSwipeInfo[Device].DelayTime;
            set => Global.Instance.Config.GyroSwipeInfo[Device].DelayTime = value;
        }
    }
}
